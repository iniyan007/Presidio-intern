using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using TravelTourManagement.Business;
using TravelTourManagement.Business.Configuration;
using TravelTourManagement.DataAccess;
using Quartz;
using Serilog;
using TravelTourManagement.Business.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/traveltour-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/traveltour-.txt", rollingInterval: RollingInterval.Day));

    builder.Services.AddDistributedMemoryCache();

    builder.Services.AddControllers(options =>
{
    options.Filters.Add<TravelTourManagement.API.Filters.RequireEmailVerificationFilter>();
    options.Filters.Add<TravelTourManagement.API.Filters.GlobalExceptionFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value != null && e.Value.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { Field = kvp.Key, Error = e.ErrorMessage }))
            .ToList();

        var response = new 
        {
            success = false,
            message = "Validation failed.",
            details = errors,
            errorCode = "VALIDATION_ERROR"
        };

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // needed for SignalR and cookies if any
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Register Data Access Services (DbContext and Repositories)
builder.Services.AddDataAccessServices(builder.Configuration);

// Register Business Services
builder.Services.AddBusinessServices(builder.Configuration);

// Add User Context and HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TravelTourManagement.DataAccess.Interface.IUserContextService, TravelTourManagement.API.Services.UserContextService>();

// Add SignalR and its dispatcher
builder.Services.AddSignalR();
builder.Services.AddScoped<TravelTourManagement.Business.Interface.INotificationDispatcher, TravelTourManagement.API.Services.SignalRNotificationDispatcher>();
builder.Services.AddScoped<TravelTourManagement.Business.Interface.IMessageDispatcher, TravelTourManagement.API.Services.SignalRMessageDispatcher>();

// Add Quartz services
builder.Services.AddQuartz(q =>
{
    var bookingJobKey = new JobKey("BookingTimeoutJob");
    q.AddJob<BookingTimeoutJob>(opts => opts.WithIdentity(bookingJobKey).StoreDurably()); // Store durably since it will be triggered dynamically

    var completionJobKey = new JobKey("PackageCompletionJob");
    q.AddJob<PackageCompletionJob>(opts => opts.WithIdentity(completionJobKey));
    q.AddTrigger(opts => opts
        .ForJob(completionJobKey)
        .WithIdentity("PackageCompletionJob-trigger")
        .WithCronSchedule("0 0 0 ? * *") // Runs every day at midnight
    );
});

// Add Quartz hosted service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


// Add JWT Authentication
var jwtOptionsSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtOptionsSection.Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && 
                (path.StartsWithSegments("/hubs/notifications") || path.StartsWithSegments("/hubs/chat")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("Auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRateLimiter();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TravelTourManagement.API.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<TravelTourManagement.API.Hubs.ChatHub>("/hubs/chat");

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

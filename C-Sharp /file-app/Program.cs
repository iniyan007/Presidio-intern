using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=files.db"));

// 2. Add services for Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Enable Swagger Middleware
    app.UseSwagger();
    // Enable the Swagger UI
    app.UseSwaggerUI();
}

app.UseAuthorization();

// 4. Map the controllers
app.MapControllers();

app.Run();
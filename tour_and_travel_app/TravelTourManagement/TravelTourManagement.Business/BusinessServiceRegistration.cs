using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelTourManagement.Business.Configuration;
using TravelTourManagement.Business.Providers;
using TravelTourManagement.Business.Services;
using TravelTourManagement.Business.Interface;

namespace TravelTourManagement.Business;

public static class BusinessServiceRegistration
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure JwtOptions
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Register Providers
        services.AddScoped<IJwtProvider, JwtProvider>();

        // Register Services
        services.AddMemoryCache();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPackagerService, PackagerService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IPlatformConfigService, PlatformConfigService>();
        services.AddScoped<IPaymentService, PaymentService>();

        // Register AutoMapper
        services.AddAutoMapper(config => config.AddMaps(typeof(BusinessServiceRegistration).Assembly));

        return services;
    }
}

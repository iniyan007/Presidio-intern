using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.Interface;
using TravelTourManagement.DataAccess.Repository;

namespace TravelTourManagement.DataAccess;

public static class DataAccessServiceRegistration
{
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure DbContext with PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Register Repositories
        services.AddScoped(typeof(IRepository<,>), typeof(GenericRepository<,>));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingTravelerRepository, BookingTravelerRepository>();
        services.AddScoped<IItineraryActivityRepository, ItineraryActivityRepository>();
        services.AddScoped<IItineraryDayMealRepository, ItineraryDayMealRepository>();
        services.AddScoped<IItineraryDayRepository, ItineraryDayRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IMessageThreadRepository, MessageThreadRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IPackageAccommodationRepository, PackageAccommodationRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IPackageTransportRepository, PackageTransportRepository>();
        services.AddScoped<IPackagerRepository, PackagerRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IReviewMediumRepository, ReviewMediumRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}

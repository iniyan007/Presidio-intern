using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.Interface;
using TravelTourManagement.DataAccess.Repository;

using TravelTourManagement.DataAccess.Enums;
using Npgsql;

namespace TravelTourManagement.DataAccess;

public static class DataAccessServiceRegistration
{
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
    {


        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DefaultConnection"));

        dataSourceBuilder.MapEnum<PackageType>("package_type");
        dataSourceBuilder.MapEnum<PackageStatus>("package_status");
        dataSourceBuilder.MapEnum<InclusionType>("inclusion_type");
        dataSourceBuilder.MapEnum<MediaCategory>("media_category");
        dataSourceBuilder.MapEnum<MealType>("meal_type");
        dataSourceBuilder.MapEnum<TransportMode>("transport_mode");
        dataSourceBuilder.MapEnum<DaySession>("day_session");
        dataSourceBuilder.MapEnum<BookingStatus>("booking_status");
        dataSourceBuilder.MapEnum<PaymentStatus>("payment_status");
        dataSourceBuilder.MapEnum<DocumentStatus>("document_status");
        dataSourceBuilder.MapEnum<NotificationType>("notification_type");
        
        dataSourceBuilder.EnableUnmappedTypes();

        var dataSource = dataSourceBuilder.Build();

        // Configure DbContext with PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(dataSource));

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

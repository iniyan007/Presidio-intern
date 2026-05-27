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

#pragma warning disable CS0618 // Type or member is obsolete
        NpgsqlConnection.GlobalTypeMapper.MapEnum<PackageType>("package_type");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<PackageStatus>("package_status");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<InclusionType>("inclusion_type");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<MediaCategory>("media_category");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<MealType>("meal_type");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<TransportMode>("transport_mode");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<DaySession>("day_session");
#pragma warning restore CS0618

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DefaultConnection"));

        dataSourceBuilder.MapEnum<PackageType>("package_type");
        dataSourceBuilder.MapEnum<PackageStatus>("package_status");
        dataSourceBuilder.MapEnum<InclusionType>("inclusion_type");
        dataSourceBuilder.MapEnum<MediaCategory>("media_category");
        dataSourceBuilder.MapEnum<MealType>("meal_type");
        dataSourceBuilder.MapEnum<TransportMode>("transport_mode");
        dataSourceBuilder.MapEnum<DaySession>("day_session");
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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Business.Services;

public class BookingTimeoutService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingTimeoutService> _logger;

    public BookingTimeoutService(IServiceProvider serviceProvider, ILogger<BookingTimeoutService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Booking Timeout Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTimeoutsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing booking timeouts.");
            }

            // Wait for 1 minute before running again
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessTimeoutsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var packageRepository = scope.ServiceProvider.GetRequiredService<IPackageRepository>();
        var pricingRepository = scope.ServiceProvider.GetRequiredService<IRepository<PackageSeasonalPricing, Guid>>();

        // Find all pending bookings older than 5 minutes
        var cutoffTime = DateTime.UtcNow.AddMinutes(-5);

        // Fetch expired pending bookings
        var expiredBookings = await bookingRepository.GetExpiredPendingBookingsAsync(cutoffTime, cancellationToken);

        if (!expiredBookings.Any())
            return;

        _logger.LogInformation($"Found {expiredBookings.Count} expired bookings to cancel.");

        foreach (var booking in expiredBookings)
        {
            // Update booking status
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = "Auto-cancelled due to payment timeout";
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            await bookingRepository.UpdateAsync(booking, cancellationToken);

            var totalTravelers = booking.AdultCount + booking.ChildCount + booking.InfantCount;

            // Revert Pricing Capacity
            var pricing = await pricingRepository.GetByIdAsync(booking.SeasonalPricingId, cancellationToken);
            if (pricing != null)
            {
                pricing.AvailableSlots += totalTravelers;
                await pricingRepository.UpdateAsync(pricing, cancellationToken);
            }

            // Revert Package Capacity
            var package = await packageRepository.GetByIdAsync(booking.PackageId, cancellationToken);
            if (package != null)
            {
                package.CurrentBookings -= totalTravelers;
                // prevent negative bookings if something was out of sync
                if (package.CurrentBookings < 0) package.CurrentBookings = 0; 
                await packageRepository.UpdateAsync(package, cancellationToken);
            }

            _logger.LogInformation($"Successfully auto-cancelled booking {booking.BookingReference}");
        }
    }
}

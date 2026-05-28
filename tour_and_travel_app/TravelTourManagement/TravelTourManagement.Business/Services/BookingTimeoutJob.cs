using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Business.Services;

public class BookingTimeoutJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingTimeoutJob> _logger;

    public BookingTimeoutJob(IServiceProvider serviceProvider, ILogger<BookingTimeoutJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var cancellationToken = context.CancellationToken;
            
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

            _logger.LogInformation($"[Quartz Job] Found {expiredBookings.Count} expired bookings to cancel.");

            foreach (var booking in expiredBookings)
            {
                booking.Status = BookingStatus.Cancelled;
                booking.CancellationReason = "Auto-cancelled due to payment timeout (via Cron Job)";
                booking.CancelledAt = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;

                await bookingRepository.UpdateAsync(booking, cancellationToken);

                var totalTravelers = booking.AdultCount + booking.ChildCount + booking.InfantCount;

                var pricing = await pricingRepository.GetByIdAsync(booking.SeasonalPricingId, cancellationToken);
                if (pricing != null)
                {
                    pricing.AvailableSlots += totalTravelers;
                    await pricingRepository.UpdateAsync(pricing, cancellationToken);
                }

                var package = await packageRepository.GetByIdAsync(booking.PackageId, cancellationToken);
                if (package != null)
                {
                    package.CurrentBookings -= totalTravelers;
                    if (package.CurrentBookings < 0) package.CurrentBookings = 0; 
                    await packageRepository.UpdateAsync(package, cancellationToken);
                }

                _logger.LogInformation($"[Quartz Job] Successfully auto-cancelled booking {booking.BookingReference}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Quartz Job] An error occurred while processing booking timeouts.");
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
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
            var dataMap = context.MergedJobDataMap;
            if (!dataMap.ContainsKey("BookingId"))
                return;

            if (!Guid.TryParse(dataMap.GetString("BookingId"), out Guid bookingId))
                return;

            using var scope = _serviceProvider.CreateScope();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var packageRepository = scope.ServiceProvider.GetRequiredService<IPackageRepository>();
            var pricingRepository = scope.ServiceProvider.GetRequiredService<IRepository<PackageSeasonalPricing, Guid>>();
            var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

            var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null) return;
            if (booking.Status != BookingStatus.Pending)
            {
                _logger.LogInformation($"[Quartz Job] Booking {booking.BookingReference} is no longer pending. Doing nothing.");
                return;
            }

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = "Auto-cancelled due to payment timeout (via Dynamic Quartz Job)";
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            await bookingRepository.UpdateAsync(booking, cancellationToken);

            var seatConsumingTravelers = booking.AdultCount + booking.ChildCount;

            var package = await packageRepository.GetByIdAsync(booking.PackageId, cancellationToken);
            bool isUnlimitedSlotsType = package != null && 
                                        (package.Type == TravelTourManagement.DataAccess.Enums.PackageType.Honeymoon ||
                                         package.Type == TravelTourManagement.DataAccess.Enums.PackageType.Family ||
                                         package.Type == TravelTourManagement.DataAccess.Enums.PackageType.Private);

            var pricing = await pricingRepository.GetByIdAsync(booking.SeasonalPricingId, cancellationToken);
            if (pricing != null && !isUnlimitedSlotsType)
            {
                pricing.AvailableSlots += seatConsumingTravelers;
                await pricingRepository.UpdateAsync(pricing, cancellationToken);
            }

            if (package != null && !isUnlimitedSlotsType)
            {
                package.CurrentBookings -= seatConsumingTravelers;
                if (package.CurrentBookings < 0) package.CurrentBookings = 0; 
                await packageRepository.UpdateAsync(package, cancellationToken);
                await cache.RemoveAsync($"Package_{package.Id}", cancellationToken);
            }

            _logger.LogInformation($"[Quartz Job] Successfully auto-cancelled booking {booking.BookingReference}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Quartz Job] An error occurred while processing booking timeouts.");
        }
    }
}

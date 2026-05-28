using System;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Bookings;

namespace TravelTourManagement.Business.Interface;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingResponse> VerifyBookingAsync(Guid packagerUserId, Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookingResponse>> GetBookingsByPackageIdAsync(Guid userId, string userRole, Guid packageId, CancellationToken cancellationToken = default);
}

using System;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Bookings;

namespace TravelTourManagement.Business.Interface;

public interface IPaymentService
{
    Task<BookingResponse> ProcessPaymentAsync(Guid userId, Guid bookingId, ProcessPaymentRequest request, CancellationToken cancellationToken = default);
}

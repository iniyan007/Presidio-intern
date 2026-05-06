using backend.DTOs;
using backend.Controllers;

namespace backend.Interfaces
{
    public interface IBookingService
    {
        Task<(bool Success, string Message, int? BookingId, DateTime? LockedUntil, List<string>? ConflictingSeats)> LockSeatsAsync(int userId, int tripId, List<string> seatNumbers);
        Task<(bool Success, string Message, int? BookingId)> ConfirmBookingAsync(int userId, int bookingId, List<PassengerDetail> passengers);
        Task<List<BookingHistoryDto>> GetMyBookingsAsync(int userId);
    }
}

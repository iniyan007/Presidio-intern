using BusBooking.Infrastructure.Data;
using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class PaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> ProcessPayment(int userId, int bookingId)
    {
        var booking = _context.Bookings
            .Include(b => b.BookingSeats)
            .FirstOrDefault(b => b.Id == bookingId && b.UserId == userId);

        if (booking == null)
            return "Booking not found";

        if (booking.Status != "PENDING")
            return "Invalid booking state";

        // ⏱ Timeout check (1 min)
        if ((DateTime.UtcNow - booking.CreatedAt).TotalMinutes > 1)
        {
            booking.Status = "CANCELLED";
            await _context.SaveChangesAsync();
            return "Payment time expired";
        }

        // ✅ Confirm booking
        booking.Status = "CONFIRMED";

        // 🔥 GET ALL SEATS
        var seatIds = booking.BookingSeats
            .Select(bs => bs.SeatId)
            .ToList();

        // 🔥 REMOVE ALL LOCKS
        var locks = _context.SeatLocks
            .Where(s => s.TripId == booking.TripId && seatIds.Contains(s.SeatId));

        _context.SeatLocks.RemoveRange(locks);

        await _context.SaveChangesAsync();

        return "Payment successful, booking confirmed";
    }
}
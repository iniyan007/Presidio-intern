using BusBooking.Infrastructure.Data;
using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class BookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> LockSeat(int userId, int tripId, int seatId)
    {
        var alreadyBooked = _context.Bookings
            .Any(b => b.SeatId == seatId && b.TripId == tripId && b.Status == "CONFIRMED");

        if (alreadyBooked)
            return "Seat already booked";

        var existingLock = _context.SeatLocks
            .FirstOrDefault(s => s.SeatId == seatId && s.TripId == tripId);

        if (existingLock != null &&
            (DateTime.UtcNow - existingLock.LockedAt).TotalMinutes < 5)
        {
            return "Seat already locked";
        }
        var lockSeat = new SeatLock
        {
            TripId = tripId,
            SeatId = seatId,
            UserId = userId,
            LockedAt = DateTime.UtcNow
        };

        _context.SeatLocks.Add(lockSeat);
        await _context.SaveChangesAsync();

        return "Seat locked for 5 minutes";
    }

    public async Task<string> ConfirmBooking(int userId, int tripId, int seatId)
    {
        var alreadyBooked = _context.Bookings
            .Any(b => b.SeatId == seatId && b.TripId == tripId);

        if (alreadyBooked)
            return "Seat already booked";

        var lockSeat = _context.SeatLocks
            .FirstOrDefault(s => s.SeatId == seatId && s.TripId == tripId && s.UserId == userId);

        if (lockSeat == null)
            return "Seat not locked";
        
        var trip = _context.Trips
            .Include(t => t.Bus)
            .FirstOrDefault(t => t.Id == tripId);

        if (trip == null)
            return "Trip not found";

        var basePrice = trip.Bus.Price;
        var platformFee = Math.Round(basePrice * 0.04m);
        var totalPrice = basePrice + platformFee;

        var booking = new Booking
        {
            UserId = userId,
            TripId = tripId,
            SeatId = seatId,

            BasePrice = basePrice,
            PlatformFee = platformFee,
            TotalPrice = totalPrice,

            Status = "CONFIRMED",
            CreatedAt = DateTime.UtcNow
        };
        _context.Bookings.Add(booking);
        _context.SeatLocks.Remove(lockSeat);

        await _context.SaveChangesAsync();

        return "Booking confirmed";
    }
    public async Task<string> CancelBooking(int userId, int bookingId)
    {
        var booking = _context.Bookings
            .Include(b => b.Trip)
            .FirstOrDefault(b => b.Id == bookingId && b.UserId == userId);

        if (booking == null)
            return "Booking not found";

        var hoursLeft = (booking.Trip.DepartureTime - DateTime.UtcNow).TotalHours;

        if (hoursLeft < 24)
            return "Cannot cancel within 24 hours of departure";

        booking.Status = "CANCELLED";

        await _context.SaveChangesAsync();

        return "Booking cancelled and refund initiated";
    }
    public List<object> GetUserBookings(int userId)
    {
        return _context.Bookings
            .Include(b => b.Trip)
                .ThenInclude(t => t.Route)
            .Include(b => b.Trip)
                .ThenInclude(t => t.Bus)
            .Where(b => b.UserId == userId)
            .Select(b => new
            {
                b.Id,

                BusName = b.Trip.Bus.Name,
                Source = b.Trip.Route.Source,
                Destination = b.Trip.Route.Destination,
                DepartureTime = b.Trip.DepartureTime,

                BasePrice = b.BasePrice,
                PlatformFee = b.PlatformFee,
                TotalPrice = b.TotalPrice,

                b.Status
            })
            .ToList<object>();
    }
    public decimal GetOperatorRevenue(int userId)
    {
        var op = _context.Operators.FirstOrDefault(o => o.UserId == userId);

        if (op == null) return 0;

        return _context.Bookings
            .Include(b => b.Trip)
            .ThenInclude(t => t.Bus)
            .Where(b => b.Trip.Bus.OperatorId == op.Id && b.Status == "CONFIRMED")
            .Sum(b => b.BasePrice);
    }
    public decimal GetTotalRevenue()
    {
        return _context.Bookings
            .Where(b => b.Status == "CONFIRMED")
            .Sum(b => b.PlatformFee);
    }
}
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

    public async Task<string> LockSeats(int userId, int tripId, List<int> seatIds)
    {
        if (seatIds == null || seatIds.Count == 0)
            return "No seats selected";

        if (seatIds.Count != seatIds.Distinct().Count())
            return "Duplicate seats selected";

        foreach (var seatId in seatIds)
        {
            // 🔥 Check already booked
            var alreadyBooked = _context.BookingSeats
                .Any(bs => bs.SeatId == seatId &&
                        bs.Booking.TripId == tripId &&
                        bs.Booking.Status == "CONFIRMED");

            if (alreadyBooked)
                return $"Seat {seatId} already booked";

            var existingLock = _context.SeatLocks
                .FirstOrDefault(s => s.SeatId == seatId && s.TripId == tripId);

            if (existingLock != null &&
                existingLock.UserId != userId &&  
                (DateTime.UtcNow - existingLock.LockedAt).TotalMinutes < 5)
            {
                return $"Seat {seatId} already locked";
            }
        }

        // 🔥 Lock all seats
        foreach (var seatId in seatIds)
        {
            _context.SeatLocks.Add(new SeatLock
            {
                TripId = tripId,
                SeatId = seatId,
                UserId = userId,
                LockedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        return "Seats locked successfully for 5 minutes";
    }

   public async Task<string> CreateBooking(int userId, int tripId, List<int> seatIds)
    {
        // 🔥 VALIDATION
        if (seatIds == null || seatIds.Count == 0)
            return "No seats selected";

        if (seatIds.Count != seatIds.Distinct().Count())
            return "Duplicate seats selected";

        var trip = _context.Trips
            .Include(t => t.Bus)
            .FirstOrDefault(t => t.Id == tripId);

        if (trip == null)
            return "Trip not found";

        // 🔥 CHECK ALL SEATS LOCKED
        foreach (var seatId in seatIds)
        {
            var alreadyBooked = _context.BookingSeats
                .Any(bs => bs.SeatId == seatId &&
                            bs.Booking.TripId == tripId &&
                            bs.Booking.Status == "CONFIRMED");

            if (alreadyBooked)
                return $"Seat {seatId} already booked";
            var lockSeat = _context.SeatLocks
                .FirstOrDefault(s =>
                    s.SeatId == seatId &&
                    s.TripId == tripId &&
                    s.UserId == userId);

            if (lockSeat == null)
                return $"Seat {seatId} not locked";
        }

        // 🔥 PRICING
        var basePrice = trip.Bus.Price;
        var fee = Math.Round(basePrice * 0.04m);

        var booking = new Booking
        {
            UserId = userId,
            TripId = tripId,
            BasePrice = basePrice * seatIds.Count,
            PlatformFee = fee * seatIds.Count,
            TotalPrice = (basePrice + fee) * seatIds.Count,
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow,
            BookingSeats = new List<BookingSeat>()
        };

        // 🔥 ADD MULTIPLE SEATS
        foreach (var seatId in seatIds)
        {
            booking.BookingSeats.Add(new BookingSeat
            {
                SeatId = seatId
            });
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return "Booking created. Proceed to payment";
    }
    public async Task<string> CancelBooking(int userId, int bookingId)
    {
        var booking = _context.Bookings
            .Include(b => b.Trip)
            .Include(b => b.BookingSeats)
            .FirstOrDefault(b => b.Id == bookingId && b.UserId == userId);

        if (booking == null)
            return "Booking not found";

        var hoursLeft = (booking.Trip.DepartureTime - DateTime.UtcNow).TotalHours;

        if (hoursLeft < 24)
            return "Cannot cancel within 24 hours of departure";

        // 🔥 CANCEL BOOKING
        booking.Status = "CANCELLED";

        // 🔥 GET ALL SEAT IDS
        var seatIds = booking.BookingSeats
            .Select(bs => bs.SeatId)
            .ToList();

        // 🔥 REMOVE LOCKS FOR ALL SEATS
        var locks = _context.SeatLocks
            .Where(s => s.TripId == booking.TripId && seatIds.Contains(s.SeatId));

        _context.SeatLocks.RemoveRange(locks);

        await _context.SaveChangesAsync();

        return "Booking cancelled and refund initiated";
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

    public List<object> GetOperatorBookings(int userId, int page, int pageSize, int? tripId = null)
    {
        var op = _context.Operators.FirstOrDefault(o => o.UserId == userId);

        if (op == null) return new List<object>();

        var query = _context.Bookings
            .Include(b => b.Trip)
                .ThenInclude(t => t.Bus)
            .Include(b => b.Trip)
                .ThenInclude(t => t.Route)
            .Include(b => b.User)
            .Include(b => b.BookingSeats)                 // 🔥 FIX
                .ThenInclude(bs => bs.Seat)               // 🔥 FIX
            .Where(b => b.Trip.Bus.OperatorId == op.Id && b.Status == "CONFIRMED");

        // 🔥 OPTIONAL FILTER
        if (tripId.HasValue)
        {
            query = query.Where(b => b.TripId == tripId.Value);
        }

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new
            {
                BookingId = b.Id,

                TripId = b.TripId,
                BusName = b.Trip.Bus.Name,

                Source = b.Trip.Route.Source,
                Destination = b.Trip.Route.Destination,
                DepartureTime = b.Trip.DepartureTime,

                UserName = b.User.Name,
                Phone = b.User.Phone,

                // 🔥 MULTI-SEAT FIX
                SeatNumbers = b.BookingSeats
                    .Select(bs => bs.Seat.SeatNumber)
                    .ToList(),

                TotalPrice = b.TotalPrice
            })
            .ToList<object>();
    }
    public List<object> GetUserBookings(int userId)
    {
        return _context.Bookings
            .Include(b => b.Trip)
                .ThenInclude(t => t.Route)
            .Include(b => b.Trip)
                .ThenInclude(t => t.Bus)
            .Include(b => b.BookingSeats)
                .ThenInclude(bs => bs.Seat)
            .Where(b => b.UserId == userId)
            .Select(b => new
            {
                BookingId = b.Id,

                Source = b.Trip.Route.Source,
                Destination = b.Trip.Route.Destination,
                DepartureTime = b.Trip.DepartureTime,

                BusName = b.Trip.Bus.Name,

                SeatNumbers = b.BookingSeats
                    .Select(bs => bs.Seat.SeatNumber)
                    .ToList(),

                BasePrice = b.BasePrice,
                PlatformFee = b.PlatformFee,
                TotalPrice = b.TotalPrice,

                Status = b.Status
            })
            .ToList<object>();
    }
}
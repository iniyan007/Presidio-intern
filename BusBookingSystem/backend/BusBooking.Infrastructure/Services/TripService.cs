using BusBooking.Domain.Entities;
using BusBooking.Infrastructure.Data;
using BusBooking.Application.DTOs;
using Microsoft.EntityFrameworkCore;

public class TripService
{
    private readonly ApplicationDbContext _context;

    private readonly EmailService _emailService;

    public TripService(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }
    public async Task<string> CreateTrip(int userId, CreateTripRequest request)
    {
        // Check operator
        var op = _context.Operators
            .FirstOrDefault(o => o.UserId == userId && o.IsApproved);

        if (op == null)
            return "Operator not approved";

        // Check bus belongs to operator & approved
        var bus = _context.Buses
            .FirstOrDefault(b => b.Id == request.BusId && b.OperatorId == op.Id && b.IsApproved);

        if (bus == null)
            return "Bus not found or not approved";

        var trip = new Trip
        {
            BusId = request.BusId,
            RouteId = request.RouteId,
            DepartureTime = DateTime.SpecifyKind(request.DepartureTime, DateTimeKind.Utc),
            ArrivalTime = DateTime.SpecifyKind(request.ArrivalTime, DateTimeKind.Utc)
        };

        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

        return "Trip created successfully";
    }
    public List<object> GetAllTrips()
    {
        var trips = _context.Trips
            .Include(t => t.Bus)
            .Include(t => t.Route)
            .Where(t => t.IsActive)
            .ToList();

        return trips.Select(t =>
        {
            var basePrice = t.Bus.Price;
            var fee = Math.Round(basePrice * 0.04m);

            return new
            {
                TripId = t.Id,
                BusName = t.Bus.Name,
                Source = t.Route.Source,
                Destination = t.Route.Destination,
                DepartureTime = t.DepartureTime,

                BasePrice = basePrice,
                PlatformFee = fee,
                TotalPrice = basePrice + fee
            };
        }).ToList<object>();
    }

   public List<object> Search(string source, string destination, DateTime? date)
    {
        var query = _context.Trips
            .Include(t => t.Bus)
            .Include(t => t.Route)
            .Where(t => t.IsActive &&
                        t.Route.Source == source &&
                        t.Route.Destination == destination);

        if (date.HasValue)
        {
            query = query.Where(t => t.DepartureTime.Date == date.Value.Date);
        }

        return query.ToList().Select(t =>
        {
            var basePrice = t.Bus.Price;
            var fee = Math.Round(basePrice * 0.04m);

            return new
            {
                TripId = t.Id,
                BusName = t.Bus.Name,
                DepartureTime = t.DepartureTime,

                BasePrice = basePrice,
                PlatformFee = fee,
                TotalPrice = basePrice + fee
            };
        }).ToList<object>();
    }
    public async Task<string> UpdateTrip(int userId, int tripId, CreateTripRequest request)
    {
        var op = _context.Operators.FirstOrDefault(o => o.UserId == userId && o.IsApproved);

        if (op == null)
            return "Operator not approved";

        var trip = _context.Trips
            .Include(t => t.Bus)
            .FirstOrDefault(t => t.Id == tripId);

        if (trip == null)
            return "Trip not found";

        if (trip.Bus.OperatorId != op.Id)
            return "Unauthorized";

        trip.RouteId = request.RouteId;
        trip.DepartureTime = DateTime.SpecifyKind(request.DepartureTime, DateTimeKind.Utc);
        trip.ArrivalTime = DateTime.SpecifyKind(request.ArrivalTime, DateTimeKind.Utc);

        await _context.SaveChangesAsync();

        return "Trip updated successfully";
    }
    public async Task<string> DeleteTrip(int userId, int tripId)
    {
        var op = _context.Operators.FirstOrDefault(o => o.UserId == userId && o.IsApproved);

        if (op == null)
            return "Operator not approved";

        var trip = _context.Trips
            .Include(t => t.Bus)
            .FirstOrDefault(t => t.Id == tripId);

        if (trip == null)
            return "Trip not found";

        if (trip.Bus.OperatorId != op.Id)
            return "Unauthorized";

        trip.IsActive = false;

        // 🔥 GET AFFECTED BOOKINGS
        var bookings = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Trip)
            .ThenInclude(t => t.Route)
            .Where(b =>
                b.TripId == tripId &&
                b.Status == "CONFIRMED" &&
                b.Trip.DepartureTime > DateTime.UtcNow)
            .ToList();

        foreach (var booking in bookings)
        {
            booking.Status = "CANCELLED";

            // 🔥 SEND EMAIL
            await _emailService.SendEmail(
                booking.User.Email,
                "Trip Cancelled - Refund Initiated",
                $"Dear {booking.User.Name},\n\n" +
                $"Your trip from {booking.Trip.Route.Source} to {booking.Trip.Route.Destination} has been cancelled.\n" +
                $"Your refund of ₹{booking.TotalPrice} will be processed shortly.\n\n" +
                $"Thank you,\nBusBooking Team"
            );
        }

        await _context.SaveChangesAsync();

        return "Trip cancelled, users notified, refunds initiated";
    }
    public List<object> GetSeatAvailability(int tripId)
    {
        var busId = _context.Trips
            .Where(t => t.Id == tripId)
            .Select(t => t.BusId)
            .FirstOrDefault();

        var seats = _context.Seats
            .Where(s => s.BusId == busId)
            .ToList();

        // 🔥 FIXED: use BookingSeats
        var bookedSeats = _context.BookingSeats
            .Where(bs => bs.Booking.TripId == tripId &&
                        bs.Booking.Status == "CONFIRMED")
            .Select(bs => bs.SeatId)
            .ToList();

        var lockedSeats = _context.SeatLocks
            .Where(s => s.TripId == tripId &&
                (DateTime.UtcNow - s.LockedAt).TotalMinutes < 5)
            .Select(s => s.SeatId)
            .ToList();

        return seats.Select(s => new
        {
            SeatNumber = s.SeatNumber,
            Status = bookedSeats.Contains(s.Id)
                ? "BOOKED"
                : lockedSeats.Contains(s.Id)
                    ? "LOCKED"
                    : "AVAILABLE"
        }).ToList<object>();
    }
}
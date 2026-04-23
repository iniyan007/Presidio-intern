using BusBooking.Domain.Entities;
using BusBooking.Infrastructure.Data;
using BusBooking.Application.DTOs;
using Microsoft.EntityFrameworkCore;

public class TripService
{
    private readonly ApplicationDbContext _context;

    public TripService(ApplicationDbContext context)
    {
        _context = context;
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

   public List<object> Search(string source, string destination)
    {
        var trips = _context.Trips
            .Include(t => t.Bus)
            .Include(t => t.Route)
            .Where(t => t.IsActive &&
                t.Route.Source == source &&
                t.Route.Destination == destination)
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

        trip.IsActive = false;   // 🔥 soft delete

        await _context.SaveChangesAsync();

        return "Trip deactivated";
    }
}
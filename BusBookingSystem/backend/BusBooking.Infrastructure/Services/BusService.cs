using BusBooking.Domain.Entities;
using BusBooking.Infrastructure.Data;
using BusBooking.Application.DTOs;
using Microsoft.EntityFrameworkCore;

public class BusService
{
    private readonly ApplicationDbContext _context;

    public BusService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Operator adds bus
    public async Task<string> AddBus(int userId, CreateBusRequest request)
    {
        var op = _context.Operators
            .FirstOrDefault(o => o.UserId == userId && o.IsApproved);

        if (op == null)
            return "Operator not approved";

        var bus = new Bus
        {
            Name = request.Name,
            TotalSeats = request.TotalSeats,
            Price = request.Price,
            OperatorId = op.Id,
            IsApproved = false
        };

        _context.Buses.Add(bus);
        await _context.SaveChangesAsync();

        return "Bus added, waiting for admin approval";
    }

    // Admin approves bus
    public async Task<string> ApproveBus(int busId)
    {
        var bus = _context.Buses.FirstOrDefault(b => b.Id == busId);
        if (bus == null) return "Bus not found";

        bus.IsApproved = true;
        await _context.SaveChangesAsync();

        return "Bus approved";
    }

    // Get buses
    public List<object> GetAll()
    {
        var buses = _context.Buses
            .Include(b => b.Operator)
            .ToList();

        return buses.Select(b =>
        {
            var bookedSeats = _context.Bookings
                .Where(x => x.Trip.BusId == b.Id && x.Status == "CONFIRMED")
                .Count();

            return new
            {
                b.Id,
                b.Name,
                b.TotalSeats,
                BookedSeats = bookedSeats,
                AvailableSeats = b.TotalSeats - bookedSeats,
                b.Price
            };
        }).ToList<object>();
    }
    public async Task<string> RejectBus(int busId)
    {
        var bus = _context.Buses.FirstOrDefault(b => b.Id == busId);

        if (bus == null)
            return "Bus not found";

        bus.IsApproved = false;
        bus.IsActive = false;

        await _context.SaveChangesAsync();

        return "Bus rejected";
    }
}
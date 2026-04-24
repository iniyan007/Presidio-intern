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
            BusNumber = request.BusNumber,
            TotalSeats = request.TotalSeats,
            Price = request.Price,
            OperatorId = op.Id,
            IsApproved = false
        };

        _context.Buses.Add(bus);
        await _context.SaveChangesAsync();

        // Generate seats
        var seats = new List<Seat>();
        for (int i = 1; i <= request.TotalSeats; i++)
        {
            seats.Add(new Seat
            {
                BusId = bus.Id,
                SeatNumber = $"{i}",
                IsWindow = (i % 4 == 1 || i % 4 == 0)
            });
        }
        _context.Seats.AddRange(seats);
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
                b.BusNumber,
                b.TotalSeats,
                BookedSeats = bookedSeats,
                AvailableSeats = b.TotalSeats - bookedSeats,
                b.Price,
                b.IsApproved,
                OperatorName = b.Operator.CompanyName
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

    public async Task<string> UpdateBus(int userId, int busId, CreateBusRequest request)
    {
        var op = _context.Operators.FirstOrDefault(o => o.UserId == userId && o.IsApproved);
        if (op == null) return "Operator not approved";

        var bus = _context.Buses.FirstOrDefault(b => b.Id == busId && b.OperatorId == op.Id);
        if (bus == null) return "Bus not found";

        bus.Name = request.Name;
        bus.BusNumber = request.BusNumber;
        bus.Price = request.Price;
        // Not changing TotalSeats as it would break existing seat layouts
        
        await _context.SaveChangesAsync();
        return "Bus updated successfully";
    }

    public async Task<string> DeleteBus(int userId, int busId)
    {
        var op = _context.Operators.FirstOrDefault(o => o.UserId == userId && o.IsApproved);
        if (op == null) return "Operator not approved";

        var bus = _context.Buses.FirstOrDefault(b => b.Id == busId && b.OperatorId == op.Id);
        if (bus == null) return "Bus not found";

        bus.IsActive = false;
        await _context.SaveChangesAsync();
        return "Bus deleted successfully";
    }
}
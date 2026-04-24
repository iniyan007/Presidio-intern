using BusBooking.Domain.Entities;
using BusBooking.Infrastructure.Data;
using BusBooking.Application.DTOs;
using Microsoft.EntityFrameworkCore;


public class OperatorService
{
    private readonly ApplicationDbContext _context;

    public OperatorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> RegisterOperator(int userId, CreateOperatorRequest request)
    {
        var existing = _context.Operators.FirstOrDefault(o => o.UserId == userId);
        if (existing != null)
            return "Already registered as operator";

        var op = new Operator
        {
            UserId = userId,
            CompanyName = request.CompanyName,
            ContactNumber = request.ContactNumber,
            OperatingLocation = request.OperatingLocation,
            IsApproved = false
        };

        _context.Operators.Add(op);
        await _context.SaveChangesAsync();

        return "Operator registration submitted for approval";
    }

    public async Task<string> ApproveOperator(int operatorId)
    {
        var op = _context.Operators.FirstOrDefault(o => o.Id == operatorId);
        if (op == null) return "Operator not found";

        op.IsApproved = true;
        await _context.SaveChangesAsync();

        return "Operator approved";
    }

    public List<Operator> GetAll()
    {
        return _context.Operators.ToList();
    }
    public async Task<string> RejectOperator(int operatorId)
    {
        var op = _context.Operators.FirstOrDefault(o => o.Id == operatorId);

        if (op == null)
            return "Operator not found";

        op.IsApproved = false;
        op.IsActive = false;

        await _context.SaveChangesAsync();

        return "Operator rejected";
    }

    public async Task<string> DisableOperator(int operatorId, TripService tripService)
    {
        var op = _context.Operators
            .Include(o => o.User)
            .FirstOrDefault(o => o.Id == operatorId);

        if (op == null) return "Operator not found";

        op.IsActive = false;

        // Find all active trips for this operator
        var trips = _context.Trips
            .Include(t => t.Bus)
            .Where(t => t.Bus.OperatorId == operatorId && t.IsActive)
            .ToList();

        foreach (var trip in trips)
        {
            await tripService.DeleteTrip(op.UserId, trip.Id); // This notifies users and refunds
        }

        await _context.SaveChangesAsync();
        return "Operator disabled and all future trips cancelled";
    }
}
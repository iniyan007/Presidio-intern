using BusBooking.Domain.Entities;
using BusBooking.Infrastructure.Data;
using BusBooking.Application.DTOs;


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
}
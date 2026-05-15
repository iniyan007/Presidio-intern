using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class FineRepository : IFineRepository
{
    private readonly LibraryDbContext _context;

    public FineRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<FinePayment>> GetByMemberIdAsync(int memberId)
    {
        return await _context.FinePayments
            .Include(f => f.Borrow)
                .ThenInclude(b => b.BookCopy)
                    .ThenInclude(bc => bc.Book)
            .Where(f => f.MemberId == memberId)
            .OrderByDescending(f => f.PaymentDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalUnpaidFineAsync(int memberId)
    {
        // Calculate manually — most reliable approach in EF Core 8
        var totalFine = await _context.Borrows
            .Where(b => b.MemberId == memberId)
            .SumAsync(b => b.FineAmount);

        var totalPaid = await _context.FinePayments
            .Where(f => f.MemberId == memberId)
            .SumAsync(f => f.AmountPaid);

        var unpaid = totalFine - totalPaid;

        // Return 0 if no fines — never return negative
        return unpaid < 0 ? 0 : unpaid;
    }

    public async Task<decimal> GetTotalPaidFineAsync(int memberId)
    {
        return await _context.FinePayments
            .Where(f => f.MemberId == memberId)
            .SumAsync(f => f.AmountPaid);
    }

    public async Task AddPaymentAsync(FinePayment payment)
    {
        await _context.FinePayments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    // ── Call PostgreSQL function directly via raw SQL ──────────
    public async Task<decimal> GetUnpaidFineViaFunctionAsync(int memberId)
    {
        var conn = _context.Database.GetDbConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT calculate_member_fine(@memberId)";

        var param = cmd.CreateParameter();
        param.ParameterName = "@memberId";
        param.Value         = memberId;
        cmd.Parameters.Add(param);

        var result = await cmd.ExecuteScalarAsync();

        return result is DBNull or null ? 0 : Convert.ToDecimal(result);
    }
}
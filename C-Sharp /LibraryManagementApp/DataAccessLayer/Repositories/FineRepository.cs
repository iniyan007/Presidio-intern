using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.Context;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer.Repositories;
public class FineRepository : IFineRepository
{
    private readonly AppDbContext _context;

    public FineRepository(AppDbContext context)
    {
        _context = context;
    }
    public List<FinePayment> GetByMemberId(int memberId)
    {
        return _context.FinePayments.Where(f => f.MemberId == memberId).ToList();
    }
    public decimal? GetTotalUnpaidFine(int memberId)
    {
        var result = _context.Database
            .SqlQueryRaw<decimal>("SELECT calculate_member_fine({0})", memberId)
            .FirstOrDefault();

        return result;
    }
    public void AddPayment(FinePayment payment)
    {
        _context.FinePayments.Add(payment);
        _context.SaveChanges();
    }
    public decimal GetTotalPaidFine(int memberId)
    {
        return _context.FinePayments
            .Where(f => f.MemberId == memberId)
            .Sum(f => f.AmountPaid);
    }
    
}
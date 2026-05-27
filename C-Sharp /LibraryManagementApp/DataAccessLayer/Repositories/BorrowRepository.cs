using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Repositories;
public class BorrowRepository : IBorrowRepository
{
    private readonly AppDbContext _context;

    public BorrowRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Borrow> GetAll()
    {
        return _context.Borrows.ToList();
    }

    public List<Borrow> GetBorrowsByMemberId(int memberid)
    {
        return _context.Borrows.Where(b => b.MemberId == memberid).ToList();
    }
    public List<Borrow> GetActiveBorrowsByMemberId(int memberId)
    {
        return _context.Borrows.Where(b => b.MemberId == memberId && b.Status == (int)BorrowStatus.Borrowed).ToList();
    }
    public List<Borrow> GetActiveBorrowByMemberAndBook(int memberId, int bookId)
    {
        return _context.Borrows.Where(b => b.MemberId == memberId && b.BookCopyId == bookId && b.Status == (int)BorrowStatus.Borrowed).ToList();
    }
    public void AddBorrow(Borrow borrow)
    {
        _context.Borrows.Add(borrow);
        _context.SaveChanges();
    }
    public List<Borrow> GetOverdueBorrows()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return _context.Borrows.Where(b => b.DueDate < today && b.Status == (int)BorrowStatus.Borrowed).ToList();
    }

    public void UpdateBorrow(Borrow borrow)
    {
        _context.Borrows.Update(borrow);
        _context.SaveChanges();
    }
}
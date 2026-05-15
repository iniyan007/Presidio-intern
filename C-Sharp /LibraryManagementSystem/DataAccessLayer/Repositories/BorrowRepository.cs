using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class BorrowRepository : IBorrowRepository
{
    private readonly LibraryDbContext _context;

    public BorrowRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<Borrow>> GetAllAsync()
    {
        return await _context.Borrows
            .Include(b => b.Member)
            .Include(b => b.BookCopy)
                .ThenInclude(bc => bc.Book)
            .ToListAsync();
    }

    public async Task<Borrow?> GetByIdAsync(int id)
    {
        return await _context.Borrows
            .Include(b => b.Member)
            .Include(b => b.BookCopy)
                .ThenInclude(bc => bc.Book)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Borrow>> GetByMemberIdAsync(int memberId)
    {
        return await _context.Borrows
            .Include(b => b.BookCopy)
                .ThenInclude(bc => bc.Book)
            .Where(b => b.MemberId == memberId)
            .ToListAsync();
    }

    public async Task<List<Borrow>> GetActiveBorrowsByMemberIdAsync(int memberId)
    {
        return await _context.Borrows
            .Include(b => b.BookCopy)
                .ThenInclude(bc => bc.Book)
            .Where(b => b.MemberId == memberId &&
                        b.Status == (int)BorrowStatus.Borrowed)
            .ToListAsync();
    }

    public async Task<Borrow?> GetActiveBorrowByMemberAndBookAsync(int memberId, int bookId)
    {
        return await _context.Borrows
            .Include(b => b.BookCopy)
            .FirstOrDefaultAsync(b => b.MemberId == memberId &&
                                      b.BookCopy.BookId == bookId &&
                                      b.Status == (int)BorrowStatus.Borrowed);
    }

    public async Task<List<Borrow>> GetOverdueBorrowsAsync()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        return await _context.Borrows
            .Include(b => b.Member)
            .Include(b => b.BookCopy)
                .ThenInclude(bc => bc.Book)
            .Where(b => b.Status == (int)BorrowStatus.Borrowed &&
                        b.DueDate < today)
            .ToListAsync();
    }

    public async Task AddAsync(Borrow borrow)
    {
        await _context.Borrows.AddAsync(borrow);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Borrow borrow)
    {
        _context.Borrows.Update(borrow);
        await _context.SaveChangesAsync();
    }
}
using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookCopies)
            .ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookCopies)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Book>> SearchAsync(string keyword)
    {
        string lower = keyword.ToLower();
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookCopies)
            .Where(b => b.Title.ToLower().Contains(lower)  ||
                        b.Author.ToLower().Contains(lower) ||
                        b.Category.Name.ToLower().Contains(lower))
            .ToListAsync();
    }

    public async Task<List<Book>> GetByCategoryAsync(int categoryId)
    {
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookCopies)
            .Where(b => b.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task AddAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Book book)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Books.AnyAsync(b => b.Id == id);
    }

    // BookCopy methods
    public async Task AddCopyAsync(BookCopy bookCopy)
    {
        await _context.BookCopies.AddAsync(bookCopy);
        await _context.SaveChangesAsync();
    }

    public async Task<BookCopy?> GetAvailableCopyAsync(int bookId)
    {
        return await _context.BookCopies
            .FirstOrDefaultAsync(bc => bc.BookId == bookId &&
                                       bc.Status == (int)CopyStatus.Available);
    }

    public async Task<List<BookCopy>> GetCopiesByBookIdAsync(int bookId)
    {
        return await _context.BookCopies
            .Where(bc => bc.BookId == bookId)
            .ToListAsync();
    }

    public async Task UpdateCopyAsync(BookCopy bookCopy)
    {
        _context.BookCopies.Update(bookCopy);
        await _context.SaveChangesAsync();
    }
}
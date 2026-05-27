using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using System.Collections.Generic;

namespace DataAccessLayer.Repositories;
public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;
    public BookRepository(AppDbContext context)
    {
        _context = context;
    }
    public List<Book> GetAllBooks()
    {
        return _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookCopies)
            .ToList();
    }
    public Book? GetBookById(int id)
    {
        return _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookCopies)
            .FirstOrDefault(b => b.Id == id);
    }
    public List<Book> SearchBooks(string keyword)
    {
        return _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookCopies)
            .Where(b => b.Title.Contains(keyword) ||
                        b.Author.Contains(keyword) ||
                        b.Category.Name.Contains(keyword))
            .ToList();
    }
    public List<Book> GetBooksByCategory(int categoryId)
    {
        return _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookCopies)
            .Where(b => b.CategoryId == categoryId)
            .ToList();
    }
    public void AddBook(Book book)
    {
        _context.Books.Add(book);
        _context.SaveChanges();
    }
    public void UpdateBook(Book book)
    {
        _context.Books.Update(book);
        _context.SaveChanges();
    }

    public void AddBookCopy(BookCopy bookCopy)
    {
        _context.BookCopies.Add(bookCopy);
        _context.SaveChanges();
    }
    public BookCopy? GetAvailableCopy(int bookId)
    {
        return _context.BookCopies
            .FirstOrDefault(bc => bc.BookId == bookId && bc.Status == (int)CopyStatus.Available);
    }
    public List<BookCopy> GetCopiesByBookId(int bookId)
    {
        return _context.BookCopies
            .Where(bc => bc.BookId == bookId)
            .ToList();
    }
    public void UpdateBookCopy(BookCopy bookCopy)
    {
        _context.BookCopies.Update(bookCopy);
        _context.SaveChanges();
    }
}
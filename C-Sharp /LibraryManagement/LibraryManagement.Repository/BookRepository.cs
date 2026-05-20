using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;

namespace LibraryManagement.Repository;
public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public List<Book> GetAllBooks()
    {
        return _context.Books.ToList();
    }

    public Book? GetBookById(int id)
    {
        return _context.Books.FirstOrDefault(b => b.BookId == id);
    }

    public void AddBook(Book book)
    {
        _context.Books.Add(book);
        _context.SaveChanges();
    }
}
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Repository;
using LibraryManagement.Service.Exceptions;

namespace LibraryManagement.Service;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public List<Book> GetAllBooks()
    {
        return _bookRepository.GetAllBooks();
    }

    public Book? GetBookById(int id)
    {
        return _bookRepository.GetBookById(id);
    }

    public Book? SearchBookByTitle(string title)
    {
        var books = _bookRepository.GetAllBooks();
        return books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
    }
    public void AddBook(Book book)
    {
        if (string.IsNullOrEmpty(book.Title))
        {
            throw new FieldEmptyException("Title cannot be empty.");
        }
        if(string.IsNullOrEmpty(book.Author))
        {
            throw new FieldEmptyException("Author name cannot be empty.");
        }
        if(book.AvailableCopies < 0)
        {
            throw new InsufficientCopyException("Number of copies cannot be negative.");
        }
        _bookRepository.AddBook(book);
    }
}
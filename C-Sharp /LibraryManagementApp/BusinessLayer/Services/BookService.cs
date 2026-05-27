using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;


namespace BusinessLayer.Services;
public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ICategoryRepository _categoryRepository;

    public BookService(IBookRepository bookRepository, ICategoryRepository categoryRepository)
    {
        _bookRepository = bookRepository;
        _categoryRepository = categoryRepository;
    }

    public List<BookDto> GetAllBooks()
    {
        return _bookRepository.GetAllBooks().Select(MapToDto).ToList();
    }

    public List<BookDto> GetBooksByCategory(int categoryId)
    {
        return _bookRepository.GetBooksByCategory(categoryId).Select(MapToDto).ToList();
    }

    public (bool Success, string Message) AddBook( string isbn, string title, string author, int categoryId)
    {
        var book = new Book
        {
            Isbn       = isbn,
            Title      = title,
            Author     = author,
            CategoryId = categoryId
        };
        if (_categoryRepository.GetCategoryById(book.CategoryId) == null)
            return (false, "Category not found.");
        _bookRepository.AddBook(book);
        return (true, $"Book '{title}' added successfully.");
    }
    public (bool Success, string Message) AddBookCopy(int bookId, string? remarks)
    {
        var book = _bookRepository.GetBookById(bookId);
        if (book == null)
            return (false, "Book not found.");

        var copy = new BookCopy
        {
            BookId  = bookId,
            Status  = (int)CopyStatus.Available,
            Remarks = remarks
        };

        _bookRepository.AddBookCopy(copy);
        return (true, "Book copy added successfully.");
    }
    public List<BookDto> SearchBooks(string keyword)
    {
        return _bookRepository.SearchBooks(keyword).Select(MapToDto).ToList();
    }
    public (bool Success, string Message) UpdateCopyStatus(int copyId, CopyStatus status, string? remarks)
    {
        var copy = _bookRepository.GetAvailableCopy(copyId);
        if (copy == null)
            return (false, "Book copy not found.");

        copy.Status  = (int)status;
        copy.Remarks = remarks;

        _bookRepository.UpdateBookCopy(copy);
        return (true, "Book copy status updated successfully.");
    }
    public List<CategoryDto> GetAllCategories()
    {
        return _categoryRepository.GetAllCategories().Select(MapCategoryToDto).ToList();
    }
    public (bool Success, string Message) AddCategory(string name)
    {
        var category = new Category { Name = name };
        _categoryRepository.AddCategory(category);
        return (true, $"Category '{name}' added successfully.");
    }
    private static BookDto MapToDto(Book b) => new()
    {
        Id              = b.Id,
        Isbn            = b.Isbn,
        Title           = b.Title,
        Author          = b.Author,
        CategoryName    = b.Category.Name,
        TotalCopies     = b.BookCopies.Count,
        AvailableCopies = b.BookCopies.Count(bc => bc.Status == (int)CopyStatus.Available)
    };
    private static CategoryDto MapCategoryToDto(Category c) => new()
    {
        Id   = c.Id,
        Name = c.Name
    };
}
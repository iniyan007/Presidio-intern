using BusinessLayer.DTOs;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Validators;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;

namespace BusinessLayer.Services;

public class BookService : IBookService
{
    private readonly IBookRepository     _bookRepo;
    private readonly ICategoryRepository _categoryRepo;

    public BookService(IBookRepository bookRepo, ICategoryRepository categoryRepo)
    {
        _bookRepo     = bookRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<List<BookDto>> GetAllBooksAsync()
    {
        var books = await _bookRepo.GetAllAsync();
        return books.Select(MapToDto).ToList();
    }

    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        var book = await _bookRepo.GetByIdAsync(id);
        return book is null ? null : MapToDto(book);
    }

    public async Task<List<BookDto>> SearchBooksAsync(string keyword)
    {
        var books = await _bookRepo.SearchAsync(keyword);
        return books.Select(MapToDto).ToList();
    }

    public async Task<List<BookDto>> GetBooksByCategoryAsync(int categoryId)
    {
        var books = await _bookRepo.GetByCategoryAsync(categoryId);
        return books.Select(MapToDto).ToList();
    }

    public async Task<(bool Success, string Message)> AddBookAsync(
        string isbn, string title, string author, int categoryId)
    {
        InputValidator.ValidateIsbn(isbn);
        InputValidator.ValidateTitle(title);
        InputValidator.ValidateAuthor(author);
        InputValidator.ValidateId(categoryId, "Category ID");

        if (!await _categoryRepo.ExistsAsync(categoryId))
            throw new LibraryException("Category not found. Please select a valid category.");

        var book = new Book
        {
            Isbn       = isbn.Trim(),
            Title      = title.Trim(),
            Author     = author.Trim(),
            CategoryId = categoryId
        };

        await _bookRepo.AddAsync(book);
        return (true, $"Book '{title}' added successfully.");
    }

    public async Task<(bool Success, string Message)> AddBookCopyAsync(int bookId, string? remarks)
    {
        InputValidator.ValidateId(bookId, "Book ID");

        if (!await _bookRepo.ExistsAsync(bookId))
            throw new BookNotFoundException(bookId);

        var copy = new BookCopy
        {
            BookId  = bookId,
            Status  = (int)CopyStatus.Available,
            Remarks = remarks?.Trim()
        };

        await _bookRepo.AddCopyAsync(copy);
        return (true, "Book copy added successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateCopyStatusAsync(
        int copyId, CopyStatus status, string? remarks)
    {
        InputValidator.ValidateId(copyId, "Copy ID");

        var copies = await _bookRepo.GetCopiesByBookIdAsync(copyId);
        var copy   = copies.FirstOrDefault(c => c.Id == copyId);

        if (copy is null)
            throw new LibraryException($"Book copy with ID {copyId} not found.");

        copy.Status  = (int)status;
        copy.Remarks = remarks?.Trim() ?? copy.Remarks;

        await _bookRepo.UpdateCopyAsync(copy);
        return (true, $"Book copy status updated to {status}.");
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return categories.Select(c => new CategoryDto
        {
            Id   = c.Id,
            Name = c.Name
        }).ToList();
    }

    public async Task<(bool Success, string Message)> AddCategoryAsync(string name)
    {
        InputValidator.ValidateCategoryName(name);
        var category = new Category { Name = name.Trim() };
        await _categoryRepo.AddAsync(category);
        return (true, $"Category '{name}' added successfully.");
    }

    // ── Mapper ────────────────────────────────────────────────
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
}
using BusinessLayer.DTOs;
using DataAccessLayer.Enums;

namespace BusinessLayer.Interfaces;

public interface IBookService
{
    Task<List<BookDto>> GetAllBooksAsync();
    Task<BookDto?> GetBookByIdAsync(int id);
    Task<List<BookDto>> SearchBooksAsync(string keyword);
    Task<List<BookDto>> GetBooksByCategoryAsync(int categoryId);
    Task<(bool Success, string Message)> AddBookAsync(string isbn, string title, string author, int categoryId);
    Task<(bool Success, string Message)> AddBookCopyAsync(int bookId, string? remarks);
    Task<(bool Success, string Message)> UpdateCopyStatusAsync(int copyId, CopyStatus status, string? remarks);
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<(bool Success, string Message)> AddCategoryAsync(string name);
}
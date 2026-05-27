using BusinessLayer.DTOs;
using DataAccessLayer.Enums;

namespace BusinessLayer.Interfaces;

public interface IBookService
{
    List<BookDto> GetAllBooks();
    List<BookDto> SearchBooks(string keyword);
    List<BookDto> GetBooksByCategory(int categoryId);
    (bool Success, string Message) AddBook(string isbn, string title, string author, int categoryId);
    (bool Success, string Message) AddBookCopy(int bookId, string? remarks);
    (bool Success, string Message) UpdateCopyStatus(int copyId, CopyStatus status, string? remarks);
    List<CategoryDto> GetAllCategories();
    (bool Success, string Message) AddCategory(string name);    
}
using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IBookRepository
{
    Task<List<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(int id);
    Task<List<Book>> SearchAsync(string keyword);
    Task<List<Book>> GetByCategoryAsync(int categoryId);
    Task AddAsync(Book book);
    Task UpdateAsync(Book book);
    Task<bool> ExistsAsync(int id);

    // BookCopy
    Task AddCopyAsync(BookCopy bookCopy);
    Task<BookCopy?> GetAvailableCopyAsync(int bookId);
    Task<List<BookCopy>> GetCopiesByBookIdAsync(int bookId);
    Task UpdateCopyAsync(BookCopy bookCopy);
}
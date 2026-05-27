using DataAccessLayer.Models;
namespace DataAccessLayer.Repositories.Interfaces;
public interface IBookRepository
{
    List<Book> GetAllBooks();
    Book? GetBookById(int id);
    List<Book> SearchBooks(string keyword);
    List<Book> GetBooksByCategory(int categoryId);
    void AddBook(Book book);
    void UpdateBook(Book book);
    void AddBookCopy(BookCopy bookCopy);
    BookCopy? GetAvailableCopy(int bookId);

    List<BookCopy> GetCopiesByBookId(int bookId);
    void UpdateBookCopy(BookCopy bookCopy);
}
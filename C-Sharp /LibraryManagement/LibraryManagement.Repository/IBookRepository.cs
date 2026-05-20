using LibraryManagement.Models;

namespace LibraryManagement.Repository;

public interface IBookRepository
{
    public List<Book> GetAllBooks();
    public Book? GetBookById(int id);
    public void AddBook(Book book);
}

using LibraryManagement.Repository;
using LibraryManagement.Models;

namespace LibraryManagement.Service;
public interface IBookService
{
    public List<Book> GetAllBooks();
    public Book? GetBookById(int id);
    public void AddBook(Book book);
    public Book? SearchBookByTitle(string title);
}
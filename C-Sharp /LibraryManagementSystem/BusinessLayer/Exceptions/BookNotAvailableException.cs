namespace BusinessLayer.Exceptions;

public class BookNotAvailableException : LibraryException
{
    public BookNotAvailableException(int bookId) 
        : base($"No available copies found for Book ID {bookId}.") { }
}
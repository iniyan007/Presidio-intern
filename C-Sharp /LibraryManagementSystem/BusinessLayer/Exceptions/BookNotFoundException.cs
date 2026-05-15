namespace BusinessLayer.Exceptions;

public class BookNotFoundException : LibraryException
{
    public BookNotFoundException(int id) 
        : base($"Book with ID {id} was not found.") { }
}
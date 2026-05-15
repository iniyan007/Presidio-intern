namespace BusinessLayer.Exceptions;

public class AlreadyReturnedException : LibraryException
{
    public AlreadyReturnedException(int borrowId) 
        : base($"Borrow record ID {borrowId} has already been returned.") { }
}
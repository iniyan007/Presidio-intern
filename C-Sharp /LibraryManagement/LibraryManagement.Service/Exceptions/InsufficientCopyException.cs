namespace LibraryManagement.Service.Exceptions;

public class InsufficientCopyException : Exception
{
    public InsufficientCopyException(string message) : base(message)
    {
    }
}
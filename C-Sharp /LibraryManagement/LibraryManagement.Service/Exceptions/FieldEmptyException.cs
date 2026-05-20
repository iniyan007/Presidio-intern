namespace LibraryManagement.Service.Exceptions;
public class FieldEmptyException : Exception
{
    public FieldEmptyException(string message) : base(message)
    {
    }
}
namespace BusinessLayer.Exceptions;

public class InvalidInputException : LibraryException
{
    public InvalidInputException(string field, string reason) 
        : base($"Invalid input for '{field}': {reason}") { }
}
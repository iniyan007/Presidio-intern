namespace BusinessLayer.Exceptions;

public class InactiveMemberException : LibraryException
{
    public InactiveMemberException(string name) 
        : base($"Member '{name}' is inactive. Please activate the membership before borrowing.") { }
}
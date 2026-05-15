namespace BusinessLayer.Exceptions;

public class MemberNotFoundException : LibraryException
{
    public MemberNotFoundException(int id) 
        : base($"Member with ID {id} was not found.") { }
    
    public MemberNotFoundException(string detail) 
        : base($"Member not found: {detail}") { }
}
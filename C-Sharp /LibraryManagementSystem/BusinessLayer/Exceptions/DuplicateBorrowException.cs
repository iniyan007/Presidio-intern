namespace BusinessLayer.Exceptions;

public class DuplicateBorrowException : LibraryException
{
    public DuplicateBorrowException() 
        : base("You already have an active borrow for this book. Return it before borrowing again.") { }
}
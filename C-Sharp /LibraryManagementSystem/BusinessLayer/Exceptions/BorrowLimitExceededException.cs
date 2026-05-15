namespace BusinessLayer.Exceptions;

public class BorrowLimitExceededException : LibraryException
{
    public BorrowLimitExceededException(int limit) 
        : base($"Borrow limit reached. Maximum {limit} books allowed for your membership.") { }
}
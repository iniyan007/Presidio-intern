namespace BusinessLayer.Exceptions;

public class UnpaidFineException : LibraryException
{
    public UnpaidFineException(decimal fine) 
        : base($"Unpaid fine of ₹{fine} exceeds ₹500 limit. Please clear dues before borrowing.") { }
}
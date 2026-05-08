namespace BusinessLayer.Exceptions;
public class InvalidMessageException: Exception
{
    string _message;
    public InvalidMessageException(string message)
    {
        _message = message;
    }
    public override string Message=> _message;
}
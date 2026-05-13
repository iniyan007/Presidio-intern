namespace Models
{
    public class Email: Notification
    {
        public Email(){}
        public Email(User user , string message)
        {
            ToAddress = user.Email;
            Message = message;
            UserId = user.Id;
        }
    }
}
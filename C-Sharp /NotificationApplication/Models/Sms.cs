namespace Models
{
    public class Sms : Notification
    {
        public Sms(){}
        public Sms(User user, string message)
        {
            ToAddress = user.Phone;
            Message = message;
            UserId = user.Id;
        }
    }
}

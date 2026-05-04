using NotificationApp.Interfaces;


namespace NotificationApp.Models
{
    internal class SMSNotification : INotification
    {
        public void SendMessage(User user, string message)
        {
            Console.WriteLine($"SMS sent to {user.Phone} with message: {message} on {DateTime.Now} For the user {user.Name}.");
        }
    }

}
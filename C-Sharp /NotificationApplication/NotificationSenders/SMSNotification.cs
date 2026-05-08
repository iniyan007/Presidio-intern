using Interfaces;
using Models;

namespace NotificationSenders
{
    public class SMSNotification : INotification
    {
        public void SendMessage(Notification notification)
        {
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"SMS sent to {notification.ToAddress} with message: {notification.Message} on {notification.Time}.");
            Console.WriteLine("--------------------------------------------------");
        }
    }

}
using System.Reflection.Metadata;
using System.Transactions;
using Interfaces;
using Models;

namespace NotificationSenders
{
    public class EmailNotification : INotification
    {
        public void SendMessage(Notification notification)
        {
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Email sent to {notification.ToAddress} with message: {notification.Message} on {notification.Time}.");
            Console.WriteLine("--------------------------------------------------");
        }
    }
}
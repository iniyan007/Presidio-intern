using System.Reflection.Metadata;
using System.Transactions;
using NotificationApp.Interfaces;

namespace NotificationApp.Models
{
    internal class EmailNotification : INotification
    {
        public void SendMessage(User user, string message)
        {
            Console.WriteLine($"Email sent to {user.Email} with message: {message} on {DateTime.Now} For the user {user.Name}.");
        }
    }
}
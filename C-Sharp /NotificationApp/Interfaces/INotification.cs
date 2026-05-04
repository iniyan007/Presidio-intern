using System.Reflection.Metadata;
using NotificationApp.Models;

namespace NotificationApp.Interfaces
{
    internal interface INotification
    {
        public void SendMessage(User user, string message);
    }
}
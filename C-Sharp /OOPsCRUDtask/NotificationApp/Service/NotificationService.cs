using System.Reflection.Metadata;
using NotificationApp.Interfaces;
using NotificationApp.Models;

namespace NotificationApp.Service
{
    internal class NotificationService
    {
        public void SendEmailNotification(User user, string message)
        {
            EmailNotification emailNotification = new EmailNotification();
            emailNotification.SendMessage(user, message);
        }
        public void SendSMSNotification(User user, string message)
        {
            SMSNotification smsNotification = new SMSNotification();
            smsNotification.SendMessage(user, message);
        }
    }
}
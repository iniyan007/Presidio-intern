using System.Reflection.Metadata;
using NotificationApp.Interfaces;
using NotificationApp.Models;

namespace NotificationApp.Service
{
    internal class NotificationService
    {
        List<User> users = new List<User>();
        public User CreateUser(string name, string email, string phoneNumber)
        {
            User user = new User(name, email, phoneNumber);
            users.Add(user);
            return user;
        }
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
        public User GetUserByName(string name)
        {
            User user = null;
            foreach (var item in users)
            {
                if (item.Name == name)
                {
                    user = item;
                    break;
                }
            }
            return user;
        }

    }
}
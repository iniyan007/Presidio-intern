using System.Reflection.Metadata;
using NotificationApp.Interfaces;
using NotificationApp.Models;

namespace NotificationApp.Service
{
    internal class NotificationService
    {
        private readonly UserService userService;
        public NotificationService(UserService userService)
        {
            this.userService = userService;
        }
        public void SendEmailNotificationService()
        {
            EmailNotification emailNotification = new EmailNotification();
            System.Console.WriteLine("Enter user name to send email notification");
            string userNameForEmail = Console.ReadLine() ?? "";
            User user = userService.GetUserByName(userNameForEmail);
            if(user == null)
            {
                System.Console.WriteLine("User not found");
                return;
            }
            System.Console.WriteLine("Enter message to send");
            string messageForEmail = Console.ReadLine() ?? "";
            emailNotification.SendMessage(user, messageForEmail);
        }
        public void SendSMSNotificationService()
        {
            SMSNotification smsNotification = new SMSNotification();
            System.Console.WriteLine("Enter user name to send sms notification");
            string userNameForSMS = Console.ReadLine() ?? "";
            User user = userService.GetUserByName(userNameForSMS);
            if(user == null)
            {
                System.Console.WriteLine("User not found");
                return;
            }
            System.Console.WriteLine("Enter message to send");
            string messageForSMS = Console.ReadLine() ?? "";
            smsNotification.SendMessage(user, messageForSMS);
        }

    }
}
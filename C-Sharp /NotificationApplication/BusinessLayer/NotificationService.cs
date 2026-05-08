using System.Reflection.Metadata;
using DataAccessLayer;
using BusinessLayer.Exceptions;
using Models;
using Interfaces;
using NotificationSenders;
using System.Net.WebSockets;

namespace BusinessLayer
{
    public class NotificationService
    {
        private NotificationRepository notificationRepository;
        private readonly UserService userService;
        public NotificationService(UserService userService)
        {
            this.userService = userService;
            this.notificationRepository = new NotificationRepository();
        }
        public bool CheckValidMessage(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new InvalidMessageException("Message cannot be null or empty");
                }
                if(message.Length > 160)
                {
                    throw new InvalidMessageException("Message cannot be more than 160 characters");
                }
                if (message.Length < 5)
                {
                    throw new InvalidMessageException("Message cannot be less than 5 characters");
                }
            }
            catch (InvalidMessageException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(ex.Message);
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("An error occurred while validating the message: " + ex.Message);
                Console.ResetColor();
                return false;
            }
            return true;
        }
        public void SendEmailNotificationService()
        {
            EmailNotification emailNotification = new EmailNotification();
            System.Console.WriteLine("Enter user name to send email notification");
            string userNameForEmail = Console.ReadLine() ?? "";
            User user = userService.GetUserByNameService(userNameForEmail);
            if(user == null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("User not found");
                Console.ResetColor();
                return;
            }
            System.Console.WriteLine("Enter message to send");
            string messageForEmail = Console.ReadLine() ?? "";
            while(!CheckValidMessage(messageForEmail))
            {
                messageForEmail = Console.ReadLine() ?? "";
            }
            Notification notification = notificationRepository.SaveNotification("Email", user.Email, messageForEmail);
            emailNotification.SendMessage(notification);
        }
        public void SendSMSNotificationService()
        {
            SMSNotification smsNotification = new SMSNotification();
            System.Console.WriteLine("Enter user name to send sms notification");
            string userNameForSMS = Console.ReadLine() ?? "";
            User user = userService.GetUserByNameService(userNameForSMS);
            if(user == null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("User not found");
                Console.ResetColor();
                return;
            }
            System.Console.WriteLine("Enter message to send");
            string messageForSMS = Console.ReadLine() ?? "";
            while(!CheckValidMessage(messageForSMS))
            {
                messageForSMS = Console.ReadLine() ?? "";
            }
            Notification notification = notificationRepository.SaveNotification("SMS", user.Phone, messageForSMS);
            smsNotification.SendMessage(notification);
        }
        public void GetNotificationByUserService()
        {
            System.Console.WriteLine("Enter user name to get notification history");
            string userNameForHistory = Console.ReadLine() ?? "";
            User user = userService.GetUserByNameService(userNameForHistory);
            if(user == null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("User not found");
                Console.ResetColor();
                return;
            }
            List<Notification> notifications = notificationRepository.GetNotificationsByUser(user.Email, user.Phone);
            if(notifications.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("No notifications found for the user");
                Console.ResetColor();
                return;
            }
            System.Console.WriteLine("Notification history for the user " + user.Name);
            foreach (var notification in notifications)
            {
                Console.WriteLine("--------------------------------------------------");
                System.Console.WriteLine(notification);
                Console.WriteLine("--------------------------------------------------");
            }
        }
        public void GetAllNotificationsService()
        {
            List<Notification> notifications = notificationRepository.GetAllNotifications();
            if(notifications.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("No notifications found");
                Console.ResetColor();
                return;
            }
            System.Console.WriteLine("All notifications:");
            foreach (var notification in notifications)
            {
                Console.WriteLine("--------------------------------------------------");
                System.Console.WriteLine(notification);
                Console.WriteLine("--------------------------------------------------");

            }
        }

    }
}
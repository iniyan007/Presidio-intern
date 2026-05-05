using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using NotificationApp.Interfaces;
using NotificationApp.Models;
using NotificationApp.Service;
using NotificationApp.Repositories;

namespace NotificationApp
{
    internal class Program
    {
        NotificationService notificationService;
        UserService userService;
        public Program()
        {
            notificationService = new NotificationService();
            userService = new UserService();
        }
        void DisplayChoises()
        {
            System.Console.WriteLine("Please enter your choice");
            System.Console.WriteLine("1. create user");
            System.Console.WriteLine("2. Get user by name");
            System.Console.WriteLine("3. Get all users");
            System.Console.WriteLine("4. Update user");
            System.Console.WriteLine("5. Delete user");
            System.Console.WriteLine("6. Send Email Notification to existing user");
            System.Console.WriteLine("7. Send SMS Notification to existing user");
            System.Console.WriteLine("8. Exit");
        }
      
        void SendEmail()
        {
            INotification emailNotification = new EmailNotification();
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
        void SendSMS()
        {
            INotification smsNotification = new SMSNotification();
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
        static void Main(string[] args)
        {
            int input;
            System.Console.WriteLine("Welcome to user management and Notification app");
            Program program = new Program();
            while (true)
            {
                program.DisplayChoises();
                if (int.TryParse(Console.ReadLine(), out input))
                {
                    switch (input)
                    {
                        case 1:
                            program.userService.CreateUserService();
                            break;
                        case 2:
                            program.userService.GetUserByNameService();
                            break;
                        case 3:
                            program.userService.GetAllUsersService();
                            break;
                        case 4:
                            program.userService.UpdateUserService();
                            break;
                        case 5:
                            program.userService.DeleteUserService();
                            break;
                        case 6:
                            program.SendEmail();
                            break;
                        case 7:
                            program.SendSMS();
                            break;
                        case 8:
                            System.Console.WriteLine("Exiting the application. Goodbye!");
                            Environment.Exit(0);
                            break;
                        default:
                            System.Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
            }

        }
    }
}
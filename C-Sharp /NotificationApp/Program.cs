using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using NotificationApp.Interfaces;
using NotificationApp.Models;
using NotificationApp.Service;

namespace NotificationApp
{
    internal class Program
    {
        NotificationService notificationService;
        public Program()
        {
            notificationService = new NotificationService();
        }
        void DisplayChoises()
        {
            System.Console.WriteLine("Please enter your choice");
            System.Console.WriteLine("1. create user");
            System.Console.WriteLine("2. Send Email Notification to existing user");
            System.Console.WriteLine("3. Send SMS Notification to existing user");
            System.Console.WriteLine("4. Exit");
        }
        void Create()
        {
            System.Console.WriteLine("Enter user name");
            string name = Console.ReadLine() ?? "";
            System.Console.WriteLine("Enter user email");
            string email = Console.ReadLine() ?? "";
            System.Console.WriteLine("Enter user phone number");
            string phoneNumber = Console.ReadLine() ?? "";
            User user =  notificationService.CreateUser(name, email, phoneNumber);
            System.Console.WriteLine("User created successfully " + user);
        }
        void SendEmail()
        {
            INotification emailNotification = new EmailNotification();
            System.Console.WriteLine("Enter user name to send email notification");
            string userNameForEmail = Console.ReadLine() ?? "";
            User user = notificationService.GetUserByName(userNameForEmail);
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
            User user = notificationService.GetUserByName(userNameForSMS);
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
                            program.Create();
                            break;
                        case 2:
                            program.SendEmail();
                            break;
                        case 3:
                            program.SendSMS();
                            break;
                        case 4:
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
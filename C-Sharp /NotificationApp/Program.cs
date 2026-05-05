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
        UserService userService;
        public Program()
        {
            userService = new UserService();
            notificationService = new NotificationService(userService);
        }
        void DisplayChoises()
        {
            System.Console.WriteLine("Please enter your choice");
            System.Console.WriteLine("1. create user");
            System.Console.WriteLine("2. Send Email Notification to existing user");
            System.Console.WriteLine("3. Send SMS Notification to existing user");
            System.Console.WriteLine("4. Exit");
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
                            program.userService.CreateUser();
                            break;
                        case 2:
                            program.notificationService.SendEmailNotificationService();
                            break;
                        case 3:
                            program.notificationService.SendSMSNotificationService();
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
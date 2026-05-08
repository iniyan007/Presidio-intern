using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using BusinessLayer;

namespace Presentation
{
    public class Program
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
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--------------------------------------------------");
            System.Console.WriteLine("Please enter your choice");
            System.Console.WriteLine("1. create user");
            System.Console.WriteLine("2. Send Email Notification to existing user");
            System.Console.WriteLine("3. Send SMS Notification to existing user");
            System.Console.WriteLine("4. Get all notifications");
            System.Console.WriteLine("5. Get notifications by user");
            System.Console.WriteLine("6. Update user");
            System.Console.WriteLine("7. Delete user");
            System.Console.WriteLine("8. Exit");
            Console.WriteLine("--------------------------------------------------");
            Console.ResetColor();
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
                            program.notificationService.SendEmailNotificationService();
                            break;
                        case 3:
                            program.notificationService.SendSMSNotificationService();
                            break;
                        case 4:
                            program.notificationService.GetAllNotificationsService();
                            break;
                        case 5:
                            program.notificationService.GetNotificationByUserService();
                            break;
                        case 6:
                            program.userService.UpdateUserService();
                            break;
                        case 7:
                            program.userService.DeleteUserByPhoneService();
                            break;
                        case 8:
                            System.Console.WriteLine("Exiting the application. Goodbye!");
                            Environment.Exit(0);
                            break;
                        default:
                            System.Console.WriteLine("Invalid choice. Please enter a valid choice.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number corresponding to the choices.");
                }
            }

        }
    }
}
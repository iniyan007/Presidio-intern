using System.Net.WebSockets;
using UnderstandingOOPSApp.Interfaces;
using UnderstandingOOPSApp.Services;

namespace UnderstandingOOPSApp
{
    internal class Program
    {
        ICustomerInteract customerInteract;
        public Program()
        {
            customerInteract = new CustomerService();
        }

        void DisplayMenu()
        {
            Console.WriteLine("\n====== Banking Menu ======");
            Console.WriteLine("1. Add Account");
            Console.WriteLine("2. Print Account Details (by Account Number)");
            Console.WriteLine("3. Print Account Details (by Phone Number)");
            Console.WriteLine("4. Exit");
            Console.WriteLine("===========================");
            Console.WriteLine("Please enter your choice: ");
        }

        void HandleMenuChoice(int choice)
        {
            switch (choice)
            {
                case 1:
                    AddAccount();
                    break;
                case 2:
                    PrintDetailsByAccountNumber();
                    break;
                case 3:
                    PrintDetailsByPhoneNumber();
                    break;
                case 4:
                    Console.WriteLine("Thank you for using Banking App. Goodbye!");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }

        void AddAccount()
        {
            Console.WriteLine("\n--- Opening New Account ---");
            var account = customerInteract.OpensAccount();
            Console.WriteLine("\nAccount created successfully!");
            Console.WriteLine(account);
        }

        void PrintDetailsByAccountNumber()
        {
            Console.WriteLine("\nPlease enter the account number: ");
            string accNum = Console.ReadLine() ?? "";
            customerInteract.PrintAccountDetails(accNum);
        }

        void PrintDetailsByPhoneNumber()
        {
            Console.WriteLine("\nPlease enter the phone number: ");
            string phone = Console.ReadLine() ?? "";
            customerInteract.PrintAccountDetailsByPhone(phone);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Banking App");
            Program program = new Program();
            int choice;

            while (true)
            {
                program.DisplayMenu();
                if (int.TryParse(Console.ReadLine(), out choice))
                {
                    program.HandleMenuChoice(choice);
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            }
        }
    }
}
using Models;
using DataAccessLayer;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using Interfaces;
namespace BusinessLayer
{
    public class UserService
    {
        Regex emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        Regex phoneRegex = new Regex(@"^\d{10}$");
        IUserRepository userRepository;
        public UserService()
        {
            this.userRepository = new UserRepository();
        }
        bool CheckUserExists(string Phone)
        {
            User? user = this.userRepository.GetUserByPhone(Phone);
            if (!(user == null))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("User Already Exists with the same phone number");
                Console.ResetColor();
                return false;
            }
            return true;
        }
        public User? CreateUserService()
        {
            System.Console.WriteLine("Enter user name");
            string name = Console.ReadLine() ?? "";
            System.Console.WriteLine("Enter user email");
            string email = Console.ReadLine() ?? "";
            while (!emailRegex.IsMatch(email))
            {
                System.Console.WriteLine("Invalid email format. Please enter a valid email");
                email = Console.ReadLine() ?? "";
            }
            System.Console.WriteLine("Enter user phone number");
            string phoneNumber = Console.ReadLine() ?? "";
            while (!phoneRegex.IsMatch(phoneNumber))
            {
                System.Console.WriteLine("Invalid phone number format. Please enter a valid phone number");
                phoneNumber = Console.ReadLine() ?? "";
            }
            if(!this.CheckUserExists(phoneNumber))
            {
                return null;
            }
            User user = userRepository.CreateUser(name, email, phoneNumber);
            Console.WriteLine("--------------------------------------------------");
            System.Console.WriteLine("User created successfully " + user);
            Console.WriteLine("--------------------------------------------------");
            return user;
        }
        public User? GetUserByNameService(string name)
        {
            User? user = userRepository.GetUserByName(name);
            if (user == null)
            {
                return null;
            }
            return user;
        }
        public void DeleteUserByPhoneService()
        {
            Console.WriteLine("Enter user phone number to delete the user");
            string phone = Console.ReadLine() ?? "";
            User? user = userRepository.GetUserByPhone(phone);
            if (user == null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("User not found");
                Console.ResetColor();
                return;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Are you sure want to delete User\n"+ user + "\nPress 1 to delete any other number to cancel");
                Console.ResetColor();
                int input;
                while(!int.TryParse(Console.ReadLine(), out input))
                {
                    Console.WriteLine("Invalid input please enter 1 or 2");
                }
                if(input != 1)
                {
                    Console.WriteLine("User deletion cancelled");
                    return;
                }
                else
                {
                    userRepository.DeleteUser(user);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("--------------------------------------------------");
                    System.Console.WriteLine("User deleted successfully ");
                    Console.WriteLine("--------------------------------------------------");
                    Console.ResetColor();
                }
            }
                        
        }
        public void UpdateUserService()
        {
            Console.WriteLine("Enter Phone number to update the user");
            string phoneNumber = Console.ReadLine() ?? "";
            User? user = userRepository.GetUserByPhone(phoneNumber);
            if (user == null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("User not found");
                Console.ResetColor();
                return;
            }
            Console.WriteLine("Enter new name");
            string name = Console.ReadLine() ?? "";
            System.Console.WriteLine("Enter new email");
            string email = Console.ReadLine() ?? "";
            while (!emailRegex.IsMatch(email))
            {
                System.Console.WriteLine("Invalid email format. Please enter a valid email");
                email = Console.ReadLine() ?? "";
            }
            System.Console.WriteLine("Enter new phone number");
            string newphoneNumber = Console.ReadLine() ?? "";
            while (!phoneRegex.IsMatch(newphoneNumber))
            {
                System.Console.WriteLine("Invalid phone number format. Please enter a valid phone number");
                newphoneNumber = Console.ReadLine() ?? "";
            }
            User newuser = userRepository.UpdateUser(user, name, email, newphoneNumber);
            Console.WriteLine("--------------------------------------------------");
            System.Console.WriteLine("User updated successfully " + newuser);
            Console.WriteLine("--------------------------------------------------");
            return;
        }
    }
}
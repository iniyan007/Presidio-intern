using System.IO.Compression;
using NotificationApp.Interfaces;
using NotificationApp.Models;
using NotificationApp.Repositories;

namespace NotificationApp.Service
{
    internal class UserService : UserRepository
    {

        public void CreateUserService()
        {
            System.Console.WriteLine("Enter user name");
            string name = Console.ReadLine() ?? "";
            System.Console.WriteLine("Enter user email");
            string email = Console.ReadLine() ?? "";
            System.Console.WriteLine("Enter user phone number");
            string phoneNumber = Console.ReadLine() ?? "";
            User user1 = new User(name, email, phoneNumber);
            User user2 =  AddUser(user1);
            System.Console.WriteLine("User created successfully \n" + user2);
            System.Console.WriteLine("------------------------------");
        }
        public void GetUserByNameService()
        {
            System.Console.WriteLine("Enter User name to get details");
            string name = Console.ReadLine() ?? "";
            User user = GetUserByName(name);
            if (user != null)
            {
                System.Console.WriteLine("User found: \n" + user);
                System.Console.WriteLine("------------------------------");
            }
            else
            {
                System.Console.WriteLine("User not found");
            }
        }
        public void GetAllUsersService()
        {
            List<User> users = GetAllUsers();
            if (users != null)
            {
                System.Console.WriteLine("Users found: \n");
                foreach (var user in users)
                {
                    System.Console.WriteLine(user);
                    System.Console.WriteLine("-----------------------------");
                }
            }
            else
            {
                System.Console.WriteLine("No users found");
            }
        }
        public void UpdateUserService()
        {
            System.Console.WriteLine("Enter user name to update details");
            string name = Console.ReadLine() ?? "";
            User existingUser = GetUserByName(name);
            if (existingUser == null)
            {
                System.Console.WriteLine("User not found");
                return;
            }
            int input;
            System.Console.WriteLine("Enter 1 to update email, 2 to update phone number, 3 To update both");
            input = Convert.ToInt32(Console.ReadLine());
            switch (input)
            {
                case 1:
                    System.Console.WriteLine("Enter new email"); 
                    string email = Console.ReadLine() ?? "";
                    existingUser.Email = email;
                    break;
                case 2:
                    System.Console.WriteLine("Enter new phone number");
                    string phoneNumber = Console.ReadLine() ?? "";
                    existingUser.Phone = phoneNumber;
                    break;
                case 3:
                    System.Console.WriteLine("Enter new email");
                    email = Console.ReadLine() ?? "";
                    existingUser.Email = email;
                    System.Console.WriteLine("Enter new phone number");
                    phoneNumber = Console.ReadLine() ?? "";
                    existingUser.Phone = phoneNumber;
                    break;
                default:
                    System.Console.WriteLine("Invalid input");
                    return;
            }
            User updatedUser = UpdateUser(name, existingUser);
            System.Console.WriteLine("User updated successfully \n" + updatedUser);
            System.Console.WriteLine("------------------------------");
        }
        public void DeleteUserService()
        {
            System.Console.WriteLine("Enter user name to delete");
            string name = Console.ReadLine() ?? "";
            User deletedUser = DeleteUser(name);
            if (deletedUser != null)
            {
                System.Console.WriteLine("User deleted successfully " + deletedUser);

            System.Console.WriteLine("------------------------------");
            }
            else
            {
                System.Console.WriteLine("User not found");
            }
        }
    }
}
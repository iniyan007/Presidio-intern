using NotificationApp.Models;

namespace NotificationApp.Service
{
    internal class UserService
    {
        List<User> users = new List<User>();
        public User CreateUser()
        {
            System.Console.WriteLine("Enter user name");
            string name = Console.ReadLine() ?? "";
            System.Console.WriteLine("Enter user email");
            string email = Console.ReadLine() ?? "";
            System.Console.WriteLine("Enter user phone number");
            string phoneNumber = Console.ReadLine() ?? "";
            User user =  new User(name, email, phoneNumber);
            System.Console.WriteLine("User created successfully " + user);
            users.Add(user);
            return user;
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
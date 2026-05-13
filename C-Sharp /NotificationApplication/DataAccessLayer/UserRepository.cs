using DataAccessLayer.Contexts;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccessLayer
{
    public class UserRepository : IUserRepository
    {
        private readonly NotificationsContext _context;

        public UserRepository()
        {
            _context = new NotificationsContext();
        }

        public User CreateUser(string name, string email, string phone)
        {
            User user = new User(name, email, phone);
            _context.Users.Add(user);
            _context.SaveChanges();

            Console.WriteLine("User created successfully");

            return user;
        }

        public User? GetUserByName(string name)
        {
            return _context.Users
                .FirstOrDefault(u => u.Name == name);
        }

        public User? GetUserByPhone(string phone)
        {
            return _context.Users
                .FirstOrDefault(u => u.Phone == phone);
        }

        public User UpdateUser(
            User user,
            string newName,
            string newEmail,
            string newPhone)
        {
            user.Name = newName;
            user.Email = newEmail;
            user.Phone = newPhone;

            _context.Users.Update(user);
            _context.SaveChanges();

            Console.WriteLine("User updated successfully");

            return user;
        }

        public void DeleteUser(User user)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
            Console.WriteLine("User deleted successfully");
        }
    }
}
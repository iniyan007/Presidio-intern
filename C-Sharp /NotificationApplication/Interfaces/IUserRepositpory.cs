using System.Reflection.Metadata;
using Models;
namespace Interfaces
{
    public interface IUserRepository
    {
        public User CreateUser(string name, string email, string phone);
        public User GetUserByName(string name);
        public User GetUserByPhone(string Phone);
        public User UpdateUser(User user, string newName, string newEmail, string newPhone);
        public void DeleteUser(User user);
    }
}
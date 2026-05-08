using Models;
using Interfaces;

namespace DataAccessLayer
{
    public class UserRepository : IUserRepository
    {
        List<User> users = new List<User>();
      
        public User CreateUser(string name, string email, string phone)
        {
            User user =  new User(name, email, phone);
            users.Add(user);
            return user;
        }
        public User GetUserByName(string name)
        {
            User user = users.FirstOrDefault(u => u.Name == name);
            return user;
        }
        public void DeleteUser(User user)
        {
            if (users.Contains(user))
            {
                users.Remove(user);
            }
         }
         public User UpdateUser(User user, string newName, string newEmail, string newPhone)
         {
             if (users.Contains(user))
             {
                 user.Name = newName;
                 user.Email = newEmail;
                 user.Phone = newPhone;
                 return user;
             }
             return null;
         }
         public User GetUserByPhone(string Phone)
        {
            User user = users.FirstOrDefault(u => u.Phone == Phone);
            return user;
        }
    }
}
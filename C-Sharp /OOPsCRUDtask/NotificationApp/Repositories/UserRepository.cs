using NotificationApp.Models;
using NotificationApp.Interfaces;

namespace NotificationApp.Repositories
{
    internal class UserRepository : IRepository<string, User>
    {
        private Dictionary<string, User> users;
        public UserRepository()
        {
            users = new Dictionary<string, User>();
        }
        public User AddUser(User user)
        {
            users.Add(user.Name, user);
            return user;
        }
        public User? GetUserByName(string name)
        {
            if(users.ContainsKey(name))
            {
                return users[name];
            }
            return null;
        }

        public List<User>? GetAllUsers()
        {
            if(users.Count == 0)
            {
                return null;
            }
            return users.Values.ToList();
        }

        public User? UpdateUser(string name, User user)
        {
            if(users.ContainsKey(name))
            {
                users[name] = user;
                return user;
            }
            return null;
        }
        public User? DeleteUser(string name)
        {
            if(users.ContainsKey(name))
            {
                var user = users[name];
                users.Remove(name);
                return user;
            }
            return null;
        }
    }
}
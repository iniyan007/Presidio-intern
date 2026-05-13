using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class User
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public ICollection<Notification>? Notifications { get; set; }
        public User()
        {
            
        }
        public User(string name, string email, string phone)
        {
            Name = name;
            Email = email;
            Phone = phone;
        }
        public override string ToString()
        {
            return $"Name : {Name}\nEmail : {Email}\nPhone : {Phone}";
        }
    }
}
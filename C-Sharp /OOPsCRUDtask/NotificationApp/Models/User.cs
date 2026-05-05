namespace NotificationApp.Models
{
    internal class User
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

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
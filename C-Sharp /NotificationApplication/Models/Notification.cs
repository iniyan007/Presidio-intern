
namespace Models
{
    public class Notification
    {
        public int Id { get; set; }  
        public string ToAddress {get ; set;} = string.Empty;
        public string Message {get; set;} = string.Empty;
        public DateTime Time {get; set;} = DateTime.Now;
        public int? UserId { get; set; }
        public User? User { get; set; }
        public Notification() { }
        public Notification(string notificationType, string to_address, string message)
        {
            ToAddress = to_address;
            Message = message;
        }
        public override string ToString()
        {
            return $"\nTo Email/Phone : {ToAddress}\nMessage : {Message}\nTime : {Time}";
        }
    }
}
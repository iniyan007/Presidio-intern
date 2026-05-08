using Interfaces;
using Models;
namespace DataAccessLayer
{
    public class NotificationRepository : INotificationRepository 
    {
        List<Notification> notifications = new List<Notification>();
        public Notification SaveNotification(string notificationType, string to_address, string message)
        {
            Notification notification = new Notification(notificationType, to_address, message);
            notifications.Add(notification);
            return notification;
        }
        public List<Notification> GetAllNotifications()
        {
            return notifications;
        }
        public List<Notification> GetNotificationsByUser(string to_email, string to_phone)
        {
            return notifications.Where(n => n.ToAddress == to_email || n.ToAddress == to_phone).ToList();
        }
    }
}
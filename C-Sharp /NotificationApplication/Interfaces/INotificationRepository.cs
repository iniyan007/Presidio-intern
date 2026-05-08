using Models;
namespace Interfaces
{
    public interface INotificationRepository
    {
        public Notification SaveNotification(string notificationType, string to_address, string message);
        public List<Notification> GetAllNotifications();
        public List<Notification> GetNotificationsByUser(string to_email, string to_phone);
    }
}
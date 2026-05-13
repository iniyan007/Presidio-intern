using Models;
namespace Interfaces
{
    public interface INotificationRepository
    {
        public Notification SaveNotification(Notification notification);
        public List<Notification> GetAllNotifications();
        public List<Notification> GetNotificationsByUser(string to_email, string to_phone);
    }
}
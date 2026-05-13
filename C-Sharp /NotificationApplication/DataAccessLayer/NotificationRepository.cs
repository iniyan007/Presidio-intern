using DataAccessLayer.Contexts;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccessLayer
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationsContext _context;

        public NotificationRepository()
        {
            _context = new NotificationsContext();
        }

        public Notification SaveNotification(Notification notification)
        {
            _context.Notification.Add(notification);
            _context.SaveChanges();

            Console.WriteLine("Notification saved successfully");

            return notification;
        }

        public List<Notification> GetAllNotifications()
        {
            return _context.Notification.ToList();
        }

        public List<Notification> GetNotificationsByUser(string to_email, string to_phone)
        {
            return _context.Notification
                .Where(n =>
                    n.ToAddress == to_email ||
                    n.ToAddress == to_phone)
                .ToList();
        }
    }
}
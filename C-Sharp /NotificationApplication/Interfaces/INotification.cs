using Models;
namespace Interfaces
{
    public interface INotification
    {
        public void SendMessage(Notification notification);
    }
}
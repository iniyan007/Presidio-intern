using System;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Notifications;

namespace TravelTourManagement.Business.Interface;

public interface INotificationDispatcher
{
    Task PushNotificationAsync(Guid userId, NotificationResponse notification, CancellationToken cancellationToken = default);
}

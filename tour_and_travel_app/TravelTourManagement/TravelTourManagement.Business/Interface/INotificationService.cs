using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Notifications;

namespace TravelTourManagement.Business.Interface;

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string title, string message, Guid? referenceId = null, TravelTourManagement.DataAccess.Enums.NotificationType type = TravelTourManagement.DataAccess.Enums.NotificationType.system, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="Notification"/> entity.
/// </summary>
public interface INotificationRepository : IRepository<Notification, Guid>
{
    /// <summary>Returns all notifications belonging to the specified user, newest first.</summary>
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns only the unread notifications for the specified user.</summary>
    Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Marks a single notification as read.</summary>
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>Marks all notifications belonging to the specified user as read.</summary>
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns the count of unread notifications for the specified user.</summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
}

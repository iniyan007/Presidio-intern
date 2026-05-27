using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.DataAccess.Repository;

/// <summary>
/// EF Core repository for the <see cref="Notification"/> entity.
/// </summary>
public class NotificationRepository : GenericRepository<Notification, Guid>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task MarkAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _dbSet.FindAsync(new object?[] { notificationId }, cancellationToken);
        if (notification is not null && !notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var unread = await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count > 0)
        {
            foreach (var n in unread)
                n.IsRead = true;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.DTOs.Notifications;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationDispatcher _dispatcher;

    public NotificationService(ApplicationDbContext context, INotificationDispatcher dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    public async Task SendNotificationAsync(Guid userId, string title, string message, Guid? referenceId = null, TravelTourManagement.DataAccess.Enums.NotificationType type = TravelTourManagement.DataAccess.Enums.NotificationType.system, CancellationToken cancellationToken = default)
    {
        // 1. Create and save notification to database
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            ReferenceId = referenceId,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        var responseDto = new NotificationResponse(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.ReferenceId,
            notification.IsRead,
            notification.CreatedAt
        );

        // Push real-time event to connected SignalR clients
        await _dispatcher.PushNotificationAsync(userId, responseDto, cancellationToken);
    }

    public async Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .Select(n => new NotificationResponse(
                n.Id,
                n.Title,
                n.Message,
                n.ReferenceId,
                n.IsRead,
                n.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return notifications;
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (notification == null) return false;

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (!unreadNotifications.Any()) return true;

        foreach (var n in unreadNotifications)
        {
            n.IsRead = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

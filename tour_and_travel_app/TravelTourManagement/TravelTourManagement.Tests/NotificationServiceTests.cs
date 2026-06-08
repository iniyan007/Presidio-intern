using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.DTOs.Notifications;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.Tests;

[TestFixture]
public class NotificationServiceTests
{
    private ApplicationDbContext _dbContext;
    private Mock<INotificationDispatcher> _dispatcherMock;
    private NotificationService _notificationService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dispatcherMock = new Mock<INotificationDispatcher>();

        _notificationService = new NotificationService(_dbContext, _dispatcherMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task SendNotificationAsync_SavesToDbAndDispatches()
    {
        var userId = Guid.NewGuid();
        var title = "Test Notification";
        var message = "Test Message";
        var referenceId = Guid.NewGuid();

        await _notificationService.SendNotificationAsync(userId, title, message, referenceId, NotificationType.system);

        var dbNotification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.UserId == userId);
        dbNotification.Should().NotBeNull();
        dbNotification!.Title.Should().Be(title);
        dbNotification.Message.Should().Be(message);
        dbNotification.ReferenceId.Should().Be(referenceId);
        dbNotification.Type.Should().Be(NotificationType.system);
        dbNotification.IsRead.Should().BeFalse();

        _dispatcherMock.Verify(d => d.PushNotificationAsync(userId, It.Is<NotificationResponse>(n => n.Title == title), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetUserNotificationsAsync_ReturnsOrderedAndLimitedNotifications()
    {
        var userId = Guid.NewGuid();
        
        var notifications = Enumerable.Range(1, 10).Select(i => new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = $"Title {i}",
            Message = $"Message {i}",
            IsRead = false,
            Type = NotificationType.system,
            CreatedAt = DateTime.UtcNow.AddMinutes(i) // i=10 is newest
        }).ToList();

        await _dbContext.Notifications.AddRangeAsync(notifications);
        await _dbContext.SaveChangesAsync();

        var result = await _notificationService.GetUserNotificationsAsync(userId, 5);

        result.Should().HaveCount(5);
        result.First().Title.Should().Be("Title 10"); // Newest first
        result.Last().Title.Should().Be("Title 6");
    }

    [Test]
    public async Task MarkAsReadAsync_NotificationNotFoundOrBelongsToOther_ReturnsFalse()
    {
        var result = await _notificationService.MarkAsReadAsync(Guid.NewGuid(), Guid.NewGuid());
        result.Should().BeFalse();
    }

    [Test]
    public async Task MarkAsReadAsync_ValidNotification_MarksAsReadAndReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Test",
            Message = "Msg",
            Type = NotificationType.system,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Notifications.AddAsync(notification);
        await _dbContext.SaveChangesAsync();

        var result = await _notificationService.MarkAsReadAsync(notification.Id, userId);

        result.Should().BeTrue();
        var dbNotification = await _dbContext.Notifications.FindAsync(notification.Id);
        dbNotification!.IsRead.Should().BeTrue();
    }

    [Test]
    public async Task MarkAllAsReadAsync_NoUnreadNotifications_ReturnsTrue()
    {
        var result = await _notificationService.MarkAllAsReadAsync(Guid.NewGuid());
        result.Should().BeTrue();
    }

    [Test]
    public async Task MarkAllAsReadAsync_HasUnread_MarksAllAsRead()
    {
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            new Notification { Id = Guid.NewGuid(), UserId = userId, Title = "Test 1", Message = "Msg", Type = NotificationType.system, IsRead = false, CreatedAt = DateTime.UtcNow },
            new Notification { Id = Guid.NewGuid(), UserId = userId, Title = "Test 2", Message = "Msg", Type = NotificationType.system, IsRead = false, CreatedAt = DateTime.UtcNow },
            new Notification { Id = Guid.NewGuid(), UserId = userId, Title = "Test 3", Message = "Msg", Type = NotificationType.system, IsRead = true, CreatedAt = DateTime.UtcNow }
        };

        await _dbContext.Notifications.AddRangeAsync(notifications);
        await _dbContext.SaveChangesAsync();

        var result = await _notificationService.MarkAllAsReadAsync(userId);

        result.Should().BeTrue();
        var unreadCount = await _dbContext.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        unreadCount.Should().Be(0);
    }
}

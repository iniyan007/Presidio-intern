using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;

namespace TravelTourManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Gets all recent notifications for the authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var notifications = await _notificationService.GetUserNotificationsAsync(userId, limit, cancellationToken);
        return Ok(new { success = true, data = notifications });
    }

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var success = await _notificationService.MarkAsReadAsync(id, userId, cancellationToken);
        if (!success)
            return NotFound(new { success = false, message = "Notification not found or unauthorized." });

        return Ok(new { success = true, message = "Notification marked as read." });
    }

    /// <summary>
    /// Marks all unread notifications as read.
    /// </summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok(new { success = true, message = "All notifications marked as read." });
    }
}

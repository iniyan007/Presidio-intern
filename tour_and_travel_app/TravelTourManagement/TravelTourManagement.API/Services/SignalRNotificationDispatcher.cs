using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.API.Hubs;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Notifications;

namespace TravelTourManagement.API.Services;

public class SignalRNotificationDispatcher : INotificationDispatcher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationDispatcher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PushNotificationAsync(Guid userId, NotificationResponse notification, CancellationToken cancellationToken = default)
    {
        // SignalR uses the user identifier mapping automatically. 
        // We push the "ReceiveNotification" event to the specific user's clients.
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification, cancellationToken);
    }
}

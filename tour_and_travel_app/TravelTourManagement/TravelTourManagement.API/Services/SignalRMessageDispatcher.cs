using Microsoft.AspNetCore.SignalR;
using TravelTourManagement.API.Hubs;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Messages;

namespace TravelTourManagement.API.Services;

public class SignalRMessageDispatcher : IMessageDispatcher
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SignalRMessageDispatcher(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task DispatchMessageAsync(Guid travelerUserId, Guid packagerUserId, MessageDto message, CancellationToken cancellationToken = default)
    {
        var users = new[] { travelerUserId.ToString(), packagerUserId.ToString() };
        await _hubContext.Clients.Users(users).SendAsync("ReceiveMessage", message, cancellationToken);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace TravelTourManagement.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinThread(string threadId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, threadId);
    }

    public async Task LeaveThread(string threadId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, threadId);
    }
}

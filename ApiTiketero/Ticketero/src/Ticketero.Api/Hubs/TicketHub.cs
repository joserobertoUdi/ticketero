using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ticketero.Api.Hubs;

[Authorize]
public class TicketHub : Hub
{
    public async Task JoinAreaGroup(int areaId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"area_{areaId}");
    }

    public async Task LeaveAreaGroup(int areaId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"area_{areaId}");
    }
}

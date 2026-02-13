using Microsoft.AspNetCore.SignalR;

namespace Support.Hubs;

public class SupportHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}

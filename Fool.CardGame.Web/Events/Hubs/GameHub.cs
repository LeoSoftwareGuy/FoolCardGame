using Microsoft.AspNetCore.SignalR;

namespace Fool.CardGame.Web.Events.Hubs
{
    public class GameHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("StatusUpdate", user, message);
        }
    }
}

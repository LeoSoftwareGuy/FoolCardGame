using Microsoft.AspNetCore.SignalR;

namespace Fool.CardGame.Web.Events.Hubs
{
    public class GameHub : Hub
    {
        public async Task UpdateGameState(string user)
        {
            await Clients.All.SendAsync("StatusUpdate", user);
        }
    }
}

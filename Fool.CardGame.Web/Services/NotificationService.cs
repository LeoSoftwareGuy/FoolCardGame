using Fool.CardGame.Web.Events.Hubs;
using Fool.Core.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Fool.CardGame.Web.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<GameHub> _hubContext;

        public NotificationService(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Used for notifying all clients that the round has finished
        /// Regardless of the reason
        /// Used for both surrendering and ending when all cards were defended
        /// </summary>
        /// <returns></returns>
        public Task SendRoundFinishedAsync()
        {
            return _hubContext.Clients.All.SendAsync("RoundFinished");
        }

        public Task SendAfkPlayerWasKickedAsync(string message)
        {
            return _hubContext.Clients.All.SendAsync("AfkPlayerIsOut", message);
        }

        public Task SendAfkPlayerTimeLeftAsync(double amountOfTimeRemaining)
        {
            return _hubContext.Clients.All.SendAsync("AfkPlayerIsCounting", amountOfTimeRemaining.ToString());
        }

        public Task SendTimePassedAsync(double amountOfTimeRemaining, bool isSurrender)
        {
            return _hubContext.Clients.All.SendAsync("TimePassed", amountOfTimeRemaining.ToString(), isSurrender);
        }
    }
}

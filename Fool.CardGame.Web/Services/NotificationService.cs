﻿using Fool.CardGame.Web.Events.Hubs;
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

        public Task SendSurrenderFinishedAsync()
        {
            return _hubContext.Clients.All.SendAsync("SurrenderFinished");
        }

        public Task SendTimePassedAsync(double amountOfTimeRemaining)
        {
            return _hubContext.Clients.All.SendAsync("TimePassed", amountOfTimeRemaining.ToString());
        }
    }
}

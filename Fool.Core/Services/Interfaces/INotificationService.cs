namespace Fool.Core.Services.Interfaces
{
    /// <summary>
    /// For SignalR notifications
    /// </summary>
    public interface INotificationService
    {
        Task SendRoundFinishedAsync();
        Task SendAfkPlayerWasKickedAsync(string message);
        Task SendTimePassedAsync(double amountOfTimeRemaining, bool isSurrender);
    }
}

namespace Fool.Core.Services.Interfaces
{
    /// <summary>
    /// For SignalR notifications
    /// </summary>
    public interface INotificationService
    {
        Task SendRoundFinishedAsync();
        Task SendAfkPlayerWasKickedAsync(string message);
        Task SendAfkPlayerTimeLeftAsync(double amountOfTimeRemaining);
        Task SendTimePassedAsync(double amountOfTimeRemaining, bool isSurrender);
    }
}

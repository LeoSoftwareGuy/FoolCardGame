namespace Fool.Core.Services.Interfaces
{
    /// <summary>
    /// For SignalR notifications
    /// </summary>
    public interface INotificationService
    {
        Task SendSurrenderFinishedAsync();
        Task SendAfkPlayerWasKickedAsync(string message);
        Task SendTimePassedAsync(double amountOfTimeRemaining);
    }
}

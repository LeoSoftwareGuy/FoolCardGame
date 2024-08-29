namespace Fool.Core.Services.Interfaces
{
    /// <summary>
    /// For SignalR notifications
    /// </summary>
    public interface INotificationService
    {
        Task SendSurrenderFinishedAsync();
        Task SendTimePassedAsync(double amountOfTimeRemaining);
    }
}

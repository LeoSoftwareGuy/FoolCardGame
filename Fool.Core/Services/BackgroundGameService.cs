using Fool.Core.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace Fool.Core.Services
{
    public class BackgroundGameService : IHostedService, IDisposable
    {
        private ILogger<BackgroundGameService> _logger;
        private ITableService _tableService;
        private System.Timers.Timer _timer;
        public BackgroundGameService(ITableService tableService, ILogger<BackgroundGameService> logger)
        {
            _tableService = tableService;
            _logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background game service was started");

            _timer = new System.Timers.Timer
            {
                Interval = 1000 // 1 second interval
            };
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();

            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Stop();
            _logger.LogInformation("Background game service was stopped");

            return Task.CompletedTask;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }


        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _logger.LogInformation("Timer elapsed, executing task...");

            // Perform your background task here, e.g., interacting with _tableService
            _tableService.CheckIfRoundWasStopped();
        }

    }
}

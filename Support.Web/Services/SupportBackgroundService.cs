namespace Support.Services;

public class SupportBackgroundService : BackgroundService
{
    private readonly ILogger<SupportBackgroundService> _logger;

    public SupportBackgroundService(ILogger<SupportBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SupportBackgroundService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Placeholder: Add background processing logic here.
            // Example: Check for new support tickets periodically.
            // Example: Process AI analysis queue.

            _logger.LogInformation("SupportBackgroundService is working.");

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("SupportBackgroundService is stopping.");
    }
}

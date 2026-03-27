namespace QPhising.Worker.Services;

public sealed class TaskWorkerService : BackgroundService
{
    private readonly ILogger<TaskWorkerService> _logger;

    public TaskWorkerService(ILogger<TaskWorkerService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Task worker service started at {TimestampUtc}", DateTimeOffset.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Task worker heartbeat at {TimestampUtc}", DateTimeOffset.UtcNow);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}

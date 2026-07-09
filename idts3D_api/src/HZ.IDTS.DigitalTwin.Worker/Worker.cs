namespace HZ.IDTS.DigitalTwin.Worker;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Digital twin worker started with no scheduled tasks in MVP-01.");

        await Task.CompletedTask;
    }
}

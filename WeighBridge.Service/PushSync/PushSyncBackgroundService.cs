using Microsoft.Extensions.Options;
using WeighBridge.Service.Configuration;

namespace WeighBridge.Service.PushSync;

/// <summary>Timer-driven background loop that pushes local changes to Azure SQL Hub.</summary>
public sealed class PushSyncBackgroundService : BackgroundService
{
    private readonly PushSyncEngine _engine;
    private readonly PushSyncSettings _settings;
    private readonly ILogger<PushSyncBackgroundService> _logger;

    public PushSyncBackgroundService(
        PushSyncEngine engine,
        IOptions<PushSyncSettings> settings,
        ILogger<PushSyncBackgroundService> logger)
    {
        _engine = engine;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, _settings.PollIntervalSeconds));
        _logger.LogInformation(
            "Push sync background service started (interval={IntervalSeconds}s, batch={BatchSize}, maxRetry={MaxRetryCount}).",
            interval.TotalSeconds,
            _settings.BatchSize,
            _settings.MaxRetryCount);

        using var timer = new PeriodicTimer(interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _engine.RunCycleAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error in push sync cycle.");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                    break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("Push sync background service stopped.");
    }
}

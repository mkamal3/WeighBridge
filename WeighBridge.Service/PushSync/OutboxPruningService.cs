using Microsoft.Extensions.Options;
using WeighBridge.Service.Configuration;

namespace WeighBridge.Service.PushSync;

/// <summary>Deletes old Synced rows from sync_outbox (Failed rows are retained).</summary>
public sealed class OutboxPruningService : BackgroundService
{
    private readonly OutboxSyncRepository _outboxRepository;
    private readonly PushSyncSettings _settings;
    private readonly ILogger<OutboxPruningService> _logger;

    public OutboxPruningService(
        OutboxSyncRepository outboxRepository,
        IOptions<PushSyncSettings> settings,
        ILogger<OutboxPruningService> logger)
    {
        _outboxRepository = outboxRepository;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await PruneAsync(stoppingToken).ConfigureAwait(false);

        var interval = TimeSpan.FromHours(Math.Max(1, _settings.OutboxPruneIntervalHours));
        using var timer = new PeriodicTimer(interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                    break;

                await PruneAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Outbox pruning failed.");
            }
        }
    }

    private async Task PruneAsync(CancellationToken cancellationToken)
    {
        var deleted = await _outboxRepository.PruneSyncedEntriesAsync(cancellationToken).ConfigureAwait(false);
        if (deleted > 0)
        {
            _logger.LogInformation(
                "Pruned {Count} synced outbox row(s) older than {RetentionDays} day(s).",
                deleted,
                _settings.SyncedOutboxRetentionDays);
        }
        else
        {
            _logger.LogDebug(
                "Outbox prune completed; no synced rows older than {RetentionDays} day(s).",
                _settings.SyncedOutboxRetentionDays);
        }
    }
}

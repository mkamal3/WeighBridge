using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeighBridge.Service.Configuration;

namespace WeighBridge.Service.PushSync;

/// <summary>Generic push sync orchestrator driven by sync_outbox entries.</summary>
public sealed class PushSyncEngine
{
    private readonly SyncTableRegistry _registry;
    private readonly OutboxSyncRepository _outboxRepository;
    private readonly HubSqlPushRepository _hubRepository;
    private readonly PushSyncSettings _settings;
    private readonly ILogger<PushSyncEngine> _logger;
    private readonly HashSet<string> _maxRetryLoggedKeys = new(StringComparer.OrdinalIgnoreCase);

    public PushSyncEngine(
        SyncTableRegistry registry,
        OutboxSyncRepository outboxRepository,
        HubSqlPushRepository hubRepository,
        IOptions<PushSyncSettings> settings,
        ILogger<PushSyncEngine> logger)
    {
        _registry = registry;
        _outboxRepository = outboxRepository;
        _hubRepository = hubRepository;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task RunCycleAsync(CancellationToken cancellationToken)
    {
        string stationId;
        try
        {
            stationId = await _outboxRepository.GetStationIdAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Push sync cycle skipped: unable to resolve StationId.");
            return;
        }

        foreach (var config in _registry.All)
        {
            await SyncEntityTypeAsync(config, stationId, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task SyncEntityTypeAsync(
        ISyncableTableConfig config,
        string stationId,
        CancellationToken cancellationToken)
    {
        await LogMaxRetryEntriesAsync(config, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<OutboxEntry> pendingEntries;
        try
        {
            pendingEntries = await _outboxRepository.GetPendingEntriesAsync(config.EntityType, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read sync_outbox for entity type {EntityType}.", config.EntityType);
            return;
        }

        _logger.LogDebug(
            "Push sync cycle for {EntityType}: {PendingCount} outbox row(s) found.",
            config.EntityType,
            pendingEntries.Count);

        if (pendingEntries.Count == 0)
            return;

        var pushRows = new List<(OutboxEntry Entry, SyncRow Row)>();
        foreach (var entry in pendingEntries)
        {
            var row = await _outboxRepository.LoadEntityRowAsync(config, entry.EntityKey, cancellationToken)
                .ConfigureAwait(false);
            if (row is null)
            {
                _logger.LogWarning(
                    "Outbox entry {OutboxId} references missing {EntityType} key {EntityKey}.",
                    entry.OutboxId,
                    config.EntityType,
                    entry.EntityKey);
                await _outboxRepository.MarkFailedAsync(
                    entry.OutboxId,
                    $"Local {config.LocalTableName} row not found for key {entry.EntityKey}.",
                    cancellationToken).ConfigureAwait(false);
                continue;
            }

            pushRows.Add((entry, row));
        }

        if (pushRows.Count == 0)
            return;

        try
        {
            await _hubRepository.UpsertBatchAsync(
                config,
                pushRows.Select(x => x.Row).ToList(),
                stationId,
                cancellationToken).ConfigureAwait(false);

            await _outboxRepository.MarkSyncedAsync(
                pushRows.Select(x => x.Entry.OutboxId).ToList(),
                cancellationToken).ConfigureAwait(false);

            _logger.LogDebug(
                "Push sync cycle for {EntityType}: pushed {Count} row(s).",
                config.EntityType,
                pushRows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Push sync batch failed for entity type {EntityType}. Marking {Count} outbox row(s) as Failed.",
                config.EntityType,
                pushRows.Count);

            foreach (var (entry, row) in pushRows)
            {
                try
                {
                    await _outboxRepository.MarkFailedAsync(entry.OutboxId, ex.Message, cancellationToken)
                        .ConfigureAwait(false);

                    var nextRetryCount = entry.RetryCount + 1;
                    if (nextRetryCount >= _settings.MaxRetryCount)
                    {
                        var logKey = $"{config.EntityType}:{row.BusinessKey}";
                        if (_maxRetryLoggedKeys.Add(logKey))
                        {
                            _logger.LogError(
                                "Push sync exceeded max retries ({MaxRetryCount}) for {EntityType} key {BusinessKey}. " +
                                "Query stuck rows: SELECT * FROM sync_outbox WHERE Status='Failed' AND RetryCount >= {MaxRetryCount}.",
                                _settings.MaxRetryCount,
                                config.EntityType,
                                row.BusinessKey,
                                _settings.MaxRetryCount);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Push sync failed for {EntityType} key {BusinessKey}: {Error}",
                            config.EntityType,
                            row.BusinessKey,
                            ex.Message);
                    }
                }
                catch (Exception markFailedEx)
                {
                    _logger.LogWarning(
                        markFailedEx,
                        "Failed to mark outbox row {OutboxId} as Failed.",
                        entry.OutboxId);
                }
            }
        }
    }

    private async Task LogMaxRetryEntriesAsync(ISyncableTableConfig config, CancellationToken cancellationToken)
    {
        IReadOnlyList<(long OutboxId, string EntityKey, int RetryCount)> stuckRows;
        try
        {
            stuckRows = await _outboxRepository.GetMaxRetryEntriesAsync(config.EntityType, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to query max-retry outbox rows for {EntityType}.", config.EntityType);
            return;
        }

        foreach (var (outboxId, entityKey, retryCount) in stuckRows)
        {
            var logKey = $"{config.EntityType}:{entityKey}";
            if (!_maxRetryLoggedKeys.Add(logKey))
                continue;

            _logger.LogError(
                "Push sync outbox row {OutboxId} stuck at max retries for {EntityType} key {EntityKey} (RetryCount={RetryCount}). " +
                "Manual review: sync_outbox WHERE Status='Failed' AND RetryCount >= {MaxRetryCount}.",
                outboxId,
                config.EntityType,
                entityKey,
                retryCount,
                _settings.MaxRetryCount);
        }
    }
}

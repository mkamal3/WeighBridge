using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeighBridge.Service.Configuration;

namespace WeighBridge.Service.PushSync;

/// <summary>Generic push sync orchestrator for all registered table configs.</summary>
public sealed class PushSyncEngine
{
    private readonly SyncTableRegistry _registry;
    private readonly LocalSyncRepository _localRepository;
    private readonly HubSqlPushRepository _hubRepository;
    private readonly PushSyncSettings _settings;
    private readonly ILogger<PushSyncEngine> _logger;
    private readonly HashSet<string> _maxRetryLoggedKeys = new(StringComparer.OrdinalIgnoreCase);

    public PushSyncEngine(
        SyncTableRegistry registry,
        LocalSyncRepository localRepository,
        HubSqlPushRepository hubRepository,
        IOptions<PushSyncSettings> settings,
        ILogger<PushSyncEngine> logger)
    {
        _registry = registry;
        _localRepository = localRepository;
        _hubRepository = hubRepository;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task RunCycleAsync(CancellationToken cancellationToken)
    {
        string stationId;
        try
        {
            stationId = await _localRepository.GetStationIdAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Push sync cycle skipped: unable to resolve StationId.");
            return;
        }

        foreach (var config in _registry.All)
        {
            await SyncTableAsync(config, stationId, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task SyncTableAsync(
        ISyncableTableConfig config,
        string stationId,
        CancellationToken cancellationToken)
    {
        await LogMaxRetryRowsAsync(config, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<SyncRow> pendingRows;
        try
        {
            pendingRows = await _localRepository.GetPendingRowsAsync(config, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to read pending rows for local table {Table}.",
                config.LocalTableName);
            return;
        }

        _logger.LogDebug(
            "Push sync cycle for {Table}: {PendingCount} pending row(s) found.",
            config.LocalTableName,
            pendingRows.Count);

        if (pendingRows.Count == 0)
            return;

        try
        {
            await _hubRepository.UpsertBatchAsync(config, pendingRows, stationId, cancellationToken).ConfigureAwait(false);
            await _localRepository.MarkSyncedAsync(
                config,
                pendingRows.Select(x => x.BusinessKey).ToList(),
                cancellationToken).ConfigureAwait(false);

            _logger.LogDebug(
                "Push sync cycle for {Table}: pushed {Count} row(s).",
                config.LocalTableName,
                pendingRows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Push sync batch failed for local table {Table}. Marking {Count} row(s) as Failed.",
                config.LocalTableName,
                pendingRows.Count);

            foreach (var row in pendingRows)
            {
                try
                {
                    await _localRepository.MarkFailedAsync(
                        config,
                        row.BusinessKey,
                        ex.Message,
                        cancellationToken).ConfigureAwait(false);

                    var nextRetryCount = row.RetryCount + 1;
                    if (nextRetryCount >= _settings.MaxRetryCount)
                    {
                        var logKey = $"{config.LocalTableName}:{row.BusinessKey}";
                        if (_maxRetryLoggedKeys.Add(logKey))
                        {
                            _logger.LogError(
                                "Push sync exceeded max retries ({MaxRetryCount}) for {Table} key {BusinessKey}. " +
                                "Query stuck rows with SyncStatus='Failed' AND RetryCount >= {MaxRetryCount}.",
                                _settings.MaxRetryCount,
                                config.LocalTableName,
                                row.BusinessKey,
                                _settings.MaxRetryCount);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Push sync failed for {Table} key {BusinessKey}: {Error}",
                            config.LocalTableName,
                            row.BusinessKey,
                            ex.Message);
                    }
                }
                catch (Exception markFailedEx)
                {
                    _logger.LogWarning(
                        markFailedEx,
                        "Failed to mark row {BusinessKey} as Failed for table {Table}.",
                        row.BusinessKey,
                        config.LocalTableName);
                }
            }
        }
    }

    private async Task LogMaxRetryRowsAsync(ISyncableTableConfig config, CancellationToken cancellationToken)
    {
        IReadOnlyList<(string BusinessKey, int RetryCount)> stuckRows;
        try
        {
            stuckRows = await _localRepository.GetMaxRetryRowsAsync(config, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to query max-retry rows for {Table}.", config.LocalTableName);
            return;
        }

        foreach (var (businessKey, retryCount) in stuckRows)
        {
            var logKey = $"{config.LocalTableName}:{businessKey}";
            if (!_maxRetryLoggedKeys.Add(logKey))
                continue;

            _logger.LogError(
                "Push sync row stuck at max retries for {Table} key {BusinessKey} (RetryCount={RetryCount}). " +
                "Manual review required: SyncStatus='Failed' AND RetryCount >= {MaxRetryCount}.",
                config.LocalTableName,
                businessKey,
                retryCount,
                _settings.MaxRetryCount);
        }
    }
}

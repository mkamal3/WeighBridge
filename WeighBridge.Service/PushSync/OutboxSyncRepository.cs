using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using WeighBridge.Service.Configuration;

namespace WeighBridge.Service.PushSync;

/// <summary>Reads/writes Hub sync_outbox in bridgeone.db (independent of ADLS sync_state).</summary>
public sealed class OutboxSyncRepository
{
    private readonly BridgeOneDatabasePathResolver _pathResolver;
    private readonly PushSyncSettings _settings;
    private string? _sqlitePath;

    public OutboxSyncRepository(
        BridgeOneDatabasePathResolver pathResolver,
        IOptions<PushSyncSettings> settings)
    {
        _pathResolver = pathResolver;
        _settings = settings.Value;
    }

    public async Task<string> GetStationIdAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT SelectedWeighbridgeCode
            FROM DeviceSettings
            WHERE SettingId = 1;
            """;
        var value = Convert.ToString(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)) ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(value))
            return value.Trim();

        if (!string.IsNullOrWhiteSpace(_settings.StationId))
            return _settings.StationId.Trim();

        throw new InvalidOperationException(
            "StationId is not configured. Set DeviceSettings.SelectedWeighbridgeCode or PushSync:StationId.");
    }

    public async Task<IReadOnlyList<OutboxEntry>> GetPendingEntriesAsync(
        string entityType,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT OutboxId, EntityType, EntityKey, Operation, RetryCount, EnqueuedUtc
            FROM sync_outbox
            WHERE EntityType = $EntityType
              AND (
                  Status = 'Pending'
                  OR (
                      Status = 'Failed'
                      AND RetryCount < $MaxRetryCount
                      AND datetime(EnqueuedUtc) <= datetime(
                          'now',
                          '-' || MIN(
                              $MaxBackoffSeconds,
                              CAST(POWER(2, MAX(RetryCount, 1)) AS INTEGER)) || ' seconds')
                  )
              )
            ORDER BY EnqueuedUtc
            LIMIT $BatchSize;
            """;
        command.Parameters.AddWithValue("$EntityType", entityType);
        command.Parameters.AddWithValue("$MaxRetryCount", _settings.MaxRetryCount);
        command.Parameters.AddWithValue("$MaxBackoffSeconds", _settings.MaxBackoffSeconds);
        command.Parameters.AddWithValue("$BatchSize", _settings.BatchSize);

        var entries = new List<OutboxEntry>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            entries.Add(new OutboxEntry
            {
                OutboxId = reader.GetInt64(0),
                EntityType = reader.GetString(1),
                EntityKey = reader.GetString(2),
                Operation = reader.GetString(3),
                RetryCount = reader.GetInt32(4),
                EnqueuedUtc = reader.GetString(5)
            });
        }

        return entries;
    }

    public async Task<SyncRow?> LoadEntityRowAsync(
        ISyncableTableConfig config,
        string entityKey,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT {string.Join(", ", config.SelectColumns.Select(QuoteIdent))}
            FROM {QuoteIdent(config.LocalTableName)}
            WHERE {QuoteIdent(config.BusinessKeyColumn)} = $EntityKey;
            """;
        command.Parameters.AddWithValue("$EntityKey", entityKey);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in config.SelectColumns)
        {
            var ordinal = reader.GetOrdinal(column);
            values[column] = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
        }

        return new SyncRow
        {
            BusinessKey = entityKey,
            Values = values
        };
    }

    public async Task<IReadOnlyList<(long OutboxId, string EntityKey, int RetryCount)>> GetMaxRetryEntriesAsync(
        string entityType,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT OutboxId, EntityKey, RetryCount
            FROM sync_outbox
            WHERE EntityType = $EntityType
              AND Status = 'Failed'
              AND RetryCount >= $MaxRetryCount;
            """;
        command.Parameters.AddWithValue("$EntityType", entityType);
        command.Parameters.AddWithValue("$MaxRetryCount", _settings.MaxRetryCount);

        var rows = new List<(long, string, int)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            rows.Add((reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2)));
        }

        return rows;
    }

    public async Task MarkSyncedAsync(IReadOnlyCollection<long> outboxIds, CancellationToken cancellationToken)
    {
        if (outboxIds.Count == 0)
            return;

        var syncedAtUtc = DateTime.UtcNow.ToString("o");
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        foreach (var outboxId in outboxIds)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE sync_outbox
                SET Status = 'Synced',
                    SyncedAtUtc = $SyncedAtUtc,
                    LastError = NULL
                WHERE OutboxId = $OutboxId;
                """;
            command.Parameters.AddWithValue("$SyncedAtUtc", syncedAtUtc);
            command.Parameters.AddWithValue("$OutboxId", outboxId);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkFailedAsync(long outboxId, string errorMessage, CancellationToken cancellationToken)
    {
        var failedAtUtc = DateTime.UtcNow.ToString("o");
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE sync_outbox
            SET Status = 'Failed',
                RetryCount = RetryCount + 1,
                LastError = $LastError,
                EnqueuedUtc = $EnqueuedUtc
            WHERE OutboxId = $OutboxId;
            """;
        command.Parameters.AddWithValue("$LastError", Truncate(errorMessage, 2000));
        command.Parameters.AddWithValue("$EnqueuedUtc", failedAtUtc);
        command.Parameters.AddWithValue("$OutboxId", outboxId);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> PruneSyncedEntriesAsync(CancellationToken cancellationToken)
    {
        var cutoffUtc = DateTime.UtcNow.AddDays(-_settings.SyncedOutboxRetentionDays).ToString("o");
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM sync_outbox
            WHERE Status = 'Synced'
              AND trim(ifnull(SyncedAtUtc, '')) <> ''
              AND datetime(SyncedAtUtc) < datetime($CutoffUtc);
            """;
        command.Parameters.AddWithValue("$CutoffUtc", cutoffUtc);
        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        _sqlitePath ??= _pathResolver.Resolve();
        Directory.CreateDirectory(Path.GetDirectoryName(_sqlitePath)!);

        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _sqlitePath,
            Cache = SqliteCacheMode.Shared,
            Mode = SqliteOpenMode.ReadWriteCreate,
            DefaultTimeout = 5
        }.ToString());

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await ApplyDurabilityPragmasAsync(connection, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static async Task ApplyDurabilityPragmasAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        foreach (var pragma in new[]
                 {
                     "PRAGMA journal_mode=WAL;",
                     "PRAGMA synchronous=FULL;",
                     "PRAGMA busy_timeout=5000;"
                 })
        {
            await using var command = connection.CreateCommand();
            command.CommandText = pragma;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static string QuoteIdent(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}

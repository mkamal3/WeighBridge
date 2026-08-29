using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeighBridge.Service.Configuration;

namespace WeighBridge.Service.PushSync;

/// <summary>Reads pending local rows and updates sync status columns in bridgeone.db.</summary>
public sealed class LocalSyncRepository
{
    private readonly BridgeOneDatabasePathResolver _pathResolver;
    private readonly PushSyncSettings _settings;
    private readonly ILogger<LocalSyncRepository> _logger;
    private string? _sqlitePath;

    public LocalSyncRepository(
        BridgeOneDatabasePathResolver pathResolver,
        IOptions<PushSyncSettings> settings,
        ILogger<LocalSyncRepository> logger)
    {
        _pathResolver = pathResolver;
        _settings = settings.Value;
        _logger = logger;
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

    public async Task<IReadOnlyList<SyncRow>> GetPendingRowsAsync(
        ISyncableTableConfig config,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = config.BuildSelectPendingSql();
        command.Parameters.AddWithValue("$MaxRetryCount", _settings.MaxRetryCount);
        command.Parameters.AddWithValue("$BatchSize", _settings.BatchSize);
        command.Parameters.AddWithValue("$MaxBackoffSeconds", _settings.MaxBackoffSeconds);

        var rows = new List<SyncRow>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var column in config.SelectColumns)
            {
                var ordinal = reader.GetOrdinal(column);
                values[column] = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
            }

            var businessKey = Convert.ToString(values[config.BusinessKeyColumn]) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(businessKey))
                continue;

            var retryCount = values.TryGetValue("RetryCount", out var retryValue) && retryValue is not null
                ? Convert.ToInt32(retryValue)
                : 0;

            rows.Add(new SyncRow
            {
                BusinessKey = businessKey,
                Values = values,
                RetryCount = retryCount
            });
        }

        return rows;
    }

    public async Task<IReadOnlyList<(string BusinessKey, int RetryCount)>> GetMaxRetryRowsAsync(
        ISyncableTableConfig config,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT {QuoteIdent(config.BusinessKeyColumn)}, RetryCount
            FROM {QuoteIdent(config.LocalTableName)}
            WHERE SyncStatus = 'Failed'
              AND RetryCount >= $MaxRetryCount
              AND trim(ifnull({QuoteIdent(config.BusinessKeyColumn)}, '')) <> '';
            """;
        command.Parameters.AddWithValue("$MaxRetryCount", _settings.MaxRetryCount);

        var rows = new List<(string, int)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            rows.Add((reader.GetString(0), reader.GetInt32(1)));
        }

        return rows;
    }

    public async Task MarkSyncedAsync(
        ISyncableTableConfig config,
        IReadOnlyCollection<string> businessKeys,
        CancellationToken cancellationToken)
    {
        if (businessKeys.Count == 0)
            return;

        var syncedAtUtc = DateTime.UtcNow.ToString("o");
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        foreach (var key in businessKeys)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"""
                UPDATE {QuoteIdent(config.LocalTableName)}
                SET SyncStatus = 'Synced',
                    SyncedAtUtc = $SyncedAtUtc,
                    LastSyncError = NULL
                WHERE {QuoteIdent(config.BusinessKeyColumn)} = $BusinessKey;
                """;
            command.Parameters.AddWithValue("$SyncedAtUtc", syncedAtUtc);
            command.Parameters.AddWithValue("$BusinessKey", key);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkFailedAsync(
        ISyncableTableConfig config,
        string businessKey,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            UPDATE {QuoteIdent(config.LocalTableName)}
            SET SyncStatus = 'Failed',
                RetryCount = RetryCount + 1,
                LastSyncError = $LastSyncError
            WHERE {QuoteIdent(config.BusinessKeyColumn)} = $BusinessKey;
            """;
        command.Parameters.AddWithValue("$LastSyncError", Truncate(errorMessage, 2000));
        command.Parameters.AddWithValue("$BusinessKey", businessKey);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
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

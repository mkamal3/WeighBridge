using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeighBridge.Service.Configuration;

namespace WeighBridge.Service.PushSync;

/// <summary>Batch upserts pending rows into Azure SQL Hub tables.</summary>
public sealed class HubSqlPushRepository
{
    private readonly PushSyncSettings _settings;
    private readonly ILogger<HubSqlPushRepository> _logger;

    public HubSqlPushRepository(IOptions<PushSyncSettings> settings, ILogger<HubSqlPushRepository> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task UpsertBatchAsync(
        ISyncableTableConfig config,
        IReadOnlyList<SyncRow> rows,
        string stationId,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return;

        if (string.IsNullOrWhiteSpace(_settings.AzureSqlConnectionString))
        {
            throw new InvalidOperationException("PushSync:AzureSqlConnectionString is required.");
        }

        await using var connection = new SqlConnection(_settings.AzureSqlConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        foreach (var row in rows)
        {
            await UpsertSingleAsync(connection, transaction, config, row, stationId, cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Upserted {Count} row(s) into Hub table {Table}", rows.Count, config.HubTableName);
    }

    private static async Task UpsertSingleAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        ISyncableTableConfig config,
        SyncRow row,
        string stationId,
        CancellationToken cancellationToken)
    {
        var hubTable = $"[dbo].[{config.HubTableName}]";
        var sourceProjection = config.HubColumns
            .Select(x => $"{ParamName(x.HubColumn)} AS [{x.HubColumn}]")
            .Concat(["@StationId AS [StationId]", "@SourceLastModifiedUtc AS [SourceLastModifiedUtc]"]);

        var updateSet = config.HubColumns
            .Where(x => !string.Equals(x.HubColumn, config.BusinessKeyColumn, StringComparison.OrdinalIgnoreCase))
            .Select(x => $"target.[{x.HubColumn}] = source.[{x.HubColumn}]")
            .Concat([
                "target.[StationId] = source.[StationId]",
                "target.[SourceLastModifiedUtc] = source.[SourceLastModifiedUtc]",
                "target.[HubUpdatedUtc] = SYSUTCDATETIME()"
            ]);

        var insertColumns = config.HubColumns
            .Select(x => $"[{x.HubColumn}]")
            .Concat(["[StationId]", "[SourceLastModifiedUtc]", "[HubReceivedUtc]", "[HubUpdatedUtc]"]);

        var insertValues = config.HubColumns
            .Select(x => $"source.[{x.HubColumn}]")
            .Concat(["source.[StationId]", "source.[SourceLastModifiedUtc]", "SYSUTCDATETIME()", "SYSUTCDATETIME()"]);

        var mergeSql = $"""
            MERGE {hubTable} WITH (HOLDLOCK) AS target
            USING (SELECT {string.Join(", ", sourceProjection)}) AS source
            ON target.[{config.BusinessKeyColumn}] = source.[{config.BusinessKeyColumn}]
            WHEN MATCHED THEN UPDATE SET
                {string.Join(",\n                ", updateSet)}
            WHEN NOT MATCHED THEN INSERT ({string.Join(", ", insertColumns)})
            VALUES ({string.Join(", ", insertValues)});
            """;

        await using var command = new SqlCommand(mergeSql, connection, transaction);
        foreach (var mapping in config.HubColumns)
        {
            row.Values.TryGetValue(mapping.LocalColumn, out var rawValue);
            command.Parameters.AddWithValue(ParamName(mapping.HubColumn), ConvertHubValue(mapping, rawValue));
        }

        command.Parameters.AddWithValue("@StationId", stationId);
        command.Parameters.AddWithValue("@SourceLastModifiedUtc", ParseSourceLastModifiedUtc(row));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string ParamName(string hubColumn) => "@" + hubColumn;

    private static object ConvertHubValue(HubColumnMapping mapping, object? rawValue)
    {
        if (rawValue is null)
            return DBNull.Value;

        return mapping.ColumnType switch
        {
            HubColumnType.Boolean => Convert.ToInt32(rawValue) != 0,
            HubColumnType.Guid => Guid.Parse(Convert.ToString(rawValue, CultureInfo.InvariantCulture) ?? string.Empty),
            HubColumnType.DateTimeOffset => ParseDateTime(rawValue),
            _ => Convert.ToString(rawValue, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    private static DateTime ParseDateTime(object rawValue)
    {
        if (rawValue is DateTime dateTime)
            return dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();

        var text = Convert.ToString(rawValue, CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(text))
            return DateTime.UtcNow;

        return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
    }

    private static DateTime ParseSourceLastModifiedUtc(SyncRow row)
    {
        if (row.Values.TryGetValue("LastModifiedUtc", out var rawValue) && rawValue is not null)
            return ParseDateTime(rawValue);

        return DateTime.UtcNow;
    }
}

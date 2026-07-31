using System.Globalization;
using DeltaToSqlitePoc.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DeltaToSqlitePoc.Services;

/// <summary>
/// SQLite persistence for Vendor rows + sync_state watermarks.
/// Creates/evolves columns dynamically from the Synapse Link Parquet schema.
/// </summary>
public sealed class SqliteVendorRepository : IAsyncDisposable
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteVendorRepository> _logger;
    private SqliteConnection? _connection;

    public SqliteVendorRepository(string sqlitePath, ILogger<SqliteVendorRepository> logger)
    {
        var fullPath = Path.GetFullPath(sqlitePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? ".");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
        _logger = logger;
        _logger.LogInformation("SQLite database: {Path}", fullPath);
    }

    public async Task OpenAsync(CancellationToken ct)
    {
        _connection = new SqliteConnection(_connectionString);
        await _connection.OpenAsync(ct).ConfigureAwait(false);
        await EnsureSyncStateTableAsync(ct).ConfigureAwait(false);
    }

    public async Task EnsureVendorTableAsync(string tableName, IEnumerable<string> columns, CancellationToken ct)
    {
        var safeTable = QuoteIdent(tableName);

        //var mappings = ColumnMapper.GetColumnMappings(tableName);

        //var mappedColumns = columns
        //    .Select(c => mappings.TryGetValue(c, out var mapped) ? mapped : c)
        //    .ToList();

        //var cols = NormalizeColumnOrder(tableName, mappedColumns);

        var cols = NormalizeColumnOrder(tableName, columns);

        var columnDefs = string.Join(",\n                ",
            cols.Select(c =>
                c.Equals("Id", StringComparison.OrdinalIgnoreCase)
                    ? "Id TEXT NOT NULL PRIMARY KEY"
                    : $"{QuoteIdent(c)} TEXT NULL"));

        var createSql = $"""
            CREATE TABLE IF NOT EXISTS {safeTable} (
                {columnDefs}
            );
            """;
        await ExecuteNonQueryAsync(createSql, ct).ConfigureAwait(false);

        var existing = await GetExistingColumnsAsync(tableName, ct).ConfigureAwait(false);
        foreach (var col in cols)
        {
            if (existing.Contains(col))
            {
                continue;
            }

            var alter = $"ALTER TABLE {safeTable} ADD COLUMN {QuoteIdent(col)} TEXT NULL;";
            _logger.LogInformation("Schema evolution: adding column {Column} to {Table}", col, tableName);
            await ExecuteNonQueryAsync(alter, ct).ConfigureAwait(false);
            existing.Add(col);
        }
    }

    public async Task DropAndRecreateVendorTableAsync(
        string tableName,
        IEnumerable<string> columns,
        CancellationToken ct)
    {
        var safeTable = QuoteIdent(tableName);
        await ExecuteNonQueryAsync($"DROP TABLE IF EXISTS {safeTable};", ct).ConfigureAwait(false);
        await EnsureVendorTableAsync(tableName, columns, ct).ConfigureAwait(false);
    }

    public async Task<long> UpsertBatchAsync(
        string tableName,
        IReadOnlyList<VendorRow> vendors,
        IReadOnlyList<string> columns,
        CancellationToken ct)
    {
        var upserts = vendors.Where(v => !v.IsDelete).ToList();
        if (upserts.Count == 0)
        {
            return 0;
        }

        var conn = RequireConnection();
        var allCols = NormalizeColumnOrder(tableName, columns);
        await EnsureVendorTableAsync(tableName, allCols, ct).ConfigureAwait(false);

        var colList = string.Join(", ", allCols.Select(QuoteIdent));
        var paramList = string.Join(", ", allCols.Select((_, i) => $"@p{i}"));
        var updateList = string.Join(", ",
            allCols.Where(c => !c.Equals("Id", StringComparison.OrdinalIgnoreCase))
                .Select(c => $"{QuoteIdent(c)}=excluded.{QuoteIdent(c)}"));

        var sql = $"""
            INSERT INTO {QuoteIdent(tableName)} ({colList})
            VALUES ({paramList})
            ON CONFLICT(Id) DO UPDATE SET {updateList};
            """;

        await using var tx = (SqliteTransaction)await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
        long written = 0;

        foreach (var vendor in upserts)
        {
            ct.ThrowIfCancellationRequested();
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;

            for (var i = 0; i < allCols.Count; i++)
            {
                var col = allCols[i];
                var value = FormatValue(GetValue(vendor, col));
                cmd.Parameters.AddWithValue($"@p{i}", value ?? DBNull.Value);
            }

            written += await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        await tx.CommitAsync(ct).ConfigureAwait(false);
        return written;
    }

    public async Task<long> DeleteByIdsAsync(string tableName, IEnumerable<string> ids, CancellationToken ct)
    {
        var idList = ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (idList.Count == 0)
        {
            return 0;
        }

        var conn = RequireConnection();
        await EnsureVendorTableAsync(tableName, ["Id"], ct).ConfigureAwait(false);

        await using var tx = (SqliteTransaction)await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
        long deleted = 0;

        foreach (var id in idList)
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = $"DELETE FROM {QuoteIdent(tableName)} WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            deleted += await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        await tx.CommitAsync(ct).ConfigureAwait(false);
        return deleted;
    }

    public async Task<SyncState?> GetSyncStateAsync(string entityName, CancellationToken ct)
    {
        var conn = RequireConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT EntityName, LastDeltaVersion, LastUpdatedAt, LastSyncedAt, RowsProcessed
            FROM sync_state
            WHERE EntityName = @name;
            """;
        cmd.Parameters.AddWithValue("@name", entityName);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return new SyncState
        {
            EntityName = reader.GetString(0),
            LastDeltaVersion = reader.IsDBNull(1) ? null : reader.GetInt64(1),
            LastUpdatedAt = reader.IsDBNull(2) ? null : DateTimeOffset.Parse(reader.GetString(2), CultureInfo.InvariantCulture),
            LastSyncedAt = DateTimeOffset.Parse(reader.GetString(3), CultureInfo.InvariantCulture),
            RowsProcessed = reader.IsDBNull(4) ? 0 : reader.GetInt64(4)
        };
    }

    public async Task SaveSyncStateAsync(SyncState state, CancellationToken ct)
    {
        var conn = RequireConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO sync_state (EntityName, LastDeltaVersion, LastUpdatedAt, LastSyncedAt, RowsProcessed)
            VALUES (@name, @ver, @upd, @synced, @rows)
            ON CONFLICT(EntityName) DO UPDATE SET
                LastDeltaVersion = excluded.LastDeltaVersion,
                LastUpdatedAt = excluded.LastUpdatedAt,
                LastSyncedAt = excluded.LastSyncedAt,
                RowsProcessed = excluded.RowsProcessed;
            """;
        cmd.Parameters.AddWithValue("@name", state.EntityName);
        cmd.Parameters.AddWithValue("@ver", (object?)state.LastDeltaVersion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@upd", (object?)state.LastUpdatedAt?.ToString("O") ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@synced", state.LastSyncedAt.ToString("O"));
        cmd.Parameters.AddWithValue("@rows", state.RowsProcessed);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static List<string> NormalizeColumnOrder(IEnumerable<string> columns)
    {
        return NormalizeColumnOrder(null, columns);
    }

    private static List<string> NormalizeColumnOrder(string? tableName, IEnumerable<string> columns)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<string> { "Id" };
        set.Add("Id");

        IEnumerable<string> seed = tableName switch
        {
            var t when string.Equals(t, VendorSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase) => VendorSchema.Columns,
            var t when string.Equals(t, CustomerSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase) => CustomerSchema.Columns,
            var t when string.Equals(t, WarehouseSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase) => WarehouseSchema.Columns,
            _ => VendorSchema.Columns
        };

        var mappings = ColumnMapper.GetColumnMappings(tableName);

        var mappedColumns = columns
        .Select(c => mappings.TryGetValue(c, out var mapped) ? mapped : c)
        .ToList();

        seed = mappedColumns;

        foreach (var col in seed)
        {
            if (set.Add(col))
            {
                ordered.Add(col);
            }
        }

        //foreach (var col in columns)
        //{
        //    if (string.IsNullOrWhiteSpace(col))
        //    {
        //        continue;
        //    }

        //    if (set.Add(col))
        //    {
        //        ordered.Add(col);
        //    }
        //}

        return ordered;
        //return columns.ToList();
    }

    private static object? GetValue(VendorRow vendor, string column)
    {
        if (column.Equals("Id", StringComparison.OrdinalIgnoreCase))
        {
            return vendor.Id;
        }

        return vendor.Values.TryGetValue(column, out var v) ? v : null;
    }

    private static object? FormatValue(object? value) =>
        value switch
        {
            null => null,
            bool b => b ? "1" : "0",
            DateTimeOffset dto => dto.ToString("O"),
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };

    private async Task EnsureSyncStateTableAsync(CancellationToken ct)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS sync_state (
                EntityName TEXT NOT NULL PRIMARY KEY,
                LastDeltaVersion INTEGER NULL,
                LastUpdatedAt TEXT NULL,
                LastSyncedAt TEXT NOT NULL,
                RowsProcessed INTEGER NOT NULL DEFAULT 0
            );
            """;
        await ExecuteNonQueryAsync(sql, ct).ConfigureAwait(false);
    }

    private async Task<HashSet<string>> GetExistingColumnsAsync(string tableName, CancellationToken ct)
    {
        var conn = RequireConnection();
        var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info({QuoteIdent(tableName)});";
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            cols.Add(reader.GetString(1));
        }

        return cols;
    }

    private async Task ExecuteNonQueryAsync(string sql, CancellationToken ct)
    {
        var conn = RequireConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private SqliteConnection RequireConnection() =>
        _connection ?? throw new InvalidOperationException("SQLite connection is not open. Call OpenAsync first.");

    private static string QuoteIdent(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Any(c => !(char.IsLetterOrDigit(c) || c is '_')))
        {
            return "\"" + name.Replace("\"", "\"\"") + "\"";
        }

        return name;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
        }
    }
}

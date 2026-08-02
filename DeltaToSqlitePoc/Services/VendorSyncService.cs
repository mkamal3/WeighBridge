using System.Diagnostics;
using DeltaToSqlitePoc.Cli;
using DeltaToSqlitePoc.Configuration;
using DeltaToSqlitePoc.Demo;
using DeltaToSqlitePoc.Models;
using Microsoft.Extensions.Logging;

namespace DeltaToSqlitePoc.Services;

/// <summary>
/// Orchestrates full and incremental Vendor sync from Delta Lake → SQLite.
/// Honors Synapse Link <c>IsDelete</c> (hard-delete in SQLite on incremental).
/// </summary>
public sealed class VendorSyncService
{
    private readonly SyncSettings _settings;
    private readonly AdlsDeltaTableReader? _adlsReader;
    private readonly LocalDemoDeltaSource? _demoSource;
    private readonly ParquetVendorMapper _mapper;
    private readonly SqliteVendorRepository _sqlite;
    private readonly ILogger<VendorSyncService> _logger;
    private readonly bool _demoMode;

    public VendorSyncService(
        SyncSettings settings,
        AdlsDeltaTableReader? adlsReader,
        LocalDemoDeltaSource? demoSource,
        ParquetVendorMapper mapper,
        SqliteVendorRepository sqlite,
        ILogger<VendorSyncService> logger,
        bool demoMode)
    {
        _settings = settings;
        _adlsReader = adlsReader;
        _demoSource = demoSource;
        _mapper = mapper;
        _sqlite = sqlite;
        _logger = logger;
        _demoMode = demoMode;
    }

    public async Task<SyncResult> RunAsync(SyncCliOptions cli, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var mode = cli.IsIncremental ? "incremental" : "full";
        var entity = _settings.TableName;
        var deltaPath = _settings.DeltaTablePath;

        Console.WriteLine($"Starting {mode} sync for entity '{entity}'...");
        _logger.LogInformation("Starting {Mode} sync. Entity={Entity}, Path={Path}, Demo={Demo}",
            mode, entity, deltaPath, _demoMode);

        if (!_demoMode)
        {
            await _adlsReader!.EnsureContainerAccessibleAsync(ct).ConfigureAwait(false);
        }

        var snapshot = _demoMode
            ? await _demoSource!.ReadSnapshotAsync(ct).ConfigureAwait(false)
            : await _adlsReader!.ReadSnapshotAsync(deltaPath, ct).ConfigureAwait(false);

        Console.WriteLine($"Source Delta version: {snapshot.Version} ({snapshot.DataFiles.Count} active Parquet file(s))");

        if (cli.IsIncremental)
        {
            return await RunIncrementalAsync(entity, deltaPath, snapshot, sw, ct).ConfigureAwait(false);
        }

        return await RunFullAsync(entity, deltaPath, snapshot, sw, ct).ConfigureAwait(false);
    }

    private async Task<SyncResult> RunFullAsync(
        string entity,
        string deltaPath,
        DeltaTableSnapshot snapshot,
        Stopwatch sw,
        CancellationToken ct)
    {
        var (rows, allColumns) = await ReadVendorsFromFilesAsync(deltaPath, snapshot.DataFiles, ct)
            .ConfigureAwait(false);

        var active = rows.Where(r => !r.IsDelete).ToList();
        var deletedSkipped = rows.Count - active.Count;
        if (deletedSkipped > 0)
        {
            Console.WriteLine($"Skipping {deletedSkipped} row(s) marked IsDelete=true on full sync...");
        }

        Console.WriteLine($"Processed {active.Count} active Vendor row(s) from source...");
        await _sqlite.DropAndRecreateVendorTableAsync(entity, allColumns, ct).ConfigureAwait(false);

        var written = await WriteInBatchesAsync(entity, active, allColumns, ct).ConfigureAwait(false);
        var maxUpdated = active.Where(r => r.ModifiedOn.HasValue).Select(r => r.ModifiedOn!.Value).DefaultIfEmpty().Max();

        await _sqlite.SaveSyncStateAsync(new SyncState
        {
            EntityName = entity,
            LastDeltaVersion = snapshot.Version,
            LastUpdatedAt = maxUpdated == default ? null : maxUpdated,
            LastSyncedAt = DateTimeOffset.UtcNow,
            RowsProcessed = active.Count
        }, ct).ConfigureAwait(false);

        sw.Stop();
        Console.WriteLine($"Sync completed in {sw.Elapsed.TotalSeconds:F2} seconds. Rows written: {written}");

        return new SyncResult
        {
            Mode = "full",
            EntityName = entity,
            RowsRead = rows.Count,
            RowsWritten = written,
            SourceDeltaVersion = snapshot.Version,
            Duration = sw.Elapsed,
            Message = "Full sync replaced the Vendor table."
        };
    }

    private async Task<SyncResult> RunIncrementalAsync(
        string entity,
        string deltaPath,
        DeltaTableSnapshot snapshot,
        Stopwatch sw,
        CancellationToken ct)
    {
        var state = await _sqlite.GetSyncStateAsync(entity, ct).ConfigureAwait(false);
        if (state?.LastDeltaVersion is null)
        {
            Console.WriteLine("No prior sync state found — falling back to full sync.");
            _logger.LogWarning("Incremental requested but no watermark; running full sync.");
            return await RunFullAsync(entity, deltaPath, snapshot, sw, ct).ConfigureAwait(false);
        }

        if (snapshot.Version <= state.LastDeltaVersion.Value)
        {
            sw.Stop();
            Console.WriteLine(
                $"No new Delta versions (source={snapshot.Version}, last={state.LastDeltaVersion}). Nothing to do.");
            return new SyncResult
            {
                Mode = "incremental",
                EntityName = entity,
                RowsRead = 0,
                RowsWritten = 0,
                SourceDeltaVersion = snapshot.Version,
                Duration = sw.Elapsed,
                Skipped = true,
                Message = "Already up to date."
            };
        }

        // No Delta Change Feed on Synapse Link source tables, and the Delta version alone
        // doesn't tell us *which* files changed reliably once the ADLS log/checkpoints have
        // moved on. So incremental sync rescans the current active file set (already resolved
        // correctly by AdlsDeltaTableReader.ReadSnapshotAsync, checkpoint-aware) and filters
        // rows by the sink's own modified-timestamp watermark — robust regardless of Delta log
        // retention.
        Console.WriteLine("Incremental: scanning active files with SinkModifiedOn watermark filter...");
        var (rows, allColumns) = await ReadVendorsFromFilesAsync(deltaPath, snapshot.DataFiles, ct)
            .ConfigureAwait(false);
        if (state.LastUpdatedAt is not null)
        {
            rows = rows
                .Where(r => r.ModifiedOn is null || r.ModifiedOn > state.LastUpdatedAt)
                .ToList();
        }

        var toDelete = rows.Where(r => r.IsDelete).Select(r => r.Id).ToList();
        var toUpsert = rows.Where(r => !r.IsDelete).ToList();

        Console.WriteLine($"Processed {rows.Count} rows ({toUpsert.Count} upsert, {toDelete.Count} delete)...");

        var deleted = await _sqlite.DeleteByIdsAsync(entity, toDelete, ct).ConfigureAwait(false);
        var written = await WriteInBatchesAsync(entity, toUpsert, allColumns, ct).ConfigureAwait(false);

        var maxUpdated = toUpsert.Where(r => r.ModifiedOn.HasValue).Select(r => r.ModifiedOn!.Value)
            .Concat(state.LastUpdatedAt is null ? [] : new[] { state.LastUpdatedAt.Value })
            .DefaultIfEmpty()
            .Max();

        await _sqlite.SaveSyncStateAsync(new SyncState
        {
            EntityName = entity,
            LastDeltaVersion = snapshot.Version,
            LastUpdatedAt = maxUpdated == default ? state.LastUpdatedAt : maxUpdated,
            LastSyncedAt = DateTimeOffset.UtcNow,
            RowsProcessed = rows.Count
        }, ct).ConfigureAwait(false);

        sw.Stop();
        Console.WriteLine(
            $"Sync completed in {sw.Elapsed.TotalSeconds:F2} seconds. Upserted: {written}, deleted: {deleted}");

        return new SyncResult
        {
            Mode = "incremental",
            EntityName = entity,
            RowsRead = rows.Count,
            RowsWritten = written,
            SourceDeltaVersion = snapshot.Version,
            Duration = sw.Elapsed,
            Message = $"Advanced watermark from v{state.LastDeltaVersion} to v{snapshot.Version} (deleted {deleted})."
        };
    }

    private async Task<(List<VendorRow> Rows, List<string> Columns)> ReadVendorsFromFilesAsync(
        string deltaPath,
        IReadOnlyList<DeltaDataFile> files,
        CancellationToken ct)
    {
        var rows = new List<VendorRow>();
        var columns = new HashSet<string>(VendorSchema.Columns, StringComparer.OrdinalIgnoreCase);

        var index = 0;
        foreach (var file in files)
        {
            index++;
            Console.WriteLine($"  Reading file {index}/{files.Count}: {file.RelativePath}");

            await using var stream = _demoMode
                ? await _demoSource!.OpenParquetStreamAsync(file.RelativePath, ct).ConfigureAwait(false)
                : await _adlsReader!.OpenParquetStreamAsync(deltaPath, file.RelativePath, ct).ConfigureAwait(false);

            var (batch, fileCols) = await _mapper.ReadVendorsAsync(stream, ct).ConfigureAwait(false);
            rows.AddRange(batch);
            foreach (var c in fileCols)
            {
                columns.Add(c);
            }
        }

        return (rows, columns.ToList());
    }

    private async Task<long> WriteInBatchesAsync(
        string entity,
        IReadOnlyList<VendorRow> rows,
        IReadOnlyList<string> columns,
        CancellationToken ct)
    {
        long written = 0;
        var batchSize = Math.Max(1, _settings.BatchSize);

        for (var i = 0; i < rows.Count; i += batchSize)
        {
            var batch = rows.Skip(i).Take(batchSize).ToList();
            written += await _sqlite.UpsertBatchAsync(entity, batch, columns, ct).ConfigureAwait(false);
            Console.WriteLine($"  Upserted batch {i / batchSize + 1} ({batch.Count} rows)...");
        }

        return written;
    }
}

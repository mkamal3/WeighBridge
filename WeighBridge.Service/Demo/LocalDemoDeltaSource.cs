using System.Text;
using System.Text.Json;
using DeltaToSqlitePoc.Delta;
using DeltaToSqlitePoc.Models;
using Microsoft.Extensions.Logging;
using Parquet;
using Parquet.Schema;

namespace DeltaToSqlitePoc.Demo;

/// <summary>
/// Generates a local Vendor Delta Lake layout matching Synapse Link column names.
/// </summary>
public sealed class LocalDemoDeltaSource
{
    private readonly string _tableRoot;
    private readonly ILogger<LocalDemoDeltaSource> _logger;

    public LocalDemoDeltaSource(string demoRoot, ILogger<LocalDemoDeltaSource> logger)
    {
        _tableRoot = Path.Combine(demoRoot, "d365", "tables", "mserp_vendvendoraientity");
        _logger = logger;
    }

    public string TableRoot => _tableRoot;

    public async Task EnsureDemoDataAsync(bool prepareIncrementalCommit, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.Combine(_tableRoot, "_delta_log"));

        var v0Exists = File.Exists(Path.Combine(_tableRoot, "_delta_log", "00000000000000000000.json"));
        if (!v0Exists)
        {
            _logger.LogInformation("Generating demo Vendor Delta table at {Root}", _tableRoot);
            Console.WriteLine($"Generating local demo Vendor Delta data at '{_tableRoot}'...");

            var vendorsV0 = CreateSeedVendors(version: 0);
            var file0 = Path.Combine("PartitionId=demo", "part-00000-demo.parquet");
            Directory.CreateDirectory(Path.Combine(_tableRoot, "PartitionId=demo"));
            await WriteParquetAsync(Path.Combine(_tableRoot, file0), vendorsV0, ct).ConfigureAwait(false);
            await WriteCommitAsync(0, addPath: file0.Replace('\\', '/'), ct).ConfigureAwait(false);
        }

        if (!prepareIncrementalCommit)
        {
            return;
        }

        var v1Path = Path.Combine(_tableRoot, "_delta_log", "00000000000000000001.json");
        if (!File.Exists(v1Path))
        {
            Console.WriteLine("Appending demo incremental Vendor Delta commit (version 1)...");
            var vendorsV1 = CreateSeedVendors(version: 1);
            var file1 = Path.Combine("PartitionId=demo", "part-00001-demo-incr.parquet");
            Directory.CreateDirectory(Path.Combine(_tableRoot, "PartitionId=demo"));
            await WriteParquetAsync(Path.Combine(_tableRoot, file1), vendorsV1, ct).ConfigureAwait(false);
            await WriteCommitAsync(1, addPath: file1.Replace('\\', '/'), ct).ConfigureAwait(false);
        }
    }

    public Task<DeltaTableSnapshot> ReadSnapshotAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(DeltaLogParser.BuildSnapshot(
            _tableRoot.Replace('\\', '/'),
            LoadCommits()));
    }

    public Task<Stream> OpenParquetStreamAsync(string relativePath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var full = Path.Combine(_tableRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Stream stream = File.OpenRead(full);
        return Task.FromResult(stream);
    }

    private List<(string FileName, string JsonContent)> LoadCommits()
    {
        var logDir = Path.Combine(_tableRoot, "_delta_log");
        return Directory.EnumerateFiles(logDir, "*.json")
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .Select(f => (Path.GetFileName(f), File.ReadAllText(f)))
            .ToList();
    }

    private static List<VendorRow> CreateSeedVendors(int version)
    {
        var now = DateTimeOffset.UtcNow;
        if (version == 0)
        {
            return
            [
                BuildVendor("V-001", "100001", "Contoso Supplies", "contoso@example.com", "Active", now.AddDays(-10), now.AddDays(-2), isDelete: false),
                BuildVendor("V-002", "100002", "Fabrikam Parts", "fabrikam@example.com", "Active", now.AddDays(-8), now.AddDays(-1), isDelete: false),
                BuildVendor("V-003", "100003", "Northwind Traders", "northwind@example.com", "OnHold", now.AddDays(-5), now.AddDays(-5), isDelete: false)
            ];
        }

        // Incremental: update V-002, insert V-004, soft-delete V-003
        return
        [
            BuildVendor("V-002", "100002", "Fabrikam Parts LLC", "fabrikam.updated@example.com", "Active", now.AddDays(-8), now, isDelete: false),
            BuildVendor("V-004", "100004", "Adventure Works Vendor", "aw@example.com", "Active", now, now, isDelete: false),
            BuildVendor("V-003", "100003", "Northwind Traders", "northwind@example.com", "OnHold", now.AddDays(-5), now, isDelete: true)
        ];
    }

    private static VendorRow BuildVendor(
        string id,
        string account,
        string orgName,
        string email,
        string holdStatus,
        DateTimeOffset created,
        DateTimeOffset modified,
        bool isDelete)
    {
        var onHold = holdStatus.Equals("OnHold", StringComparison.OrdinalIgnoreCase) ? 1L : 0L;
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = id,
            ["SinkCreatedOn"] = created,
            ["SinkModifiedOn"] = modified,
            ["mserp_vendoraccountnumber"] = account,
            ["mserp_vendororganizationname"] = orgName,
            ["mserp_vendorsearchname"] = orgName,
            ["mserp_vendorknownasname"] = orgName,
            ["mserp_primaryemailaddress"] = email,
            ["mserp_currencycode"] = "USD",
            ["mserp_dataareaid"] = "usmf",
            ["mserp_vendorgroupid"] = "10",
            ["mserp_onholdstatus"] = onHold,
            ["mserp_creditlimit"] = 50000.00m,
            ["mserp_vendvendoraientityid"] = id,
            ["versionnumber"] = 1L,
            ["IsDelete"] = isDelete,
            ["CreatedOn"] = created,
            ["createdonpartition"] = created.ToString("yyyy-MM-dd"),
            ["PartitionId"] = "demo"
        };

        return new VendorRow
        {
            Id = id,
            IsDelete = isDelete,
            ModifiedOn = modified,
            Values = values
        };
    }

    private static async Task WriteParquetAsync(string path, IReadOnlyList<VendorRow> vendors, CancellationToken ct)
    {
        // Representative subset of the real Synapse Link Vendor schema for local demos.
        var schema = new ParquetSchema(
            new DataField<string>("Id"),
            new DataField<DateTime?>("SinkCreatedOn"),
            new DataField<DateTime?>("SinkModifiedOn"),
            new DataField<string?>("mserp_vendoraccountnumber"),
            new DataField<string?>("mserp_vendororganizationname"),
            new DataField<string?>("mserp_vendorsearchname"),
            new DataField<string?>("mserp_vendorknownasname"),
            new DataField<string?>("mserp_primaryemailaddress"),
            new DataField<string?>("mserp_currencycode"),
            new DataField<string?>("mserp_dataareaid"),
            new DataField<string?>("mserp_vendorgroupid"),
            new DataField<long?>("mserp_onholdstatus"),
            new DataField<decimal?>("mserp_creditlimit"),
            new DataField<string?>("mserp_vendvendoraientityid"),
            new DataField<long?>("versionnumber"),
            new DataField<bool?>("IsDelete"),
            new DataField<DateTime?>("CreatedOn"),
            new DataField<string?>("createdonpartition"),
            new DataField<string?>("PartitionId"));

        string?[] GetString(string col) =>
            vendors.Select(v => v.Values.TryGetValue(col, out var x) ? x?.ToString() : null).ToArray();

        DateTime?[] GetDt(string col) =>
            vendors.Select(v =>
            {
                if (!v.Values.TryGetValue(col, out var x) || x is null)
                {
                    return (DateTime?)null;
                }

                return x switch
                {
                    DateTimeOffset dto => dto.UtcDateTime,
                    DateTime dt => dt,
                    _ => (DateTime?)null
                };
            }).ToArray();

        long?[] GetLong(string col) =>
            vendors.Select(v =>
            {
                if (!v.Values.TryGetValue(col, out var x) || x is null)
                {
                    return (long?)null;
                }

                return (long?)Convert.ToInt64(x, System.Globalization.CultureInfo.InvariantCulture);
            }).ToArray();

        decimal?[] GetDec(string col) =>
            vendors.Select(v =>
            {
                if (!v.Values.TryGetValue(col, out var x) || x is null)
                {
                    return (decimal?)null;
                }

                return (decimal?)Convert.ToDecimal(x, System.Globalization.CultureInfo.InvariantCulture);
            }).ToArray();

        bool?[] GetBool(string col) =>
            vendors.Select(v =>
            {
                if (!v.Values.TryGetValue(col, out var x) || x is null)
                {
                    return (bool?)null;
                }

                return (bool?)(x is bool b ? b : Convert.ToBoolean(x, System.Globalization.CultureInfo.InvariantCulture));
            }).ToArray();

        await using var fs = File.Create(path);
        await using var writer = await ParquetWriter.CreateAsync(schema, fs, cancellationToken: ct).ConfigureAwait(false);
        using var group = writer.CreateRowGroup();

        await group.WriteAsync(schema.DataFields[0], (IReadOnlyCollection<string>)GetString("Id")!).ConfigureAwait(false);
        await group.WriteAsync<DateTime>(schema.DataFields[1], GetDt("SinkCreatedOn"), cancellationToken: ct).ConfigureAwait(false);
        await group.WriteAsync<DateTime>(schema.DataFields[2], GetDt("SinkModifiedOn"), cancellationToken: ct).ConfigureAwait(false);
        await group.WriteAsync(schema.DataFields[3], (IReadOnlyCollection<string>)GetString("mserp_vendoraccountnumber")!).ConfigureAwait(false);
        await group.WriteAsync(schema.DataFields[4], (IReadOnlyCollection<string>)GetString("mserp_vendororganizationname")!).ConfigureAwait(false);
        await group.WriteAsync(schema.DataFields[5], (IReadOnlyCollection<string>)GetString("mserp_vendorsearchname")!).ConfigureAwait(false);
        await group.WriteAsync(schema.DataFields[6], (IReadOnlyCollection<string>)GetString("mserp_vendorknownasname")!).ConfigureAwait(false);
        await group.WriteAsync(schema.DataFields[7], (IReadOnlyCollection<string>)GetString("mserp_primaryemailaddress")!).ConfigureAwait(false);
        await group.WriteAsync(schema.DataFields[8], (IReadOnlyCollection<string>)GetString("mserp_currencycode")!).ConfigureAwait(false);
        await group.WriteAsync(schema.DataFields[9], (IReadOnlyCollection<string>)GetString("mserp_dataareaid")!).ConfigureAwait(false);
        await group.WriteAsync(schema.DataFields[10], (IReadOnlyCollection<string>)GetString("mserp_vendorgroupid")!).ConfigureAwait(false);
        await group.WriteAsync<long>(schema.DataFields[11], GetLong("mserp_onholdstatus"), cancellationToken: ct).ConfigureAwait(false);
        await group.WriteAsync<decimal>(schema.DataFields[12], GetDec("mserp_creditlimit"), cancellationToken: ct).ConfigureAwait(false);
        await group.WriteAsync(schema.DataFields[13], (IReadOnlyCollection<string>)GetString("mserp_vendvendoraientityid")!).ConfigureAwait(false);
        await group.WriteAsync<long>(schema.DataFields[14], GetLong("versionnumber"), cancellationToken: ct).ConfigureAwait(false);
        await group.WriteAsync<bool>(schema.DataFields[15], GetBool("IsDelete"), cancellationToken: ct).ConfigureAwait(false);
        await group.WriteAsync<DateTime>(schema.DataFields[16], GetDt("CreatedOn"), cancellationToken: ct).ConfigureAwait(false);
        await group.WriteAsync(schema.DataFields[17], (IReadOnlyCollection<string>)GetString("createdonpartition")!).ConfigureAwait(false);
        await group.WriteAsync(schema.DataFields[18], (IReadOnlyCollection<string>)GetString("PartitionId")!).ConfigureAwait(false);
    }

    private async Task WriteCommitAsync(long version, string addPath, CancellationToken ct)
    {
        var sb = new StringBuilder();
        if (version == 0)
        {
            sb.AppendLine("""{"protocol":{"minReaderVersion":1,"minWriterVersion":2}}""");
        }

        var fields = VendorSchema.Columns.Select(name => new
        {
            name,
            type = GuessDeltaType(name),
            nullable = true,
            metadata = new { }
        }).ToList();

        var schema = new { type = "struct", fields };
        var meta = new
        {
            metaData = new
            {
                id = Guid.NewGuid().ToString("N"),
                name = "mserp_vendvendoraientity",
                schemaString = JsonSerializer.Serialize(schema),
                format = new { provider = "parquet", options = new { } },
                partitionColumns = new[] { VendorSchema.PartitionColumn },
                configuration = new Dictionary<string, string>
                {
                    ["delta.logRetentionDuration"] = "2 days"
                },
                createdTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            }
        };
        sb.AppendLine(JsonSerializer.Serialize(meta));

        var fi = new FileInfo(Path.Combine(_tableRoot, addPath.Replace('/', Path.DirectorySeparatorChar)));
        var add = new
        {
            add = new
            {
                path = addPath,
                partitionValues = new Dictionary<string, string> { [VendorSchema.PartitionColumn] = "demo" },
                size = fi.Exists ? fi.Length : 0,
                modificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                dataChange = true
            }
        };
        sb.AppendLine(JsonSerializer.Serialize(add));

        var commitPath = Path.Combine(_tableRoot, "_delta_log", $"{version:D20}.json");
        //var commitPath = _tableRoot;
        await File.WriteAllTextAsync(commitPath, sb.ToString(), ct).ConfigureAwait(false);
    }

    private static string GuessDeltaType(string columnName)
    {
        if (columnName.Equals("IsDelete", StringComparison.OrdinalIgnoreCase))
        {
            return "boolean";
        }

        if (columnName.Contains("creditlimit", StringComparison.OrdinalIgnoreCase))
        {
            return "decimal(38,6)";
        }

        if (columnName.EndsWith("On", StringComparison.OrdinalIgnoreCase)
            || columnName.Contains("date", StringComparison.OrdinalIgnoreCase)
            || columnName.Equals("SinkCreatedOn", StringComparison.OrdinalIgnoreCase)
            || columnName.Equals("SinkModifiedOn", StringComparison.OrdinalIgnoreCase)
            || columnName.Equals("CreatedOn", StringComparison.OrdinalIgnoreCase)
            || columnName.Equals("mserp_vendorholdreleasedate", StringComparison.OrdinalIgnoreCase))
        {
            return "timestamp";
        }

        if (columnName.StartsWith("mserp_is", StringComparison.OrdinalIgnoreCase)
            || columnName.Equals("mserp_onholdstatus", StringComparison.OrdinalIgnoreCase)
            || columnName.Equals("mserp_arepricesincludingsalestax", StringComparison.OrdinalIgnoreCase)
            || columnName.Equals("versionnumber", StringComparison.OrdinalIgnoreCase))
        {
            return "long";
        }

        return "string";
    }
}

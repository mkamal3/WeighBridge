using System.Globalization;
using DeltaToSqlitePoc.Models;
using Microsoft.Extensions.Logging;
using Parquet;
using Parquet.Schema;

namespace DeltaToSqlitePoc.Services;

/// <summary>
/// Reads Synapse Link Vendor Parquet into <see cref="VendorRow"/> (all columns preserved).
/// </summary>
public sealed class ParquetVendorMapper
{
    private readonly ILogger<ParquetVendorMapper> _logger;

    public ParquetVendorMapper(ILogger<ParquetVendorMapper> logger)
    {
        _logger = logger;
    }

    public async Task<(IReadOnlyList<VendorRow> Rows, IReadOnlyList<string> AllColumnNames)> ReadVendorsAsync(
        Stream parquetStream,
        CancellationToken ct)
    {
        await using var reader = await ParquetReader.CreateAsync(parquetStream, leaveStreamOpen: false, cancellationToken: ct)
            .ConfigureAwait(false);

        var schemaFields = reader.Schema.GetDataFields();
        var allColumns = schemaFields.Select(f => f.Name).ToList();
        var rows = new List<VendorRow>();

        _logger.LogDebug("Vendor parquet columns ({Count}): {Columns}",
            allColumns.Count,
            string.Join(", ", allColumns.Take(12)) + (allColumns.Count > 12 ? ", ..." : string.Empty));

        for (var i = 0; i < reader.RowGroupCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            using var group = reader.OpenRowGroupReader(i);
            var columns = new Dictionary<string, Array>(StringComparer.OrdinalIgnoreCase);

            foreach (var field in schemaFields)
            {
                columns[field.Name] = await ReadFieldAsArrayAsync(group, field, ct).ConfigureAwait(false);
            }

            var rowCount = (int)group.RowCount;
            for (var row = 0; row < rowCount; row++)
            {
                var vendor = MapRow(columns, allColumns, row);
                if (string.IsNullOrWhiteSpace(vendor.Id))
                {
                    _logger.LogWarning("Skipping parquet row {Row} with empty Id", row);
                    continue;
                }

                rows.Add(vendor);
            }
        }

        return (rows, allColumns);
    }

    private static VendorRow MapRow(
        IReadOnlyDictionary<string, Array> columns,
        IReadOnlyList<string> allColumns,
        int rowIndex)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var col in allColumns)
        {
            if (columns.TryGetValue(col, out var data) && rowIndex < data.Length)
            {
                values[col] = NormalizeValue(data.GetValue(rowIndex));
            }
            else
            {
                values[col] = null;
            }
        }

        var id = GetString(values, "Id") ?? string.Empty;
        var isDelete = GetBool(values, "IsDelete");
        var modified =
            GetDate(values, "SinkModifiedOn")
            ?? GetDate(values, "CreatedOn")
            ?? GetDate(values, "SinkCreatedOn");

        return new VendorRow
        {
            Id = id,
            IsDelete = isDelete,
            ModifiedOn = modified,
            Values = values
        };
    }

    private static string? GetString(IReadOnlyDictionary<string, object?> values, string name) =>
        values.TryGetValue(name, out var v) && v is not null
            ? Convert.ToString(v, CultureInfo.InvariantCulture)
            : null;

    private static bool GetBool(IReadOnlyDictionary<string, object?> values, string name)
    {
        if (!values.TryGetValue(name, out var v) || v is null)
        {
            return false;
        }

        return v switch
        {
            bool b => b,
            long l => l != 0,
            int i => i != 0,
            string s when bool.TryParse(s, out var parsed) => parsed,
            string s when s is "1" or "true" or "True" => true,
            _ => false
        };
    }

    private static DateTimeOffset? GetDate(IReadOnlyDictionary<string, object?> values, string name)
    {
        if (!values.TryGetValue(name, out var v) || v is null)
        {
            return null;
        }

        return v switch
        {
            DateTimeOffset dto => dto,
            DateTime dt => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc)),
            string s when DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
                => parsed,
            _ => null
        };
    }

    private static object? NormalizeValue(object? value) =>
        value switch
        {
            null or DBNull => null,
            DateTime dt => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc)),
            byte[] bytes => Convert.ToBase64String(bytes),
            _ => value
        };

    private static async Task<Array> ReadFieldAsArrayAsync(
        ParquetRowGroupReader group,
        DataField field,
        CancellationToken ct)
    {
        var rowCount = (int)group.RowCount;
        var clr = Nullable.GetUnderlyingType(field.ClrType) ?? field.ClrType;

        if (clr == typeof(string))
        {
            var buffer = new string?[rowCount];
            await group.ReadAsync(field, buffer, cancellationToken: ct).ConfigureAwait(false);
            return buffer;
        }

        if (clr == typeof(bool))
        {
            return await ReadTypedAsync<bool>(group, field, rowCount, ct).ConfigureAwait(false);
        }

        if (clr == typeof(int))
        {
            return await ReadTypedAsync<int>(group, field, rowCount, ct).ConfigureAwait(false);
        }

        if (clr == typeof(long))
        {
            return await ReadTypedAsync<long>(group, field, rowCount, ct).ConfigureAwait(false);
        }

        if (clr == typeof(float))
        {
            return await ReadTypedAsync<float>(group, field, rowCount, ct).ConfigureAwait(false);
        }

        if (clr == typeof(double))
        {
            return await ReadTypedAsync<double>(group, field, rowCount, ct).ConfigureAwait(false);
        }

        if (clr == typeof(decimal))
        {
            return await ReadTypedAsync<decimal>(group, field, rowCount, ct).ConfigureAwait(false);
        }

        if (clr == typeof(DateTime))
        {
            return await ReadTypedAsync<DateTime>(group, field, rowCount, ct).ConfigureAwait(false);
        }

        if (clr == typeof(Guid))
        {
            return await ReadTypedAsync<Guid>(group, field, rowCount, ct).ConfigureAwait(false);
        }

        if (clr == typeof(byte[]))
        {
            var buffer = new byte[]?[rowCount];
            await group.ReadAsync(field, buffer, cancellationToken: ct).ConfigureAwait(false);
            return buffer;
        }

        var asString = new string?[rowCount];
        try
        {
            await group.ReadAsync(field, asString, cancellationToken: ct).ConfigureAwait(false);
            return asString;
        }
        catch
        {
            return new object?[rowCount];
        }
    }

    private static async Task<Array> ReadTypedAsync<T>(
        ParquetRowGroupReader group,
        DataField field,
        int rowCount,
        CancellationToken ct)
        where T : struct
    {
        if (field.IsNullable)
        {
            var buffer = new T?[rowCount];
            await group.ReadAsync<T>(field, buffer, cancellationToken: ct).ConfigureAwait(false);
            return buffer;
        }

        var nonNull = new T[rowCount];
        await group.ReadAsync<T>(field, nonNull, cancellationToken: ct).ConfigureAwait(false);
        return nonNull;
    }
}

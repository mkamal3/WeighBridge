using DeltaToSqlitePoc.Models;
using Parquet;
using Parquet.Schema;

namespace DeltaToSqlitePoc.Delta;

/// <summary>
/// Reads a Delta Lake checkpoint Parquet file — the consolidated <c>add</c> state at a given
/// version — so snapshot resolution stays correct once older <c>_delta_log</c> JSON commits
/// have been pruned by log retention. This is the root cause of "0 active files" on production
/// Synapse Link tables: without this, only trailing JSON commits were ever read.
/// Pure C#/Parquet.Net; no native FFI dependency involved.
/// </summary>
public static class DeltaCheckpointReader
{
    /// <summary>
    /// Parses checkpoint Parquet file names, e.g.
    /// <c>00000000000000000020.checkpoint.parquet</c> (single-part) or
    /// <c>00000000000000000020.checkpoint.0000000001.0000000003.parquet</c> (multi-part).
    /// </summary>
    public static bool TryParseCheckpointFileName(string fileName, out long version, out int partIndex, out int partCount)
    {
        version = 0;
        partIndex = 1;
        partCount = 1;

        var name = Path.GetFileName(fileName);
        if (!name.EndsWith(".parquet", StringComparison.OrdinalIgnoreCase)
            || !name.Contains(".checkpoint", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var segments = name.Split('.');
        if (segments.Length < 3
            || !long.TryParse(segments[0], out version)
            || !segments[1].Equals("checkpoint", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (segments.Length == 3)
        {
            return true; // single-part: {version}.checkpoint.parquet
        }

        if (segments.Length == 5
            && int.TryParse(segments[2], out var pi)
            && int.TryParse(segments[3], out var pc))
        {
            partIndex = pi;
            partCount = pc;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parses one checkpoint Parquet part, returning the active files it contributes and,
    /// if present, the table's schema string (from a <c>metaData</c> row).
    /// </summary>
    public static async Task<(Dictionary<string, DeltaDataFile> ActiveFiles, string? SchemaString)> ReadAsync(
        Stream parquetStream,
        CancellationToken ct)
    {
        var active = new Dictionary<string, DeltaDataFile>(StringComparer.OrdinalIgnoreCase);
        string? schemaString = null;

        await using var reader = await ParquetReader.CreateAsync(parquetStream, leaveStreamOpen: false, cancellationToken: ct)
            .ConfigureAwait(false);

        // Parquet.Net's FieldPath.ToString() joins nested segments with '/', not '.'
        // (e.g. "add/path", "add/size") — despite what the schema-string docs suggest.
        var fields = reader.Schema.GetDataFields();
        DataField? Find(string path) => fields.FirstOrDefault(f => f.Path.ToString() == path);

        var addPathField = Find("add/path");
        if (addPathField is null)
        {
            // Not a recognizable Delta checkpoint layout — nothing usable to extract.
            return (active, schemaString);
        }

        var addSizeField = Find("add/size");
        var addModTimeField = Find("add/modificationTime");
        var removePathField = Find("remove/path");
        var schemaStringField = Find("metaData/schemaString");

        for (var g = 0; g < reader.RowGroupCount; g++)
        {
            using var group = reader.OpenRowGroupReader(g);
            var rowCount = (int)group.RowCount;

            var addPaths = await ReadStringsAsync(group, addPathField, rowCount, ct).ConfigureAwait(false);
            var sizes = addSizeField is null ? null : await ReadLongsAsync(group, addSizeField, rowCount, ct).ConfigureAwait(false);
            var modTimes = addModTimeField is null ? null : await ReadLongsAsync(group, addModTimeField, rowCount, ct).ConfigureAwait(false);
            var removePaths = removePathField is null ? null : await ReadStringsAsync(group, removePathField, rowCount, ct).ConfigureAwait(false);
            var schemaStrings = schemaStringField is null ? null : await ReadStringsAsync(group, schemaStringField, rowCount, ct).ConfigureAwait(false);

            for (var i = 0; i < rowCount; i++)
            {
                var addPath = addPaths[i];
                if (!string.IsNullOrEmpty(addPath))
                {
                    active[addPath] = new DeltaDataFile
                    {
                        RelativePath = addPath,
                        Size = sizes?[i],
                        ModificationTimeMs = modTimes?[i]
                    };
                    continue;
                }

                var removePath = removePaths?[i];
                if (!string.IsNullOrEmpty(removePath))
                {
                    active.Remove(removePath);
                    continue;
                }

                var ss = schemaStrings?[i];
                if (!string.IsNullOrEmpty(ss))
                {
                    schemaString ??= ss;
                }
            }
        }

        return (active, schemaString);
    }

    private static async Task<string?[]> ReadStringsAsync(
        ParquetRowGroupReader group, DataField field, int rowCount, CancellationToken ct)
    {
        var buffer = new string?[rowCount];
        await group.ReadAsync(field, buffer, cancellationToken: ct).ConfigureAwait(false);
        return buffer;
    }

    private static async Task<long?[]> ReadLongsAsync(
        ParquetRowGroupReader group, DataField field, int rowCount, CancellationToken ct)
    {
        var clr = Nullable.GetUnderlyingType(field.ClrType) ?? field.ClrType;

        if (clr == typeof(long))
        {
            var buffer = new long?[rowCount];
            await group.ReadAsync<long>(field, buffer, cancellationToken: ct).ConfigureAwait(false);
            return buffer;
        }

        if (clr == typeof(int))
        {
            var buffer = new int?[rowCount];
            await group.ReadAsync<int>(field, buffer, cancellationToken: ct).ConfigureAwait(false);
            return buffer.Select(v => (long?)v).ToArray();
        }

        // Defensive fallback; Delta spec declares these as integer types.
        var strBuffer = new string?[rowCount];
        await group.ReadAsync(field, strBuffer, cancellationToken: ct).ConfigureAwait(false);
        return strBuffer.Select(s => long.TryParse(s, out var v) ? v : (long?)null).ToArray();
    }
}

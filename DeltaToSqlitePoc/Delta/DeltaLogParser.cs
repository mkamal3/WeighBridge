using System.Text.Json;
using DeltaToSqlitePoc.Models;

namespace DeltaToSqlitePoc.Delta;

/// <summary>
/// Pure-.NET Delta Lake transaction log reader (Synapse Link / Fabric style).
/// Applies add/remove actions across JSON commits to resolve the active file set.
/// </summary>
public static class DeltaLogParser
{
    public static DeltaTableSnapshot BuildSnapshot(
        string tableRootPath,
        IEnumerable<(string FileName, string JsonContent)> commitFiles,
        long? fromVersionExclusive = null)
    {
        var active = new Dictionary<string, DeltaDataFile>(StringComparer.OrdinalIgnoreCase);
        var schemaColumns = new List<string>();
        long maxVersion = -1;

        foreach (var (fileName, json) in commitFiles.OrderBy(f => f.FileName, StringComparer.OrdinalIgnoreCase))
        {
            if (!TryParseVersion(fileName, out var version))
            {
                continue;
            }

            maxVersion = Math.Max(maxVersion, version);

            // For incremental file discovery we still need full history to know active set,
            // but callers can filter which adds to re-read using fromVersionExclusive.
            foreach (var line in json.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;

                if (root.TryGetProperty("add", out var add))
                {
                    var path = add.GetProperty("path").GetString()
                               ?? throw new InvalidOperationException("Delta add action missing path.");
                    long? size = add.TryGetProperty("size", out var sizeEl) && sizeEl.TryGetInt64(out var s) ? s : null;
                    long? mod = add.TryGetProperty("modificationTime", out var modEl) && modEl.TryGetInt64(out var m) ? m : null;

                    // Always apply to active set (needed for correct current snapshot).
                    active[path] = new DeltaDataFile
                    {
                        RelativePath = path,
                        Size = size,
                        ModificationTimeMs = mod
                    };
                }
                else if (root.TryGetProperty("remove", out var remove))
                {
                    var path = remove.GetProperty("path").GetString();
                    if (!string.IsNullOrEmpty(path))
                    {
                        active.Remove(path);
                    }
                }
                else if (root.TryGetProperty("metaData", out var meta)
                         && meta.TryGetProperty("schemaString", out var schemaString))
                {
                    schemaColumns = ParseSchemaColumns(schemaString.GetString());
                }
            }

            _ = fromVersionExclusive; // reserved for future CDF / commit-scoped file diffs
        }

        if (maxVersion < 0)
        {
            throw new InvalidOperationException(
                $"No Delta commit files found under '{tableRootPath}/_delta_log'. Is this a Delta table?");
        }

        return new DeltaTableSnapshot
        {
            TableRootPath = tableRootPath,
            Version = maxVersion,
            DataFiles = active.Values.OrderBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase).ToList(),
            SchemaColumns = schemaColumns
        };
    }

    /// <summary>
    /// Returns Parquet paths that were added in commits after <paramref name="fromVersionExclusive"/>.
    /// Files later removed are excluded. Useful for incremental sync without re-reading everything.
    /// </summary>
    public static IReadOnlyList<DeltaDataFile> GetFilesAddedAfter(
        IEnumerable<(string FileName, string JsonContent)> commitFiles,
        long fromVersionExclusive)
    {
        var added = new Dictionary<string, DeltaDataFile>(StringComparer.OrdinalIgnoreCase);
        var removed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (fileName, json) in commitFiles.OrderBy(f => f.FileName, StringComparer.OrdinalIgnoreCase))
        {
            if (!TryParseVersion(fileName, out var version) || version <= fromVersionExclusive)
            {
                continue;
            }

            foreach (var line in json.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;

                if (root.TryGetProperty("add", out var add))
                {
                    var path = add.GetProperty("path").GetString();
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }

                    removed.Remove(path);
                    long? size = add.TryGetProperty("size", out var sizeEl) && sizeEl.TryGetInt64(out var s) ? s : null;
                    long? mod = add.TryGetProperty("modificationTime", out var modEl) && modEl.TryGetInt64(out var m) ? m : null;
                    added[path] = new DeltaDataFile { RelativePath = path, Size = size, ModificationTimeMs = mod };
                }
                else if (root.TryGetProperty("remove", out var remove))
                {
                    var path = remove.GetProperty("path").GetString();
                    if (!string.IsNullOrEmpty(path))
                    {
                        added.Remove(path);
                        removed.Add(path);
                    }
                }
            }
        }

        return added.Values.OrderBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static bool TryParseVersion(string fileName, out long version)
    {
        version = 0;
        // 00000000000000000012.json
        var name = Path.GetFileName(fileName);
        if (!name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var stem = name[..^5];
        return long.TryParse(stem, out version);
    }

    private static List<string> ParseSchemaColumns(string? schemaString)
    {
        var columns = new List<string>();
        if (string.IsNullOrWhiteSpace(schemaString))
        {
            return columns;
        }

        try
        {
            using var doc = JsonDocument.Parse(schemaString);
            if (doc.RootElement.TryGetProperty("fields", out var fields))
            {
                foreach (var field in fields.EnumerateArray())
                {
                    if (field.TryGetProperty("name", out var name))
                    {
                        var n = name.GetString();
                        if (!string.IsNullOrWhiteSpace(n))
                        {
                            columns.Add(n);
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Schema string is best-effort; Parquet files remain the source of truth.
        }

        return columns;
    }
}

using System.Text.Json;
using DeltaToSqlitePoc.Models;

namespace DeltaToSqlitePoc.Delta;

/// <summary>
/// Pure-.NET Delta Lake transaction log reader. Applies add/remove actions across JSON commits
/// to resolve the active file set. Used directly by the local demo fixture
/// (<see cref="Demo.LocalDemoDeltaSource"/>), which never has checkpoints to worry about.
///
/// For production ADLS reads (<see cref="Services.AdlsDeltaTableReader"/>), the base state is
/// first seeded from the latest checkpoint via <see cref="DeltaCheckpointReader"/> — otherwise
/// this would only ever see the trailing JSON commits still present after Synapse Link's log
/// retention has pruned older ones, which was the root cause of "0 active files".
/// </summary>
public static class DeltaLogParser
{
    public static DeltaTableSnapshot BuildSnapshot(
        string tableRootPath,
        IEnumerable<(string FileName, string JsonContent)> commitFiles,
        IReadOnlyDictionary<string, DeltaDataFile>? seedActiveFiles = null,
        long seedVersion = -1,
        string? seedSchemaString = null)
    {
        var active = new Dictionary<string, DeltaDataFile>(StringComparer.OrdinalIgnoreCase);
        if (seedActiveFiles is not null)
        {
            foreach (var (path, file) in seedActiveFiles)
            {
                active[path] = file;
            }
        }

        var schemaColumns = string.IsNullOrWhiteSpace(seedSchemaString)
            ? new List<string>()
            : ParseSchemaColumns(seedSchemaString);
        long maxVersion = seedVersion;

        foreach (var (fileName, json) in commitFiles.OrderBy(f => f.FileName, StringComparer.OrdinalIgnoreCase))
        {
            if (!TryParseVersion(fileName, out var version))
            {
                continue;
            }

            maxVersion = Math.Max(maxVersion, version);

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
        }

        if (maxVersion < 0)
        {
            throw new InvalidOperationException(
                $"No Delta commit files or checkpoint found under '{tableRootPath}/_delta_log'. Is this a Delta table?");
        }

        return new DeltaTableSnapshot
        {
            TableRootPath = tableRootPath,
            Version = maxVersion,
            DataFiles = active.Values.OrderBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase).ToList(),
            SchemaColumns = schemaColumns
        };
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

    /// <summary>
    /// Parses column names out of a Delta <c>metaData.schemaString</c> JSON payload.
    /// </summary>
    public static List<string> ParseSchemaColumns(string? schemaString)
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

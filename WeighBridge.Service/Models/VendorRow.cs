namespace DeltaToSqlitePoc.Models;

/// <summary>
/// One Vendor row from Synapse Link Delta (mserp_vendvendoraientity-style).
/// All Parquet columns are kept in <see cref="Values"/> for schema-faithful SQLite upserts.
/// </summary>
public sealed class VendorRow
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Synapse Link soft-delete flag.</summary>
    public bool IsDelete { get; set; }

    /// <summary>Prefer SinkModifiedOn, then CreatedOn, for incremental watermarks.</summary>
    public DateTimeOffset? ModifiedOn { get; set; }

    /// <summary>All source columns (including Id / IsDelete), keyed case-insensitively.</summary>
    public Dictionary<string, object?> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

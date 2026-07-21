namespace DeltaToSqlitePoc.Models;

/// <summary>
/// Persisted watermark for incremental sync (stored in SQLite sync_state table).
/// </summary>
public sealed class SyncState
{
    public string EntityName { get; set; } = string.Empty;
    public long? LastDeltaVersion { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
    public DateTimeOffset LastSyncedAt { get; set; }
    public long RowsProcessed { get; set; }
}

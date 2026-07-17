namespace DeltaToSqlitePoc.Models;

public sealed class SyncResult
{
    public required string Mode { get; init; }
    public required string EntityName { get; init; }
    public long RowsRead { get; set; }
    public long RowsWritten { get; set; }
    public long? SourceDeltaVersion { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Skipped { get; set; }
    public string? Message { get; set; }
}

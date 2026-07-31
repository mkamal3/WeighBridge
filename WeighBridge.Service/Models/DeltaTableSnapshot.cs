namespace DeltaToSqlitePoc.Models;

/// <summary>
/// Resolved view of a Delta table at a point in time: version + active Parquet data files.
/// </summary>
public sealed class DeltaTableSnapshot
{
    public required string TableRootPath { get; init; }
    public long Version { get; init; }
    public IReadOnlyList<DeltaDataFile> DataFiles { get; init; } = Array.Empty<DeltaDataFile>();
    public IReadOnlyList<string> SchemaColumns { get; init; } = Array.Empty<string>();
}

public sealed class DeltaDataFile
{
    public required string RelativePath { get; init; }
    public long? Size { get; init; }
    public long? ModificationTimeMs { get; init; }
}

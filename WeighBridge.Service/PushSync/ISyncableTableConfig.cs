namespace WeighBridge.Service.PushSync;

/// <summary>
/// Describes one local SQLite table that pushes to an Azure SQL Hub table.
/// Register additional implementations in <see cref="SyncTableRegistry"/> (e.g. Vehicle, TicketEntry).
/// </summary>
public interface ISyncableTableConfig
{
    string LocalTableName { get; }

    string HubTableName { get; }

    /// <summary>Global business key column (e.g. DriverGuid).</summary>
    string BusinessKeyColumn { get; }

    IReadOnlyList<HubColumnMapping> HubColumns { get; }

    /// <summary>Columns selected from the local table (includes business key and sync metadata).</summary>
    IReadOnlyList<string> SelectColumns { get; }

    string BuildSelectPendingSql();
}

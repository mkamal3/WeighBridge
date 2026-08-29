namespace WeighBridge.Service.PushSync;

/// <summary>
/// Describes one local SQLite table that pushes to an Azure SQL Hub table.
/// Register additional implementations in <see cref="SyncTableRegistry"/> (e.g. Vehicle, TicketEntry).
/// </summary>
public interface ISyncableTableConfig
{
    /// <summary>sync_outbox.EntityType value (e.g. Driver).</summary>
    string EntityType { get; }

    string LocalTableName { get; }

    string HubTableName { get; }

    /// <summary>Global business key column on the local table (e.g. DriverGuid).</summary>
    string BusinessKeyColumn { get; }

    IReadOnlyList<HubColumnMapping> HubColumns { get; }

    /// <summary>Business columns loaded from the local table when pushing.</summary>
    IReadOnlyList<string> SelectColumns { get; }
}

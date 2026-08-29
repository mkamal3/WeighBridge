namespace WeighBridge.Service.PushSync;

/// <summary>
/// Registry of table configs participating in push sync.
/// Add new configs here when extending to Vehicle, TicketEntry, etc.
/// </summary>
public sealed class SyncTableRegistry
{
    private readonly IReadOnlyList<ISyncableTableConfig> _configs;

    public SyncTableRegistry(IEnumerable<ISyncableTableConfig> configs)
    {
        _configs = configs.ToList();
    }

    public IReadOnlyList<ISyncableTableConfig> All => _configs;
}

namespace WeighBridge.Service.PushSync;

/// <summary>Entity payload loaded from a business table for Hub upsert.</summary>
public sealed class SyncRow
{
    public required string BusinessKey { get; init; }

    public required IReadOnlyDictionary<string, object?> Values { get; init; }
}

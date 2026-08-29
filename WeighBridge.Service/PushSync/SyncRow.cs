namespace WeighBridge.Service.PushSync;

/// <summary>One local row ready for Hub upsert.</summary>
public sealed class SyncRow
{
    public required string BusinessKey { get; init; }

    public required IReadOnlyDictionary<string, object?> Values { get; init; }

    public int RetryCount { get; init; }
}

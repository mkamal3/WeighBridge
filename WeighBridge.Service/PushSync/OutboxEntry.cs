namespace WeighBridge.Service.PushSync;

/// <summary>One pending or retryable row in sync_outbox.</summary>
public sealed class OutboxEntry
{
    public required long OutboxId { get; init; }

    public required string EntityType { get; init; }

    public required string EntityKey { get; init; }

    public required string Operation { get; init; }

    public required int RetryCount { get; init; }

    public required string EnqueuedUtc { get; init; }
}

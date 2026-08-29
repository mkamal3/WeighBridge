namespace WeighBridge.Service.Configuration;

/// <summary>
/// Push sync settings (local SQLite → Azure SQL Hub). Bound from the "PushSync" section.
/// </summary>
public sealed class PushSyncSettings
{
    public const string SectionName = "PushSync";

    /// <summary>Absolute or relative path to bridgeone.db. Overrides BridgeOne.config.json when set.</summary>
    public string? SqlitePath { get; set; }

    /// <summary>Optional path to BridgeOne.config.json (used when SqlitePath is not set).</summary>
    public string? BridgeOneConfigPath { get; set; }

    /// <summary>Azure SQL connection string (SQL authentication). Swap for MI/cert later without changing sync engine code.</summary>
    public string AzureSqlConnectionString { get; set; } = string.Empty;

    /// <summary>Fallback when DeviceSettings.SelectedWeighbridgeCode is empty.</summary>
    public string? StationId { get; set; }

    public int PollIntervalSeconds { get; set; } = 3;

    public int BatchSize { get; set; } = 50;

    public int MaxRetryCount { get; set; } = 10;

    /// <summary>Cap for exponential backoff interval (seconds).</summary>
    public int MaxBackoffSeconds { get; set; } = 300;

    /// <summary>Delete sync_outbox rows in Synced status older than this many days.</summary>
    public int SyncedOutboxRetentionDays { get; set; } = 7;

    /// <summary>How often to run outbox pruning (hours).</summary>
    public int OutboxPruneIntervalHours { get; set; } = 1;
}

namespace WeighBridge.D365.Models;

/// <summary>
/// Placeholder OData entity for weighbridge ticket push.
/// Property names and types will be aligned with the D365 data entity once finalized.
/// </summary>
public sealed class WeighbridgeTicketEntity
{
    /// <summary>Local queue record identifier for idempotent sync.</summary>
    public string? ExternalId { get; set; }
}

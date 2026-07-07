using WeighBridge.D365.Models;

namespace WeighBridge.D365.Client;

public interface ID365ODataClient
{
    /// <summary>
    /// Creates a weighbridge ticket record via D365 FO OData (HTTP POST to the configured entity set).
    /// </summary>
    Task CreateWeighbridgeTicketAsync(
        WeighbridgeTicketEntity entity,
        CancellationToken cancellationToken = default);
}

namespace WeighBridge.D365.Authentication;

public interface ID365TokenProvider
{
    /// <summary>
    /// Returns a valid access token for the configured D365 environment (client-credentials flow).
    /// </summary>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

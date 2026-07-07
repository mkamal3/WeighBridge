using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using WeighBridge.D365.Configuration;

namespace WeighBridge.D365.Authentication;

internal sealed class MsalD365TokenProvider : ID365TokenProvider
{
    private readonly D365Options _options;
    private readonly ILogger<MsalD365TokenProvider> _logger;
    private readonly IConfidentialClientApplication _confidentialClient;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private AuthenticationResult? _cachedResult;

    public MsalD365TokenProvider(
        IOptions<D365Options> options,
        ILogger<MsalD365TokenProvider> logger)
    {
        _options = options.Value;
        _logger = logger;

        _confidentialClient = ConfidentialClientApplicationBuilder
            .Create(_options.ClientId)
            .WithClientSecret(_options.ClientSecret!)
            .WithAuthority(AzureCloudInstance.AzurePublic, _options.TenantId)
            .Build();
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (TryGetCachedToken(out var cachedToken))
        {
            return cachedToken;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (TryGetCachedToken(out cachedToken))
            {
                return cachedToken;
            }

            _logger.LogDebug("Acquiring D365 access token via client-credentials flow.");

            var scopes = new[] { _options.GetScope() };
            _cachedResult = await _confidentialClient
                .AcquireTokenForClient(scopes)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug(
                "D365 access token acquired; expires at {ExpiresOn:u}.",
                _cachedResult.ExpiresOn);

            return _cachedResult.AccessToken;
        }
        catch (MsalServiceException ex)
        {
            throw new D365AuthenticationException(
                $"Azure AD token acquisition failed ({ex.ErrorCode}): {ex.Message}", ex);
        }
        catch (MsalClientException ex)
        {
            throw new D365AuthenticationException(
                $"Azure AD client error during token acquisition: {ex.Message}", ex);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private bool TryGetCachedToken(out string accessToken)
    {
        accessToken = string.Empty;

        if (_cachedResult is null)
        {
            return false;
        }

        var refreshThreshold = DateTimeOffset.UtcNow.Add(_options.TokenRefreshBuffer);
        if (_cachedResult.ExpiresOn <= refreshThreshold)
        {
            return false;
        }

        accessToken = _cachedResult.AccessToken;
        return true;
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using WeighBridge.D365.Authentication;

namespace WeighBridge.D365.Health;

internal sealed class D365ConnectionVerifier(
    ID365TokenProvider tokenProvider,
    ILogger<D365ConnectionVerifier> logger) : ID365ConnectionVerifier
{
    public async Task<D365ConnectionVerificationResult> VerifyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

            // MSAL caches expiry internally; parse JWT "exp" only for logging if needed.
            // A successful token acquisition confirms AAD app registration + secret + D365 scope.
            _ = token;

            logger.LogInformation("D365 authentication verification succeeded.");
            return D365ConnectionVerificationResult.Success();
        }
        catch (D365AuthenticationException ex)
        {
            logger.LogWarning(ex, "D365 authentication verification failed.");
            return D365ConnectionVerificationResult.Failure(ex.Message);
        }
        catch (MsalException ex)
        {
            logger.LogWarning(ex, "D365 authentication verification failed.");
            return D365ConnectionVerificationResult.Failure(ex.Message);
        }
    }
}

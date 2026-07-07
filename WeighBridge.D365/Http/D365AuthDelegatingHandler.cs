using System.Net.Http.Headers;
using WeighBridge.D365.Authentication;

namespace WeighBridge.D365.Http;

internal sealed class D365AuthDelegatingHandler(ID365TokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}

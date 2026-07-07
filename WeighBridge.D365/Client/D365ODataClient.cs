using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeighBridge.D365.Configuration;
using WeighBridge.D365.Models;

namespace WeighBridge.D365.Client;

internal sealed class D365ODataClient : ID365ODataClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly D365Options _options;
    private readonly ILogger<D365ODataClient> _logger;

    public D365ODataClient(
        HttpClient httpClient,
        IOptions<D365Options> options,
        ILogger<D365ODataClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task CreateWeighbridgeTicketAsync(
        WeighbridgeTicketEntity entity,
        CancellationToken cancellationToken = default)
    {
        var url = _options.OData.BuildEntitySetUri(_options.BaseUrl);
        var relativePath = _options.OData.WeighbridgeTicketEntitySet.Trim('/');

        _logger.LogInformation(
            "Creating weighbridge ticket via OData at {ODataUrl}.",
            url);

        using var request = new HttpRequestMessage(HttpMethod.Post, relativePath)
        {
            Content = JsonContent.Create(entity, options: JsonOptions)
        };

        request.Headers.TryAddWithoutValidation("Prefer", "return=minimal");

        using var response = await _httpClient
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError(
                "D365 OData create failed with status {StatusCode}: {ErrorBody}",
                (int)response.StatusCode,
                errorBody);

            response.EnsureSuccessStatusCode();
        }
    }
}

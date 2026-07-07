using System.ComponentModel.DataAnnotations;

namespace WeighBridge.D365.Configuration;

/// <summary>
/// D365 FO OData endpoint settings for data entity push.
/// Entity set name must match the published data entity plural name in D365.
/// </summary>
public sealed class ODataOptions
{
    /// <summary>OData root path segment. D365 FO default is <c>data</c>.</summary>
    public string DataPath { get; set; } = "data";

    /// <summary>
    /// OData entity set name, e.g. <c>WeighbridgeTickets</c>.
    /// Confirm via <c>{BaseUrl}/data/$metadata</c> once the data entity is published.
    /// </summary>
    [Required]
    public string WeighbridgeTicketEntitySet { get; set; } = "WeighbridgeTickets";

    public Uri BuildEntitySetUri(Uri environmentBaseUrl)
    {
        var root = environmentBaseUrl.ToString().TrimEnd('/');
        var path = DataPath.Trim('/');
        var entitySet = WeighbridgeTicketEntitySet.Trim('/');
        return new Uri($"{root}/{path}/{entitySet}");
    }
}

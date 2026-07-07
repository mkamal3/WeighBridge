using System.ComponentModel.DataAnnotations;

namespace WeighBridge.D365.Configuration;

/// <summary>
/// Azure AD client-credentials settings for D365 Finance &amp; Operations.
/// Store <see cref="ClientSecret"/> in user secrets or environment variables — never in source control.
/// </summary>
public sealed class D365Options
{
    public const string SectionName = "D365";

    /// <summary>Azure AD tenant (directory) ID.</summary>
    [Required]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>App registration (application) client ID.</summary>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// App registration client secret. Required for client-credentials flow.
    /// Set via <c>dotnet user-secrets set "D365:ClientSecret" "..."</c> or environment variable <c>D365__ClientSecret</c>.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// D365 FO environment root URL, e.g. <c>https://contoso.operations.dynamics.com</c>.
    /// </summary>
    [Required]
    public Uri BaseUrl { get; set; } = null!;

    /// <summary>Refresh the cached token this long before MSAL expiry.</summary>
    public TimeSpan TokenRefreshBuffer { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>HTTP timeout for D365 API calls.</summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>OData settings for data entity push.</summary>
    [Required]
    public ODataOptions OData { get; set; } = new();

    /// <summary>OAuth scope used for client-credentials token acquisition.</summary>
    public string GetScope() => $"{BaseUrl.ToString().TrimEnd('/')}/.default";

    /// <summary>OData service root URI, e.g. <c>https://contoso.operations.dynamics.com/data/</c>.</summary>
    public Uri GetODataRootUri()
    {
        var root = BaseUrl.ToString().TrimEnd('/');
        var path = OData.DataPath.Trim('/');
        return new Uri($"{root}/{path}/");
    }
}

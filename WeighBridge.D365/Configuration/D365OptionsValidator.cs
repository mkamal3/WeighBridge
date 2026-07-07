using Microsoft.Extensions.Options;

namespace WeighBridge.D365.Configuration;

public sealed class D365OptionsValidator : IValidateOptions<D365Options>
{
    public ValidateOptionsResult Validate(string? name, D365Options options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            failures.Add(
                "D365:ClientSecret is required. Set it via user secrets " +
                "(dotnet user-secrets set \"D365:ClientSecret\" \"<secret>\" --project WeighBridge.Service) " +
                "or environment variable D365__ClientSecret.");
        }

        if (options.BaseUrl is not null && options.BaseUrl.Scheme != Uri.UriSchemeHttps)
        {
            failures.Add("D365:BaseUrl must use HTTPS.");
        }

        if (options.TokenRefreshBuffer < TimeSpan.Zero)
        {
            failures.Add("D365:TokenRefreshBuffer must be zero or positive.");
        }

        if (options.HttpTimeout <= TimeSpan.Zero)
        {
            failures.Add("D365:HttpTimeout must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(options.OData.DataPath))
        {
            failures.Add("D365:OData:DataPath is required.");
        }

        if (string.IsNullOrWhiteSpace(options.OData.WeighbridgeTicketEntitySet))
        {
            failures.Add("D365:OData:WeighbridgeTicketEntitySet is required.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}

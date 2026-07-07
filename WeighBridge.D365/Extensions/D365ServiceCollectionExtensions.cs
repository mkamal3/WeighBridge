using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WeighBridge.D365.Authentication;
using WeighBridge.D365.Client;
using WeighBridge.D365.Configuration;
using WeighBridge.D365.Health;
using WeighBridge.D365.Http;

namespace WeighBridge.D365.Extensions;

public static class D365ServiceCollectionExtensions
{
    /// <summary>
    /// Registers D365 FO integration: options validation, MSAL token provider, authenticated OData HttpClient.
    /// </summary>
    public static IServiceCollection AddD365Integration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<D365Options>()
            .Bind(configuration.GetSection(D365Options.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<D365Options>, D365OptionsValidator>();

        services.AddSingleton<ID365TokenProvider, MsalD365TokenProvider>();
        services.AddTransient<D365AuthDelegatingHandler>();
        services.AddSingleton<ID365ConnectionVerifier, D365ConnectionVerifier>();

        services
            .AddHttpClient<ID365ODataClient, D365ODataClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<D365Options>>().Value;
                client.BaseAddress = options.GetODataRootUri();
                client.Timeout = options.HttpTimeout;
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .AddHttpMessageHandler<D365AuthDelegatingHandler>();

        return services;
    }
}

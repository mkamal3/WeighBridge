using WeighBridge.Service.Configuration;
using WeighBridge.Service.PushSync;

namespace WeighBridge.Service.Extensions;

public static class PushSyncServiceCollectionExtensions
{
    /// <summary>
    /// Registers generic push sync services. Add new <see cref="ISyncableTableConfig"/> implementations
    /// to the registry when extending beyond Drivers (e.g. Vehicle, TicketEntry).
    /// </summary>
    public static IServiceCollection AddPushSync(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PushSyncSettings>(configuration.GetSection(PushSyncSettings.SectionName));
        services.AddSingleton<BridgeOneDatabasePathResolver>();
        services.AddSingleton<DriverSyncConfig>();
        services.AddSingleton<SyncTableRegistry>(sp => new SyncTableRegistry([sp.GetRequiredService<DriverSyncConfig>()]));
        services.AddSingleton<LocalSyncRepository>();
        services.AddSingleton<HubSqlPushRepository>();
        services.AddSingleton<PushSyncEngine>();
        services.AddHostedService<PushSyncBackgroundService>();
        return services;
    }
}

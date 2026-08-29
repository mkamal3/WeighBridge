using WeighBridge.Service.Configuration;
using WeighBridge.Service.PushSync;

namespace WeighBridge.Service.Extensions;

public static class PushSyncServiceCollectionExtensions
{
    /// <summary>
    /// Registers generic Hub push sync (sync_outbox driven).
    /// Add new <see cref="ISyncableTableConfig"/> implementations to the registry for additional entity types.
    /// </summary>
    public static IServiceCollection AddPushSync(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PushSyncSettings>(configuration.GetSection(PushSyncSettings.SectionName));
        services.AddSingleton<BridgeOneDatabasePathResolver>();
        services.AddSingleton<DriverSyncConfig>();
        services.AddSingleton<SyncTableRegistry>(sp => new SyncTableRegistry([sp.GetRequiredService<DriverSyncConfig>()]));
        services.AddSingleton<OutboxSyncRepository>();
        services.AddSingleton<HubSqlPushRepository>();
        services.AddSingleton<PushSyncEngine>();
        services.AddHostedService<PushSyncBackgroundService>();
        services.AddHostedService<OutboxPruningService>();
        return services;
    }
}

using CommandLine;
using DeltaToSqlitePoc.Cli;
using DeltaToSqlitePoc.Configuration;
using DeltaToSqlitePoc.Demo;
using DeltaToSqlitePoc.Models;
using DeltaToSqlitePoc.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WeighBridge.Service.Extensions;

namespace DeltaToSqlitePoc;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (IsAdlsCliInvocation(args))
        {
            var parseResult = Parser.Default.ParseArguments<SyncCliOptions>(args);
            return await parseResult.MapResult(
                async opts => await RunAdlsSyncCliAsync(opts, args).ConfigureAwait(false),
                _ => Task.FromResult(1)).ConfigureAwait(false);
        }

        await RunPushSyncWorkerAsync(args).ConfigureAwait(false);
        return 0;
    }

    private static bool IsAdlsCliInvocation(string[] args) =>
        args.Any(static arg => arg.StartsWith("--mode", StringComparison.OrdinalIgnoreCase));

    private static async Task RunPushSyncWorkerAsync(string[] args)
    {
        var contentRoot = AppContext.BaseDirectory;
        Directory.CreateDirectory(Path.Combine(contentRoot, "logs"));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("WeighBridge.Service.PushSync", Serilog.Events.LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(contentRoot, "logs", "push-sync-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();

        try
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseContentRoot(contentRoot)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    config.Sources.Clear();
                    config.SetBasePath(contentRoot);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddUserSecrets(typeof(Program).Assembly, optional: true);
                    config.AddEnvironmentVariables(prefix: "PUSHSYNC_");
                })
                .ConfigureServices((ctx, services) =>
                {
                    services.AddPushSync(ctx.Configuration);
                })
                .Build();

            Log.Information(
                "WeighBridge push sync worker starting (environment={Environment}).",
                host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName);

            await host.RunAsync().ConfigureAwait(false);
        }
        finally
        {
            await Log.CloseAndFlushAsync().ConfigureAwait(false);
        }
    }

    private static async Task<int> RunAdlsSyncCliAsync(SyncCliOptions cli, string[] rawArgs)
    {
        if (!cli.IsFull && !cli.IsIncremental)
        {
            Console.Error.WriteLine("Invalid --mode. Use 'full' or 'incremental'.");
            return 1;
        }

        var contentRoot = AppContext.BaseDirectory;
        Directory.CreateDirectory(Path.Combine(contentRoot, "logs"));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(contentRoot, "logs", "delta-to-sqlite-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();

        try
        {
            // Environment comes from DOTNET_ENVIRONMENT / ASPNETCORE_ENVIRONMENT when set.
            // Local `dotnet run` uses Properties/launchSettings.json → Development.
            // Published/deployed apps default to Production (no manual step unless you override).
            var host = Host.CreateDefaultBuilder(rawArgs)
                .UseContentRoot(contentRoot)
                .UseSerilog()
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    config.Sources.Clear();
                    //config.SetBasePath(cli.Path);
                    config.SetBasePath(contentRoot);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                    config.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddUserSecrets(typeof(Program).Assembly, optional: true);
                    config.AddEnvironmentVariables(prefix: "DELTASYNC_");
                })
                .ConfigureServices((ctx, services) =>
                {
                    var settings = ctx.Configuration.GetSection(SyncSettings.SectionName).Get<SyncSettings>()
                                   ?? new SyncSettings();

                    ApplyCliOverrides(settings, cli);

                    if (!Path.IsPathRooted(settings.SqlitePath))
                    {
                        settings.SqlitePath = Path.Combine(contentRoot, settings.SqlitePath);
                    }

                    services.AddSingleton(settings);
                    services.AddSingleton(cli);
                    services.AddSingleton<ParquetMapper>();
                    //services.AddSingleton<ParquetItemMapper>();

                    services.AddSingleton(sp =>
                    {
                        var s = sp.GetRequiredService<SyncSettings>();
                        var logger = sp.GetRequiredService<ILogger<SqliteRepository>>();
                        return new SqliteRepository(s.SqlitePath, logger);
                    });
                    /*
                    services.AddSingleton(sp =>
                    {
                        var s = sp.GetRequiredService<SyncSettings>();
                        var logger = sp.GetRequiredService<ILogger<SqliteItemRepository>>();
                        return new SqliteItemRepository(s.SqlitePath, logger);
                    });*/

                    if (cli.Demo)
                    {
                        services.AddSingleton(sp =>
                        {
                            var logger = sp.GetRequiredService<ILogger<LocalDemoDeltaSource>>();
                            return new LocalDemoDeltaSource(Path.Combine(contentRoot, "demo-data"), logger);
                        });
                    }
                    else
                    {
                        services.AddSingleton<AdlsDeltaTableReader>();
                    }

                    services.AddSingleton(sp =>
                    {
                        var s = sp.GetRequiredService<SyncSettings>();
                        return new SyncService(
                            s,
                            cli.Demo ? null : sp.GetRequiredService<AdlsDeltaTableReader>(),
                            cli.Demo ? sp.GetRequiredService<LocalDemoDeltaSource>() : null,
                            sp.GetRequiredService<ParquetMapper>(),
                            sp.GetRequiredService<SqliteRepository>(),
                            sp.GetRequiredService<ILogger<SyncService>>(),
                            cli.Demo);
                    });

                    /*
                    services.AddSingleton(sp =>
                    {
                        var s = sp.GetRequiredService<SyncSettings>();
                        return new ItemSyncService(
                            s,
                            cli.Demo ? null : sp.GetRequiredService<AdlsDeltaTableReader>(),
                            cli.Demo ? sp.GetRequiredService<LocalDemoDeltaSource>() : null,
                            sp.GetRequiredService<ParquetItemMapper>(),
                            sp.GetRequiredService<SqliteItemRepository>(),
                            sp.GetRequiredService<ILogger<ItemSyncService>>(),
                            cli.Demo);
                    });*/
                })
                .Build();

            var settings = host.Services.GetRequiredService<SyncSettings>();

            // If the configured TableName matches a known schema, prefer that schema's default
            // Delta path so the app reads the correct Parquet files when not overridden.
            if (string.Equals(settings.TableName, ItemSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
            {
                settings.DeltaTablePath = ItemSchema.DefaultDeltaPath;
                settings.TableName = ItemSchema.DefaultTableName;
            }
            else if (string.Equals(settings.TableName, VendorSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
            {
                settings.DeltaTablePath = VendorSchema.DefaultDeltaPath;
                settings.TableName = VendorSchema.DefaultTableName;
            }
            else if (string.Equals(settings.TableName, CustomerSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
            {
                settings.DeltaTablePath = CustomerSchema.DefaultDeltaPath;
                settings.TableName = CustomerSchema.DefaultTableName;
            }
            else if (string.Equals(settings.TableName, WarehouseSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
            {
                settings.DeltaTablePath = WarehouseSchema.DefaultDeltaPath;
                settings.TableName = WarehouseSchema.DefaultTableName;
            }
            else if (string.Equals(settings.TableName, LegalEntitySchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
            {
                settings.DeltaTablePath = LegalEntitySchema.DefaultDeltaPath;
                settings.TableName = LegalEntitySchema.DefaultTableName;
            }
            else if (string.Equals(settings.TableName, UnitOfMeasureSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
            {
                settings.DeltaTablePath = UnitOfMeasureSchema.DefaultDeltaPath;
                settings.TableName = UnitOfMeasureSchema.DefaultTableName;
            }
            else if (string.Equals(settings.TableName, UnitConversionSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
            {
                settings.DeltaTablePath = UnitConversionSchema.DefaultDeltaPath;
                settings.TableName = UnitConversionSchema.DefaultTableName;
            }

            ValidateSettings(settings, cli.Demo);

            Console.WriteLine("=== Delta Lake → SQLite PoC ===");
            Console.WriteLine($"Environment: {host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName}");
            Console.WriteLine($"Mode      : {cli.Mode}");
            Console.WriteLine($"Table     : {settings.TableName}");
            Console.WriteLine($"Delta path: {settings.DeltaTablePath}");
            Console.WriteLine($"SQLite    : {settings.SqlitePath}");
            Console.WriteLine($"Demo      : {cli.Demo}");
            Console.WriteLine();

            if (cli.Demo)
            {
                var demo = host.Services.GetRequiredService<LocalDemoDeltaSource>();
                await demo.EnsureDemoDataAsync(
                    prepareIncrementalCommit: cli.IsIncremental,
                    CancellationToken.None).ConfigureAwait(false);
                settings.DeltaTablePath = demo.TableRoot;
            }

            SyncResult result;
            /*
            if (string.Equals(settings.TableName, ItemSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
            {
                var sqliteItem = host.Services.GetRequiredService<SqliteItemRepository>();
                await sqliteItem.OpenAsync(CancellationToken.None).ConfigureAwait(false);

                var syncItem = host.Services.GetRequiredService<ItemSyncService>();
                result = await syncItem.RunAsync(cli, CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                */
                var sqliteVendor = host.Services.GetRequiredService<SqliteRepository>();
                await sqliteVendor.OpenAsync(CancellationToken.None).ConfigureAwait(false);

                var syncRow = host.Services.GetRequiredService<SyncService>();
                result = await syncRow.RunAsync(cli, CancellationToken.None).ConfigureAwait(false);
            //}

            Console.WriteLine();
            Console.WriteLine("--- Result ---");
            Console.WriteLine($"Mode            : {result.Mode}");
            Console.WriteLine($"Entity          : {result.EntityName}");
            Console.WriteLine($"Rows read       : {result.RowsRead}");
            Console.WriteLine($"Rows written    : {result.RowsWritten}");
            Console.WriteLine($"Delta version   : {result.SourceDeltaVersion}");
            Console.WriteLine($"Duration (sec)  : {result.Duration.TotalSeconds:F2}");
            Console.WriteLine($"Skipped         : {result.Skipped}");
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                Console.WriteLine($"Message         : {result.Message}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Sync failed");
            Console.Error.WriteLine();
            Console.Error.WriteLine("ERROR: " + ex.Message);
            if (ex.InnerException is not null)
            {
                Console.Error.WriteLine("Inner: " + ex.InnerException.Message);
            }

            return 2;
        }
        finally
        {
            await Log.CloseAndFlushAsync().ConfigureAwait(false);
        }
    }

    private static void ApplyCliOverrides(SyncSettings settings, SyncCliOptions cli)
    {
        if (!string.IsNullOrWhiteSpace(cli.Table))
        {
            settings.TableName = cli.Table;
        }

        if (!string.IsNullOrWhiteSpace(cli.Path))
        {
            settings.DeltaTablePath = cli.Path;
        }

        if (!string.IsNullOrWhiteSpace(cli.SqlitePath))
        {
            settings.SqlitePath = cli.SqlitePath;
        }

        if (!string.IsNullOrWhiteSpace(cli.StorageAccountName))
        {
            settings.StorageAccountName = cli.StorageAccountName;
        }

        if (!string.IsNullOrWhiteSpace(cli.ContainerName))
        {
            settings.ContainerName = cli.ContainerName;
        }
    }

    private static void ValidateSettings(SyncSettings settings, bool demo)
    {
        if (string.IsNullOrWhiteSpace(settings.TableName))
        {
            throw new InvalidOperationException("Sync:TableName is required.");
        }

        if (demo)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.StorageAccountName))
        {
            throw new InvalidOperationException(
                "Sync:StorageAccountName is required (or pass --account / use --demo).");
        }

        if (string.IsNullOrWhiteSpace(settings.ContainerName))
        {
            throw new InvalidOperationException(
                "Sync:ContainerName is required (or pass --container / use --demo).");
        }

        if (string.IsNullOrWhiteSpace(settings.DeltaTablePath))
        {
            throw new InvalidOperationException("Sync:DeltaTablePath is required.");
        }
    }
}

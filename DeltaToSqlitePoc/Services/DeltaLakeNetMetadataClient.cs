using DeltaLake.Table;
using DeltaToSqlitePoc.Configuration;
using Microsoft.Extensions.Logging;

namespace DeltaToSqlitePoc.Services;

/// <summary>
/// Optional integration with DeltaLake.Net (delta-rs / delta-kernel FFI) for version discovery.
/// Azure auth for the native stack typically uses environment variables
/// (AZURE_STORAGE_ACCOUNT_NAME + key/SAS, or Azure CLI / MSI-compatible env).
/// Row materialization remains Parquet.Net-based for predictable Customer mapping.
/// </summary>
public sealed class DeltaLakeNetMetadataClient
{
    private readonly SyncSettings _settings;
    private readonly ILogger<DeltaLakeNetMetadataClient> _logger;

    public DeltaLakeNetMetadataClient(SyncSettings settings, ILogger<DeltaLakeNetMetadataClient> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<long?> TryGetVersionAsync(string deltaTablePath, CancellationToken ct)
    {
        if (!_settings.UseDeltaLakeNet)
        {
            return null;
        }

        try
        {
            var abfs = BuildAbfsUri(deltaTablePath);
            _logger.LogInformation("DeltaLake.Net: loading table metadata from {Uri}", abfs);

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME"))
                && !string.IsNullOrWhiteSpace(_settings.StorageAccountName))
            {
                Environment.SetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME", _settings.StorageAccountName);
            }

            using var engine = new DeltaEngine(EngineOptions.Default);
            using var table = await engine.LoadTableAsync(
                new TableOptions
                {
                    TableLocation = abfs,
                    WithoutFiles = true
                },
                ct).ConfigureAwait(false);

            var version = table.Version();
            _logger.LogInformation("DeltaLake.Net reported version {Version}", version);
            return version.HasValue ? (long)version.Value : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "DeltaLake.Net metadata probe failed; continuing with ADLS _delta_log JSON reader.");
            return null;
        }
    }

    private string BuildAbfsUri(string deltaTablePath)
    {
        var path = deltaTablePath.Replace('\\', '/').Trim('/');
        return $"abfss://{_settings.ContainerName}@{_settings.StorageAccountName}.dfs.core.windows.net/{path}";
    }
}

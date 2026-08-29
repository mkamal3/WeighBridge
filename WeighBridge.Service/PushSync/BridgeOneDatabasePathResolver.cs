using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeighBridge.Service.Configuration;

namespace WeighBridge.Service.PushSync;

/// <summary>
/// Resolves the BridgeOne SQLite database path from PushSync settings or BridgeOne.config.json.
/// </summary>
public sealed class BridgeOneDatabasePathResolver
{
    private readonly PushSyncSettings _settings;
    private readonly ILogger<BridgeOneDatabasePathResolver> _logger;
    private readonly string _contentRoot;

    public BridgeOneDatabasePathResolver(
        IOptions<PushSyncSettings> settings,
        IHostEnvironment hostEnvironment,
        ILogger<BridgeOneDatabasePathResolver> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _contentRoot = hostEnvironment.ContentRootPath;
    }

    public string Resolve()
    {
        if (!string.IsNullOrWhiteSpace(_settings.SqlitePath))
        {
            var configuredPath = Path.GetFullPath(_settings.SqlitePath);
            _logger.LogInformation("Using PushSync:SqlitePath {Path}", configuredPath);
            return configuredPath;
        }

        var configPath = ResolveBridgeOneConfigPath();
        if (configPath is null)
        {
            throw new InvalidOperationException(
                "Push sync SQLite path not configured. Set PushSync:SqlitePath or PushSync:BridgeOneConfigPath.");
        }

        var folderPath = ReadDatabaseFolderPath(configPath);
        var dbPath = Path.Combine(folderPath, "bridgeone.db");
        _logger.LogInformation("Using BridgeOne database from config {ConfigPath}: {DbPath}", configPath, dbPath);
        return dbPath;
    }

    private string? ResolveBridgeOneConfigPath()
    {
        if (!string.IsNullOrWhiteSpace(_settings.BridgeOneConfigPath))
        {
            var explicitPath = Path.GetFullPath(_settings.BridgeOneConfigPath);
            if (File.Exists(explicitPath))
                return explicitPath;

            throw new InvalidOperationException($"BridgeOne.config.json not found at {explicitPath}");
        }

        var candidates = new[]
        {
            Path.Combine(_contentRoot, "BridgeOne.config.json"),
            Path.Combine(_contentRoot, "..", "WeighBridge.App", "BridgeOne.config.json"),
            Path.Combine(_contentRoot, "..", "WeighBridge.App", "bin", "Debug", "net10.0-windows", "BridgeOne.config.json"),
            Path.Combine(_contentRoot, "..", "WeighBridge.App", "bin", "Release", "net10.0-windows", "BridgeOne.config.json")
        };

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
                return fullPath;
        }

        return null;
    }

    private static string ReadDatabaseFolderPath(string configPath)
    {
        var json = File.ReadAllText(configPath);
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.TryGetProperty("DatabaseFolderPath", out var folderElement))
        {
            var folderPath = folderElement.GetString()?.Trim();
            if (!string.IsNullOrWhiteSpace(folderPath))
                return Path.GetFullPath(folderPath);
        }

        throw new InvalidOperationException(
            $"DatabaseFolderPath is missing in BridgeOne.config.json ({configPath}).");
    }
}

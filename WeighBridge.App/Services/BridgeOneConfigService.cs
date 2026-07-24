using System.IO;
using System.Text.Json;

namespace WeightBridgeApp.Services;

public static class BridgeOneConfigService
{
    private sealed class BridgeOneConfig
    {
        public string DatabaseFolderPath { get; set; } = string.Empty;
    }

    public static string ConfigFilePath => System.IO.Path.Combine(AppContext.BaseDirectory, "BridgeOne.config.json");

    public static string DefaultDatabaseFolderPath => System.IO.Path.Combine(AppContext.BaseDirectory, "Database");

    public static string GetDatabaseFolderPath()
    {
        try
        {
            if (System.IO.File.Exists(ConfigFilePath))
            {
                var json = System.IO.File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<BridgeOneConfig>(json);
                var configuredPath = config?.DatabaseFolderPath?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(configuredPath))
                {
                    var normalizedConfiguredPath = System.IO.Path.GetFullPath(configuredPath);
                    System.IO.Directory.CreateDirectory(normalizedConfiguredPath);
                    return normalizedConfiguredPath;
                }
            }

            // First run: use the application folder, not a hardcoded machine path.
            // Example: <BridgeOne.exe folder>\Database\bridgeone.db
            SaveDatabaseFolderPath(DefaultDatabaseFolderPath);
            return System.IO.Path.GetFullPath(DefaultDatabaseFolderPath);
        }
        catch
        {
            // Last safe fallback for read-only/corrupt config scenarios.
            return System.IO.Path.GetFullPath(DefaultDatabaseFolderPath);
        }
    }

    public static void SaveDatabaseFolderPath(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new InvalidOperationException("Database path is not defined. Please define database path in Settings.");

        var normalizedPath = System.IO.Path.GetFullPath(folderPath.Trim());
        System.IO.Directory.CreateDirectory(normalizedPath);

        var config = new BridgeOneConfig { DatabaseFolderPath = normalizedPath };
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(ConfigFilePath, json);
    }

    public static bool IsDatabaseFolderConfigured() => !string.IsNullOrWhiteSpace(GetDatabaseFolderPath());

    public static string GetDatabaseFilePath()
    {
        var folderPath = GetDatabaseFolderPath();
        System.IO.Directory.CreateDirectory(folderPath);
        return System.IO.Path.Combine(folderPath, "bridgeone.db");
    }

    public static string GetDatabaseFilePathForFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new InvalidOperationException("Database path is not defined. Please define database path in Settings.");

        var normalizedPath = System.IO.Path.GetFullPath(folderPath.Trim());
        System.IO.Directory.CreateDirectory(normalizedPath);
        return System.IO.Path.Combine(normalizedPath, "bridgeone.db");
    }
}

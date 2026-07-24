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

    public static string GetDatabaseFolderPath()
    {
        try
        {
            if (!System.IO.File.Exists(ConfigFilePath))
                return string.Empty;

            var json = System.IO.File.ReadAllText(ConfigFilePath);
            var config = JsonSerializer.Deserialize<BridgeOneConfig>(json);
            return config?.DatabaseFolderPath?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
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
        if (string.IsNullOrWhiteSpace(folderPath))
            return GetStartupDatabaseFilePath();

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

    public static string GetStartupDatabaseFilePath()
    {
        var fallbackFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BridgeOne", "StartupDatabase");
        System.IO.Directory.CreateDirectory(fallbackFolder);
        return System.IO.Path.Combine(fallbackFolder, "bridgeone.db");
    }
}

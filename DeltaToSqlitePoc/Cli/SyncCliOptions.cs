using CommandLine;

namespace DeltaToSqlitePoc.Cli;

/// <summary>
/// Command-line arguments for the PoC.
/// Examples:
///   dotnet run -- --mode full
///   dotnet run -- --mode incremental --table Vendor --path d365/tables/mserp_vendvendoraientity
///   dotnet run -- --mode full --demo
/// </summary>
public sealed class SyncCliOptions
{
    [Option("mode", Required = true, HelpText = "Sync mode: full | incremental")]
    public string Mode { get; set; } = "full";

    [Option("table", Required = false, HelpText = "Entity / SQLite table name (default: Vendor)")]
    public string? Table { get; set; }

    [Option("path", Required = false, HelpText = "Delta table path inside the container")]
    public string? Path { get; set; }

    [Option("sqlite", Required = false, HelpText = "Override SQLite database file path")]
    public string? SqlitePath { get; set; }

    [Option("account", Required = false, HelpText = "Override Azure Storage account name")]
    public string? StorageAccountName { get; set; }

    [Option("container", Required = false, HelpText = "Override container name")]
    public string? ContainerName { get; set; }

    [Option("demo", Required = false, Default = false, HelpText = "Run against generated local demo Delta data (no Azure required)")]
    public bool Demo { get; set; }

    public bool IsFull => string.Equals(Mode, "full", StringComparison.OrdinalIgnoreCase);
    public bool IsIncremental => string.Equals(Mode, "incremental", StringComparison.OrdinalIgnoreCase);
}

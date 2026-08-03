namespace DeltaToSqlitePoc.Configuration;

/// <summary>
/// Bound from the "Sync" section of appsettings.json (and overrides).
/// Designed so additional entity mappings can be added later without changing core services.
/// </summary>
public sealed class SyncSettings
{
    public const string SectionName = "Sync";

    /// <summary>Azure Storage account name (e.g. mystorageaccount).</summary>
    public string StorageAccountName { get; set; } = string.Empty;

    /// <summary>ADLS Gen2 / Blob container name.</summary>
    public string ContainerName { get; set; } = string.Empty;

    /// <summary>Delta table root path inside the container (e.g. d365/tables/mserp_vendvendoraientity).</summary>
    //public string DeltaTablePath { get; set; } = "d365/tables/mserp_vendvendoraientity";
    //public string DeltaTablePath { get; set; } = "d365/tables/mserp_mk_wb_ecoresreleasedproductv2entity_partitioned";
    //public string DeltaTablePath { get; set; } = "d365/tables/mserp_mk_wbcustomermaster";
    public string DeltaTablePath { get; set; } = "d365/tables/mserp_mk_wbvendormaster";
    //public string deltatablepath { get; set; } = "d365/tables/mserp_mk_wbvendormaster";
    //public string deltatablepath { get; set; } = "d365/tables/mserp_mk_wbwarehousemaster";

    /// <summary>Logical entity / SQLite table name.</summary>
    public string TableName { get; set; } = "Vendor";
    //public string TableName { get; set; } = "ReleasedProducts";
    //public string TableName { get; set; } = "CustomerMaster";
    //public string TableName { get; set; } = "VendorMaster";
    //public string TableName { get; set; } = "WarehouseMaster";

    /// <summary>Relative or absolute path to the SQLite database file.</summary>
    public string SqlitePath { get; set; } = "app_data.db";

    /// <summary>Optional blob endpoint override (defaults to https://{account}.blob.core.windows.net).</summary>
    public string? BlobServiceUri { get; set; }

    /// <summary>
    /// Optional storage connection string (preferred for local PoC when Azure CLI / MI is unavailable).
    /// Store via user-secrets — do not commit real keys. When set, AAD credentials are skipped.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>Batch size for SQLite inserts/upserts.</summary>
    public int BatchSize { get; set; } = 500;

    /// <summary>Polly retry count for transient Azure failures.</summary>
    public int AzureRetryCount { get; set; } = 5;
}

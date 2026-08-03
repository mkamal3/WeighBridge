# DeltaToSqlitePoc

.NET 10 console PoC that syncs the **Vendor** Delta Lake entity (Synapse Link / `mserp_vendvendoraientity`) from Azure Data Lake Storage Gen2 into a local SQLite database.

Supports **full** and **incremental** sync, dynamic schema from Parquet, `IsDelete` handling, sync watermarks, Serilog logging, Polly retries, and a **local demo mode**.

## Entity: Vendor

Aligned with Synapse Link metadata:

| Item | Value |
|------|--------|
| Default SQLite table | `Vendor` |
| Default Delta path | `d365/tables/mserp_vendvendoraientity` |
| Primary key | `Id` |
| Partition column | `PartitionId` |
| Soft delete | `IsDelete` (boolean) |
| Watermark timestamps | `SinkModifiedOn` → `CreatedOn` → `SinkCreatedOn` |

All Parquet columns (string / long / decimal / boolean / timestamp) are upserted into SQLite. New source columns are added automatically via `ALTER TABLE`.

## Features

| Feature | Behavior |
|--------|----------|
| Full sync | Drops/recreates `Vendor`; inserts rows where `IsDelete` is false |
| Incremental sync | Upserts by `Id`; hard-deletes rows with `IsDelete=true` |
| Schema evolution | Creates table if missing; adds missing Parquet columns |
| State | `sync_state` stores last Delta version + last modified watermark |
| Auth | `DefaultAzureCredential` (all ADLS reads: log + checkpoint + data files) or a connection string |
| Delta reading | Pure C#/Parquet.Net checkpoint-aware reader: latest `_delta_log` checkpoint + trailing JSON commits |
| Retries | Polly exponential backoff for transient Azure errors |

## Build

```bash
dotnet restore DeltaToSqlitePoc/DeltaToSqlitePoc.csproj
dotnet build DeltaToSqlitePoc/DeltaToSqlitePoc.csproj
```

## Configure

### Sample `appsettings.json`

```json
{
  "Sync": {
    "StorageAccountName": "YOUR_STORAGE_ACCOUNT",
    "ContainerName": "YOUR_CONTAINER",
    "DeltaTablePath": "d365/tables/mserp_vendvendoraientity",
    "TableName": "Vendor",
    "SqlitePath": "app_data.db",
    "BatchSize": 500,
    "AzureRetryCount": 5
  }
}
```

> Set `DeltaTablePath` to the **exact** folder that contains `_delta_log` for your Vendor table (Synapse Link paths vary by environment).

### User secrets

```bash
cd DeltaToSqlitePoc
dotnet user-secrets set "Sync:StorageAccountName" "mystorageaccount"
dotnet user-secrets set "Sync:ContainerName" "synapse-link"
dotnet user-secrets set "Sync:DeltaTablePath" "d365/tables/mserp_vendvendoraientity"
```

### Azure credentials

```bash
az login
```

Grant the identity **Storage Blob Data Reader** on the container/account.

## Run

### Local demo (no Azure)

```bash
dotnet run --project DeltaToSqlitePoc -- --mode full --demo
dotnet run --project DeltaToSqlitePoc -- --mode incremental --demo
```

Demo incremental updates `V-002`, inserts `V-004`, and deletes `V-003` (`IsDelete=true`).

### Against ADLS Gen2

```bash
dotnet run --project DeltaToSqlitePoc -- --mode full
dotnet run --project DeltaToSqlitePoc -- --mode incremental
dotnet run --project DeltaToSqlitePoc -- --mode full --table Vendor --path d365/tables/mserp_vendvendoraientity
```

## Test instructions

1. **Demo full sync** — expect 3 vendors (`V-001`…`V-003`) at Delta version `0`.
2. **Demo incremental** — expect upsert of 2 rows, delete of `V-003`, watermark `1`.
3. **Azure** — point `DeltaTablePath` at your real Vendor Delta root, run full then incremental.

## SQLite

Default: `app_data.db` next to the executable.

```sql
SELECT Id, mserp_vendoraccountnumber, mserp_vendororganizationname, mserp_primaryemailaddress, IsDelete
FROM Vendor;

SELECT * FROM sync_state;
```

## Notes

- Partition folders such as `PartitionId=.../` are supported; Delta `add.path` values are used as-is.
- Full schema column list lives in `Models/VendorSchema.cs` (from your metaData `schemaString`).
- Extending to another entity: reuse the same pipeline with a new schema helper + table name/path.

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
| Delta log reading | Pure C#/Parquet.Net checkpoint-aware reader resolves the active file set from the latest checkpoint + trailing JSON commits — no native dependency |
| Retries | Polly exponential backoff for transient Azure errors |

### Delta log reading

Resolving *which Parquet files are currently active* is handled by
`AdlsDeltaTableReader.ReadSnapshotAsync`, which is checkpoint-aware:

1. List `_delta_log/` and read `_last_checkpoint` (falling back to scanning file names for the
   highest `*.checkpoint*.parquet` version if `_last_checkpoint` is missing).
2. If a checkpoint exists, parse its Parquet part(s) via `Delta/DeltaCheckpointReader.cs`
   (Parquet.Net) to get the base active-file set (and schema, from its `metaData` row).
3. Replay only the trailing `.json` commits *after* the checkpoint version on top of that base
   set, via `Delta/DeltaLogParser.cs`.

This matters because Synapse Link Delta tables checkpoint and prune `_delta_log` JSON commits
over time — reading JSON commits alone (this PoC's original bug) misses everything the
checkpoint already consolidated, which is why full sync could resolve to "0 active files" once
the raw commit history had been pruned past a checkpoint. An earlier iteration tried delegating
this to [DeltaLake.Net](https://github.com/delta-incubator/delta-dotnet) (delta-rs FFI), but its
file-listing calls (`FilesAsync`/`FileUrisAsync`) crashed with native marshalling errors
(`DecoderFallbackException`, heap corruption) against this table — so log resolution stays pure
C#/Parquet.Net instead, with no native dependency.

Since Synapse Link doesn't emit a Delta Change Feed, incremental sync doesn't try to diff
file lists between Delta versions (that has the same retention fragility). Instead it:
1. Uses the Delta version only as a cheap "did anything change" gate.
2. If changed, rescans the *current* active file set and filters rows using the sink's
   `SinkModifiedOn` watermark (`sync_state.LastUpdatedAt`), handling `IsDelete` as a hard delete.

The local demo mode reuses the same `Delta/DeltaLogParser.cs` JSON replay directly (no
checkpoint seed needed) since its fixture data has no checkpoints/retention to worry about.

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

Everything — `_delta_log` listing, checkpoint Parquet, JSON commits, and data Parquet files —
goes through the same `Azure.Storage.Blobs` client (via `DefaultAzureCredential`), so `az login`
covers all of it. If `Sync:ConnectionString` is set instead, that's used and AAD is skipped
entirely.

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

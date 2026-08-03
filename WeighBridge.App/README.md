# BridgeOne

BridgeOne is a WPF desktop weighbridge application using:

- WPF UI
- C# / .NET 10 Windows
- SQLite local database
- Serial Port communication
- TCP/IP communication
- Mock device mode for testing without hardware

## Initial Setup Login

Login is controlled from **Operator Master**.

On first run, BridgeOne creates `ApplicationFolder\Database\bridgeone.db` and seeds an administrator operator for initial access:

```text
Username: admin
Password: admin123
```

The seeded administrator is saved in the database as an operator record. Change the password after first login.

## Latest Update - Operator-Based Security

User Management has been removed from the UI.

All login, screen access, and transaction permissions are now controlled from:

```text
Masters > Operator Master
```

Operator Master now controls:

- Username
- Password / Confirm Password
- Operator Name
- Legal Entity / Default Legal Entity
- Screen access:
  - Can Access Weighment
  - Can Access Masters
  - Can Access Reports
  - Can Access Transactions
  - Can Access Settings
- Transaction permissions:
  - Can Capture First Weight
  - Can Capture Second Weight
  - Can Perform Manual Weight Entry
  - Can Correct Transactions
  - Can Cancel Transactions
- Status

## Transaction Rules

- `First Weight By` stores the current operator username.
- `Second Weight By` stores the current operator username.
- Slip output displays operator as:

```text
Operator Name (username)
```

## Completed Transaction Permissions

Old permissions were replaced:

```text
Can Edit Completed Transaction   -> Can Correct Transactions
Can Delete Completed Transaction -> Can Cancel Transactions
```

Transactions are corrected and cancelled from the Transactions screen when the operator has the required permissions.

## Weighbridge Settings

Settings tab is placed at the end of the main tab sequence.

Settings contains:

- Database Folder Path
- Browse button
- Selected Weighbridge lookup
- Read-only communication settings copied from Weighbridge Master

Live weight connection uses the selected Weighbridge Master communication setup.

## Database Path

Database Folder Path is maintained in Settings only.

BridgeOne stores the selected folder in:

```text
BridgeOne.config.json
```

The SQLite database file is created/used as:

```text
Selected Folder\bridgeone.db
```

## Device Communication Types

Weighbridge Master supports:

- Mock
- TCP/IP
- Serial
- USB
- OPC
- API

Use **Mock** for testing without physical hardware.

## Weighment Process

1. Login as an operator.
2. Select/configure the weighbridge in Settings.
3. Go to Weighment.
4. Click Connect.
5. Enter/select Vehicle, Driver, Party, and Item.
6. Save First Weight.
7. Load the open ticket when the vehicle returns.
8. Save Second Weight.
9. Net Weight is calculated automatically.

## Reports and Slip

- Reports use field-wise filters.
- `Apply` loads filtered data.
- `Clear` resets filters.
- `Export CSV` exports report data.
- `Print Slip` prints the selected ticket.

## Notes

If old columns or old security behavior appears, delete the old `bridgeone.db` from the configured database folder and run BridgeOne again.


## Database path behavior

- On first run, BridgeOne creates the database in the application folder: `ApplicationFolder\Database\bridgeone.db`.
- The selected database folder path is stored in `BridgeOne.config.json`.
- Settings includes `Database Folder Path` with a Browse option.
- If the selected folder already contains `bridgeone.db`, BridgeOne uses that database after restart.
- If the selected folder does not contain `bridgeone.db`, BridgeOne creates a new database with seed data.
- New databases include default login: `admin` / `admin123`. Change this password after first login.

### Latest Update - Mandatory Weighment Fields

- Vehicle No, Driver Name, Party Type, Party, and Item are mandatory on the Weighment Entry screen.
- Mandatory fields are marked with `*` only on the entry form labels.
- Open Tickets grid now includes Driver column.

### Latest Update - Transactions Screen

- Added a new **Transactions** screen before Settings.
- Transaction filters follow the same layout style as the Report screen.
- Added **Correct Transaction** and **Cancel Transaction** buttons on the Transactions screen.
- Completed Today is now view-only; correction/cancellation is handled only from Transactions.
- Correct Transaction opens a separate form with transaction data and updates the same transaction after Save.
- Correction is allowed for both Open and Completed transactions, except Cancelled transactions.
- Cancel Transaction can cancel both Open and Completed transactions and changes status to Cancelled.


### Latest Update - Operator Transaction Access and Status Logic

- Added `Can Access Transactions` in Operator Master screen access.
- The Transactions tab is controlled by `Can Access Transactions`.
- Removed `Can Override Weight`, `Can Approve QC`, and `Can Retry Integration` from Operator Master.
- Removed Active checkbox/column from Vehicle, Driver, Weighbridge, and Operator masters.
- Availability/login logic now uses Status fields:
  - Vehicle/Driver Status = Active -> available in weighment lookups.
  - Weighbridge Operating Status = Active -> available in Settings lookup.
  - Operator Status = Active -> can login.

## Latest DataAreaId / Legal Entity Update
- Added `DataAreaId` to all master models and master database tables.
- Frontend label is shown as **Legal Entity**.
- Operator Master now uses one Legal Entity field only; duplicate Default Legal Entity field is removed from the UI.
- Existing `LegalEntity`/`DefaultLegalEntity` columns are migrated into `DataAreaId` for backward compatibility with old SQLite databases.
- Master lookups are filtered by the current operator's `DataAreaId`/Legal Entity.
- Weighment transactions also store `DataAreaId` together with Company/Legal Entity for reporting and identification.

## Latest Change - Backend D365 Sync Fields

Added backend-only D365/Dataverse sync fields to the following masters. These fields are stored in SQLite and model/database layer only; they are not shown on the WPF UI.

### Customer Master
- mserp_mk_wbcustomermasterId
- SinkCreatedOn
- SinkModifiedOn
- mserp_dataareaid_id
- mserp_dataareaid_id_entitytype
- mserp_dataareaid
- versionnumber
- IsDelete
- CreatedOn
- createdonpartition

### Vendor Master
- mserp_mk_wbvendormasterId
- SinkCreatedOn
- SinkModifiedOn
- mserp_dataareaid_id
- mserp_dataareaid_id_entitytype
- mserp_dataareaid
- versionnumber
- IsDelete
- CreatedOn
- createdonpartition

### Item Master
- mserp_mk_wb_ecoresreleasedproductv2entityId
- SinkCreatedOn
- SinkModifiedOn
- mserp_dataareaid_id
- mserp_dataareaid_id_entitytype
- mserp_dataareaid
- versionnumber
- IsDelete
- CreatedOn
- createdonpartition

### Warehouse Master
- Id
- mserp_mk_wbwarehousemasterId
- SinkCreatedOn
- SinkModifiedOn
- mserp_dataareaid_id
- mserp_dataareaid_id_entitytype
- mserp_dataareaid
- versionnumber
- IsDelete
- CreatedOn
- createdonpartition

All listed backend-only sync fields are stored as TEXT in SQLite.

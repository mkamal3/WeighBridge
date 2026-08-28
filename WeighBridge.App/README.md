# BridgeOne - Dynamic Transaction Forms Update

This package contains the latest BridgeOne WPF + SQLite source with dynamic weighment detail sections controlled by **Transaction Type Master > Form**.

## Dynamic forms included
- Purchase / Receipt / Collection
- Contract Collection
- Transfer Form
- Sales / Dispatch
- Production Weighing
- Return
- Disposal / Waste Movement

## Latest additions
- Separate SQLite detail tables and models for Transfer, Sales/Dispatch, Production, Return, and Disposal/Waste Movement.
- Detail records save against the same WeighmentId/Slip Number and reload with an open ticket.
- Form-specific mandatory validation is applied before First Weight.
- Master-based lookups are used where required for legal entities, locations, customers, vendors, reasons, and operators.
- **Production Line remains a manual text field. No Production Line Master is included.**

## Transaction Type Master mapping
The dynamic section shown on Weighment is determined by the exact Form value selected on the Transaction Type Master.

## Unit of Measure Master
- Added a new **Unit of Measure Master** under Masters.
- The master is read-only on the BridgeOne UI and is designed for synchronized source data.
- Frontend fields: `symbol`, `isbaseunit`, `issystemunit`, `systemofunits`, `unitofmeasureclass`, `sysdatastatecode`, `decimalprecision`.
- Backend synchronization fields are stored in SQLite but hidden from the UI: `Id`, `SinkCreatedOn`, `SinkModifiedOn`, `modifieddatetime`, `modifiedby`, `modifiedtransactionid`, `createddatetime`, `createdby`, `createdtransactionid`, `dataareaid`, `recversion`, `partition`, `sysrowversion`, `recid`, `tableid`, `versionnumber`, `createdon`, `modifiedon`, `IsDelete`, `PartitionId`.
- Added filtering and paging for Symbol, System of Units, Unit of Measure Class, and Sys Data State Code.

## Demo UOM seed data
- Unit of Measure Master seeds: kg, g, ton, lb, pcs.
- ITEM-0001 is seeded with Product Number PROD-0001 and kg for Purchase, Sales, BOM and Cost units.
- Product unit conversion demo records for PROD-0001: kg↔g and kg↔ton.
- Seed statements are idempotent and only fill blank unit/product fields on the existing demo item.

## Correction Workflow Update (2026-08-25)
- Added controlled Transaction Correction window for Completed transactions.
- Workflow: Draft -> Submitted -> Approved / Rejected.
- Added correction permissions: access, submit, approve/reject, and correct weight.
- Added header value comparison and material-line actions: Add, Modify, Replace, Remove.
- Approved corrections update the active transaction while retaining original/corrected values in correction history.
- Added transaction correction metadata: IsCorrected, CorrectionVersion, LastCorrectionNumber, LastCorrectedDateTime, LastCorrectedBy.
- Material-line removal is soft (IsActive = 0); original rows are never physically deleted by correction approval.
- Corrected slips display correction/version information.
- Direct editing of Completed transactions is blocked; users must use the Correction workflow.

## Correction approval permission
Correction **Approve** and **Reject** actions are controlled by Operator Master -> **Can Approve / Reject Correction**. There is no additional maker-checker restriction; a user with this permission can approve or reject any Submitted correction request.

## Consolidated UI and correction controls (2026-08-25)
- Correction Header no longer exposes Primary Item Number or Primary Item Name; item changes are handled only through Material Line Correction.
- Correction reason is sourced from the generic Reason Master lookup.
- After a correction is Submitted, header values and material-line controls become read-only; only permitted Approve/Reject actions remain available.
- Correction Approve and Reject use the same green/red workflow styling as Cancellation / Void.
- Cancellation / Void and Correction screens now show the request list on the left and request details on the right.
- Submit/Approve/Reject buttons use a readable disabled state so labels remain visible when actions are unavailable.


## Reason Master and Transaction Review (2026-08-26)
- Reason Master now contains only **Code** and **Description**.
- Cancellation / Void and Correction use the same Reason Master lookup and store the selected Reason Code.
- Existing legacy category-based reasons and historical reason references are migrated to the generic Code/Description model.
- Transactions screen now uses a two-pane inquiry layout: transaction grid on the left and read-only transaction details on the right.
- Selecting a transaction loads common transaction information, material lines, correction/cancellation metadata, and transaction-specific dynamic fields based on Transaction Type Master -> Form mapping.
- Transaction review supports Purchase, Contract Collection, Transfer, Sales / Dispatch, Production Weighing, Return, and Disposal / Waste Movement forms.
## 2026-08-26 - Material line duplicate item support
- Material Lines now allow the same Item Number to be selected on multiple lines.
- Each line remains independent by Line Number / MaterialLineId, including UOM, Expected Qty and Remarks.
- Correction processing continues to identify material lines by OriginalMaterialLineId / LineNo rather than by Item Number.


## 2026-08-27 - Schema and model cleanup
- Standardized Operator, Vehicle and Driver company ownership on `DataAreaId`; removed the duplicate `LegalEntity` field.
- Removed `DefaultLegalEntity` from Operator Master. Operator company assignments/default selection continue through `OperatorLegalEntities`.
- Removed Driver legacy fields `CNIC`, `MobileNo` and `LicenseNo`; current fields are `IdentificationNumber`, `MobileNumber` and `DrivingLicenceNumber`.
- Removed legacy `IsActive` fields from Operator, Vehicle, Driver and Weighbridge masters. `Status` / `OperatingStatus` is the single source of truth.
- Removed obsolete Operator permission/compatibility fields superseded by the Correction and Cancellation/Void workflow permissions.
- Removed obsolete standalone `Users`, `Materials` and `Parties` SQLite tables and their old model/CRUD infrastructure. Customer/Vendor party lookup still uses the lightweight `Party` result model; material selection uses Item Master.
- Existing databases are migrated before obsolete columns/tables are removed, preserving legacy company values, driver identification/contact/licence values, status and workflow permissions.
- `VehicleNo` is intentionally retained as a compatibility alias for `PlateNumber` because existing weighment/lookup code still uses it.

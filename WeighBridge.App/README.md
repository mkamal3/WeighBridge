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
- General Weighing Service

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
- Transaction review supports Purchase, Contract Collection, Transfer, Sales / Dispatch, Production Weighing, Return, Disposal / Waste Movement, and General Weighing Service forms.
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
- The project now uses the clean target schema directly; no `MigrateAndRemoveLegacySchema` compatibility routine is included because the application is not live.
- `VehicleNo` is intentionally retained as a compatibility alias for `PlateNumber` because existing weighment/lookup code still uses it.


## 2026-08-31 - General Weighing Service
- Added **General Weighing Service** as a Transaction Type Master form with its own `WeighmentGeneralWeighingServiceDetails` table/model.
- Dynamic fields: External Party Name, optional Customer lookup, Mobile Number, Material Description, Service Mode, Service Charge, Currency, Payment Status and conditional Receipt Number.
- General Weighing Service does not require Item Master / Material Lines; Material Description is entered manually.
- Service Mode is sourced from Service Charge Master for the current Legal Entity. Service Charge and Currency are automatically fetched and read-only.
- Service Charge Master uses **Legal Entity + Service Mode** as the unique business key; `Validity` is a Date field. Demo setup seeds `Single Weight` and `Two Weight` once for DAT.
- **Single Weight:** Second Weight is stored as 0, Net Weight equals First Weight, and the transaction completes at W1.
- **Two Weight:** standard first/second weighing applies and Net Weight is the absolute difference.
- Payment Status is Paid/Unpaid. Receipt Number is mandatory at completion when Payment Status is Paid.
- The Transactions two-pane review includes all General Weighing Service dynamic fields.


## 2026-08-31 General Weighing Service UI follow-up
- Mobile Number remains optional and no format/validity validation is applied.
- Material Lines stay visible for General Weighing Service and remain optional.
- General Weighing Service detail pane uses a compact non-scrolling layout so Payment Status and Receipt Number are visible without an internal scrollbar.

## 2026-08-31 - Return layout and Legal Entity consolidation
- Return dynamic details no longer use an internal vertical scrollbar; the pane expands with a compact layout so all Return fields remain directly visible.
- Transaction review Common Transaction Details no longer show header-level Item Number or Item Name; item information remains in Material Lines.
- `DataAreaId` is now the single Legal Entity/company field for weighments. The duplicate `CompanyName` field/column and related logic were removed.
- Reports, transaction filters, correction review, printed slips and CSV export now use **Legal Entity / DataAreaId** instead of a separate Company value.

## 2026-09-01 - Open Transactions Inquiry
- Added a separate **Open Transactions Inquiry** tab with Operator Master screen access permission.
- Added permissions: **Can Access Open Transactions Inquiry**, **Can Resume Open Transactions**, and **Can Export Open Transactions**.
- Inquiry uses a two-pane layout: open transaction grid on the left and the existing read-only common/dynamic/material-line review on the right.
- Grid includes Current Stage, Open Age, stale indicator, Weighbridge, W1, In Use By, Last Updated By and Last Updated Date/Time.
- Filters include optional From/To date, Slip Number, Transaction Type, Scenario, Vehicle, Weighbridge and Stage.
- Open transaction count is shown in the inquiry header; the inquiry auto-refreshes every 60 seconds while the tab is active.
- **Resume** acquires a 30-minute operator lock, records a Resume audit event, loads the same transaction into the Weighment screen and continues from its existing stage. The existing Open Slips loader also observes the same lock.
- Clearing the resumed Weighment releases the current operator's lock; completion clears the lock automatically.
- Added `WeighmentTransactionHistory` for resume audit events and lock/last-update fields on `Weighments`.
- Excel export creates an `.xlsx` workbook without an external Excel library. Sheet 1 contains common fields plus transaction-type-specific dynamic fields; Sheet 2 contains Material Lines keyed by Slip Number.

## 2026-09-01 - Open Transactions Excel sheet structure
- Open Transactions Excel export now separates data by purpose instead of combining all dynamic fields in the Header sheet.
- **Header** sheet contains common weighment fields only.
- **Lines** sheet contains Material Lines keyed by Slip Number.
- Every configured Transaction Type gets its own worksheet for its mapped dynamic fields. Slip Number, Legal Entity and Transaction Type are included as linking columns.
- Transaction Type worksheet names are automatically made Excel-safe (for example `/` is replaced) and limited to Excel's 31-character worksheet-name limit.
- Empty configured transaction types still receive a worksheet with the correct dynamic-field column headers.

## 2026-09-01 - Transactions Export / Open Inquiry Resume Placement
- Added Export Excel to the Transactions inquiry screen.
- The Transactions export respects the currently filtered transaction rows.
- Workbook structure: Header, Lines, and one separate dynamic sheet per Transaction Type.
- Dynamic sheets use Slip Number / Legal Entity / Transaction Type as linking fields.
- Removed the duplicate Resume action from the left side of Open Transactions Inquiry.
- Resume remains available in the right-side Open Transaction Review pane and keeps the existing permission, lock and audit behavior.

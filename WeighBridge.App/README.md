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

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

# BridgeOne Latest Changes

- Added Open Transactions Inquiry with operator-level access permissions.
- Resume is available in the right-side review pane and opens the selected slip on the Weighment screen.
- Removed the duplicate Resume action from the left-side transaction list.
- Added Excel export to both Transactions and Open Transactions Inquiry.
- Excel workbooks contain Header and Lines sheets plus a separate sheet for each transaction type.
- Added configuration-driven dynamic details for Contract Collection, Transfer, Sales/Dispatch, Production Weighing, Return, Disposal/Waste Movement, and General Weighing Service.
- Vehicle and Driver masters and lookups are restricted by Legal Entity.
- First-weight and second-weight operator information is retained for transaction and slip output.
- Replaced the generic Open lifecycle with Draft, Pending Second Weight, Awaiting Confirmation, Awaiting QC, Completed, and Cancelled.
- Added an independent Integration Status field with Not Ready, Pending, Not Required, Integrated, Failed, and reversal-ready values.
- Weight capture now moves the transaction to Awaiting Confirmation instead of completing it.
- Added green Confirm buttons to the bottom-right read-only panes in Transactions and Open Transactions.
- Confirm completes non-QC transactions or hands QC-required transactions to Awaiting QC.
- QC approval completes the linked transaction; QC rejection leaves the transaction in Awaiting QC.
- Material lines now populate the selected item's UOM using the canonical UOM master symbol, including case-insensitive matching such as kg to KG.
- Vehicle and Driver masters are now global and available across all Legal Entities; lookups no longer filter by company and uniqueness is enforced globally.

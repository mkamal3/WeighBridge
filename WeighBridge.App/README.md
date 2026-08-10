# BridgeOne Update - Professional Lookup UI

This package includes the latest BridgeOne WPF source code with these updates:

## Weighment screen lookup design

- Reworked Vehicle, Driver, Party, and Item controls into a cleaner lookup layout.
- Removed large text-based `Lookup` buttons from the Weighment Entry screen.
- Added icon-only lookup actions with no visible button border, background, or button text.
- Vehicle and Driver are selected through lookup popup windows.
- Party and Item are selected through lookup popup windows.
- Party and Item display as a single merged value on the Weighment screen:
  - `PartyAccount - PartyName`
  - `ItemNumber - ItemName`
- Backend values are still maintained separately for reporting, slip printing, and integration:
  - `PartyAccount`
  - `PartyName`
  - `ItemNumber`
  - `ItemName`

## Lookup popup behavior

- Lookup filter fields are shown on top of each lookup form.
- No Search or Clear buttons are shown in the filter area.
- Filtering runs automatically while typing.
- Result grids return a limited number of rows for performance.
- Filters are cleared automatically when the lookup closes after selection or cancel.

## Grid cleanup retained

- Disabled empty new-row placeholder in DataGrids.
- Disabled grid row deletion.
- Disabled auto-generated backend/internal columns.
- Kept grid virtualization for performance.

## Performance behavior retained

- Large master data is not loaded directly into heavy dropdowns on the Weighment Entry screen.
- Lookup windows query SQLite directly and return a limited result set.
- Customer/Vendor/Item/Warehouse master pages remain paginated.

## Build note

The environment used to prepare this package does not include the .NET SDK, so the project was not compiled here.

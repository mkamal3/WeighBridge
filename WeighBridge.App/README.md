# BridgeOne - Gate Pass Update

This package includes the Gate Pass screen and operator access control update.

## Added
- Gate Pass screen with Type values: Inbound and Outbound.
- Gate Pass fields: Gate Pass Number, Type, Entry Date & Time, Vehicle Plate, Driver Mobile, Party Type, Party, Expected Transaction Type, Expected Item, Source, Destination, Security Officer, Exit Date Time, Status, Linked Ticket, Remarks.
- Gate Pass Number is generated locally from SQLite number sequence logic.
- After save, Gate Pass fields are read-only. Wrong gate passes should be cancelled and recreated.
- Actions: New, Save / Open Gate Pass, Close, Cancel, Print.
- Operator Master permission: Can Access Gate Pass.
- Gate Pass tab visibility is controlled by Can Access Gate Pass.
- Weighment screen Gate Pass No field can be linked from the selected open Gate Pass.
- First Weight save links the Gate Pass and prevents reusing the same open pass.

## Notes
- Gate Pass is local-only SQLite data.
- One open Gate Pass should link to only one Weighment transaction.
- Closed and cancelled Gate Passes are retained for audit; they are not deleted.

## Login binding fix
- Fixed WPF binding error by setting CurrentUserCompany display binding to OneWay.
- Legal Entity selection continues to use the settable SelectedLegalEntityDataAreaId property.

## Latest purchase behavior update
- FOC Flag checked: Rate / Amount is automatically set to 0 and disabled/greyed out.
- FOC Flag unchecked: Rate / Amount is editable before First Weight.
- Walk-in Vendor checked: Vendor is automatically set to WALK-IN - Walk-in Vendor and vendor lookup is disabled.
- Walk-in Vendor unchecked: Vendor can be selected from Vendor Master lookup before First Weight.

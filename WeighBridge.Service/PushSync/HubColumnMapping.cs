namespace WeighBridge.Service.PushSync;

public sealed record HubColumnMapping(
    string LocalColumn,
    string HubColumn,
    HubColumnType ColumnType = HubColumnType.String);

namespace WeighBridge.Service.PushSync;

/// <summary>Push sync config for the local Drivers table → Hub dbo.Drivers.</summary>
public sealed class DriverSyncConfig : ISyncableTableConfig
{
    public string EntityType => HubSyncEntityTypes.Driver;

    public string LocalTableName => "Drivers";

    public string HubTableName => "Drivers";

    public string BusinessKeyColumn => "DriverGuid";

    public IReadOnlyList<string> SelectColumns { get; } =
    [
        "DriverGuid",
        "DataAreaId",
        "DriverName",
        "MobileNumber",
        "SecondaryMobile",
        "Email",
        "Nationality",
        "DriverType",
        "EmployerPartyType",
        "EmployerAccount",
        "IdentificationType",
        "IdentificationNumber",
        "IdentificationExpiryDate",
        "EmiratesIdExpiryDate",
        "PassportNumber",
        "PassportExpiryDate",
        "DrivingLicenceNumber",
        "DrivingLicenceIssuedBy",
        "DrivingLicenceExpiryDate",
        "LicenceCategories",
        "DefaultVehicle",
        "Address",
        "DriverPhoto",
        "EmiratesIdAttachment",
        "PassportAttachment",
        "DrivingLicenceAttachment",
        "Status",
        "Blacklisted",
        "BlacklistReason",
        "EffectiveFrom",
        "Remarks",
        "LastModifiedUtc"
    ];

    public IReadOnlyList<HubColumnMapping> HubColumns { get; } =
    [
        new("DriverGuid", "DriverGuid", HubColumnType.Guid),
        new("DataAreaId", "DataAreaId"),
        new("DriverName", "DriverName"),
        new("MobileNumber", "MobileNumber"),
        new("SecondaryMobile", "SecondaryMobile"),
        new("Email", "Email"),
        new("Nationality", "Nationality"),
        new("DriverType", "DriverType"),
        new("EmployerPartyType", "EmployerPartyType"),
        new("EmployerAccount", "EmployerAccount"),
        new("IdentificationType", "IdentificationType"),
        new("IdentificationNumber", "IdentificationNumber"),
        new("IdentificationExpiryDate", "IdentificationExpiryDate"),
        new("EmiratesIdExpiryDate", "EmiratesIdExpiryDate"),
        new("PassportNumber", "PassportNumber"),
        new("PassportExpiryDate", "PassportExpiryDate"),
        new("DrivingLicenceNumber", "DrivingLicenceNumber"),
        new("DrivingLicenceIssuedBy", "DrivingLicenceIssuedBy"),
        new("DrivingLicenceExpiryDate", "DrivingLicenceExpiryDate"),
        new("LicenceCategories", "LicenceCategories"),
        new("DefaultVehicle", "DefaultVehicle"),
        new("Address", "Address"),
        new("DriverPhoto", "DriverPhoto"),
        new("EmiratesIdAttachment", "EmiratesIdAttachment"),
        new("PassportAttachment", "PassportAttachment"),
        new("DrivingLicenceAttachment", "DrivingLicenceAttachment"),
        new("Status", "Status"),
        new("Blacklisted", "Blacklisted", HubColumnType.Boolean),
        new("BlacklistReason", "BlacklistReason"),
        new("EffectiveFrom", "EffectiveFrom"),
        new("Remarks", "Remarks")
        //new("LastModifiedUtc", "SourceLastModifiedUtc", HubColumnType.DateTimeOffset)
    ];
}

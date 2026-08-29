-- Azure SQL Hub: central Drivers registry (one row per DriverGuid globally).
-- Run against the Hub database before enabling push sync.

IF OBJECT_ID(N'dbo.Drivers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Drivers
    (
        DriverGuid UNIQUEIDENTIFIER NOT NULL,
        DataAreaId NVARCHAR(10) NOT NULL,
        DriverName NVARCHAR(200) NOT NULL,
        MobileNumber NVARCHAR(50) NOT NULL CONSTRAINT DF_Drivers_MobileNumber DEFAULT (N''),
        SecondaryMobile NVARCHAR(50) NOT NULL CONSTRAINT DF_Drivers_SecondaryMobile DEFAULT (N''),
        Email NVARCHAR(200) NOT NULL CONSTRAINT DF_Drivers_Email DEFAULT (N''),
        Nationality NVARCHAR(100) NOT NULL CONSTRAINT DF_Drivers_Nationality DEFAULT (N''),
        DriverType NVARCHAR(100) NOT NULL CONSTRAINT DF_Drivers_DriverType DEFAULT (N''),
        EmployerPartyType NVARCHAR(100) NOT NULL CONSTRAINT DF_Drivers_EmployerPartyType DEFAULT (N''),
        EmployerAccount NVARCHAR(100) NOT NULL CONSTRAINT DF_Drivers_EmployerAccount DEFAULT (N''),
        IdentificationType NVARCHAR(100) NOT NULL CONSTRAINT DF_Drivers_IdentificationType DEFAULT (N''),
        IdentificationNumber NVARCHAR(100) NOT NULL CONSTRAINT DF_Drivers_IdentificationNumber DEFAULT (N''),
        IdentificationExpiryDate NVARCHAR(30) NULL,
        EmiratesIdExpiryDate NVARCHAR(30) NULL,
        PassportNumber NVARCHAR(100) NOT NULL CONSTRAINT DF_Drivers_PassportNumber DEFAULT (N''),
        PassportExpiryDate NVARCHAR(30) NULL,
        DrivingLicenceNumber NVARCHAR(100) NOT NULL CONSTRAINT DF_Drivers_DrivingLicenceNumber DEFAULT (N''),
        DrivingLicenceIssuedBy NVARCHAR(100) NOT NULL CONSTRAINT DF_Drivers_DrivingLicenceIssuedBy DEFAULT (N''),
        DrivingLicenceExpiryDate NVARCHAR(30) NULL,
        LicenceCategories NVARCHAR(200) NOT NULL CONSTRAINT DF_Drivers_LicenceCategories DEFAULT (N''),
        DefaultVehicle NVARCHAR(100) NOT NULL CONSTRAINT DF_Drivers_DefaultVehicle DEFAULT (N''),
        Address NVARCHAR(500) NOT NULL CONSTRAINT DF_Drivers_Address DEFAULT (N''),
        DriverPhoto NVARCHAR(500) NOT NULL CONSTRAINT DF_Drivers_DriverPhoto DEFAULT (N''),
        EmiratesIdAttachment NVARCHAR(500) NOT NULL CONSTRAINT DF_Drivers_EmiratesIdAttachment DEFAULT (N''),
        PassportAttachment NVARCHAR(500) NOT NULL CONSTRAINT DF_Drivers_PassportAttachment DEFAULT (N''),
        DrivingLicenceAttachment NVARCHAR(500) NOT NULL CONSTRAINT DF_Drivers_DrivingLicenceAttachment DEFAULT (N''),
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Drivers_Status DEFAULT (N'Active'),
        Blacklisted BIT NOT NULL CONSTRAINT DF_Drivers_Blacklisted DEFAULT (0),
        BlacklistReason NVARCHAR(500) NOT NULL CONSTRAINT DF_Drivers_BlacklistReason DEFAULT (N''),
        EffectiveFrom NVARCHAR(30) NULL,
        Remarks NVARCHAR(1000) NOT NULL CONSTRAINT DF_Drivers_Remarks DEFAULT (N''),
        StationId NVARCHAR(50) NOT NULL,
        SourceLastModifiedUtc DATETIME2(3) NOT NULL,
        HubReceivedUtc DATETIME2(3) NOT NULL CONSTRAINT DF_Drivers_HubReceivedUtc DEFAULT (SYSUTCDATETIME()),
        HubUpdatedUtc DATETIME2(3) NOT NULL CONSTRAINT DF_Drivers_HubUpdatedUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Drivers PRIMARY KEY CLUSTERED (DriverGuid)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Drivers_DataArea_DriverName' AND object_id = OBJECT_ID(N'dbo.Drivers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Drivers_DataArea_DriverName
        ON dbo.Drivers (DataAreaId, DriverName);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Drivers_StationId' AND object_id = OBJECT_ID(N'dbo.Drivers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Drivers_StationId
        ON dbo.Drivers (StationId);
END
GO

-- Stuck rows for manual review:
-- SELECT * FROM dbo.Drivers d
-- INNER JOIN (SELECT DriverGuid FROM ... local Failed with RetryCount >= max) ...

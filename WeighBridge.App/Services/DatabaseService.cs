using System.IO;
using Microsoft.Data.Sqlite;
using WeightBridgeApp.Models;

namespace WeightBridgeApp.Services;

public class DatabaseService
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    private const string WeighmentSelectSql = @"
SELECT w.*,
       CASE WHEN u1.UserId IS NULL THEN w.FirstWeightBy ELSE u1.FullName || ' (' || u1.Username || ')' END AS FirstWeightByDisplay,
       CASE WHEN u2.UserId IS NULL THEN w.SecondWeightBy ELSE u2.FullName || ' (' || u2.Username || ')' END AS SecondWeightByDisplay
FROM Weighments w
LEFT JOIN Users u1 ON lower(w.FirstWeightBy) = lower(u1.Username) OR w.FirstWeightBy = CAST(u1.UserId AS TEXT)
LEFT JOIN Users u2 ON lower(w.SecondWeightBy) = lower(u2.Username) OR w.SecondWeightBy = CAST(u2.UserId AS TEXT)";

    public string DatabaseFilePath => _dbPath;

    public DatabaseService()
    {
        _dbPath = BridgeOneConfigService.GetDatabaseFilePath();
        _connectionString = $"Data Source={_dbPath}";
    }

    public Task InitializeAsync() => Task.Run(() =>
    {
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_dbPath)!);

        using var connection = CreateConnection();
        connection.Open();

        ExecuteNonQuery(connection, @"
CREATE TABLE IF NOT EXISTS DeviceSettings (
    SettingId INTEGER PRIMARY KEY,
    SelectedWeighbridgeCode TEXT NOT NULL DEFAULT '',
    ConnectionType TEXT NOT NULL,
    ComPort TEXT NOT NULL,
    BaudRate INTEGER NOT NULL,
    Parity TEXT NOT NULL,
    DataBits INTEGER NOT NULL,
    StopBits TEXT NOT NULL,
    IpAddress TEXT NOT NULL,
    TcpPort INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Parties (
    PartyId INTEGER PRIMARY KEY AUTOINCREMENT,
    PartyName TEXT NOT NULL UNIQUE,
    PartyType TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Materials (
    MaterialId INTEGER PRIMARY KEY AUTOINCREMENT,
    MaterialName TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS Vehicles (
    VehicleId INTEGER PRIMARY KEY AUTOINCREMENT,
    VehicleNo TEXT NOT NULL UNIQUE,
    PlateNumber TEXT NOT NULL DEFAULT '',
    PlateEmirate TEXT NOT NULL DEFAULT '',
    PlateCategory TEXT NOT NULL DEFAULT '',
    VehicleType TEXT NOT NULL DEFAULT '',
    OwnershipType TEXT NOT NULL DEFAULT '',
    OwnerPartyAccount TEXT NOT NULL DEFAULT '',
    Transporter TEXT NOT NULL DEFAULT '',
    Capacity REAL NOT NULL DEFAULT 0,
    DefaultDriver TEXT NOT NULL DEFAULT '',
    RegistrationExpiryDate TEXT,
    LegalEntity TEXT NOT NULL DEFAULT '',
    Status TEXT NOT NULL DEFAULT 'Active',
    IsActive INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Drivers (
    DriverId INTEGER PRIMARY KEY AUTOINCREMENT,
    DriverName TEXT NOT NULL UNIQUE,
    MobileNumber TEXT NOT NULL DEFAULT '',
    SecondaryMobile TEXT NOT NULL DEFAULT '',
    Email TEXT NOT NULL DEFAULT '',
    Nationality TEXT NOT NULL DEFAULT '',
    DriverType TEXT NOT NULL DEFAULT '',
    EmployerPartyType TEXT NOT NULL DEFAULT '',
    EmployerAccount TEXT NOT NULL DEFAULT '',
    IdentificationType TEXT NOT NULL DEFAULT '',
    IdentificationNumber TEXT NOT NULL DEFAULT '',
    IdentificationExpiryDate TEXT,
    EmiratesIdExpiryDate TEXT,
    PassportNumber TEXT NOT NULL DEFAULT '',
    PassportExpiryDate TEXT,
    DrivingLicenceNumber TEXT NOT NULL DEFAULT '',
    DrivingLicenceIssuedBy TEXT NOT NULL DEFAULT '',
    DrivingLicenceExpiryDate TEXT,
    LicenceCategories TEXT NOT NULL DEFAULT '',
    DefaultVehicle TEXT NOT NULL DEFAULT '',
    Address TEXT NOT NULL DEFAULT '',
    DriverPhoto TEXT NOT NULL DEFAULT '',
    EmiratesIdAttachment TEXT NOT NULL DEFAULT '',
    PassportAttachment TEXT NOT NULL DEFAULT '',
    DrivingLicenceAttachment TEXT NOT NULL DEFAULT '',
    LegalEntity TEXT NOT NULL DEFAULT '',
    Status TEXT NOT NULL DEFAULT 'Active',
    Blacklisted INTEGER NOT NULL DEFAULT 0,
    BlacklistReason TEXT NOT NULL DEFAULT '',
    EffectiveFrom TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    Remarks TEXT NOT NULL DEFAULT '',
    CNIC TEXT NOT NULL DEFAULT '',
    MobileNo TEXT NOT NULL DEFAULT '',
    LicenseNo TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS Weighments (
    WeighmentId INTEGER PRIMARY KEY AUTOINCREMENT,
    TicketNo TEXT NOT NULL UNIQUE,
    CompanyName TEXT NOT NULL DEFAULT '',
    VehicleNo TEXT NOT NULL,
    DriverName TEXT,
    PartyId INTEGER,
    PartyAccount TEXT NOT NULL DEFAULT '',
    PartyName TEXT,
    PartyType TEXT,
    MaterialId INTEGER,
    ItemNumber TEXT NOT NULL DEFAULT '',
    ItemName TEXT NOT NULL DEFAULT '',
    MaterialName TEXT,
    FirstWeight REAL NOT NULL,
    FirstWeightTime TEXT NOT NULL,
    FirstWeightBy TEXT NOT NULL DEFAULT '',
    SecondWeight REAL,
    SecondWeightTime TEXT,
    SecondWeightBy TEXT NOT NULL DEFAULT '',
    NetWeight REAL,
    Status TEXT NOT NULL,
    Remarks TEXT,
    CreatedAt TEXT NOT NULL
);



CREATE TABLE IF NOT EXISTS Customers (
    CustomerId INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerAccount TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    MethodOfPayment TEXT NOT NULL DEFAULT '',
    TermsOfPayment TEXT NOT NULL DEFAULT '',
    DeliveryTerms TEXT NOT NULL DEFAULT '',
    AccountStatus TEXT NOT NULL DEFAULT '',
    AccountStatusReason TEXT NOT NULL DEFAULT '',
    CustomerGroup TEXT NOT NULL DEFAULT '',
    EmployeeResponsible TEXT NOT NULL DEFAULT '',
    Currency TEXT NOT NULL DEFAULT '',
    Telephone TEXT NOT NULL DEFAULT '',
    OrganizationPerson TEXT NOT NULL DEFAULT '',
    SearchName TEXT NOT NULL DEFAULT '',
    ClassificationGroup TEXT NOT NULL DEFAULT '',
    AddressNameDescription TEXT NOT NULL DEFAULT '',
    Address TEXT NOT NULL DEFAULT '',
    AddressPurpose TEXT NOT NULL DEFAULT '',
    ContactDescription TEXT NOT NULL DEFAULT '',
    ContactType TEXT NOT NULL DEFAULT '',
    ContactNumberAddress TEXT NOT NULL DEFAULT '',
    ContactExtension TEXT NOT NULL DEFAULT '',
    InvoiceAccount TEXT NOT NULL DEFAULT '',
    ModeOfDelivery TEXT NOT NULL DEFAULT '',
    SalesTaxGroup TEXT NOT NULL DEFAULT '',
    CreatedAt TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS Vendors (
    VendorId INTEGER PRIMARY KEY AUTOINCREMENT,
    VendorAccount TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    MethodOfPayment TEXT NOT NULL DEFAULT '',
    TermsOfPayment TEXT NOT NULL DEFAULT '',
    DeliveryTerms TEXT NOT NULL DEFAULT '',
    AccountStatus TEXT NOT NULL DEFAULT '',
    AccountStatusReason TEXT NOT NULL DEFAULT '',
    VendorGroup TEXT NOT NULL DEFAULT '',
    EmployeeResponsible TEXT NOT NULL DEFAULT '',
    Currency TEXT NOT NULL DEFAULT '',
    Telephone TEXT NOT NULL DEFAULT '',
    Type TEXT NOT NULL DEFAULT '',
    VendorClassificationGroup TEXT NOT NULL DEFAULT '',
    SearchName TEXT NOT NULL DEFAULT '',
    AddressNameDescription TEXT NOT NULL DEFAULT '',
    Address TEXT NOT NULL DEFAULT '',
    AddressPurpose TEXT NOT NULL DEFAULT '',
    ContactDescription TEXT NOT NULL DEFAULT '',
    ContactType TEXT NOT NULL DEFAULT '',
    ContactNumberAddress TEXT NOT NULL DEFAULT '',
    ContactExtension TEXT NOT NULL DEFAULT '',
    InvoiceAccount TEXT NOT NULL DEFAULT '',
    ModeOfDelivery TEXT NOT NULL DEFAULT '',
    SalesTaxGroup TEXT NOT NULL DEFAULT '',
    CreatedAt TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS ItemMasters (
    ItemMasterId INTEGER PRIMARY KEY AUTOINCREMENT,
    ItemNumber TEXT NOT NULL UNIQUE,
    ProductName TEXT NOT NULL,
    SearchName TEXT NOT NULL DEFAULT '',
    ProductType TEXT NOT NULL DEFAULT '',
    ProductSubtype TEXT NOT NULL DEFAULT '',
    ProductNumber TEXT NOT NULL DEFAULT '',
    Description TEXT NOT NULL DEFAULT '',
    StorageDimensionGroup TEXT NOT NULL DEFAULT '',
    TrackingDimensionGroup TEXT NOT NULL DEFAULT '',
    ItemModelGroup TEXT NOT NULL DEFAULT '',
    ReservationHierarchy TEXT NOT NULL DEFAULT '',
    PurchaseUnit TEXT NOT NULL DEFAULT '',
    PurchaseOverDelivery REAL,
    PurchaseUnderDelivery REAL,
    BuyerGroup TEXT NOT NULL DEFAULT '',
    ItemPriceToleranceGroup TEXT NOT NULL DEFAULT '',
    Vendor TEXT NOT NULL DEFAULT '',
    PurchaseItemSalesTaxGroup TEXT NOT NULL DEFAULT '',
    SellUnit TEXT NOT NULL DEFAULT '',
    SellOverDelivery REAL,
    SellUnderDelivery REAL,
    SellItemSalesTaxGroup TEXT NOT NULL DEFAULT '',
    BatchNumberGroup TEXT NOT NULL DEFAULT '',
    SerialNumberGroup TEXT NOT NULL DEFAULT '',
    InventoryOverDelivery REAL,
    InventoryUnderDelivery REAL,
    CatchWeightItem INTEGER NOT NULL DEFAULT 0,
    CWUnit TEXT NOT NULL DEFAULT '',
    NominalQuantity REAL,
    MinimumQuantity REAL,
    MaximumQuantity REAL,
    BOMUnit TEXT NOT NULL DEFAULT '',
    ConstantScrap REAL,
    VariableScrap REAL,
    CostingLevel INTEGER,
    PlanningLevel INTEGER,
    CostCalculationLevel INTEGER,
    Phantom INTEGER NOT NULL DEFAULT 0,
    CalculationGroup TEXT NOT NULL DEFAULT '',
    ProductionType TEXT NOT NULL DEFAULT '',
    ItemGroup TEXT NOT NULL DEFAULT '',
    CostUnit TEXT NOT NULL DEFAULT '',
    LastCostPrice REAL,
    DateOfPrice TEXT,
    UnitSequenceGroupId TEXT NOT NULL DEFAULT '',
    CreatedAt TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS WarehouseMasters (
    WarehouseMasterId INTEGER PRIMARY KEY AUTOINCREMENT,
    Warehouse TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    Site TEXT NOT NULL DEFAULT '',
    Type TEXT NOT NULL DEFAULT '',
    QuarantineWarehouse TEXT NOT NULL DEFAULT '',
    TransitWarehouse TEXT NOT NULL DEFAULT '',
    GoodsInTransitWarehouse TEXT NOT NULL DEFAULT '',
    UnderDeliveryWarehouse TEXT NOT NULL DEFAULT '',
    VendorAccount TEXT NOT NULL DEFAULT '',
    DefaultReceiptLocation TEXT NOT NULL DEFAULT '',
    DefaultIssueLocation TEXT NOT NULL DEFAULT '',
    DefaultProductionFinishedGood TEXT NOT NULL DEFAULT '',
    AddressNameDescription TEXT NOT NULL DEFAULT '',
    Address TEXT NOT NULL DEFAULT '',
    Purpose TEXT NOT NULL DEFAULT '',
    CreatedAt TEXT NOT NULL DEFAULT ''
);


CREATE TABLE IF NOT EXISTS WeighbridgeMasters (
    WeighbridgeId INTEGER PRIMARY KEY AUTOINCREMENT,
    WeighbridgeCode TEXT NOT NULL UNIQUE,
    WeighbridgeName TEXT NOT NULL,
    Description TEXT NOT NULL DEFAULT '',
    PlantSite TEXT NOT NULL DEFAULT '',
    Warehouse TEXT NOT NULL DEFAULT '',
    WarehouseAddress TEXT NOT NULL DEFAULT '',
    WeighbridgeType TEXT NOT NULL DEFAULT '',
    ScaleType TEXT NOT NULL DEFAULT '',
    ScaleCapacity REAL NOT NULL DEFAULT 0,
    CapacityUnit TEXT NOT NULL DEFAULT 'kg',
    MinimumWeight REAL,
    WeightIncrement REAL,
    WeightStabilityTime INTEGER,
    ScaleIpAddress TEXT NOT NULL DEFAULT '',
    TcpPort INTEGER NOT NULL DEFAULT 4001,
    ScaleComPort TEXT NOT NULL DEFAULT '',
    BaudRate INTEGER NOT NULL DEFAULT 9600,
    Parity TEXT NOT NULL DEFAULT 'None',
    DataBits INTEGER NOT NULL DEFAULT 8,
    StopBits TEXT NOT NULL DEFAULT 'One',
    CommunicationType TEXT NOT NULL DEFAULT '',
    ScaleManufacturer TEXT NOT NULL DEFAULT '',
    ScaleModel TEXT NOT NULL DEFAULT '',
    ScaleSerialNumber TEXT NOT NULL DEFAULT '',
    CalibrationCertificateNo TEXT NOT NULL DEFAULT '',
    LastCalibrationDate TEXT,
    NextCalibrationDate TEXT,
    Printer TEXT NOT NULL DEFAULT '',
    CameraAvailable INTEGER NOT NULL DEFAULT 0,
    AnprAvailable INTEGER NOT NULL DEFAULT 0,
    TrafficLightAvailable INTEGER NOT NULL DEFAULT 0,
    BoomBarrierAvailable INTEGER NOT NULL DEFAULT 0,
    CctvAvailable INTEGER NOT NULL DEFAULT 0,
    DefaultTicketTemplate TEXT NOT NULL DEFAULT '',
    DefaultCurrency TEXT NOT NULL DEFAULT '',
    DefaultOperator TEXT NOT NULL DEFAULT '',
    AllowedOperators TEXT NOT NULL DEFAULT '',
    OperatingStatus TEXT NOT NULL DEFAULT 'Active',
    EffectiveFrom TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    Remarks TEXT NOT NULL DEFAULT '',
    CreatedAt TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS OperatorMasters (
    OperatorId INTEGER PRIMARY KEY AUTOINCREMENT,
    EmployeeId TEXT NOT NULL UNIQUE,
    OperatorName TEXT NOT NULL,
    Username TEXT NOT NULL,
    Email TEXT NOT NULL DEFAULT '',
    MobileNumber TEXT NOT NULL DEFAULT '',
    Designation TEXT NOT NULL DEFAULT '',
    Department TEXT NOT NULL DEFAULT '',
    LegalEntity TEXT NOT NULL DEFAULT '',
    DefaultLegalEntity TEXT NOT NULL DEFAULT '',
    DefaultWeighbridge TEXT NOT NULL DEFAULT '',
    AssignedWeighbridges TEXT NOT NULL DEFAULT '',
    DefaultShift TEXT NOT NULL DEFAULT '',
    Role TEXT NOT NULL DEFAULT '',
    PermissionProfile TEXT NOT NULL DEFAULT '',
    CanCaptureFirstWeight INTEGER NOT NULL DEFAULT 1,
    CanCaptureSecondWeight INTEGER NOT NULL DEFAULT 1,
    CanPerformManualWeightEntry INTEGER NOT NULL DEFAULT 0,
    CanCorrectTransactions INTEGER NOT NULL DEFAULT 0,
    CanCancelTransactions INTEGER NOT NULL DEFAULT 0,
    CanOverrideWeight INTEGER NOT NULL DEFAULT 0,
    CanApproveQc INTEGER NOT NULL DEFAULT 0,
    CanRetryIntegration INTEGER NOT NULL DEFAULT 0,
    LastLogin TEXT,
    Status TEXT NOT NULL DEFAULT 'Active',
    EffectiveFrom TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    Remarks TEXT NOT NULL DEFAULT '',
    CreatedAt TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS Users (
    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    FullName TEXT NOT NULL,
    CompanyName TEXT NOT NULL DEFAULT '',
    PasswordHash TEXT NOT NULL,
    PasswordSalt TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CanAccessWeighment INTEGER NOT NULL DEFAULT 1,
    CanAccessSettings INTEGER NOT NULL DEFAULT 0,
    CanAccessMasters INTEGER NOT NULL DEFAULT 0,
    CanAccessReports INTEGER NOT NULL DEFAULT 1,
    CanAccessUserManagement INTEGER NOT NULL DEFAULT 0,
    CanEditCompletedTransaction INTEGER NOT NULL DEFAULT 0,
    CanDeleteCompletedTransaction INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL
);
");

        MigrateSchema(connection);
        EnsureDefaultSettings(connection);
        SeedMasterData(connection);
        EnsureDefaultAdminUser(connection);
    });

    public Task<DeviceSettings> GetSettingsAsync() => Task.Run(() =>
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM DeviceSettings WHERE SettingId = 1";
        using var reader = command.ExecuteReader();
        if (reader.Read())
            return MapDeviceSettings(reader);

        return new DeviceSettings();
    });

    public Task SaveSettingsAsync(DeviceSettings settings) => Task.Run(() =>
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO DeviceSettings
(SettingId, SelectedWeighbridgeCode, ConnectionType, ComPort, BaudRate, Parity, DataBits, StopBits, IpAddress, TcpPort)
VALUES
(1, $SelectedWeighbridgeCode, $ConnectionType, $ComPort, $BaudRate, $Parity, $DataBits, $StopBits, $IpAddress, $TcpPort)
ON CONFLICT(SettingId) DO UPDATE SET
SelectedWeighbridgeCode = excluded.SelectedWeighbridgeCode,
ConnectionType = excluded.ConnectionType,
ComPort = excluded.ComPort,
BaudRate = excluded.BaudRate,
Parity = excluded.Parity,
DataBits = excluded.DataBits,
StopBits = excluded.StopBits,
IpAddress = excluded.IpAddress,
TcpPort = excluded.TcpPort;";
        command.Parameters.AddWithValue("$SelectedWeighbridgeCode", settings.SelectedWeighbridgeCode);
        command.Parameters.AddWithValue("$ConnectionType", settings.ConnectionType);
        command.Parameters.AddWithValue("$ComPort", settings.ComPort);
        command.Parameters.AddWithValue("$BaudRate", settings.BaudRate);
        command.Parameters.AddWithValue("$Parity", settings.Parity);
        command.Parameters.AddWithValue("$DataBits", settings.DataBits);
        command.Parameters.AddWithValue("$StopBits", settings.StopBits);
        command.Parameters.AddWithValue("$IpAddress", settings.IpAddress);
        command.Parameters.AddWithValue("$TcpPort", settings.TcpPort);
        command.ExecuteNonQuery();
    });

    public Task<List<Party>> GetPartiesAsync() => Task.Run(() =>
    {
        var result = new List<Party>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT PartyId, PartyName, PartyType FROM Parties ORDER BY PartyName";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Party
            {
                PartyId = reader.GetInt32(0),
                PartyName = reader.GetString(1),
                PartyType = reader.GetString(2)
            });
        }
        return result;
    });

    public Task<List<Material>> GetMaterialsAsync() => Task.Run(() =>
    {
        var result = new List<Material>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT MaterialId, MaterialName FROM Materials ORDER BY MaterialName";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Material
            {
                MaterialId = reader.GetInt32(0),
                MaterialName = reader.GetString(1)
            });
        }
        return result;
    });

    public Task<List<Vehicle>> GetVehiclesAsync() => Task.Run(() =>
    {
        var result = new List<Vehicle>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Vehicles ORDER BY PlateNumber, VehicleNo";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var plateNumber = ReadText(reader, "PlateNumber");
            if (string.IsNullOrWhiteSpace(plateNumber))
                plateNumber = ReadText(reader, "VehicleNo");

            result.Add(new Vehicle
            {
                VehicleId = Convert.ToInt32(reader["VehicleId"]),
                PlateNumber = plateNumber,
                PlateEmirate = ReadText(reader, "PlateEmirate"),
                PlateCategory = ReadText(reader, "PlateCategory"),
                VehicleType = ReadText(reader, "VehicleType"),
                OwnershipType = ReadText(reader, "OwnershipType"),
                OwnerPartyAccount = ReadText(reader, "OwnerPartyAccount"),
                Transporter = ReadText(reader, "Transporter"),
                Capacity = ReadDecimal(reader, "Capacity") ?? 0m,
                DefaultDriver = ReadText(reader, "DefaultDriver"),
                RegistrationExpiryDate = ReadDate(reader, "RegistrationExpiryDate"),
                LegalEntity = ReadText(reader, "LegalEntity"),
                Status = string.IsNullOrWhiteSpace(ReadText(reader, "Status")) ? "Active" : ReadText(reader, "Status"),
                IsActive = ReadBool(reader, "IsActive")
            });
        }
        return result;
    });

    public Task<List<Driver>> GetDriversAsync() => Task.Run(() =>
    {
        var result = new List<Driver>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Drivers ORDER BY DriverName";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Driver
            {
                DriverId = Convert.ToInt32(reader["DriverId"]),
                DriverName = ReadText(reader, "DriverName"),
                MobileNumber = string.IsNullOrWhiteSpace(ReadText(reader, "MobileNumber")) ? ReadText(reader, "MobileNo") : ReadText(reader, "MobileNumber"),
                SecondaryMobile = ReadText(reader, "SecondaryMobile"),
                Email = ReadText(reader, "Email"),
                Nationality = ReadText(reader, "Nationality"),
                DriverType = ReadText(reader, "DriverType"),
                EmployerPartyType = ReadText(reader, "EmployerPartyType"),
                EmployerAccount = ReadText(reader, "EmployerAccount"),
                IdentificationType = ReadText(reader, "IdentificationType"),
                IdentificationNumber = string.IsNullOrWhiteSpace(ReadText(reader, "IdentificationNumber")) ? ReadText(reader, "CNIC") : ReadText(reader, "IdentificationNumber"),
                IdentificationExpiryDate = ReadDate(reader, "IdentificationExpiryDate"),
                EmiratesIdExpiryDate = ReadDate(reader, "EmiratesIdExpiryDate"),
                PassportNumber = ReadText(reader, "PassportNumber"),
                PassportExpiryDate = ReadDate(reader, "PassportExpiryDate"),
                DrivingLicenceNumber = string.IsNullOrWhiteSpace(ReadText(reader, "DrivingLicenceNumber")) ? ReadText(reader, "LicenseNo") : ReadText(reader, "DrivingLicenceNumber"),
                DrivingLicenceIssuedBy = ReadText(reader, "DrivingLicenceIssuedBy"),
                DrivingLicenceExpiryDate = ReadDate(reader, "DrivingLicenceExpiryDate"),
                LicenceCategories = ReadText(reader, "LicenceCategories"),
                DefaultVehicle = ReadText(reader, "DefaultVehicle"),
                Address = ReadText(reader, "Address"),
                DriverPhoto = ReadText(reader, "DriverPhoto"),
                EmiratesIdAttachment = ReadText(reader, "EmiratesIdAttachment"),
                PassportAttachment = ReadText(reader, "PassportAttachment"),
                DrivingLicenceAttachment = ReadText(reader, "DrivingLicenceAttachment"),
                LegalEntity = ReadText(reader, "LegalEntity"),
                Status = string.IsNullOrWhiteSpace(ReadText(reader, "Status")) ? "Active" : ReadText(reader, "Status"),
                Blacklisted = ReadBool(reader, "Blacklisted"),
                BlacklistReason = ReadText(reader, "BlacklistReason"),
                EffectiveFrom = ReadDate(reader, "EffectiveFrom"),
                IsActive = ReadBool(reader, "IsActive"),
                Remarks = ReadText(reader, "Remarks")
            });
        }
        return result;
    });

    public Task AddPartyAsync(string partyName, string partyType) => Task.Run(() =>
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO Parties (PartyName, PartyType) VALUES ($PartyName, $PartyType)";
        command.Parameters.AddWithValue("$PartyName", partyName.Trim());
        command.Parameters.AddWithValue("$PartyType", partyType.Trim());
        command.ExecuteNonQuery();
    });

    public Task AddMaterialAsync(string materialName) => Task.Run(() =>
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO Materials (MaterialName) VALUES ($MaterialName)";
        command.Parameters.AddWithValue("$MaterialName", materialName.Trim());
        command.ExecuteNonQuery();
    });

    public Task AddVehicleAsync(string vehicleNo) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(vehicleNo))
            return;

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO Vehicles (VehicleNo, PlateNumber, Status, IsActive) VALUES ($VehicleNo, $PlateNumber, 'Active', 1)";
        var plateNumber = vehicleNo.Trim().ToUpperInvariant();
        command.Parameters.AddWithValue("$VehicleNo", plateNumber);
        command.Parameters.AddWithValue("$PlateNumber", plateNumber);
        command.ExecuteNonQuery();
    });

    public Task SaveVehicleAsync(Vehicle vehicle) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(vehicle.PlateNumber))
            throw new InvalidOperationException("Plate Number is mandatory.");
        if (string.IsNullOrWhiteSpace(vehicle.PlateEmirate))
            throw new InvalidOperationException("Plate Emirate is mandatory.");
        if (string.IsNullOrWhiteSpace(vehicle.VehicleType))
            throw new InvalidOperationException("Vehicle Type is mandatory.");

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        if (vehicle.VehicleId > 0)
        {
            command.CommandText = @"
UPDATE Vehicles SET
VehicleNo = $VehicleNo,
PlateNumber = $PlateNumber,
PlateEmirate = $PlateEmirate,
PlateCategory = $PlateCategory,
VehicleType = $VehicleType,
OwnershipType = $OwnershipType,
OwnerPartyAccount = $OwnerPartyAccount,
Transporter = $Transporter,
Capacity = $Capacity,
DefaultDriver = $DefaultDriver,
RegistrationExpiryDate = $RegistrationExpiryDate,
LegalEntity = $LegalEntity,
Status = $Status,
IsActive = $IsActive
WHERE VehicleId = $VehicleId;";
            command.Parameters.AddWithValue("$VehicleId", vehicle.VehicleId);
        }
        else
        {
            command.CommandText = @"
INSERT INTO Vehicles
(VehicleNo, PlateNumber, PlateEmirate, PlateCategory, VehicleType, OwnershipType, OwnerPartyAccount, Transporter, Capacity, DefaultDriver, RegistrationExpiryDate, LegalEntity, Status, IsActive)
VALUES
($VehicleNo, $PlateNumber, $PlateEmirate, $PlateCategory, $VehicleType, $OwnershipType, $OwnerPartyAccount, $Transporter, $Capacity, $DefaultDriver, $RegistrationExpiryDate, $LegalEntity, $Status, $IsActive);";
        }

        AddVehicleParameters(command, vehicle);
        command.ExecuteNonQuery();
    });

    public Task AddDriverAsync(string driverName) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(driverName))
            return;

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO Drivers (DriverName, Status, EffectiveFrom, IsActive) VALUES ($DriverName, 'Active', $EffectiveFrom, 1)";
        command.Parameters.AddWithValue("$DriverName", driverName.Trim());
        command.Parameters.AddWithValue("$EffectiveFrom", DateTime.Today.ToString("yyyy-MM-dd"));
        command.ExecuteNonQuery();
    });

    public Task SaveDriverAsync(Driver driver) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(driver.DriverName))
            throw new InvalidOperationException("Driver Name is mandatory.");
        if (string.IsNullOrWhiteSpace(driver.MobileNumber))
            throw new InvalidOperationException("Mobile Number is mandatory.");
        if (string.IsNullOrWhiteSpace(driver.DriverType))
            throw new InvalidOperationException("Driver Type is mandatory.");
        if (string.IsNullOrWhiteSpace(driver.EmployerPartyType))
            throw new InvalidOperationException("Employer Party Type is mandatory.");
        if (string.IsNullOrWhiteSpace(driver.IdentificationType))
            throw new InvalidOperationException("Identification Type is mandatory.");
        if (string.IsNullOrWhiteSpace(driver.IdentificationNumber))
            throw new InvalidOperationException("Identification Number is mandatory.");
        if (string.IsNullOrWhiteSpace(driver.DrivingLicenceNumber))
            throw new InvalidOperationException("Driving Licence Number is mandatory.");
        if (string.IsNullOrWhiteSpace(driver.DrivingLicenceIssuedBy))
            throw new InvalidOperationException("Driving Licence Issued By is mandatory.");
        if (!driver.DrivingLicenceExpiryDate.HasValue)
            throw new InvalidOperationException("Driving Licence Expiry Date is mandatory.");
        if (string.IsNullOrWhiteSpace(driver.LegalEntity))
            throw new InvalidOperationException("Legal Entity is mandatory.");
        if (!driver.EffectiveFrom.HasValue)
            throw new InvalidOperationException("Effective From is mandatory.");
        if (driver.Blacklisted && string.IsNullOrWhiteSpace(driver.BlacklistReason))
            throw new InvalidOperationException("Blacklist Reason is required if driver is blacklisted.");

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        if (driver.DriverId > 0)
        {
            command.CommandText = @"
UPDATE Drivers SET
DriverName = $DriverName,
MobileNumber = $MobileNumber,
MobileNo = $MobileNumber,
SecondaryMobile = $SecondaryMobile,
Email = $Email,
Nationality = $Nationality,
DriverType = $DriverType,
EmployerPartyType = $EmployerPartyType,
EmployerAccount = $EmployerAccount,
IdentificationType = $IdentificationType,
IdentificationNumber = $IdentificationNumber,
CNIC = $IdentificationNumber,
IdentificationExpiryDate = $IdentificationExpiryDate,
EmiratesIdExpiryDate = $EmiratesIdExpiryDate,
PassportNumber = $PassportNumber,
PassportExpiryDate = $PassportExpiryDate,
DrivingLicenceNumber = $DrivingLicenceNumber,
LicenseNo = $DrivingLicenceNumber,
DrivingLicenceIssuedBy = $DrivingLicenceIssuedBy,
DrivingLicenceExpiryDate = $DrivingLicenceExpiryDate,
LicenceCategories = $LicenceCategories,
DefaultVehicle = $DefaultVehicle,
Address = $Address,
DriverPhoto = $DriverPhoto,
EmiratesIdAttachment = $EmiratesIdAttachment,
PassportAttachment = $PassportAttachment,
DrivingLicenceAttachment = $DrivingLicenceAttachment,
LegalEntity = $LegalEntity,
Status = $Status,
Blacklisted = $Blacklisted,
BlacklistReason = $BlacklistReason,
EffectiveFrom = $EffectiveFrom,
IsActive = $IsActive,
Remarks = $Remarks
WHERE DriverId = $DriverId;";
            command.Parameters.AddWithValue("$DriverId", driver.DriverId);
        }
        else
        {
            command.CommandText = @"
INSERT INTO Drivers
(DriverName, MobileNumber, MobileNo, SecondaryMobile, Email, Nationality, DriverType, EmployerPartyType, EmployerAccount, IdentificationType, IdentificationNumber, CNIC, IdentificationExpiryDate, EmiratesIdExpiryDate, PassportNumber, PassportExpiryDate, DrivingLicenceNumber, LicenseNo, DrivingLicenceIssuedBy, DrivingLicenceExpiryDate, LicenceCategories, DefaultVehicle, Address, DriverPhoto, EmiratesIdAttachment, PassportAttachment, DrivingLicenceAttachment, LegalEntity, Status, Blacklisted, BlacklistReason, EffectiveFrom, IsActive, Remarks)
VALUES
($DriverName, $MobileNumber, $MobileNumber, $SecondaryMobile, $Email, $Nationality, $DriverType, $EmployerPartyType, $EmployerAccount, $IdentificationType, $IdentificationNumber, $IdentificationNumber, $IdentificationExpiryDate, $EmiratesIdExpiryDate, $PassportNumber, $PassportExpiryDate, $DrivingLicenceNumber, $DrivingLicenceNumber, $DrivingLicenceIssuedBy, $DrivingLicenceExpiryDate, $LicenceCategories, $DefaultVehicle, $Address, $DriverPhoto, $EmiratesIdAttachment, $PassportAttachment, $DrivingLicenceAttachment, $LegalEntity, $Status, $Blacklisted, $BlacklistReason, $EffectiveFrom, $IsActive, $Remarks);";
        }

        AddDriverParameters(command, driver);
        command.ExecuteNonQuery();
    });



    public Task<List<WeighbridgeMaster>> GetWeighbridgeMastersAsync() => Task.Run(() =>
    {
        var result = new List<WeighbridgeMaster>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM WeighbridgeMasters ORDER BY WeighbridgeCode";
        using var reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(MapWeighbridgeMaster(reader));
        return result;
    });

    public Task SaveWeighbridgeMasterAsync(WeighbridgeMaster weighbridge) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(weighbridge.WeighbridgeCode))
            throw new InvalidOperationException("Weighbridge Code is mandatory.");
        if (string.IsNullOrWhiteSpace(weighbridge.WeighbridgeName))
            throw new InvalidOperationException("Weighbridge Name is mandatory.");
        if (string.IsNullOrWhiteSpace(weighbridge.PlantSite))
            throw new InvalidOperationException("Plant / Site is mandatory.");
        if (string.IsNullOrWhiteSpace(weighbridge.WeighbridgeType))
            throw new InvalidOperationException("Weighbridge Type is mandatory.");
        if (string.IsNullOrWhiteSpace(weighbridge.ScaleType))
            throw new InvalidOperationException("Scale Type is mandatory.");
        if (weighbridge.ScaleCapacity <= 0)
            throw new InvalidOperationException("Scale Capacity is mandatory and must be greater than zero.");
        if (string.IsNullOrWhiteSpace(weighbridge.CapacityUnit))
            throw new InvalidOperationException("Capacity Unit is mandatory.");
        if (string.IsNullOrWhiteSpace(weighbridge.CommunicationType))
            throw new InvalidOperationException("Communication Type is mandatory.");
        if (string.IsNullOrWhiteSpace(weighbridge.OperatingStatus))
            throw new InvalidOperationException("Operating Status is mandatory.");
        if (!weighbridge.EffectiveFrom.HasValue)
            throw new InvalidOperationException("Effective From is mandatory.");

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = weighbridge.WeighbridgeId > 0 ? @"
UPDATE WeighbridgeMasters SET
    WeighbridgeCode = $WeighbridgeCode,
    WeighbridgeName = $WeighbridgeName,
    Description = $Description,
    PlantSite = $PlantSite,
    Warehouse = $Warehouse,
    WarehouseAddress = $WarehouseAddress,
    WeighbridgeType = $WeighbridgeType,
    ScaleType = $ScaleType,
    ScaleCapacity = $ScaleCapacity,
    CapacityUnit = $CapacityUnit,
    MinimumWeight = $MinimumWeight,
    WeightIncrement = $WeightIncrement,
    WeightStabilityTime = $WeightStabilityTime,
    ScaleIpAddress = $ScaleIpAddress,
    TcpPort = $TcpPort,
    ScaleComPort = $ScaleComPort,
    BaudRate = $BaudRate,
    Parity = $Parity,
    DataBits = $DataBits,
    StopBits = $StopBits,
    CommunicationType = $CommunicationType,
    ScaleManufacturer = $ScaleManufacturer,
    ScaleModel = $ScaleModel,
    ScaleSerialNumber = $ScaleSerialNumber,
    CalibrationCertificateNo = $CalibrationCertificateNo,
    LastCalibrationDate = $LastCalibrationDate,
    NextCalibrationDate = $NextCalibrationDate,
    Printer = $Printer,
    CameraAvailable = $CameraAvailable,
    AnprAvailable = $AnprAvailable,
    TrafficLightAvailable = $TrafficLightAvailable,
    BoomBarrierAvailable = $BoomBarrierAvailable,
    CctvAvailable = $CctvAvailable,
    DefaultTicketTemplate = $DefaultTicketTemplate,
    DefaultCurrency = $DefaultCurrency,
    DefaultOperator = $DefaultOperator,
    AllowedOperators = $AllowedOperators,
    OperatingStatus = $OperatingStatus,
    EffectiveFrom = $EffectiveFrom,
    IsActive = $IsActive,
    Remarks = $Remarks
WHERE WeighbridgeId = $WeighbridgeId;" : @"
INSERT INTO WeighbridgeMasters
(WeighbridgeCode, WeighbridgeName, Description, PlantSite, Warehouse, WarehouseAddress, WeighbridgeType, ScaleType, ScaleCapacity, CapacityUnit, MinimumWeight, WeightIncrement, WeightStabilityTime, ScaleIpAddress, TcpPort, ScaleComPort, BaudRate, Parity, DataBits, StopBits, CommunicationType, ScaleManufacturer, ScaleModel, ScaleSerialNumber, CalibrationCertificateNo, LastCalibrationDate, NextCalibrationDate, Printer, CameraAvailable, AnprAvailable, TrafficLightAvailable, BoomBarrierAvailable, CctvAvailable, DefaultTicketTemplate, DefaultCurrency, DefaultOperator, AllowedOperators, OperatingStatus, EffectiveFrom, IsActive, Remarks, CreatedAt)
VALUES
($WeighbridgeCode, $WeighbridgeName, $Description, $PlantSite, $Warehouse, $WarehouseAddress, $WeighbridgeType, $ScaleType, $ScaleCapacity, $CapacityUnit, $MinimumWeight, $WeightIncrement, $WeightStabilityTime, $ScaleIpAddress, $TcpPort, $ScaleComPort, $BaudRate, $Parity, $DataBits, $StopBits, $CommunicationType, $ScaleManufacturer, $ScaleModel, $ScaleSerialNumber, $CalibrationCertificateNo, $LastCalibrationDate, $NextCalibrationDate, $Printer, $CameraAvailable, $AnprAvailable, $TrafficLightAvailable, $BoomBarrierAvailable, $CctvAvailable, $DefaultTicketTemplate, $DefaultCurrency, $DefaultOperator, $AllowedOperators, $OperatingStatus, $EffectiveFrom, $IsActive, $Remarks, $CreatedAt);";
        AddWeighbridgeMasterParameters(command, weighbridge);
        command.Parameters.AddWithValue("$WeighbridgeId", weighbridge.WeighbridgeId);
        command.Parameters.AddWithValue("$CreatedAt", DateTime.Now.ToString("O"));
        command.ExecuteNonQuery();
    });

    public Task<List<OperatorMaster>> GetOperatorMastersAsync() => Task.Run(() =>
    {
        var result = new List<OperatorMaster>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM OperatorMasters ORDER BY EmployeeId";
        using var reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(MapOperatorMaster(reader));
        return result;
    });

    public Task SaveOperatorMasterAsync(OperatorMaster operatorMaster) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(operatorMaster.EmployeeId))
            throw new InvalidOperationException("Employee ID is mandatory.");
        if (string.IsNullOrWhiteSpace(operatorMaster.OperatorName))
            throw new InvalidOperationException("Operator Name is mandatory.");
        if (string.IsNullOrWhiteSpace(operatorMaster.Username))
            throw new InvalidOperationException("Username is mandatory.");
        if (string.IsNullOrWhiteSpace(operatorMaster.Email))
            throw new InvalidOperationException("Email is mandatory.");
        if (string.IsNullOrWhiteSpace(operatorMaster.Designation))
            throw new InvalidOperationException("Designation is mandatory.");
        if (string.IsNullOrWhiteSpace(operatorMaster.Department))
            throw new InvalidOperationException("Department is mandatory.");
        if (string.IsNullOrWhiteSpace(operatorMaster.LegalEntity))
            throw new InvalidOperationException("Legal Entity is mandatory.");
        if (string.IsNullOrWhiteSpace(operatorMaster.AssignedWeighbridges))
            throw new InvalidOperationException("Assigned Weighbridges is mandatory.");
        if (string.IsNullOrWhiteSpace(operatorMaster.Role))
            throw new InvalidOperationException("Role is mandatory.");
        if (string.IsNullOrWhiteSpace(operatorMaster.PermissionProfile))
            throw new InvalidOperationException("Permission Profile is mandatory.");
        if (string.IsNullOrWhiteSpace(operatorMaster.Status))
            throw new InvalidOperationException("Status is mandatory.");
        if (!operatorMaster.EffectiveFrom.HasValue)
            throw new InvalidOperationException("Effective From is mandatory.");

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = operatorMaster.OperatorId > 0 ? @"
UPDATE OperatorMasters SET
    EmployeeId = $EmployeeId,
    OperatorName = $OperatorName,
    Username = $Username,
    Email = $Email,
    MobileNumber = $MobileNumber,
    Designation = $Designation,
    Department = $Department,
    LegalEntity = $LegalEntity,
    DefaultLegalEntity = $DefaultLegalEntity,
    DefaultWeighbridge = $DefaultWeighbridge,
    AssignedWeighbridges = $AssignedWeighbridges,
    DefaultShift = $DefaultShift,
    Role = $Role,
    PermissionProfile = $PermissionProfile,
    CanCaptureFirstWeight = $CanCaptureFirstWeight,
    CanCaptureSecondWeight = $CanCaptureSecondWeight,
    CanPerformManualWeightEntry = $CanPerformManualWeightEntry,
    CanCorrectTransactions = $CanCorrectTransactions,
    CanCancelTransactions = $CanCancelTransactions,
    CanOverrideWeight = $CanOverrideWeight,
    CanApproveQc = $CanApproveQc,
    CanRetryIntegration = $CanRetryIntegration,
    LastLogin = $LastLogin,
    Status = $Status,
    EffectiveFrom = $EffectiveFrom,
    IsActive = $IsActive,
    Remarks = $Remarks
WHERE OperatorId = $OperatorId;" : @"
INSERT INTO OperatorMasters
(EmployeeId, OperatorName, Username, Email, MobileNumber, Designation, Department, LegalEntity, DefaultLegalEntity, DefaultWeighbridge, AssignedWeighbridges, DefaultShift, Role, PermissionProfile, CanCaptureFirstWeight, CanCaptureSecondWeight, CanPerformManualWeightEntry, CanCorrectTransactions, CanCancelTransactions, CanOverrideWeight, CanApproveQc, CanRetryIntegration, LastLogin, Status, EffectiveFrom, IsActive, Remarks, CreatedAt)
VALUES
($EmployeeId, $OperatorName, $Username, $Email, $MobileNumber, $Designation, $Department, $LegalEntity, $DefaultLegalEntity, $DefaultWeighbridge, $AssignedWeighbridges, $DefaultShift, $Role, $PermissionProfile, $CanCaptureFirstWeight, $CanCaptureSecondWeight, $CanPerformManualWeightEntry, $CanCorrectTransactions, $CanCancelTransactions, $CanOverrideWeight, $CanApproveQc, $CanRetryIntegration, $LastLogin, $Status, $EffectiveFrom, $IsActive, $Remarks, $CreatedAt);";
        AddOperatorMasterParameters(command, operatorMaster);
        command.Parameters.AddWithValue("$OperatorId", operatorMaster.OperatorId);
        command.Parameters.AddWithValue("$CreatedAt", DateTime.Now.ToString("O"));
        command.ExecuteNonQuery();
    });

    public Task<List<Customer>> GetCustomersAsync() => Task.Run(() =>
    {
        var result = new List<Customer>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Customers ORDER BY CustomerAccount";
        using var reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(MapCustomer(reader));
        return result;
    });

    public Task SaveCustomerAsync(Customer customer) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(customer.CustomerAccount))
            throw new InvalidOperationException("Customer Account is mandatory.");
        if (string.IsNullOrWhiteSpace(customer.Name))
            throw new InvalidOperationException("Customer Name is mandatory.");

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = customer.CustomerId > 0 ? @"
UPDATE Customers SET
    CustomerAccount = $CustomerAccount,
    Name = $Name,
    MethodOfPayment = $MethodOfPayment,
    TermsOfPayment = $TermsOfPayment,
    DeliveryTerms = $DeliveryTerms,
    AccountStatus = $AccountStatus,
    AccountStatusReason = $AccountStatusReason,
    CustomerGroup = $CustomerGroup,
    EmployeeResponsible = $EmployeeResponsible,
    Currency = $Currency,
    Telephone = $Telephone,
    OrganizationPerson = $OrganizationPerson,
    SearchName = $SearchName,
    ClassificationGroup = $ClassificationGroup,
    AddressNameDescription = $AddressNameDescription,
    Address = $Address,
    AddressPurpose = $AddressPurpose,
    ContactDescription = $ContactDescription,
    ContactType = $ContactType,
    ContactNumberAddress = $ContactNumberAddress,
    ContactExtension = $ContactExtension,
    InvoiceAccount = $InvoiceAccount,
    ModeOfDelivery = $ModeOfDelivery,
    SalesTaxGroup = $SalesTaxGroup
WHERE CustomerId = $CustomerId;" : @"
INSERT INTO Customers
(CustomerAccount, Name, MethodOfPayment, TermsOfPayment, DeliveryTerms, AccountStatus, AccountStatusReason, CustomerGroup, EmployeeResponsible, Currency, Telephone, OrganizationPerson, SearchName, ClassificationGroup, AddressNameDescription, Address, AddressPurpose, ContactDescription, ContactType, ContactNumberAddress, ContactExtension, InvoiceAccount, ModeOfDelivery, SalesTaxGroup, CreatedAt)
VALUES
($CustomerAccount, $Name, $MethodOfPayment, $TermsOfPayment, $DeliveryTerms, $AccountStatus, $AccountStatusReason, $CustomerGroup, $EmployeeResponsible, $Currency, $Telephone, $OrganizationPerson, $SearchName, $ClassificationGroup, $AddressNameDescription, $Address, $AddressPurpose, $ContactDescription, $ContactType, $ContactNumberAddress, $ContactExtension, $InvoiceAccount, $ModeOfDelivery, $SalesTaxGroup, $CreatedAt);";
        AddCustomerParameters(command, customer);
        command.Parameters.AddWithValue("$CustomerId", customer.CustomerId);
        command.Parameters.AddWithValue("$CreatedAt", DateTime.Now.ToString("O"));
        command.ExecuteNonQuery();
    });

    public Task<List<Vendor>> GetVendorsAsync() => Task.Run(() =>
    {
        var result = new List<Vendor>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Vendors ORDER BY VendorAccount";
        using var reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(MapVendor(reader));
        return result;
    });

    public Task SaveVendorAsync(Vendor vendor) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(vendor.VendorAccount))
            throw new InvalidOperationException("Vendor Account is mandatory.");
        if (string.IsNullOrWhiteSpace(vendor.Name))
            throw new InvalidOperationException("Vendor Name is mandatory.");

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = vendor.VendorId > 0 ? @"
UPDATE Vendors SET
    VendorAccount = $VendorAccount,
    Name = $Name,
    MethodOfPayment = $MethodOfPayment,
    TermsOfPayment = $TermsOfPayment,
    DeliveryTerms = $DeliveryTerms,
    AccountStatus = $AccountStatus,
    AccountStatusReason = $AccountStatusReason,
    VendorGroup = $VendorGroup,
    EmployeeResponsible = $EmployeeResponsible,
    Currency = $Currency,
    Telephone = $Telephone,
    Type = $Type,
    VendorClassificationGroup = $VendorClassificationGroup,
    SearchName = $SearchName,
    AddressNameDescription = $AddressNameDescription,
    Address = $Address,
    AddressPurpose = $AddressPurpose,
    ContactDescription = $ContactDescription,
    ContactType = $ContactType,
    ContactNumberAddress = $ContactNumberAddress,
    ContactExtension = $ContactExtension,
    InvoiceAccount = $InvoiceAccount,
    ModeOfDelivery = $ModeOfDelivery,
    SalesTaxGroup = $SalesTaxGroup
WHERE VendorId = $VendorId;" : @"
INSERT INTO Vendors
(VendorAccount, Name, MethodOfPayment, TermsOfPayment, DeliveryTerms, AccountStatus, AccountStatusReason, VendorGroup, EmployeeResponsible, Currency, Telephone, Type, VendorClassificationGroup, SearchName, AddressNameDescription, Address, AddressPurpose, ContactDescription, ContactType, ContactNumberAddress, ContactExtension, InvoiceAccount, ModeOfDelivery, SalesTaxGroup, CreatedAt)
VALUES
($VendorAccount, $Name, $MethodOfPayment, $TermsOfPayment, $DeliveryTerms, $AccountStatus, $AccountStatusReason, $VendorGroup, $EmployeeResponsible, $Currency, $Telephone, $Type, $VendorClassificationGroup, $SearchName, $AddressNameDescription, $Address, $AddressPurpose, $ContactDescription, $ContactType, $ContactNumberAddress, $ContactExtension, $InvoiceAccount, $ModeOfDelivery, $SalesTaxGroup, $CreatedAt);";
        AddVendorParameters(command, vendor);
        command.Parameters.AddWithValue("$VendorId", vendor.VendorId);
        command.Parameters.AddWithValue("$CreatedAt", DateTime.Now.ToString("O"));
        command.ExecuteNonQuery();
    });

    public Task<List<ItemMaster>> GetItemMastersAsync() => Task.Run(() =>
    {
        var result = new List<ItemMaster>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM ItemMasters ORDER BY ItemNumber";
        using var reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(MapItemMaster(reader));
        return result;
    });

    public Task SaveItemMasterAsync(ItemMaster item) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(item.ItemNumber))
            throw new InvalidOperationException("Item Number is mandatory.");
        if (string.IsNullOrWhiteSpace(item.ProductName))
            throw new InvalidOperationException("Product Name is mandatory.");

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = item.ItemMasterId > 0 ? @"
UPDATE ItemMasters SET
    ItemNumber = $ItemNumber,
    ProductName = $ProductName,
    SearchName = $SearchName,
    ProductType = $ProductType,
    ProductSubtype = $ProductSubtype,
    ProductNumber = $ProductNumber,
    Description = $Description,
    StorageDimensionGroup = $StorageDimensionGroup,
    TrackingDimensionGroup = $TrackingDimensionGroup,
    ItemModelGroup = $ItemModelGroup,
    ReservationHierarchy = $ReservationHierarchy,
    PurchaseUnit = $PurchaseUnit,
    PurchaseOverDelivery = $PurchaseOverDelivery,
    PurchaseUnderDelivery = $PurchaseUnderDelivery,
    BuyerGroup = $BuyerGroup,
    ItemPriceToleranceGroup = $ItemPriceToleranceGroup,
    Vendor = $Vendor,
    PurchaseItemSalesTaxGroup = $PurchaseItemSalesTaxGroup,
    SellUnit = $SellUnit,
    SellOverDelivery = $SellOverDelivery,
    SellUnderDelivery = $SellUnderDelivery,
    SellItemSalesTaxGroup = $SellItemSalesTaxGroup,
    BatchNumberGroup = $BatchNumberGroup,
    SerialNumberGroup = $SerialNumberGroup,
    InventoryOverDelivery = $InventoryOverDelivery,
    InventoryUnderDelivery = $InventoryUnderDelivery,
    CatchWeightItem = $CatchWeightItem,
    CWUnit = $CWUnit,
    NominalQuantity = $NominalQuantity,
    MinimumQuantity = $MinimumQuantity,
    MaximumQuantity = $MaximumQuantity,
    BOMUnit = $BOMUnit,
    ConstantScrap = $ConstantScrap,
    VariableScrap = $VariableScrap,
    CostingLevel = $CostingLevel,
    PlanningLevel = $PlanningLevel,
    CostCalculationLevel = $CostCalculationLevel,
    Phantom = $Phantom,
    CalculationGroup = $CalculationGroup,
    ProductionType = $ProductionType,
    ItemGroup = $ItemGroup,
    CostUnit = $CostUnit,
    LastCostPrice = $LastCostPrice,
    DateOfPrice = $DateOfPrice,
    UnitSequenceGroupId = $UnitSequenceGroupId
WHERE ItemMasterId = $ItemMasterId;" : @"
INSERT INTO ItemMasters
(ItemNumber, ProductName, SearchName, ProductType, ProductSubtype, ProductNumber, Description, StorageDimensionGroup, TrackingDimensionGroup, ItemModelGroup, ReservationHierarchy, PurchaseUnit, PurchaseOverDelivery, PurchaseUnderDelivery, BuyerGroup, ItemPriceToleranceGroup, Vendor, PurchaseItemSalesTaxGroup, SellUnit, SellOverDelivery, SellUnderDelivery, SellItemSalesTaxGroup, BatchNumberGroup, SerialNumberGroup, InventoryOverDelivery, InventoryUnderDelivery, CatchWeightItem, CWUnit, NominalQuantity, MinimumQuantity, MaximumQuantity, BOMUnit, ConstantScrap, VariableScrap, CostingLevel, PlanningLevel, CostCalculationLevel, Phantom, CalculationGroup, ProductionType, ItemGroup, CostUnit, LastCostPrice, DateOfPrice, UnitSequenceGroupId, CreatedAt)
VALUES
($ItemNumber, $ProductName, $SearchName, $ProductType, $ProductSubtype, $ProductNumber, $Description, $StorageDimensionGroup, $TrackingDimensionGroup, $ItemModelGroup, $ReservationHierarchy, $PurchaseUnit, $PurchaseOverDelivery, $PurchaseUnderDelivery, $BuyerGroup, $ItemPriceToleranceGroup, $Vendor, $PurchaseItemSalesTaxGroup, $SellUnit, $SellOverDelivery, $SellUnderDelivery, $SellItemSalesTaxGroup, $BatchNumberGroup, $SerialNumberGroup, $InventoryOverDelivery, $InventoryUnderDelivery, $CatchWeightItem, $CWUnit, $NominalQuantity, $MinimumQuantity, $MaximumQuantity, $BOMUnit, $ConstantScrap, $VariableScrap, $CostingLevel, $PlanningLevel, $CostCalculationLevel, $Phantom, $CalculationGroup, $ProductionType, $ItemGroup, $CostUnit, $LastCostPrice, $DateOfPrice, $UnitSequenceGroupId, $CreatedAt);";
        AddItemMasterParameters(command, item);
        command.Parameters.AddWithValue("$ItemMasterId", item.ItemMasterId);
        command.Parameters.AddWithValue("$CreatedAt", DateTime.Now.ToString("O"));
        command.ExecuteNonQuery();
    });

    public Task<List<WarehouseMaster>> GetWarehouseMastersAsync() => Task.Run(() =>
    {
        var result = new List<WarehouseMaster>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM WarehouseMasters ORDER BY Warehouse";
        using var reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(MapWarehouseMaster(reader));
        return result;
    });

    public Task SaveWarehouseMasterAsync(WarehouseMaster warehouse) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(warehouse.Warehouse))
            throw new InvalidOperationException("Warehouse is mandatory.");
        if (string.IsNullOrWhiteSpace(warehouse.Name))
            throw new InvalidOperationException("Warehouse Name is mandatory.");

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = warehouse.WarehouseMasterId > 0 ? @"
UPDATE WarehouseMasters SET
    Warehouse = $Warehouse,
    Name = $Name,
    Site = $Site,
    Type = $Type,
    QuarantineWarehouse = $QuarantineWarehouse,
    TransitWarehouse = $TransitWarehouse,
    GoodsInTransitWarehouse = $GoodsInTransitWarehouse,
    UnderDeliveryWarehouse = $UnderDeliveryWarehouse,
    VendorAccount = $VendorAccount,
    DefaultReceiptLocation = $DefaultReceiptLocation,
    DefaultIssueLocation = $DefaultIssueLocation,
    DefaultProductionFinishedGood = $DefaultProductionFinishedGood,
    AddressNameDescription = $AddressNameDescription,
    Address = $Address,
    Purpose = $Purpose
WHERE WarehouseMasterId = $WarehouseMasterId;" : @"
INSERT INTO WarehouseMasters
(Warehouse, Name, Site, Type, QuarantineWarehouse, TransitWarehouse, GoodsInTransitWarehouse, UnderDeliveryWarehouse, VendorAccount, DefaultReceiptLocation, DefaultIssueLocation, DefaultProductionFinishedGood, AddressNameDescription, Address, Purpose, CreatedAt)
VALUES
($Warehouse, $Name, $Site, $Type, $QuarantineWarehouse, $TransitWarehouse, $GoodsInTransitWarehouse, $UnderDeliveryWarehouse, $VendorAccount, $DefaultReceiptLocation, $DefaultIssueLocation, $DefaultProductionFinishedGood, $AddressNameDescription, $Address, $Purpose, $CreatedAt);";
        AddWarehouseMasterParameters(command, warehouse);
        command.Parameters.AddWithValue("$WarehouseMasterId", warehouse.WarehouseMasterId);
        command.Parameters.AddWithValue("$CreatedAt", DateTime.Now.ToString("O"));
        command.ExecuteNonQuery();
    });

    public Task<string> GenerateTicketNoAsync() => Task.Run(() =>
    {
        var prefix = $"WB-{DateTime.Now:yyyyMMdd}-";
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT TicketNo FROM Weighments WHERE TicketNo LIKE $Prefix ORDER BY WeighmentId DESC LIMIT 1";
        command.Parameters.AddWithValue("$Prefix", prefix + "%");
        var lastTicket = command.ExecuteScalar()?.ToString();

        var next = 1;
        if (!string.IsNullOrWhiteSpace(lastTicket))
        {
            var lastPart = lastTicket.Split('-').LastOrDefault();
            if (int.TryParse(lastPart, out var lastNumber))
                next = lastNumber + 1;
        }

        return prefix + next.ToString("0000");
    });

    public Task<int> InsertFirstWeightAsync(Weighment weighment) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(weighment.CompanyName))
            throw new InvalidOperationException("Company is required for weighment transaction.");

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Weighments
(TicketNo, CompanyName, VehicleNo, DriverName, PartyId, PartyAccount, PartyName, PartyType, MaterialId, ItemNumber, ItemName, MaterialName, FirstWeight, FirstWeightTime, FirstWeightBy, Status, Remarks, CreatedAt)
VALUES
($TicketNo, $CompanyName, $VehicleNo, $DriverName, $PartyId, $PartyAccount, $PartyName, $PartyType, $MaterialId, $ItemNumber, $ItemName, $MaterialName, $FirstWeight, $FirstWeightTime, $FirstWeightBy, $Status, $Remarks, $CreatedAt);
SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$TicketNo", weighment.TicketNo);
        command.Parameters.AddWithValue("$CompanyName", weighment.CompanyName.Trim());
        command.Parameters.AddWithValue("$VehicleNo", weighment.VehicleNo);
        command.Parameters.AddWithValue("$DriverName", weighment.DriverName ?? string.Empty);
        command.Parameters.AddWithValue("$PartyId", (object?)weighment.PartyId ?? DBNull.Value);
        command.Parameters.AddWithValue("$PartyAccount", weighment.PartyAccount ?? string.Empty);
        command.Parameters.AddWithValue("$PartyName", weighment.PartyName ?? string.Empty);
        command.Parameters.AddWithValue("$PartyType", weighment.PartyType ?? string.Empty);
        command.Parameters.AddWithValue("$MaterialId", (object?)weighment.MaterialId ?? DBNull.Value);
        command.Parameters.AddWithValue("$ItemNumber", weighment.ItemNumber ?? string.Empty);
        command.Parameters.AddWithValue("$ItemName", weighment.ItemName ?? string.Empty);
        command.Parameters.AddWithValue("$MaterialName", string.IsNullOrWhiteSpace(weighment.MaterialName) ? weighment.ItemName ?? string.Empty : weighment.MaterialName);
        command.Parameters.AddWithValue("$FirstWeight", weighment.FirstWeight);
        command.Parameters.AddWithValue("$FirstWeightTime", weighment.FirstWeightTime.ToString("O"));
        command.Parameters.AddWithValue("$FirstWeightBy", weighment.FirstWeightBy ?? string.Empty);
        command.Parameters.AddWithValue("$Status", "Open");
        command.Parameters.AddWithValue("$Remarks", weighment.Remarks ?? string.Empty);
        command.Parameters.AddWithValue("$CreatedAt", weighment.CreatedAt.ToString("O"));
        return Convert.ToInt32(command.ExecuteScalar());
    });

    public Task CompleteSecondWeightAsync(int weighmentId, decimal secondWeight, DateTime secondWeightTime, string secondWeightBy) => Task.Run(() =>
    {
        using var connection = CreateConnection();
        connection.Open();

        decimal firstWeight;
        using (var getCommand = connection.CreateCommand())
        {
            getCommand.CommandText = "SELECT FirstWeight FROM Weighments WHERE WeighmentId = $WeighmentId";
            getCommand.Parameters.AddWithValue("$WeighmentId", weighmentId);
            var firstWeightObject = getCommand.ExecuteScalar();
            if (firstWeightObject == null)
                throw new InvalidOperationException("Open ticket not found.");
            firstWeight = Convert.ToDecimal(firstWeightObject);
        }

        var netWeight = Math.Abs(secondWeight - firstWeight);

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Weighments
SET SecondWeight = $SecondWeight,
    SecondWeightTime = $SecondWeightTime,
    SecondWeightBy = $SecondWeightBy,
    NetWeight = $NetWeight,
    Status = 'Completed'
WHERE WeighmentId = $WeighmentId;";
        command.Parameters.AddWithValue("$SecondWeight", secondWeight);
        command.Parameters.AddWithValue("$SecondWeightTime", secondWeightTime.ToString("O"));
        command.Parameters.AddWithValue("$SecondWeightBy", secondWeightBy ?? string.Empty);
        command.Parameters.AddWithValue("$NetWeight", netWeight);
        command.Parameters.AddWithValue("$WeighmentId", weighmentId);
        command.ExecuteNonQuery();
    });

    public Task UpdateCompletedWeighmentAsync(Weighment weighment) => Task.Run(() =>
    {
        if (weighment.WeighmentId <= 0)
            throw new InvalidOperationException("Please select a valid completed transaction.");

        if (!string.Equals(weighment.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only completed transactions can be edited from this screen.");

        if (string.IsNullOrWhiteSpace(weighment.CompanyName))
            throw new InvalidOperationException("Company is mandatory.");

        if (string.IsNullOrWhiteSpace(weighment.VehicleNo))
            throw new InvalidOperationException("Vehicle number is mandatory.");

        if (weighment.SecondWeight == null)
            throw new InvalidOperationException("Second weight is mandatory for completed transaction.");

        var secondTime = weighment.SecondWeightTime ?? DateTime.Now;
        var netWeight = Math.Abs(weighment.SecondWeight.Value - weighment.FirstWeight);

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Weighments SET
    CompanyName = $CompanyName,
    VehicleNo = $VehicleNo,
    DriverName = $DriverName,
    PartyAccount = $PartyAccount,
    PartyName = $PartyName,
    PartyType = $PartyType,
    ItemNumber = $ItemNumber,
    ItemName = $ItemName,
    MaterialName = $MaterialName,
    FirstWeight = $FirstWeight,
    FirstWeightTime = $FirstWeightTime,
    FirstWeightBy = $FirstWeightBy,
    SecondWeight = $SecondWeight,
    SecondWeightTime = $SecondWeightTime,
    SecondWeightBy = $SecondWeightBy,
    NetWeight = $NetWeight,
    Remarks = $Remarks
WHERE WeighmentId = $WeighmentId
  AND Status = 'Completed';";
        command.Parameters.AddWithValue("$WeighmentId", weighment.WeighmentId);
        command.Parameters.AddWithValue("$CompanyName", weighment.CompanyName.Trim());
        command.Parameters.AddWithValue("$VehicleNo", weighment.VehicleNo.Trim().ToUpperInvariant());
        command.Parameters.AddWithValue("$DriverName", weighment.DriverName ?? string.Empty);
        command.Parameters.AddWithValue("$PartyAccount", weighment.PartyAccount ?? string.Empty);
        command.Parameters.AddWithValue("$PartyName", weighment.PartyName ?? string.Empty);
        command.Parameters.AddWithValue("$PartyType", weighment.PartyType ?? string.Empty);
        command.Parameters.AddWithValue("$ItemNumber", weighment.ItemNumber ?? string.Empty);
        command.Parameters.AddWithValue("$ItemName", weighment.ItemName ?? string.Empty);
        command.Parameters.AddWithValue("$MaterialName", string.IsNullOrWhiteSpace(weighment.MaterialName) ? weighment.ItemName ?? string.Empty : weighment.MaterialName);
        command.Parameters.AddWithValue("$FirstWeight", weighment.FirstWeight);
        command.Parameters.AddWithValue("$FirstWeightTime", weighment.FirstWeightTime.ToString("O"));
        command.Parameters.AddWithValue("$FirstWeightBy", weighment.FirstWeightBy ?? string.Empty);
        command.Parameters.AddWithValue("$SecondWeight", weighment.SecondWeight.Value);
        command.Parameters.AddWithValue("$SecondWeightTime", secondTime.ToString("O"));
        command.Parameters.AddWithValue("$SecondWeightBy", weighment.SecondWeightBy ?? string.Empty);
        command.Parameters.AddWithValue("$NetWeight", netWeight);
        command.Parameters.AddWithValue("$Remarks", weighment.Remarks ?? string.Empty);

        var affected = command.ExecuteNonQuery();
        if (affected == 0)
            throw new InvalidOperationException("Completed transaction not found.");
    });

    public Task DeleteWeighmentAsync(int weighmentId) => Task.Run(() =>
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Weighments WHERE WeighmentId = $WeighmentId AND Status = 'Completed'";
        command.Parameters.AddWithValue("$WeighmentId", weighmentId);
        var affected = command.ExecuteNonQuery();
        if (affected == 0)
            throw new InvalidOperationException("Completed transaction not found or already deleted.");
    });

    public Task<List<Weighment>> GetOpenWeighmentsAsync() => Task.Run(() =>
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = WeighmentSelectSql + " WHERE w.Status = 'Open' ORDER BY w.FirstWeightTime DESC";
        return ReadWeighments(command);
    });

    public Task<List<Weighment>> GetCompletedTodayAsync() => Task.Run(() =>
    {
        var from = DateTime.Today;
        var to = DateTime.Today.AddDays(1);
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = WeighmentSelectSql + @"
WHERE w.Status = 'Completed'
  AND w.SecondWeightTime >= $FromDate
  AND w.SecondWeightTime < $ToDate
ORDER BY w.SecondWeightTime DESC";
        command.Parameters.AddWithValue("$FromDate", from.ToString("O"));
        command.Parameters.AddWithValue("$ToDate", to.ToString("O"));
        return ReadWeighments(command);
    });

    public Task<List<Weighment>> SearchWeighmentsAsync(DateTime fromDate, DateTime toDate) => Task.Run(() =>
    {
        var toExclusive = toDate.Date.AddDays(1);
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = WeighmentSelectSql + @"
WHERE w.CreatedAt >= $FromDate
  AND w.CreatedAt < $ToDate
ORDER BY w.CreatedAt DESC";
        command.Parameters.AddWithValue("$FromDate", fromDate.Date.ToString("O"));
        command.Parameters.AddWithValue("$ToDate", toExclusive.ToString("O"));
        return ReadWeighments(command);
    });

    public Task<AppUser?> AuthenticateUserAsync(string username, string password) => Task.Run(() =>
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Users WHERE lower(Username) = lower($Username) AND IsActive = 1 LIMIT 1";
        command.Parameters.AddWithValue("$Username", username.Trim());

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        var passwordHash = Convert.ToString(reader["PasswordHash"]) ?? string.Empty;
        var passwordSalt = Convert.ToString(reader["PasswordSalt"]) ?? string.Empty;

        if (!PasswordService.VerifyPassword(password, passwordHash, passwordSalt))
            return null;

        return MapUser(reader);
    });

    public Task<List<AppUser>> GetUsersAsync() => Task.Run(() =>
    {
        var result = new List<AppUser>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Users ORDER BY Username";
        using var reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(MapUser(reader));
        return result;
    });

    public Task AddUserAsync(AppUser user, string password) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(user.Username))
            throw new InvalidOperationException("Username is required.");

        if (string.IsNullOrWhiteSpace(user.CompanyName))
            throw new InvalidOperationException("Company is required.");

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Password is required for new user.");

        var passwordData = PasswordService.HashPassword(password);

        using var connection = CreateConnection();
        connection.Open();
        EnsureUsernameIsUnique(connection, user.Username, null);
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Users
(Username, FullName, CompanyName, PasswordHash, PasswordSalt, IsActive, CanAccessWeighment, CanAccessSettings, CanAccessMasters, CanAccessReports, CanAccessUserManagement, CanEditCompletedTransaction, CanDeleteCompletedTransaction, CreatedAt)
VALUES
($Username, $FullName, $CompanyName, $PasswordHash, $PasswordSalt, $IsActive, $CanAccessWeighment, $CanAccessSettings, $CanAccessMasters, $CanAccessReports, $CanAccessUserManagement, $CanEditCompletedTransaction, $CanDeleteCompletedTransaction, $CreatedAt);";
        command.Parameters.AddWithValue("$Username", user.Username.Trim());
        command.Parameters.AddWithValue("$FullName", string.IsNullOrWhiteSpace(user.FullName) ? user.Username.Trim() : user.FullName.Trim());
        command.Parameters.AddWithValue("$CompanyName", user.CompanyName.Trim());
        command.Parameters.AddWithValue("$PasswordHash", passwordData.Hash);
        command.Parameters.AddWithValue("$PasswordSalt", passwordData.Salt);
        AddUserPermissionParameters(command, user);
        command.Parameters.AddWithValue("$CreatedAt", DateTime.Now.ToString("O"));
        command.ExecuteNonQuery();
    });

    public Task UpdateUserAsync(AppUser user, string? newPassword) => Task.Run(() =>
    {
        if (user.UserId <= 0)
            throw new InvalidOperationException("Please select a valid user.");

        if (string.IsNullOrWhiteSpace(user.CompanyName))
            throw new InvalidOperationException("Company is required.");

        if (string.IsNullOrWhiteSpace(user.Username))
            throw new InvalidOperationException("Username is required.");

        using var connection = CreateConnection();
        connection.Open();
        EnsureUsernameIsUnique(connection, user.Username, user.UserId);
        using var command = connection.CreateCommand();

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            command.CommandText = @"
UPDATE Users SET
    Username = $Username,
    FullName = $FullName,
    CompanyName = $CompanyName,
    IsActive = $IsActive,
    CanAccessWeighment = $CanAccessWeighment,
    CanAccessSettings = $CanAccessSettings,
    CanAccessMasters = $CanAccessMasters,
    CanAccessReports = $CanAccessReports,
    CanAccessUserManagement = $CanAccessUserManagement,
    CanEditCompletedTransaction = $CanEditCompletedTransaction,
    CanDeleteCompletedTransaction = $CanDeleteCompletedTransaction
WHERE UserId = $UserId;";
        }
        else
        {
            var passwordData = PasswordService.HashPassword(newPassword);
            command.CommandText = @"
UPDATE Users SET
    Username = $Username,
    FullName = $FullName,
    CompanyName = $CompanyName,
    PasswordHash = $PasswordHash,
    PasswordSalt = $PasswordSalt,
    IsActive = $IsActive,
    CanAccessWeighment = $CanAccessWeighment,
    CanAccessSettings = $CanAccessSettings,
    CanAccessMasters = $CanAccessMasters,
    CanAccessReports = $CanAccessReports,
    CanAccessUserManagement = $CanAccessUserManagement,
    CanEditCompletedTransaction = $CanEditCompletedTransaction,
    CanDeleteCompletedTransaction = $CanDeleteCompletedTransaction
WHERE UserId = $UserId;";
            command.Parameters.AddWithValue("$PasswordHash", passwordData.Hash);
            command.Parameters.AddWithValue("$PasswordSalt", passwordData.Salt);
        }

        command.Parameters.AddWithValue("$UserId", user.UserId);
        command.Parameters.AddWithValue("$Username", user.Username.Trim());
        command.Parameters.AddWithValue("$FullName", string.IsNullOrWhiteSpace(user.FullName) ? user.Username.Trim() : user.FullName.Trim());
        command.Parameters.AddWithValue("$CompanyName", user.CompanyName.Trim());
        AddUserPermissionParameters(command, user);
        command.ExecuteNonQuery();
    });

    private static void EnsureUsernameIsUnique(SqliteConnection connection, string username, int? excludeUserId)
    {
        var trimmedUsername = username.Trim();
        using var command = connection.CreateCommand();
        command.CommandText = excludeUserId.HasValue
            ? "SELECT COUNT(1) FROM Users WHERE lower(trim(Username)) = lower(trim($Username)) AND UserId <> $UserId"
            : "SELECT COUNT(1) FROM Users WHERE lower(trim(Username)) = lower(trim($Username))";
        command.Parameters.AddWithValue("$Username", trimmedUsername);
        if (excludeUserId.HasValue)
            command.Parameters.AddWithValue("$UserId", excludeUserId.Value);

        var duplicateCount = Convert.ToInt32(command.ExecuteScalar());
        if (duplicateCount > 0)
            throw new InvalidOperationException("Username already exists. Please enter a unique username.");
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static void MigrateSchema(SqliteConnection connection)
    {
        EnsureColumn(connection, "Users", "CompanyName", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Users", "CanEditCompletedTransaction", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "Users", "CanDeleteCompletedTransaction", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "DeviceSettings", "SelectedWeighbridgeCode", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Weighments", "CompanyName", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Weighments", "PartyAccount", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Weighments", "ItemNumber", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Weighments", "ItemName", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Weighments", "FirstWeightBy", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Weighments", "SecondWeightBy", "TEXT NOT NULL DEFAULT ''");

        // Vehicle master migration
        EnsureColumn(connection, "Vehicles", "PlateNumber", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "PlateEmirate", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "PlateCategory", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "VehicleType", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "OwnershipType", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "OwnerPartyAccount", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "Transporter", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "Capacity", "REAL NOT NULL DEFAULT 0");
        EnsureColumn(connection, "Vehicles", "DefaultDriver", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "RegistrationExpiryDate", "TEXT");
        EnsureColumn(connection, "Vehicles", "LegalEntity", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "Status", "TEXT NOT NULL DEFAULT 'Active'");
        EnsureColumn(connection, "Vehicles", "IsActive", "INTEGER NOT NULL DEFAULT 1");
        ExecuteNonQuery(connection, "UPDATE Vehicles SET PlateNumber = VehicleNo WHERE trim(ifnull(PlateNumber, '')) = '' AND trim(ifnull(VehicleNo, '')) <> ''; ");

        // Driver master migration
        EnsureColumn(connection, "Drivers", "MobileNumber", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "SecondaryMobile", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "Email", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "Nationality", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "DriverType", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "EmployerPartyType", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "EmployerAccount", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "IdentificationType", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "IdentificationNumber", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "IdentificationExpiryDate", "TEXT");
        EnsureColumn(connection, "Drivers", "EmiratesIdExpiryDate", "TEXT");
        EnsureColumn(connection, "Drivers", "PassportNumber", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "PassportExpiryDate", "TEXT");
        EnsureColumn(connection, "Drivers", "DrivingLicenceNumber", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "DrivingLicenceIssuedBy", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "DrivingLicenceExpiryDate", "TEXT");
        EnsureColumn(connection, "Drivers", "LicenceCategories", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "DefaultVehicle", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "Address", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "DriverPhoto", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "EmiratesIdAttachment", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "PassportAttachment", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "DrivingLicenceAttachment", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "LegalEntity", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "Status", "TEXT NOT NULL DEFAULT 'Active'");
        EnsureColumn(connection, "Drivers", "Blacklisted", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "Drivers", "BlacklistReason", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "EffectiveFrom", "TEXT");
        EnsureColumn(connection, "Drivers", "IsActive", "INTEGER NOT NULL DEFAULT 1");
        EnsureColumn(connection, "Drivers", "Remarks", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "CNIC", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "MobileNo", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Drivers", "LicenseNo", "TEXT NOT NULL DEFAULT ''");
        ExecuteNonQuery(connection, "UPDATE Drivers SET MobileNumber = MobileNo WHERE trim(ifnull(MobileNumber, '')) = '' AND trim(ifnull(MobileNo, '')) <> ''; ");
        ExecuteNonQuery(connection, "UPDATE Drivers SET IdentificationNumber = CNIC WHERE trim(ifnull(IdentificationNumber, '')) = '' AND trim(ifnull(CNIC, '')) <> ''; ");
        ExecuteNonQuery(connection, "UPDATE Drivers SET DrivingLicenceNumber = LicenseNo WHERE trim(ifnull(DrivingLicenceNumber, '')) = '' AND trim(ifnull(LicenseNo, '')) <> ''; ");


        // Weighbridge master migration
        EnsureColumn(connection, "WeighbridgeMasters", "Description", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "WarehouseAddress", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "MinimumWeight", "REAL");
        EnsureColumn(connection, "WeighbridgeMasters", "WeightIncrement", "REAL");
        EnsureColumn(connection, "WeighbridgeMasters", "WeightStabilityTime", "INTEGER");
        EnsureColumn(connection, "WeighbridgeMasters", "ScaleIpAddress", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "TcpPort", "INTEGER NOT NULL DEFAULT 4001");
        EnsureColumn(connection, "WeighbridgeMasters", "ScaleComPort", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "BaudRate", "INTEGER NOT NULL DEFAULT 9600");
        EnsureColumn(connection, "WeighbridgeMasters", "Parity", "TEXT NOT NULL DEFAULT 'None'");
        EnsureColumn(connection, "WeighbridgeMasters", "DataBits", "INTEGER NOT NULL DEFAULT 8");
        EnsureColumn(connection, "WeighbridgeMasters", "StopBits", "TEXT NOT NULL DEFAULT 'One'");
        EnsureColumn(connection, "WeighbridgeMasters", "ScaleManufacturer", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "ScaleModel", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "ScaleSerialNumber", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "CalibrationCertificateNo", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "LastCalibrationDate", "TEXT");
        EnsureColumn(connection, "WeighbridgeMasters", "NextCalibrationDate", "TEXT");
        EnsureColumn(connection, "WeighbridgeMasters", "Printer", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "CameraAvailable", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "WeighbridgeMasters", "AnprAvailable", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "WeighbridgeMasters", "TrafficLightAvailable", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "WeighbridgeMasters", "BoomBarrierAvailable", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "WeighbridgeMasters", "CctvAvailable", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "WeighbridgeMasters", "DefaultTicketTemplate", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "DefaultCurrency", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "DefaultOperator", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "AllowedOperators", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "WeighbridgeMasters", "OperatingStatus", "TEXT NOT NULL DEFAULT 'Active'");
        EnsureColumn(connection, "WeighbridgeMasters", "EffectiveFrom", "TEXT");
        EnsureColumn(connection, "WeighbridgeMasters", "IsActive", "INTEGER NOT NULL DEFAULT 1");
        EnsureColumn(connection, "WeighbridgeMasters", "Remarks", "TEXT NOT NULL DEFAULT ''");

        // Operator master migration
        EnsureColumn(connection, "OperatorMasters", "MobileNumber", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "OperatorMasters", "Designation", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "OperatorMasters", "Department", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "OperatorMasters", "LegalEntity", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "OperatorMasters", "DefaultLegalEntity", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "OperatorMasters", "DefaultWeighbridge", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "OperatorMasters", "AssignedWeighbridges", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "OperatorMasters", "DefaultShift", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "OperatorMasters", "Role", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "OperatorMasters", "PermissionProfile", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "OperatorMasters", "CanCaptureFirstWeight", "INTEGER NOT NULL DEFAULT 1");
        EnsureColumn(connection, "OperatorMasters", "CanCaptureSecondWeight", "INTEGER NOT NULL DEFAULT 1");
        EnsureColumn(connection, "OperatorMasters", "CanPerformManualWeightEntry", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "OperatorMasters", "CanCorrectTransactions", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "OperatorMasters", "CanCancelTransactions", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "OperatorMasters", "CanOverrideWeight", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "OperatorMasters", "CanApproveQc", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "OperatorMasters", "CanRetryIntegration", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "OperatorMasters", "LastLogin", "TEXT");
        EnsureColumn(connection, "OperatorMasters", "Status", "TEXT NOT NULL DEFAULT 'Active'");
        EnsureColumn(connection, "OperatorMasters", "EffectiveFrom", "TEXT");
        EnsureColumn(connection, "OperatorMasters", "IsActive", "INTEGER NOT NULL DEFAULT 1");
        EnsureColumn(connection, "OperatorMasters", "Remarks", "TEXT NOT NULL DEFAULT ''");

        ExecuteNonQuery(connection, "UPDATE Users SET CompanyName = 'Default Company' WHERE trim(ifnull(CompanyName, '')) = ''; ");
        ExecuteNonQuery(connection, "UPDATE Weighments SET FirstWeightBy = (SELECT Username FROM Users WHERE CAST(Users.UserId AS TEXT) = Weighments.FirstWeightBy LIMIT 1) WHERE trim(ifnull(FirstWeightBy, '')) <> '' AND EXISTS (SELECT 1 FROM Users WHERE CAST(Users.UserId AS TEXT) = Weighments.FirstWeightBy);");
        ExecuteNonQuery(connection, "UPDATE Weighments SET SecondWeightBy = (SELECT Username FROM Users WHERE CAST(Users.UserId AS TEXT) = Weighments.SecondWeightBy LIMIT 1) WHERE trim(ifnull(SecondWeightBy, '')) <> '' AND EXISTS (SELECT 1 FROM Users WHERE CAST(Users.UserId AS TEXT) = Weighments.SecondWeightBy);");
        ExecuteNonQuery(connection, "UPDATE Users SET CanEditCompletedTransaction = 1, CanDeleteCompletedTransaction = 1 WHERE lower(Username) = 'admin';");
        ExecuteNonQuery(connection, "UPDATE Weighments SET CompanyName = 'Default Company' WHERE trim(ifnull(CompanyName, '')) = ''; ");
        ExecuteNonQuery(connection, "UPDATE DeviceSettings SET SelectedWeighbridgeCode = 'WB-001' WHERE trim(ifnull(SelectedWeighbridgeCode, '')) = ''; ");
    }

    private static void EnsureColumn(SqliteConnection connection, string tableName, string columnName, string definition)
    {
        if (ColumnExists(connection, tableName, columnName))
            return;

        ExecuteNonQuery(connection, $"ALTER TABLE {tableName} ADD COLUMN {columnName} {definition};");
    }

    private static bool ColumnExists(SqliteConnection connection, string tableName, string columnName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName});";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var existingColumn = Convert.ToString(reader["name"]);
            if (string.Equals(existingColumn, columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static void EnsureDefaultSettings(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM DeviceSettings WHERE SettingId = 1";
        var count = Convert.ToInt32(command.ExecuteScalar());
        if (count > 0)
            return;

        using var insert = connection.CreateCommand();
        insert.CommandText = @"
INSERT INTO DeviceSettings
(SettingId, SelectedWeighbridgeCode, ConnectionType, ComPort, BaudRate, Parity, DataBits, StopBits, IpAddress, TcpPort)
VALUES
(1, 'WB-001', 'Mock', 'COM1', 9600, 'None', 8, 'One', '192.168.1.100', 4001);";
        insert.ExecuteNonQuery();
    }

    private static void EnsureDefaultAdminUser(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users";
        var count = Convert.ToInt32(command.ExecuteScalar());
        if (count > 0)
            return;

        var passwordData = PasswordService.HashPassword("admin123");

        using var insert = connection.CreateCommand();
        insert.CommandText = @"
INSERT INTO Users
(Username, FullName, CompanyName, PasswordHash, PasswordSalt, IsActive, CanAccessWeighment, CanAccessSettings, CanAccessMasters, CanAccessReports, CanAccessUserManagement, CanEditCompletedTransaction, CanDeleteCompletedTransaction, CreatedAt)
VALUES
('admin', 'System Administrator', 'Default Company', $PasswordHash, $PasswordSalt, 1, 1, 1, 1, 1, 1, 1, 1, $CreatedAt);";
        insert.Parameters.AddWithValue("$PasswordHash", passwordData.Hash);
        insert.Parameters.AddWithValue("$PasswordSalt", passwordData.Salt);
        insert.Parameters.AddWithValue("$CreatedAt", DateTime.Now.ToString("O"));
        insert.ExecuteNonQuery();
    }

    private static void SeedMasterData(SqliteConnection connection)
    {
        ExecuteNonQuery(connection, @"
INSERT OR IGNORE INTO Parties (PartyName, PartyType) VALUES ('Default Customer', 'Customer');
INSERT OR IGNORE INTO Parties (PartyName, PartyType) VALUES ('Default Vendor', 'Vendor');
INSERT OR IGNORE INTO Materials (MaterialName) VALUES ('General Material');
INSERT OR IGNORE INTO Customers (CustomerAccount, Name, CreatedAt) VALUES ('CUST-0001', 'Default Customer', datetime('now'));
INSERT OR IGNORE INTO Vendors (VendorAccount, Name, CreatedAt) VALUES ('VEND-0001', 'Default Vendor', datetime('now'));
INSERT OR IGNORE INTO ItemMasters (ItemNumber, ProductName, CreatedAt) VALUES ('ITEM-0001', 'General Item', datetime('now'));
INSERT OR IGNORE INTO Vehicles (VehicleNo, PlateNumber, PlateEmirate, PlateCategory, VehicleType, Status, IsActive) VALUES ('TEST-0001', 'TEST-0001', 'Dubai', 'Commercial', 'Truck', 'Active', 1);
INSERT OR IGNORE INTO Drivers (DriverName, MobileNumber, MobileNo, DriverType, EmployerPartyType, IdentificationType, IdentificationNumber, CNIC, DrivingLicenceNumber, LicenseNo, DrivingLicenceIssuedBy, DrivingLicenceExpiryDate, LegalEntity, Status, EffectiveFrom, IsActive) VALUES ('Default Driver', '0000000000', '0000000000', 'Company Driver', 'Legal Entity', 'Emirates ID', 'ID-0001', 'ID-0001', 'LIC-0001', 'LIC-0001', 'Dubai', date('now', '+1 year'), 'Default Company', 'Active', date('now'), 1);
INSERT OR IGNORE INTO WeighbridgeMasters (WeighbridgeCode, WeighbridgeName, PlantSite, Warehouse, WeighbridgeType, ScaleType, ScaleCapacity, CapacityUnit, CommunicationType, ScaleIpAddress, TcpPort, ScaleComPort, BaudRate, Parity, DataBits, StopBits, OperatingStatus, EffectiveFrom, IsActive, CreatedAt) VALUES ('WB-001', 'Default Weighbridge', 'Default Site', 'Default Warehouse', 'Bidirectional', 'Platform Scale', 100000, 'kg', 'Mock', '192.168.1.100', 4001, 'COM1', 9600, 'None', 8, 'One', 'Active', date('now'), 1, datetime('now'));
INSERT OR IGNORE INTO OperatorMasters (EmployeeId, OperatorName, Username, Email, Designation, Department, LegalEntity, DefaultWeighbridge, AssignedWeighbridges, Role, PermissionProfile, CanCaptureFirstWeight, CanCaptureSecondWeight, Status, EffectiveFrom, IsActive, CreatedAt) VALUES ('EMP-0001', 'System Administrator', 'admin', 'admin@bridgeone.local', 'Administrator', 'IT', 'Default Company', 'WB-001', 'WB-001', 'Administrator', 'Admin', 1, 1, 'Active', date('now'), 1, datetime('now'));
");
    }

    private static void AddUserPermissionParameters(SqliteCommand command, AppUser user)
    {
        command.Parameters.AddWithValue("$IsActive", user.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("$CanAccessWeighment", user.CanAccessWeighment ? 1 : 0);
        command.Parameters.AddWithValue("$CanAccessSettings", user.CanAccessSettings ? 1 : 0);
        command.Parameters.AddWithValue("$CanAccessMasters", user.CanAccessMasters ? 1 : 0);
        command.Parameters.AddWithValue("$CanAccessReports", user.CanAccessReports ? 1 : 0);
        command.Parameters.AddWithValue("$CanAccessUserManagement", user.CanAccessUserManagement ? 1 : 0);
        command.Parameters.AddWithValue("$CanEditCompletedTransaction", user.CanEditCompletedTransaction ? 1 : 0);
        command.Parameters.AddWithValue("$CanDeleteCompletedTransaction", user.CanDeleteCompletedTransaction ? 1 : 0);
    }


    private static string ReadText(SqliteDataReader reader, string columnName) =>
        Convert.ToString(reader[columnName]) ?? string.Empty;

    private static decimal? ReadDecimal(SqliteDataReader reader, string columnName)
    {
        var value = reader[columnName];
        if (value == DBNull.Value || value == null)
            return null;
        return decimal.TryParse(Convert.ToString(value), out var result) ? result : null;
    }

    private static int? ReadInt(SqliteDataReader reader, string columnName)
    {
        var value = reader[columnName];
        if (value == DBNull.Value || value == null)
            return null;
        return int.TryParse(Convert.ToString(value), out var result) ? result : null;
    }

    private static bool ReadBool(SqliteDataReader reader, string columnName)
    {
        var value = Convert.ToString(reader[columnName]) ?? string.Empty;
        return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    private static DateTime? ReadDate(SqliteDataReader reader, string columnName)
    {
        var value = Convert.ToString(reader[columnName]);
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return DateTime.TryParse(value, out var result) ? result : null;
    }

    private static object DbValue(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    private static object DbValue(decimal? value) => value.HasValue ? value.Value : DBNull.Value;
    private static object DbValue(int? value) => value.HasValue ? value.Value : DBNull.Value;
    private static object DbValue(DateTime? value) => value.HasValue ? value.Value.ToString("yyyy-MM-dd") : DBNull.Value;
    private static object DbValue(bool value) => value ? 1 : 0;

    private static void AddVehicleParameters(SqliteCommand command, Vehicle vehicle)
    {
        var plate = vehicle.PlateNumber.Trim().ToUpperInvariant();
        command.Parameters.AddWithValue("$VehicleNo", plate);
        command.Parameters.AddWithValue("$PlateNumber", plate);
        command.Parameters.AddWithValue("$PlateEmirate", DbValue(vehicle.PlateEmirate));
        command.Parameters.AddWithValue("$PlateCategory", DbValue(vehicle.PlateCategory));
        command.Parameters.AddWithValue("$VehicleType", DbValue(vehicle.VehicleType));
        command.Parameters.AddWithValue("$OwnershipType", DbValue(vehicle.OwnershipType));
        command.Parameters.AddWithValue("$OwnerPartyAccount", DbValue(vehicle.OwnerPartyAccount));
        command.Parameters.AddWithValue("$Transporter", DbValue(vehicle.Transporter));
        command.Parameters.AddWithValue("$Capacity", vehicle.Capacity);
        command.Parameters.AddWithValue("$DefaultDriver", DbValue(vehicle.DefaultDriver));
        command.Parameters.AddWithValue("$RegistrationExpiryDate", DbValue(vehicle.RegistrationExpiryDate));
        command.Parameters.AddWithValue("$LegalEntity", DbValue(vehicle.LegalEntity));
        command.Parameters.AddWithValue("$Status", string.IsNullOrWhiteSpace(vehicle.Status) ? "Active" : vehicle.Status.Trim());
        command.Parameters.AddWithValue("$IsActive", DbValue(vehicle.IsActive));
    }

    private static void AddDriverParameters(SqliteCommand command, Driver driver)
    {
        command.Parameters.AddWithValue("$DriverName", DbValue(driver.DriverName));
        command.Parameters.AddWithValue("$MobileNumber", DbValue(driver.MobileNumber));
        command.Parameters.AddWithValue("$SecondaryMobile", DbValue(driver.SecondaryMobile));
        command.Parameters.AddWithValue("$Email", DbValue(driver.Email));
        command.Parameters.AddWithValue("$Nationality", DbValue(driver.Nationality));
        command.Parameters.AddWithValue("$DriverType", DbValue(driver.DriverType));
        command.Parameters.AddWithValue("$EmployerPartyType", DbValue(driver.EmployerPartyType));
        command.Parameters.AddWithValue("$EmployerAccount", DbValue(driver.EmployerAccount));
        command.Parameters.AddWithValue("$IdentificationType", DbValue(driver.IdentificationType));
        command.Parameters.AddWithValue("$IdentificationNumber", DbValue(driver.IdentificationNumber));
        command.Parameters.AddWithValue("$IdentificationExpiryDate", DbValue(driver.IdentificationExpiryDate));
        command.Parameters.AddWithValue("$EmiratesIdExpiryDate", DbValue(driver.EmiratesIdExpiryDate));
        command.Parameters.AddWithValue("$PassportNumber", DbValue(driver.PassportNumber));
        command.Parameters.AddWithValue("$PassportExpiryDate", DbValue(driver.PassportExpiryDate));
        command.Parameters.AddWithValue("$DrivingLicenceNumber", DbValue(driver.DrivingLicenceNumber));
        command.Parameters.AddWithValue("$DrivingLicenceIssuedBy", DbValue(driver.DrivingLicenceIssuedBy));
        command.Parameters.AddWithValue("$DrivingLicenceExpiryDate", DbValue(driver.DrivingLicenceExpiryDate));
        command.Parameters.AddWithValue("$LicenceCategories", DbValue(driver.LicenceCategories));
        command.Parameters.AddWithValue("$DefaultVehicle", DbValue(driver.DefaultVehicle));
        command.Parameters.AddWithValue("$Address", DbValue(driver.Address));
        command.Parameters.AddWithValue("$DriverPhoto", DbValue(driver.DriverPhoto));
        command.Parameters.AddWithValue("$EmiratesIdAttachment", DbValue(driver.EmiratesIdAttachment));
        command.Parameters.AddWithValue("$PassportAttachment", DbValue(driver.PassportAttachment));
        command.Parameters.AddWithValue("$DrivingLicenceAttachment", DbValue(driver.DrivingLicenceAttachment));
        command.Parameters.AddWithValue("$LegalEntity", DbValue(driver.LegalEntity));
        command.Parameters.AddWithValue("$Status", string.IsNullOrWhiteSpace(driver.Status) ? "Active" : driver.Status.Trim());
        command.Parameters.AddWithValue("$Blacklisted", DbValue(driver.Blacklisted));
        command.Parameters.AddWithValue("$BlacklistReason", DbValue(driver.BlacklistReason));
        command.Parameters.AddWithValue("$EffectiveFrom", DbValue(driver.EffectiveFrom));
        command.Parameters.AddWithValue("$IsActive", DbValue(driver.IsActive));
        command.Parameters.AddWithValue("$Remarks", DbValue(driver.Remarks));
    }

    private static void AddCustomerParameters(SqliteCommand command, Customer customer)
    {
        command.Parameters.AddWithValue("$CustomerAccount", customer.CustomerAccount.Trim());
        command.Parameters.AddWithValue("$Name", customer.Name.Trim());
        command.Parameters.AddWithValue("$MethodOfPayment", customer.MethodOfPayment.Trim());
        command.Parameters.AddWithValue("$TermsOfPayment", customer.TermsOfPayment.Trim());
        command.Parameters.AddWithValue("$DeliveryTerms", customer.DeliveryTerms.Trim());
        command.Parameters.AddWithValue("$AccountStatus", customer.AccountStatus.Trim());
        command.Parameters.AddWithValue("$AccountStatusReason", customer.AccountStatusReason.Trim());
        command.Parameters.AddWithValue("$CustomerGroup", customer.CustomerGroup.Trim());
        command.Parameters.AddWithValue("$EmployeeResponsible", customer.EmployeeResponsible.Trim());
        command.Parameters.AddWithValue("$Currency", customer.Currency.Trim());
        command.Parameters.AddWithValue("$Telephone", customer.Telephone.Trim());
        command.Parameters.AddWithValue("$OrganizationPerson", customer.OrganizationPerson.Trim());
        command.Parameters.AddWithValue("$SearchName", customer.SearchName.Trim());
        command.Parameters.AddWithValue("$ClassificationGroup", customer.ClassificationGroup.Trim());
        command.Parameters.AddWithValue("$AddressNameDescription", customer.AddressNameDescription.Trim());
        command.Parameters.AddWithValue("$Address", customer.Address.Trim());
        command.Parameters.AddWithValue("$AddressPurpose", customer.AddressPurpose.Trim());
        command.Parameters.AddWithValue("$ContactDescription", customer.ContactDescription.Trim());
        command.Parameters.AddWithValue("$ContactType", customer.ContactType.Trim());
        command.Parameters.AddWithValue("$ContactNumberAddress", customer.ContactNumberAddress.Trim());
        command.Parameters.AddWithValue("$ContactExtension", customer.ContactExtension.Trim());
        command.Parameters.AddWithValue("$InvoiceAccount", customer.InvoiceAccount.Trim());
        command.Parameters.AddWithValue("$ModeOfDelivery", customer.ModeOfDelivery.Trim());
        command.Parameters.AddWithValue("$SalesTaxGroup", customer.SalesTaxGroup.Trim());
    }

    private static void AddVendorParameters(SqliteCommand command, Vendor vendor)
    {
        command.Parameters.AddWithValue("$VendorAccount", vendor.VendorAccount.Trim());
        command.Parameters.AddWithValue("$Name", vendor.Name.Trim());
        command.Parameters.AddWithValue("$MethodOfPayment", vendor.MethodOfPayment.Trim());
        command.Parameters.AddWithValue("$TermsOfPayment", vendor.TermsOfPayment.Trim());
        command.Parameters.AddWithValue("$DeliveryTerms", vendor.DeliveryTerms.Trim());
        command.Parameters.AddWithValue("$AccountStatus", vendor.AccountStatus.Trim());
        command.Parameters.AddWithValue("$AccountStatusReason", vendor.AccountStatusReason.Trim());
        command.Parameters.AddWithValue("$VendorGroup", vendor.VendorGroup.Trim());
        command.Parameters.AddWithValue("$EmployeeResponsible", vendor.EmployeeResponsible.Trim());
        command.Parameters.AddWithValue("$Currency", vendor.Currency.Trim());
        command.Parameters.AddWithValue("$Telephone", vendor.Telephone.Trim());
        command.Parameters.AddWithValue("$Type", vendor.Type.Trim());
        command.Parameters.AddWithValue("$VendorClassificationGroup", vendor.VendorClassificationGroup.Trim());
        command.Parameters.AddWithValue("$SearchName", vendor.SearchName.Trim());
        command.Parameters.AddWithValue("$AddressNameDescription", vendor.AddressNameDescription.Trim());
        command.Parameters.AddWithValue("$Address", vendor.Address.Trim());
        command.Parameters.AddWithValue("$AddressPurpose", vendor.AddressPurpose.Trim());
        command.Parameters.AddWithValue("$ContactDescription", vendor.ContactDescription.Trim());
        command.Parameters.AddWithValue("$ContactType", vendor.ContactType.Trim());
        command.Parameters.AddWithValue("$ContactNumberAddress", vendor.ContactNumberAddress.Trim());
        command.Parameters.AddWithValue("$ContactExtension", vendor.ContactExtension.Trim());
        command.Parameters.AddWithValue("$InvoiceAccount", vendor.InvoiceAccount.Trim());
        command.Parameters.AddWithValue("$ModeOfDelivery", vendor.ModeOfDelivery.Trim());
        command.Parameters.AddWithValue("$SalesTaxGroup", vendor.SalesTaxGroup.Trim());
    }

    private static void AddItemMasterParameters(SqliteCommand command, ItemMaster item)
    {
        command.Parameters.AddWithValue("$ItemNumber", DbValue(item.ItemNumber));
        command.Parameters.AddWithValue("$ProductName", DbValue(item.ProductName));
        command.Parameters.AddWithValue("$SearchName", DbValue(item.SearchName));
        command.Parameters.AddWithValue("$ProductType", DbValue(item.ProductType));
        command.Parameters.AddWithValue("$ProductSubtype", DbValue(item.ProductSubtype));
        command.Parameters.AddWithValue("$ProductNumber", DbValue(item.ProductNumber));
        command.Parameters.AddWithValue("$Description", DbValue(item.Description));
        command.Parameters.AddWithValue("$StorageDimensionGroup", DbValue(item.StorageDimensionGroup));
        command.Parameters.AddWithValue("$TrackingDimensionGroup", DbValue(item.TrackingDimensionGroup));
        command.Parameters.AddWithValue("$ItemModelGroup", DbValue(item.ItemModelGroup));
        command.Parameters.AddWithValue("$ReservationHierarchy", DbValue(item.ReservationHierarchy));
        command.Parameters.AddWithValue("$PurchaseUnit", DbValue(item.PurchaseUnit));
        command.Parameters.AddWithValue("$PurchaseOverDelivery", DbValue(item.PurchaseOverDelivery));
        command.Parameters.AddWithValue("$PurchaseUnderDelivery", DbValue(item.PurchaseUnderDelivery));
        command.Parameters.AddWithValue("$BuyerGroup", DbValue(item.BuyerGroup));
        command.Parameters.AddWithValue("$ItemPriceToleranceGroup", DbValue(item.ItemPriceToleranceGroup));
        command.Parameters.AddWithValue("$Vendor", DbValue(item.Vendor));
        command.Parameters.AddWithValue("$PurchaseItemSalesTaxGroup", DbValue(item.PurchaseItemSalesTaxGroup));
        command.Parameters.AddWithValue("$SellUnit", DbValue(item.SellUnit));
        command.Parameters.AddWithValue("$SellOverDelivery", DbValue(item.SellOverDelivery));
        command.Parameters.AddWithValue("$SellUnderDelivery", DbValue(item.SellUnderDelivery));
        command.Parameters.AddWithValue("$SellItemSalesTaxGroup", DbValue(item.SellItemSalesTaxGroup));
        command.Parameters.AddWithValue("$BatchNumberGroup", DbValue(item.BatchNumberGroup));
        command.Parameters.AddWithValue("$SerialNumberGroup", DbValue(item.SerialNumberGroup));
        command.Parameters.AddWithValue("$InventoryOverDelivery", DbValue(item.InventoryOverDelivery));
        command.Parameters.AddWithValue("$InventoryUnderDelivery", DbValue(item.InventoryUnderDelivery));
        command.Parameters.AddWithValue("$CatchWeightItem", DbValue(item.CatchWeightItem));
        command.Parameters.AddWithValue("$CWUnit", DbValue(item.CWUnit));
        command.Parameters.AddWithValue("$NominalQuantity", DbValue(item.NominalQuantity));
        command.Parameters.AddWithValue("$MinimumQuantity", DbValue(item.MinimumQuantity));
        command.Parameters.AddWithValue("$MaximumQuantity", DbValue(item.MaximumQuantity));
        command.Parameters.AddWithValue("$BOMUnit", DbValue(item.BOMUnit));
        command.Parameters.AddWithValue("$ConstantScrap", DbValue(item.ConstantScrap));
        command.Parameters.AddWithValue("$VariableScrap", DbValue(item.VariableScrap));
        command.Parameters.AddWithValue("$CostingLevel", DbValue(item.CostingLevel));
        command.Parameters.AddWithValue("$PlanningLevel", DbValue(item.PlanningLevel));
        command.Parameters.AddWithValue("$CostCalculationLevel", DbValue(item.CostCalculationLevel));
        command.Parameters.AddWithValue("$Phantom", DbValue(item.Phantom));
        command.Parameters.AddWithValue("$CalculationGroup", DbValue(item.CalculationGroup));
        command.Parameters.AddWithValue("$ProductionType", DbValue(item.ProductionType));
        command.Parameters.AddWithValue("$ItemGroup", DbValue(item.ItemGroup));
        command.Parameters.AddWithValue("$CostUnit", DbValue(item.CostUnit));
        command.Parameters.AddWithValue("$LastCostPrice", DbValue(item.LastCostPrice));
        command.Parameters.AddWithValue("$DateOfPrice", DbValue(item.DateOfPrice));
        command.Parameters.AddWithValue("$UnitSequenceGroupId", DbValue(item.UnitSequenceGroupId));
    }

    private static void AddWarehouseMasterParameters(SqliteCommand command, WarehouseMaster warehouse)
    {
        command.Parameters.AddWithValue("$Warehouse", warehouse.Warehouse.Trim());
        command.Parameters.AddWithValue("$Name", warehouse.Name.Trim());
        command.Parameters.AddWithValue("$Site", warehouse.Site.Trim());
        command.Parameters.AddWithValue("$Type", warehouse.Type.Trim());
        command.Parameters.AddWithValue("$QuarantineWarehouse", warehouse.QuarantineWarehouse.Trim());
        command.Parameters.AddWithValue("$TransitWarehouse", warehouse.TransitWarehouse.Trim());
        command.Parameters.AddWithValue("$GoodsInTransitWarehouse", warehouse.GoodsInTransitWarehouse.Trim());
        command.Parameters.AddWithValue("$UnderDeliveryWarehouse", warehouse.UnderDeliveryWarehouse.Trim());
        command.Parameters.AddWithValue("$VendorAccount", warehouse.VendorAccount.Trim());
        command.Parameters.AddWithValue("$DefaultReceiptLocation", warehouse.DefaultReceiptLocation.Trim());
        command.Parameters.AddWithValue("$DefaultIssueLocation", warehouse.DefaultIssueLocation.Trim());
        command.Parameters.AddWithValue("$DefaultProductionFinishedGood", warehouse.DefaultProductionFinishedGood.Trim());
        command.Parameters.AddWithValue("$AddressNameDescription", warehouse.AddressNameDescription.Trim());
        command.Parameters.AddWithValue("$Address", warehouse.Address.Trim());
        command.Parameters.AddWithValue("$Purpose", warehouse.Purpose.Trim());
    }


    private static void AddWeighbridgeMasterParameters(SqliteCommand command, WeighbridgeMaster weighbridge)
    {
        command.Parameters.AddWithValue("$WeighbridgeCode", DbValue(weighbridge.WeighbridgeCode));
        command.Parameters.AddWithValue("$WeighbridgeName", DbValue(weighbridge.WeighbridgeName));
        command.Parameters.AddWithValue("$Description", DbValue(weighbridge.Description));
        command.Parameters.AddWithValue("$PlantSite", DbValue(weighbridge.PlantSite));
        command.Parameters.AddWithValue("$Warehouse", DbValue(weighbridge.Warehouse));
        command.Parameters.AddWithValue("$WarehouseAddress", DbValue(weighbridge.WarehouseAddress));
        command.Parameters.AddWithValue("$WeighbridgeType", DbValue(weighbridge.WeighbridgeType));
        command.Parameters.AddWithValue("$ScaleType", DbValue(weighbridge.ScaleType));
        command.Parameters.AddWithValue("$ScaleCapacity", weighbridge.ScaleCapacity);
        command.Parameters.AddWithValue("$CapacityUnit", DbValue(weighbridge.CapacityUnit));
        command.Parameters.AddWithValue("$MinimumWeight", DbValue(weighbridge.MinimumWeight));
        command.Parameters.AddWithValue("$WeightIncrement", DbValue(weighbridge.WeightIncrement));
        command.Parameters.AddWithValue("$WeightStabilityTime", DbValue(weighbridge.WeightStabilityTime));
        command.Parameters.AddWithValue("$ScaleIpAddress", DbValue(weighbridge.ScaleIpAddress));
        command.Parameters.AddWithValue("$TcpPort", weighbridge.TcpPort);
        command.Parameters.AddWithValue("$ScaleComPort", DbValue(weighbridge.ScaleComPort));
        command.Parameters.AddWithValue("$BaudRate", weighbridge.BaudRate);
        command.Parameters.AddWithValue("$Parity", DbValue(weighbridge.Parity));
        command.Parameters.AddWithValue("$DataBits", weighbridge.DataBits);
        command.Parameters.AddWithValue("$StopBits", DbValue(weighbridge.StopBits));
        command.Parameters.AddWithValue("$CommunicationType", DbValue(weighbridge.CommunicationType));
        command.Parameters.AddWithValue("$ScaleManufacturer", DbValue(weighbridge.ScaleManufacturer));
        command.Parameters.AddWithValue("$ScaleModel", DbValue(weighbridge.ScaleModel));
        command.Parameters.AddWithValue("$ScaleSerialNumber", DbValue(weighbridge.ScaleSerialNumber));
        command.Parameters.AddWithValue("$CalibrationCertificateNo", DbValue(weighbridge.CalibrationCertificateNo));
        command.Parameters.AddWithValue("$LastCalibrationDate", DbValue(weighbridge.LastCalibrationDate));
        command.Parameters.AddWithValue("$NextCalibrationDate", DbValue(weighbridge.NextCalibrationDate));
        command.Parameters.AddWithValue("$Printer", DbValue(weighbridge.Printer));
        command.Parameters.AddWithValue("$CameraAvailable", DbValue(weighbridge.CameraAvailable));
        command.Parameters.AddWithValue("$AnprAvailable", DbValue(weighbridge.AnprAvailable));
        command.Parameters.AddWithValue("$TrafficLightAvailable", DbValue(weighbridge.TrafficLightAvailable));
        command.Parameters.AddWithValue("$BoomBarrierAvailable", DbValue(weighbridge.BoomBarrierAvailable));
        command.Parameters.AddWithValue("$CctvAvailable", DbValue(weighbridge.CctvAvailable));
        command.Parameters.AddWithValue("$DefaultTicketTemplate", DbValue(weighbridge.DefaultTicketTemplate));
        command.Parameters.AddWithValue("$DefaultCurrency", DbValue(weighbridge.DefaultCurrency));
        command.Parameters.AddWithValue("$DefaultOperator", DbValue(weighbridge.DefaultOperator));
        command.Parameters.AddWithValue("$AllowedOperators", DbValue(weighbridge.AllowedOperators));
        command.Parameters.AddWithValue("$OperatingStatus", DbValue(weighbridge.OperatingStatus));
        command.Parameters.AddWithValue("$EffectiveFrom", DbValue(weighbridge.EffectiveFrom));
        command.Parameters.AddWithValue("$IsActive", DbValue(weighbridge.IsActive));
        command.Parameters.AddWithValue("$Remarks", DbValue(weighbridge.Remarks));
    }

    private static void AddOperatorMasterParameters(SqliteCommand command, OperatorMaster operatorMaster)
    {
        command.Parameters.AddWithValue("$EmployeeId", DbValue(operatorMaster.EmployeeId));
        command.Parameters.AddWithValue("$OperatorName", DbValue(operatorMaster.OperatorName));
        command.Parameters.AddWithValue("$Username", DbValue(operatorMaster.Username));
        command.Parameters.AddWithValue("$Email", DbValue(operatorMaster.Email));
        command.Parameters.AddWithValue("$MobileNumber", DbValue(operatorMaster.MobileNumber));
        command.Parameters.AddWithValue("$Designation", DbValue(operatorMaster.Designation));
        command.Parameters.AddWithValue("$Department", DbValue(operatorMaster.Department));
        command.Parameters.AddWithValue("$LegalEntity", DbValue(operatorMaster.LegalEntity));
        command.Parameters.AddWithValue("$DefaultLegalEntity", DbValue(operatorMaster.DefaultLegalEntity));
        command.Parameters.AddWithValue("$DefaultWeighbridge", DbValue(operatorMaster.DefaultWeighbridge));
        command.Parameters.AddWithValue("$AssignedWeighbridges", DbValue(operatorMaster.AssignedWeighbridges));
        command.Parameters.AddWithValue("$DefaultShift", DbValue(operatorMaster.DefaultShift));
        command.Parameters.AddWithValue("$Role", DbValue(operatorMaster.Role));
        command.Parameters.AddWithValue("$PermissionProfile", DbValue(operatorMaster.PermissionProfile));
        command.Parameters.AddWithValue("$CanCaptureFirstWeight", DbValue(operatorMaster.CanCaptureFirstWeight));
        command.Parameters.AddWithValue("$CanCaptureSecondWeight", DbValue(operatorMaster.CanCaptureSecondWeight));
        command.Parameters.AddWithValue("$CanPerformManualWeightEntry", DbValue(operatorMaster.CanPerformManualWeightEntry));
        command.Parameters.AddWithValue("$CanCorrectTransactions", DbValue(operatorMaster.CanCorrectTransactions));
        command.Parameters.AddWithValue("$CanCancelTransactions", DbValue(operatorMaster.CanCancelTransactions));
        command.Parameters.AddWithValue("$CanOverrideWeight", DbValue(operatorMaster.CanOverrideWeight));
        command.Parameters.AddWithValue("$CanApproveQc", DbValue(operatorMaster.CanApproveQc));
        command.Parameters.AddWithValue("$CanRetryIntegration", DbValue(operatorMaster.CanRetryIntegration));
        command.Parameters.AddWithValue("$LastLogin", DbValue(operatorMaster.LastLogin));
        command.Parameters.AddWithValue("$Status", DbValue(operatorMaster.Status));
        command.Parameters.AddWithValue("$EffectiveFrom", DbValue(operatorMaster.EffectiveFrom));
        command.Parameters.AddWithValue("$IsActive", DbValue(operatorMaster.IsActive));
        command.Parameters.AddWithValue("$Remarks", DbValue(operatorMaster.Remarks));
    }

    private static WeighbridgeMaster MapWeighbridgeMaster(SqliteDataReader reader) => new()
    {
        WeighbridgeId = Convert.ToInt32(reader["WeighbridgeId"]),
        WeighbridgeCode = ReadText(reader, "WeighbridgeCode"),
        WeighbridgeName = ReadText(reader, "WeighbridgeName"),
        Description = ReadText(reader, "Description"),
        PlantSite = ReadText(reader, "PlantSite"),
        Warehouse = ReadText(reader, "Warehouse"),
        WarehouseAddress = ReadText(reader, "WarehouseAddress"),
        WeighbridgeType = ReadText(reader, "WeighbridgeType"),
        ScaleType = ReadText(reader, "ScaleType"),
        ScaleCapacity = ReadDecimal(reader, "ScaleCapacity") ?? 0m,
        CapacityUnit = ReadText(reader, "CapacityUnit"),
        MinimumWeight = ReadDecimal(reader, "MinimumWeight"),
        WeightIncrement = ReadDecimal(reader, "WeightIncrement"),
        WeightStabilityTime = ReadInt(reader, "WeightStabilityTime"),
        ScaleIpAddress = ReadText(reader, "ScaleIpAddress"),
        TcpPort = ReadInt(reader, "TcpPort") ?? 4001,
        ScaleComPort = ReadText(reader, "ScaleComPort"),
        BaudRate = ReadInt(reader, "BaudRate") ?? 9600,
        Parity = ReadText(reader, "Parity"),
        DataBits = ReadInt(reader, "DataBits") ?? 8,
        StopBits = ReadText(reader, "StopBits"),
        CommunicationType = ReadText(reader, "CommunicationType"),
        ScaleManufacturer = ReadText(reader, "ScaleManufacturer"),
        ScaleModel = ReadText(reader, "ScaleModel"),
        ScaleSerialNumber = ReadText(reader, "ScaleSerialNumber"),
        CalibrationCertificateNo = ReadText(reader, "CalibrationCertificateNo"),
        LastCalibrationDate = ReadDate(reader, "LastCalibrationDate"),
        NextCalibrationDate = ReadDate(reader, "NextCalibrationDate"),
        Printer = ReadText(reader, "Printer"),
        CameraAvailable = ReadBool(reader, "CameraAvailable"),
        AnprAvailable = ReadBool(reader, "AnprAvailable"),
        TrafficLightAvailable = ReadBool(reader, "TrafficLightAvailable"),
        BoomBarrierAvailable = ReadBool(reader, "BoomBarrierAvailable"),
        CctvAvailable = ReadBool(reader, "CctvAvailable"),
        DefaultTicketTemplate = ReadText(reader, "DefaultTicketTemplate"),
        DefaultCurrency = ReadText(reader, "DefaultCurrency"),
        DefaultOperator = ReadText(reader, "DefaultOperator"),
        AllowedOperators = ReadText(reader, "AllowedOperators"),
        OperatingStatus = ReadText(reader, "OperatingStatus"),
        EffectiveFrom = ReadDate(reader, "EffectiveFrom"),
        IsActive = ReadBool(reader, "IsActive"),
        Remarks = ReadText(reader, "Remarks")
    };

    private static OperatorMaster MapOperatorMaster(SqliteDataReader reader) => new()
    {
        OperatorId = Convert.ToInt32(reader["OperatorId"]),
        EmployeeId = ReadText(reader, "EmployeeId"),
        OperatorName = ReadText(reader, "OperatorName"),
        Username = ReadText(reader, "Username"),
        Email = ReadText(reader, "Email"),
        MobileNumber = ReadText(reader, "MobileNumber"),
        Designation = ReadText(reader, "Designation"),
        Department = ReadText(reader, "Department"),
        LegalEntity = ReadText(reader, "LegalEntity"),
        DefaultLegalEntity = ReadText(reader, "DefaultLegalEntity"),
        DefaultWeighbridge = ReadText(reader, "DefaultWeighbridge"),
        AssignedWeighbridges = ReadText(reader, "AssignedWeighbridges"),
        DefaultShift = ReadText(reader, "DefaultShift"),
        Role = ReadText(reader, "Role"),
        PermissionProfile = ReadText(reader, "PermissionProfile"),
        CanCaptureFirstWeight = ReadBool(reader, "CanCaptureFirstWeight"),
        CanCaptureSecondWeight = ReadBool(reader, "CanCaptureSecondWeight"),
        CanPerformManualWeightEntry = ReadBool(reader, "CanPerformManualWeightEntry"),
        CanCorrectTransactions = ReadBool(reader, "CanCorrectTransactions"),
        CanCancelTransactions = ReadBool(reader, "CanCancelTransactions"),
        CanOverrideWeight = ReadBool(reader, "CanOverrideWeight"),
        CanApproveQc = ReadBool(reader, "CanApproveQc"),
        CanRetryIntegration = ReadBool(reader, "CanRetryIntegration"),
        LastLogin = ReadDate(reader, "LastLogin"),
        Status = ReadText(reader, "Status"),
        EffectiveFrom = ReadDate(reader, "EffectiveFrom"),
        IsActive = ReadBool(reader, "IsActive"),
        Remarks = ReadText(reader, "Remarks")
    };

    private static Customer MapCustomer(SqliteDataReader reader) => new()
    {
        CustomerId = Convert.ToInt32(reader["CustomerId"]),
        CustomerAccount = ReadText(reader, "CustomerAccount"),
        Name = ReadText(reader, "Name"),
        MethodOfPayment = ReadText(reader, "MethodOfPayment"),
        TermsOfPayment = ReadText(reader, "TermsOfPayment"),
        DeliveryTerms = ReadText(reader, "DeliveryTerms"),
        AccountStatus = ReadText(reader, "AccountStatus"),
        AccountStatusReason = ReadText(reader, "AccountStatusReason"),
        CustomerGroup = ReadText(reader, "CustomerGroup"),
        EmployeeResponsible = ReadText(reader, "EmployeeResponsible"),
        Currency = ReadText(reader, "Currency"),
        Telephone = ReadText(reader, "Telephone"),
        OrganizationPerson = ReadText(reader, "OrganizationPerson"),
        SearchName = ReadText(reader, "SearchName"),
        ClassificationGroup = ReadText(reader, "ClassificationGroup"),
        AddressNameDescription = ReadText(reader, "AddressNameDescription"),
        Address = ReadText(reader, "Address"),
        AddressPurpose = ReadText(reader, "AddressPurpose"),
        ContactDescription = ReadText(reader, "ContactDescription"),
        ContactType = ReadText(reader, "ContactType"),
        ContactNumberAddress = ReadText(reader, "ContactNumberAddress"),
        ContactExtension = ReadText(reader, "ContactExtension"),
        InvoiceAccount = ReadText(reader, "InvoiceAccount"),
        ModeOfDelivery = ReadText(reader, "ModeOfDelivery"),
        SalesTaxGroup = ReadText(reader, "SalesTaxGroup")
    };

    private static Vendor MapVendor(SqliteDataReader reader) => new()
    {
        VendorId = Convert.ToInt32(reader["VendorId"]),
        VendorAccount = ReadText(reader, "VendorAccount"),
        Name = ReadText(reader, "Name"),
        MethodOfPayment = ReadText(reader, "MethodOfPayment"),
        TermsOfPayment = ReadText(reader, "TermsOfPayment"),
        DeliveryTerms = ReadText(reader, "DeliveryTerms"),
        AccountStatus = ReadText(reader, "AccountStatus"),
        AccountStatusReason = ReadText(reader, "AccountStatusReason"),
        VendorGroup = ReadText(reader, "VendorGroup"),
        EmployeeResponsible = ReadText(reader, "EmployeeResponsible"),
        Currency = ReadText(reader, "Currency"),
        Telephone = ReadText(reader, "Telephone"),
        Type = ReadText(reader, "Type"),
        VendorClassificationGroup = ReadText(reader, "VendorClassificationGroup"),
        SearchName = ReadText(reader, "SearchName"),
        AddressNameDescription = ReadText(reader, "AddressNameDescription"),
        Address = ReadText(reader, "Address"),
        AddressPurpose = ReadText(reader, "AddressPurpose"),
        ContactDescription = ReadText(reader, "ContactDescription"),
        ContactType = ReadText(reader, "ContactType"),
        ContactNumberAddress = ReadText(reader, "ContactNumberAddress"),
        ContactExtension = ReadText(reader, "ContactExtension"),
        InvoiceAccount = ReadText(reader, "InvoiceAccount"),
        ModeOfDelivery = ReadText(reader, "ModeOfDelivery"),
        SalesTaxGroup = ReadText(reader, "SalesTaxGroup")
    };

    private static ItemMaster MapItemMaster(SqliteDataReader reader) => new()
    {
        ItemMasterId = Convert.ToInt32(reader["ItemMasterId"]),
        ItemNumber = ReadText(reader, "ItemNumber"),
        ProductName = ReadText(reader, "ProductName"),
        SearchName = ReadText(reader, "SearchName"),
        ProductType = ReadText(reader, "ProductType"),
        ProductSubtype = ReadText(reader, "ProductSubtype"),
        ProductNumber = ReadText(reader, "ProductNumber"),
        Description = ReadText(reader, "Description"),
        StorageDimensionGroup = ReadText(reader, "StorageDimensionGroup"),
        TrackingDimensionGroup = ReadText(reader, "TrackingDimensionGroup"),
        ItemModelGroup = ReadText(reader, "ItemModelGroup"),
        ReservationHierarchy = ReadText(reader, "ReservationHierarchy"),
        PurchaseUnit = ReadText(reader, "PurchaseUnit"),
        PurchaseOverDelivery = ReadDecimal(reader, "PurchaseOverDelivery"),
        PurchaseUnderDelivery = ReadDecimal(reader, "PurchaseUnderDelivery"),
        BuyerGroup = ReadText(reader, "BuyerGroup"),
        ItemPriceToleranceGroup = ReadText(reader, "ItemPriceToleranceGroup"),
        Vendor = ReadText(reader, "Vendor"),
        PurchaseItemSalesTaxGroup = ReadText(reader, "PurchaseItemSalesTaxGroup"),
        SellUnit = ReadText(reader, "SellUnit"),
        SellOverDelivery = ReadDecimal(reader, "SellOverDelivery"),
        SellUnderDelivery = ReadDecimal(reader, "SellUnderDelivery"),
        SellItemSalesTaxGroup = ReadText(reader, "SellItemSalesTaxGroup"),
        BatchNumberGroup = ReadText(reader, "BatchNumberGroup"),
        SerialNumberGroup = ReadText(reader, "SerialNumberGroup"),
        InventoryOverDelivery = ReadDecimal(reader, "InventoryOverDelivery"),
        InventoryUnderDelivery = ReadDecimal(reader, "InventoryUnderDelivery"),
        CatchWeightItem = ReadBool(reader, "CatchWeightItem"),
        CWUnit = ReadText(reader, "CWUnit"),
        NominalQuantity = ReadDecimal(reader, "NominalQuantity"),
        MinimumQuantity = ReadDecimal(reader, "MinimumQuantity"),
        MaximumQuantity = ReadDecimal(reader, "MaximumQuantity"),
        BOMUnit = ReadText(reader, "BOMUnit"),
        ConstantScrap = ReadDecimal(reader, "ConstantScrap"),
        VariableScrap = ReadDecimal(reader, "VariableScrap"),
        CostingLevel = ReadInt(reader, "CostingLevel"),
        PlanningLevel = ReadInt(reader, "PlanningLevel"),
        CostCalculationLevel = ReadInt(reader, "CostCalculationLevel"),
        Phantom = ReadBool(reader, "Phantom"),
        CalculationGroup = ReadText(reader, "CalculationGroup"),
        ProductionType = ReadText(reader, "ProductionType"),
        ItemGroup = ReadText(reader, "ItemGroup"),
        CostUnit = ReadText(reader, "CostUnit"),
        LastCostPrice = ReadDecimal(reader, "LastCostPrice"),
        DateOfPrice = ReadDate(reader, "DateOfPrice"),
        UnitSequenceGroupId = ReadText(reader, "UnitSequenceGroupId")
    };

    private static WarehouseMaster MapWarehouseMaster(SqliteDataReader reader) => new()
    {
        WarehouseMasterId = Convert.ToInt32(reader["WarehouseMasterId"]),
        Warehouse = ReadText(reader, "Warehouse"),
        Name = ReadText(reader, "Name"),
        Site = ReadText(reader, "Site"),
        Type = ReadText(reader, "Type"),
        QuarantineWarehouse = ReadText(reader, "QuarantineWarehouse"),
        TransitWarehouse = ReadText(reader, "TransitWarehouse"),
        GoodsInTransitWarehouse = ReadText(reader, "GoodsInTransitWarehouse"),
        UnderDeliveryWarehouse = ReadText(reader, "UnderDeliveryWarehouse"),
        VendorAccount = ReadText(reader, "VendorAccount"),
        DefaultReceiptLocation = ReadText(reader, "DefaultReceiptLocation"),
        DefaultIssueLocation = ReadText(reader, "DefaultIssueLocation"),
        DefaultProductionFinishedGood = ReadText(reader, "DefaultProductionFinishedGood"),
        AddressNameDescription = ReadText(reader, "AddressNameDescription"),
        Address = ReadText(reader, "Address"),
        Purpose = ReadText(reader, "Purpose")
    };

    private static AppUser MapUser(SqliteDataReader reader) => new()
    {
        UserId = Convert.ToInt32(reader["UserId"]),
        Username = Convert.ToString(reader["Username"]) ?? string.Empty,
        FullName = Convert.ToString(reader["FullName"]) ?? string.Empty,
        CompanyName = Convert.ToString(reader["CompanyName"]) ?? string.Empty,
        IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
        CanAccessWeighment = Convert.ToInt32(reader["CanAccessWeighment"]) == 1,
        CanAccessSettings = Convert.ToInt32(reader["CanAccessSettings"]) == 1,
        CanAccessMasters = Convert.ToInt32(reader["CanAccessMasters"]) == 1,
        CanAccessReports = Convert.ToInt32(reader["CanAccessReports"]) == 1,
        CanAccessUserManagement = Convert.ToInt32(reader["CanAccessUserManagement"]) == 1,
        CanEditCompletedTransaction = Convert.ToInt32(reader["CanEditCompletedTransaction"]) == 1,
        CanDeleteCompletedTransaction = Convert.ToInt32(reader["CanDeleteCompletedTransaction"]) == 1,
        CreatedAt = DateTime.Parse(Convert.ToString(reader["CreatedAt"]) ?? DateTime.MinValue.ToString("O"))
    };

    private static DeviceSettings MapDeviceSettings(SqliteDataReader reader) => new()
    {
        SettingId = Convert.ToInt32(reader["SettingId"]),
        SelectedWeighbridgeCode = ReadText(reader, "SelectedWeighbridgeCode"),
        ConnectionType = Convert.ToString(reader["ConnectionType"]) ?? "Mock",
        ComPort = Convert.ToString(reader["ComPort"]) ?? "COM1",
        BaudRate = Convert.ToInt32(reader["BaudRate"]),
        Parity = Convert.ToString(reader["Parity"]) ?? "None",
        DataBits = Convert.ToInt32(reader["DataBits"]),
        StopBits = Convert.ToString(reader["StopBits"]) ?? "One",
        IpAddress = Convert.ToString(reader["IpAddress"]) ?? "192.168.1.100",
        TcpPort = Convert.ToInt32(reader["TcpPort"])
    };

    private static List<Weighment> ReadWeighments(SqliteCommand command)
    {
        var result = new List<Weighment>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(MapWeighment(reader));
        return result;
    }

    private static string ReadWeighmentText(SqliteDataReader reader, string columnName)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                return reader.IsDBNull(i) ? string.Empty : Convert.ToString(reader.GetValue(i)) ?? string.Empty;
        }

        return string.Empty;
    }

    private static Weighment MapWeighment(SqliteDataReader reader)
    {
        return new Weighment
        {
            WeighmentId = Convert.ToInt32(reader["WeighmentId"]),
            TicketNo = Convert.ToString(reader["TicketNo"]) ?? string.Empty,
            CompanyName = Convert.ToString(reader["CompanyName"]) ?? string.Empty,
            VehicleNo = Convert.ToString(reader["VehicleNo"]) ?? string.Empty,
            DriverName = Convert.ToString(reader["DriverName"]) ?? string.Empty,
            PartyId = reader["PartyId"] == DBNull.Value ? null : Convert.ToInt32(reader["PartyId"]),
            PartyAccount = ReadWeighmentText(reader, "PartyAccount"),
            PartyName = Convert.ToString(reader["PartyName"]) ?? string.Empty,
            PartyType = Convert.ToString(reader["PartyType"]) ?? string.Empty,
            MaterialId = reader["MaterialId"] == DBNull.Value ? null : Convert.ToInt32(reader["MaterialId"]),
            ItemNumber = ReadWeighmentText(reader, "ItemNumber"),
            ItemName = string.IsNullOrWhiteSpace(ReadWeighmentText(reader, "ItemName")) ? Convert.ToString(reader["MaterialName"]) ?? string.Empty : ReadWeighmentText(reader, "ItemName"),
            MaterialName = Convert.ToString(reader["MaterialName"]) ?? string.Empty,
            FirstWeight = Convert.ToDecimal(reader["FirstWeight"]),
            FirstWeightTime = DateTime.Parse(Convert.ToString(reader["FirstWeightTime"]) ?? DateTime.MinValue.ToString("O")),
            FirstWeightBy = ReadWeighmentText(reader, "FirstWeightBy"),
            FirstWeightByDisplay = ReadWeighmentText(reader, "FirstWeightByDisplay"),
            SecondWeight = reader["SecondWeight"] == DBNull.Value ? null : Convert.ToDecimal(reader["SecondWeight"]),
            SecondWeightTime = reader["SecondWeightTime"] == DBNull.Value ? null : DateTime.Parse(Convert.ToString(reader["SecondWeightTime"])!),
            SecondWeightBy = ReadWeighmentText(reader, "SecondWeightBy"),
            SecondWeightByDisplay = ReadWeighmentText(reader, "SecondWeightByDisplay"),
            NetWeight = reader["NetWeight"] == DBNull.Value ? null : Convert.ToDecimal(reader["NetWeight"]),
            Status = Convert.ToString(reader["Status"]) ?? string.Empty,
            Remarks = Convert.ToString(reader["Remarks"]) ?? string.Empty,
            CreatedAt = DateTime.Parse(Convert.ToString(reader["CreatedAt"]) ?? DateTime.MinValue.ToString("O"))
        };
    }
}

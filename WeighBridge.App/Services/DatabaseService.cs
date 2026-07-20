using System.IO;
using Microsoft.Data.Sqlite;
using WeightBridgeApp.Models;

namespace WeightBridgeApp.Services;

public class DatabaseService
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public DatabaseService()
    {
        _dbPath = Path.Combine(AppContext.BaseDirectory, "bridgeone.db");
        _connectionString = $"Data Source={_dbPath}";
    }

    public Task InitializeAsync() => Task.Run(() =>
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);

        using var connection = CreateConnection();
        connection.Open();

        ExecuteNonQuery(connection, @"
CREATE TABLE IF NOT EXISTS DeviceSettings (
    SettingId INTEGER PRIMARY KEY,
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
    VehicleType TEXT NOT NULL DEFAULT '',
    OwnerName TEXT NOT NULL DEFAULT '',
    ContactNo TEXT NOT NULL DEFAULT '',
    IsActive INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Drivers (
    DriverId INTEGER PRIMARY KEY AUTOINCREMENT,
    DriverName TEXT NOT NULL UNIQUE,
    CNIC TEXT NOT NULL DEFAULT '',
    MobileNo TEXT NOT NULL DEFAULT '',
    LicenseNo TEXT NOT NULL DEFAULT '',
    IsActive INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Weighments (
    WeighmentId INTEGER PRIMARY KEY AUTOINCREMENT,
    TicketNo TEXT NOT NULL UNIQUE,
    CompanyName TEXT NOT NULL DEFAULT '',
    VehicleNo TEXT NOT NULL,
    DriverName TEXT,
    PartyId INTEGER,
    PartyName TEXT,
    PartyType TEXT,
    MaterialId INTEGER,
    MaterialName TEXT,
    FirstWeight REAL NOT NULL,
    FirstWeightTime TEXT NOT NULL,
    SecondWeight REAL,
    SecondWeightTime TEXT,
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
    PurchaseOverDelivery TEXT NOT NULL DEFAULT '',
    PurchaseUnderDelivery TEXT NOT NULL DEFAULT '',
    BuyerGroup TEXT NOT NULL DEFAULT '',
    ItemPriceToleranceGroup TEXT NOT NULL DEFAULT '',
    Vendor TEXT NOT NULL DEFAULT '',
    PurchaseItemSalesTaxGroup TEXT NOT NULL DEFAULT '',
    SellUnit TEXT NOT NULL DEFAULT '',
    SellOverDelivery TEXT NOT NULL DEFAULT '',
    SellUnderDelivery TEXT NOT NULL DEFAULT '',
    SellItemSalesTaxGroup TEXT NOT NULL DEFAULT '',
    BatchNumberGroup TEXT NOT NULL DEFAULT '',
    SerialNumberGroup TEXT NOT NULL DEFAULT '',
    InventoryOverDelivery TEXT NOT NULL DEFAULT '',
    InventoryUnderDelivery TEXT NOT NULL DEFAULT '',
    CatchWeightItem TEXT NOT NULL DEFAULT '',
    CWUnit TEXT NOT NULL DEFAULT '',
    NominalQuantity TEXT NOT NULL DEFAULT '',
    MinimumQuantity TEXT NOT NULL DEFAULT '',
    MaximumQuantity TEXT NOT NULL DEFAULT '',
    BOMUnit TEXT NOT NULL DEFAULT '',
    ConstantScrap TEXT NOT NULL DEFAULT '',
    VariableScrap TEXT NOT NULL DEFAULT '',
    CostingLevel TEXT NOT NULL DEFAULT '',
    PlanningLevel TEXT NOT NULL DEFAULT '',
    CostCalculationLevel TEXT NOT NULL DEFAULT '',
    Phantom TEXT NOT NULL DEFAULT '',
    CalculationGroup TEXT NOT NULL DEFAULT '',
    ProductionType TEXT NOT NULL DEFAULT '',
    ItemGroup TEXT NOT NULL DEFAULT '',
    CostUnit TEXT NOT NULL DEFAULT '',
    LastCostPrice TEXT NOT NULL DEFAULT '',
    DateOfPrice TEXT NOT NULL DEFAULT '',
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
(SettingId, ConnectionType, ComPort, BaudRate, Parity, DataBits, StopBits, IpAddress, TcpPort)
VALUES
(1, $ConnectionType, $ComPort, $BaudRate, $Parity, $DataBits, $StopBits, $IpAddress, $TcpPort)
ON CONFLICT(SettingId) DO UPDATE SET
ConnectionType = excluded.ConnectionType,
ComPort = excluded.ComPort,
BaudRate = excluded.BaudRate,
Parity = excluded.Parity,
DataBits = excluded.DataBits,
StopBits = excluded.StopBits,
IpAddress = excluded.IpAddress,
TcpPort = excluded.TcpPort;";
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
        command.CommandText = "SELECT VehicleId, VehicleNo, VehicleType, OwnerName, ContactNo, IsActive FROM Vehicles ORDER BY VehicleNo";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Vehicle
            {
                VehicleId = Convert.ToInt32(reader["VehicleId"]),
                VehicleNo = Convert.ToString(reader["VehicleNo"]) ?? string.Empty,
                VehicleType = Convert.ToString(reader["VehicleType"]) ?? string.Empty,
                OwnerName = Convert.ToString(reader["OwnerName"]) ?? string.Empty,
                ContactNo = Convert.ToString(reader["ContactNo"]) ?? string.Empty,
                IsActive = Convert.ToInt32(reader["IsActive"]) == 1
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
        command.CommandText = "SELECT DriverId, DriverName, CNIC, MobileNo, LicenseNo, IsActive FROM Drivers ORDER BY DriverName";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Driver
            {
                DriverId = Convert.ToInt32(reader["DriverId"]),
                DriverName = Convert.ToString(reader["DriverName"]) ?? string.Empty,
                CNIC = Convert.ToString(reader["CNIC"]) ?? string.Empty,
                MobileNo = Convert.ToString(reader["MobileNo"]) ?? string.Empty,
                LicenseNo = Convert.ToString(reader["LicenseNo"]) ?? string.Empty,
                IsActive = Convert.ToInt32(reader["IsActive"]) == 1
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
        command.CommandText = "INSERT OR IGNORE INTO Vehicles (VehicleNo) VALUES ($VehicleNo)";
        command.Parameters.AddWithValue("$VehicleNo", vehicleNo.Trim().ToUpperInvariant());
        command.ExecuteNonQuery();
    });

    public Task SaveVehicleAsync(Vehicle vehicle) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(vehicle.VehicleNo))
            throw new InvalidOperationException("Vehicle number is mandatory.");

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        if (vehicle.VehicleId > 0)
        {
            command.CommandText = @"
UPDATE Vehicles SET
VehicleNo = $VehicleNo,
VehicleType = $VehicleType,
OwnerName = $OwnerName,
ContactNo = $ContactNo,
IsActive = $IsActive
WHERE VehicleId = $VehicleId;";
            command.Parameters.AddWithValue("$VehicleId", vehicle.VehicleId);
        }
        else
        {
            command.CommandText = @"
INSERT INTO Vehicles (VehicleNo, VehicleType, OwnerName, ContactNo, IsActive)
VALUES ($VehicleNo, $VehicleType, $OwnerName, $ContactNo, $IsActive);";
        }

        command.Parameters.AddWithValue("$VehicleNo", vehicle.VehicleNo.Trim().ToUpperInvariant());
        command.Parameters.AddWithValue("$VehicleType", vehicle.VehicleType ?? string.Empty);
        command.Parameters.AddWithValue("$OwnerName", vehicle.OwnerName ?? string.Empty);
        command.Parameters.AddWithValue("$ContactNo", vehicle.ContactNo ?? string.Empty);
        command.Parameters.AddWithValue("$IsActive", vehicle.IsActive ? 1 : 0);
        command.ExecuteNonQuery();
    });

    public Task AddDriverAsync(string driverName) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(driverName))
            return;

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO Drivers (DriverName) VALUES ($DriverName)";
        command.Parameters.AddWithValue("$DriverName", driverName.Trim());
        command.ExecuteNonQuery();
    });

    public Task SaveDriverAsync(Driver driver) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(driver.DriverName))
            throw new InvalidOperationException("Driver name is mandatory.");

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        if (driver.DriverId > 0)
        {
            command.CommandText = @"
UPDATE Drivers SET
DriverName = $DriverName,
CNIC = $CNIC,
MobileNo = $MobileNo,
LicenseNo = $LicenseNo,
IsActive = $IsActive
WHERE DriverId = $DriverId;";
            command.Parameters.AddWithValue("$DriverId", driver.DriverId);
        }
        else
        {
            command.CommandText = @"
INSERT INTO Drivers (DriverName, CNIC, MobileNo, LicenseNo, IsActive)
VALUES ($DriverName, $CNIC, $MobileNo, $LicenseNo, $IsActive);";
        }

        command.Parameters.AddWithValue("$DriverName", driver.DriverName.Trim());
        command.Parameters.AddWithValue("$CNIC", driver.CNIC ?? string.Empty);
        command.Parameters.AddWithValue("$MobileNo", driver.MobileNo ?? string.Empty);
        command.Parameters.AddWithValue("$LicenseNo", driver.LicenseNo ?? string.Empty);
        command.Parameters.AddWithValue("$IsActive", driver.IsActive ? 1 : 0);
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
(TicketNo, CompanyName, VehicleNo, DriverName, PartyId, PartyName, PartyType, MaterialId, MaterialName, FirstWeight, FirstWeightTime, Status, Remarks, CreatedAt)
VALUES
($TicketNo, $CompanyName, $VehicleNo, $DriverName, $PartyId, $PartyName, $PartyType, $MaterialId, $MaterialName, $FirstWeight, $FirstWeightTime, $Status, $Remarks, $CreatedAt);
SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$TicketNo", weighment.TicketNo);
        command.Parameters.AddWithValue("$CompanyName", weighment.CompanyName.Trim());
        command.Parameters.AddWithValue("$VehicleNo", weighment.VehicleNo);
        command.Parameters.AddWithValue("$DriverName", weighment.DriverName ?? string.Empty);
        command.Parameters.AddWithValue("$PartyId", (object?)weighment.PartyId ?? DBNull.Value);
        command.Parameters.AddWithValue("$PartyName", weighment.PartyName ?? string.Empty);
        command.Parameters.AddWithValue("$PartyType", weighment.PartyType ?? string.Empty);
        command.Parameters.AddWithValue("$MaterialId", (object?)weighment.MaterialId ?? DBNull.Value);
        command.Parameters.AddWithValue("$MaterialName", weighment.MaterialName ?? string.Empty);
        command.Parameters.AddWithValue("$FirstWeight", weighment.FirstWeight);
        command.Parameters.AddWithValue("$FirstWeightTime", weighment.FirstWeightTime.ToString("O"));
        command.Parameters.AddWithValue("$Status", "Open");
        command.Parameters.AddWithValue("$Remarks", weighment.Remarks ?? string.Empty);
        command.Parameters.AddWithValue("$CreatedAt", weighment.CreatedAt.ToString("O"));
        return Convert.ToInt32(command.ExecuteScalar());
    });

    public Task CompleteSecondWeightAsync(int weighmentId, decimal secondWeight, DateTime secondWeightTime) => Task.Run(() =>
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
    NetWeight = $NetWeight,
    Status = 'Completed'
WHERE WeighmentId = $WeighmentId;";
        command.Parameters.AddWithValue("$SecondWeight", secondWeight);
        command.Parameters.AddWithValue("$SecondWeightTime", secondWeightTime.ToString("O"));
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
    PartyName = $PartyName,
    PartyType = $PartyType,
    MaterialName = $MaterialName,
    FirstWeight = $FirstWeight,
    FirstWeightTime = $FirstWeightTime,
    SecondWeight = $SecondWeight,
    SecondWeightTime = $SecondWeightTime,
    NetWeight = $NetWeight,
    Remarks = $Remarks
WHERE WeighmentId = $WeighmentId
  AND Status = 'Completed';";
        command.Parameters.AddWithValue("$WeighmentId", weighment.WeighmentId);
        command.Parameters.AddWithValue("$CompanyName", weighment.CompanyName.Trim());
        command.Parameters.AddWithValue("$VehicleNo", weighment.VehicleNo.Trim().ToUpperInvariant());
        command.Parameters.AddWithValue("$DriverName", weighment.DriverName ?? string.Empty);
        command.Parameters.AddWithValue("$PartyName", weighment.PartyName ?? string.Empty);
        command.Parameters.AddWithValue("$PartyType", weighment.PartyType ?? string.Empty);
        command.Parameters.AddWithValue("$MaterialName", weighment.MaterialName ?? string.Empty);
        command.Parameters.AddWithValue("$FirstWeight", weighment.FirstWeight);
        command.Parameters.AddWithValue("$FirstWeightTime", weighment.FirstWeightTime.ToString("O"));
        command.Parameters.AddWithValue("$SecondWeight", weighment.SecondWeight.Value);
        command.Parameters.AddWithValue("$SecondWeightTime", secondTime.ToString("O"));
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
        command.CommandText = "SELECT * FROM Weighments WHERE Status = 'Open' ORDER BY FirstWeightTime DESC";
        return ReadWeighments(command);
    });

    public Task<List<Weighment>> GetCompletedTodayAsync() => Task.Run(() =>
    {
        var from = DateTime.Today;
        var to = DateTime.Today.AddDays(1);
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT * FROM Weighments
WHERE Status = 'Completed'
  AND SecondWeightTime >= $FromDate
  AND SecondWeightTime < $ToDate
ORDER BY SecondWeightTime DESC";
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
        command.CommandText = @"
SELECT * FROM Weighments
WHERE CreatedAt >= $FromDate
  AND CreatedAt < $ToDate
ORDER BY CreatedAt DESC";
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

        using var connection = CreateConnection();
        connection.Open();
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
        EnsureColumn(connection, "Weighments", "CompanyName", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "VehicleType", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "OwnerName", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "ContactNo", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Vehicles", "IsActive", "INTEGER NOT NULL DEFAULT 1");
        ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS Drivers (
    DriverId INTEGER PRIMARY KEY AUTOINCREMENT,
    DriverName TEXT NOT NULL UNIQUE,
    CNIC TEXT NOT NULL DEFAULT '',
    MobileNo TEXT NOT NULL DEFAULT '',
    LicenseNo TEXT NOT NULL DEFAULT '',
    IsActive INTEGER NOT NULL DEFAULT 1
);");

        ExecuteNonQuery(connection, "UPDATE Users SET CompanyName = 'Default Company' WHERE trim(ifnull(CompanyName, '')) = ''; ");
        ExecuteNonQuery(connection, "UPDATE Users SET CanEditCompletedTransaction = 1, CanDeleteCompletedTransaction = 1 WHERE lower(Username) = 'admin';");
        ExecuteNonQuery(connection, "UPDATE Weighments SET CompanyName = 'Default Company' WHERE trim(ifnull(CompanyName, '')) = ''; ");
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
(SettingId, ConnectionType, ComPort, BaudRate, Parity, DataBits, StopBits, IpAddress, TcpPort)
VALUES
(1, 'Mock', 'COM1', 9600, 'None', 8, 'One', '192.168.1.100', 4001);";
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
INSERT OR IGNORE INTO Vehicles (VehicleNo) VALUES ('TEST-0001');
INSERT OR IGNORE INTO Drivers (DriverName) VALUES ('Default Driver');
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
        command.Parameters.AddWithValue("$ItemNumber", item.ItemNumber.Trim());
        command.Parameters.AddWithValue("$ProductName", item.ProductName.Trim());
        command.Parameters.AddWithValue("$SearchName", item.SearchName.Trim());
        command.Parameters.AddWithValue("$ProductType", item.ProductType.Trim());
        command.Parameters.AddWithValue("$ProductSubtype", item.ProductSubtype.Trim());
        command.Parameters.AddWithValue("$ProductNumber", item.ProductNumber.Trim());
        command.Parameters.AddWithValue("$Description", item.Description.Trim());
        command.Parameters.AddWithValue("$StorageDimensionGroup", item.StorageDimensionGroup.Trim());
        command.Parameters.AddWithValue("$TrackingDimensionGroup", item.TrackingDimensionGroup.Trim());
        command.Parameters.AddWithValue("$ItemModelGroup", item.ItemModelGroup.Trim());
        command.Parameters.AddWithValue("$ReservationHierarchy", item.ReservationHierarchy.Trim());
        command.Parameters.AddWithValue("$PurchaseUnit", item.PurchaseUnit.Trim());
        command.Parameters.AddWithValue("$PurchaseOverDelivery", item.PurchaseOverDelivery.Trim());
        command.Parameters.AddWithValue("$PurchaseUnderDelivery", item.PurchaseUnderDelivery.Trim());
        command.Parameters.AddWithValue("$BuyerGroup", item.BuyerGroup.Trim());
        command.Parameters.AddWithValue("$ItemPriceToleranceGroup", item.ItemPriceToleranceGroup.Trim());
        command.Parameters.AddWithValue("$Vendor", item.Vendor.Trim());
        command.Parameters.AddWithValue("$PurchaseItemSalesTaxGroup", item.PurchaseItemSalesTaxGroup.Trim());
        command.Parameters.AddWithValue("$SellUnit", item.SellUnit.Trim());
        command.Parameters.AddWithValue("$SellOverDelivery", item.SellOverDelivery.Trim());
        command.Parameters.AddWithValue("$SellUnderDelivery", item.SellUnderDelivery.Trim());
        command.Parameters.AddWithValue("$SellItemSalesTaxGroup", item.SellItemSalesTaxGroup.Trim());
        command.Parameters.AddWithValue("$BatchNumberGroup", item.BatchNumberGroup.Trim());
        command.Parameters.AddWithValue("$SerialNumberGroup", item.SerialNumberGroup.Trim());
        command.Parameters.AddWithValue("$InventoryOverDelivery", item.InventoryOverDelivery.Trim());
        command.Parameters.AddWithValue("$InventoryUnderDelivery", item.InventoryUnderDelivery.Trim());
        command.Parameters.AddWithValue("$CatchWeightItem", item.CatchWeightItem.Trim());
        command.Parameters.AddWithValue("$CWUnit", item.CWUnit.Trim());
        command.Parameters.AddWithValue("$NominalQuantity", item.NominalQuantity.Trim());
        command.Parameters.AddWithValue("$MinimumQuantity", item.MinimumQuantity.Trim());
        command.Parameters.AddWithValue("$MaximumQuantity", item.MaximumQuantity.Trim());
        command.Parameters.AddWithValue("$BOMUnit", item.BOMUnit.Trim());
        command.Parameters.AddWithValue("$ConstantScrap", item.ConstantScrap.Trim());
        command.Parameters.AddWithValue("$VariableScrap", item.VariableScrap.Trim());
        command.Parameters.AddWithValue("$CostingLevel", item.CostingLevel.Trim());
        command.Parameters.AddWithValue("$PlanningLevel", item.PlanningLevel.Trim());
        command.Parameters.AddWithValue("$CostCalculationLevel", item.CostCalculationLevel.Trim());
        command.Parameters.AddWithValue("$Phantom", item.Phantom.Trim());
        command.Parameters.AddWithValue("$CalculationGroup", item.CalculationGroup.Trim());
        command.Parameters.AddWithValue("$ProductionType", item.ProductionType.Trim());
        command.Parameters.AddWithValue("$ItemGroup", item.ItemGroup.Trim());
        command.Parameters.AddWithValue("$CostUnit", item.CostUnit.Trim());
        command.Parameters.AddWithValue("$LastCostPrice", item.LastCostPrice.Trim());
        command.Parameters.AddWithValue("$DateOfPrice", item.DateOfPrice.Trim());
        command.Parameters.AddWithValue("$UnitSequenceGroupId", item.UnitSequenceGroupId.Trim());
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
        PurchaseOverDelivery = ReadText(reader, "PurchaseOverDelivery"),
        PurchaseUnderDelivery = ReadText(reader, "PurchaseUnderDelivery"),
        BuyerGroup = ReadText(reader, "BuyerGroup"),
        ItemPriceToleranceGroup = ReadText(reader, "ItemPriceToleranceGroup"),
        Vendor = ReadText(reader, "Vendor"),
        PurchaseItemSalesTaxGroup = ReadText(reader, "PurchaseItemSalesTaxGroup"),
        SellUnit = ReadText(reader, "SellUnit"),
        SellOverDelivery = ReadText(reader, "SellOverDelivery"),
        SellUnderDelivery = ReadText(reader, "SellUnderDelivery"),
        SellItemSalesTaxGroup = ReadText(reader, "SellItemSalesTaxGroup"),
        BatchNumberGroup = ReadText(reader, "BatchNumberGroup"),
        SerialNumberGroup = ReadText(reader, "SerialNumberGroup"),
        InventoryOverDelivery = ReadText(reader, "InventoryOverDelivery"),
        InventoryUnderDelivery = ReadText(reader, "InventoryUnderDelivery"),
        CatchWeightItem = ReadText(reader, "CatchWeightItem"),
        CWUnit = ReadText(reader, "CWUnit"),
        NominalQuantity = ReadText(reader, "NominalQuantity"),
        MinimumQuantity = ReadText(reader, "MinimumQuantity"),
        MaximumQuantity = ReadText(reader, "MaximumQuantity"),
        BOMUnit = ReadText(reader, "BOMUnit"),
        ConstantScrap = ReadText(reader, "ConstantScrap"),
        VariableScrap = ReadText(reader, "VariableScrap"),
        CostingLevel = ReadText(reader, "CostingLevel"),
        PlanningLevel = ReadText(reader, "PlanningLevel"),
        CostCalculationLevel = ReadText(reader, "CostCalculationLevel"),
        Phantom = ReadText(reader, "Phantom"),
        CalculationGroup = ReadText(reader, "CalculationGroup"),
        ProductionType = ReadText(reader, "ProductionType"),
        ItemGroup = ReadText(reader, "ItemGroup"),
        CostUnit = ReadText(reader, "CostUnit"),
        LastCostPrice = ReadText(reader, "LastCostPrice"),
        DateOfPrice = ReadText(reader, "DateOfPrice"),
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
            PartyName = Convert.ToString(reader["PartyName"]) ?? string.Empty,
            PartyType = Convert.ToString(reader["PartyType"]) ?? string.Empty,
            MaterialId = reader["MaterialId"] == DBNull.Value ? null : Convert.ToInt32(reader["MaterialId"]),
            MaterialName = Convert.ToString(reader["MaterialName"]) ?? string.Empty,
            FirstWeight = Convert.ToDecimal(reader["FirstWeight"]),
            FirstWeightTime = DateTime.Parse(Convert.ToString(reader["FirstWeightTime"]) ?? DateTime.MinValue.ToString("O")),
            SecondWeight = reader["SecondWeight"] == DBNull.Value ? null : Convert.ToDecimal(reader["SecondWeight"]),
            SecondWeightTime = reader["SecondWeightTime"] == DBNull.Value ? null : DateTime.Parse(Convert.ToString(reader["SecondWeightTime"])!),
            NetWeight = reader["NetWeight"] == DBNull.Value ? null : Convert.ToDecimal(reader["NetWeight"]),
            Status = Convert.ToString(reader["Status"]) ?? string.Empty,
            Remarks = Convert.ToString(reader["Remarks"]) ?? string.Empty,
            CreatedAt = DateTime.Parse(Convert.ToString(reader["CreatedAt"]) ?? DateTime.MinValue.ToString("O"))
        };
    }
}

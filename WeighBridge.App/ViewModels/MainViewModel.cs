using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;

namespace WeightBridgeApp.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly AppUser _currentUser;
    private IWeightReader? _weightReader;
    private int? _loadedOpenWeighmentId;

    private DeviceSettings _settings = new();
    private decimal _liveWeight;
    private bool _isStable;
    private string _liveRaw = string.Empty;
    private string _connectionStatus = "Disconnected";
    private bool _isConnected;
    private string _statusMessage = "Ready.";
    private string _ticketNo = string.Empty;
    private string _vehicleNo = string.Empty;
    private string _driverName = string.Empty;
    private string _partyAccount = string.Empty;
    private string _partyName = string.Empty;
    private string _itemNumber = string.Empty;
    private string _itemName = string.Empty;
    private string _remarks = string.Empty;
    private Party? _selectedParty;
    private Material? _selectedMaterial;
    private Weighment? _selectedOpenWeighment;
    private Weighment? _selectedCompletedWeighment;
    private Weighment? _selectedReportWeighment;
    private decimal? _firstWeight;
    private decimal? _secondWeight;
    private decimal? _netWeight;
    private DateTime _reportFrom = DateTime.Today;
    private DateTime _reportTo = DateTime.Today;
    private string _reportTicketFilter = string.Empty;
    private string _reportCompanyFilter = string.Empty;
    private string _reportVehicleFilter = string.Empty;
    private string _reportDriverFilter = string.Empty;
    private string _reportPartyFilter = string.Empty;
    private string _reportPartyTypeFilter = string.Empty;
    private string _reportItemFilter = string.Empty;
    private string _reportStatusFilter = string.Empty;
    private string _newPartyName = string.Empty;
    private string _newPartyType = "Customer";
    private string _newMaterialName = string.Empty;
    private string _newVehicleNo = string.Empty;
    private string _selectedPartyType = "Customer";
    private Party? _selectedWeighmentParty;
    private ItemMaster? _selectedWeighmentItem;
    private Vehicle? _selectedVehicleMaster;
    private Vehicle _vehicleMasterForm = new();
    private Driver? _selectedDriverMaster;
    private Driver _driverMasterForm = new();
    private WeighbridgeMaster? _selectedWeighbridgeMaster;
    private WeighbridgeMaster? _selectedSettingsWeighbridge;
    private WeighbridgeMaster _weighbridgeMasterForm = new();
    private string _databaseFolderPath = string.Empty;
    private OperatorMaster? _selectedOperatorMaster;
    private OperatorMaster _operatorMasterForm = new();
    private string _customerFilter = string.Empty;
    private string _vendorFilter = string.Empty;
    private string _itemMasterFilter = string.Empty;
    private string _warehouseFilter = string.Empty;
    private string _vehicleFilter = string.Empty;
    private string _driverFilter = string.Empty;
    private string _customerAccountFilter = string.Empty;
    private string _customerNameFilter = string.Empty;
    private string _customerGroupFilter = string.Empty;
    private string _customerStatusFilter = string.Empty;
    private string _vendorAccountFilter = string.Empty;
    private string _vendorNameFilter = string.Empty;
    private string _vendorGroupFilter = string.Empty;
    private string _vendorStatusFilter = string.Empty;
    private string _itemNumberFilter = string.Empty;
    private string _itemProductNameFilter = string.Empty;
    private string _itemSearchNameFilter = string.Empty;
    private string _itemProductTypeFilter = string.Empty;
    private string _warehouseCodeFilter = string.Empty;
    private string _warehouseNameFilter = string.Empty;
    private string _warehouseSiteFilter = string.Empty;
    private string _warehouseTypeFilter = string.Empty;
    private string _vehicleIdFilter = string.Empty;
    private string _vehicleNoFilter = string.Empty;
    private string _vehicleEmirateFilter = string.Empty;
    private string _vehicleCategoryFilter = string.Empty;
    private string _vehicleTypeFilter = string.Empty;
    private string _vehicleOwnerFilter = string.Empty;
    private string _vehicleContactFilter = string.Empty;
    private string _driverIdFilter = string.Empty;
    private string _driverNameFilter = string.Empty;
    private string _driverCnicFilter = string.Empty;
    private string _driverMobileFilter = string.Empty;
    private string _driverTypeFilter = string.Empty;
    private string _driverEmployerFilter = string.Empty;
    private string _driverLicenseFilter = string.Empty;
    private string _driverStatusFilter = string.Empty;
    private string _weighbridgeCodeFilter = string.Empty;
    private string _weighbridgeNameFilter = string.Empty;
    private string _weighbridgeSiteFilter = string.Empty;
    private string _weighbridgeWarehouseFilter = string.Empty;
    private string _weighbridgeStatusFilter = string.Empty;
    private string _operatorIdFilter = string.Empty;
    private string _operatorNameFilter = string.Empty;
    private string _operatorUsernameFilter = string.Empty;
    private string _operatorDesignationFilter = string.Empty;
    private string _operatorDepartmentFilter = string.Empty;
    private string _operatorWeighbridgeFilter = string.Empty;
    private string _operatorStatusFilter = string.Empty;
    private Customer? _selectedCustomer;
    private Customer _customerForm = new();
    private Vendor? _selectedVendor;
    private Vendor _vendorForm = new();
    private ItemMaster? _selectedItemMaster;
    private ItemMaster _itemMasterForm = new();
    private WarehouseMaster? _selectedWarehouseMaster;
    private WarehouseMaster _warehouseMasterForm = new();
    private AppUser? _selectedUser;
    private int? _editingUserId;
    private string _userUsername = string.Empty;
    private string _userFullName = string.Empty;
    private string _userCompanyName = string.Empty;
    private string _userPassword = string.Empty;
    private bool _userIsActive = true;
    private bool _userCanAccessWeighment = true;
    private bool _userCanAccessSettings;
    private bool _userCanAccessMasters;
    private bool _userCanAccessReports = true;
    private bool _userCanAccessUserManagement;
    private bool _userCanEditCompletedTransaction;
    private bool _userCanDeleteCompletedTransaction;

    public MainViewModel(DatabaseService databaseService, AppUser currentUser)
    {
        _databaseService = databaseService;
        _currentUser = currentUser;

        ConnectionTypes = new ObservableCollection<string> { "Mock", "TCP/IP", "Serial", "USB", "OPC", "API" };
        ParityOptions = new ObservableCollection<string> { "None", "Odd", "Even", "Mark", "Space" };
        StopBitsOptions = new ObservableCollection<string> { "One", "Two", "OnePointFive" };
        PartyTypes = new ObservableCollection<string> { "Customer", "Vendor" };

        ConnectCommand = new RelayCommand(ConnectAsync, () => !IsConnected);
        DisconnectCommand = new RelayCommand(DisconnectAsync, () => IsConnected);
        SaveSettingsCommand = new RelayCommand(SaveSettingsAsync);
        SaveFirstWeightCommand = new RelayCommand(SaveFirstWeightAsync);
        SaveSecondWeightCommand = new RelayCommand(SaveSecondWeightAsync);
        LoadOpenTicketCommand = new RelayCommand(LoadSelectedOpenTicket);
        RefreshCommand = new RelayCommand(RefreshAllAsync);
        ClearCommand = new RelayCommand(ClearEntry);
        AddPartyCommand = new RelayCommand(AddPartyAsync);
        AddMaterialCommand = new RelayCommand(AddMaterialAsync);
        AddVehicleCommand = new RelayCommand(AddVehicleAsync);
        LoadReportCommand = new RelayCommand(LoadReportAsync);
        ClearReportFiltersCommand = new RelayCommand(ClearReportFilters);
        ExportReportCommand = new RelayCommand(ExportReportAsync);
        PrintSlipCommand = new RelayCommand(PrintSlipAsync);
        SaveCompletedEditCommand = new RelayCommand(SaveCompletedEditAsync);
        DeleteCompletedCommand = new RelayCommand(DeleteCompletedAsync);
        SaveReportEditCommand = new RelayCommand(SaveReportEditAsync);
        DeleteReportRowCommand = new RelayCommand(DeleteReportRowAsync);
        SaveUserCommand = new RelayCommand(SaveUserAsync);
        ClearUserFormCommand = new RelayCommand(ClearUserForm);
        SaveCustomerCommand = new RelayCommand(SaveCustomerAsync);
        ClearCustomerFormCommand = new RelayCommand(ClearCustomerForm);
        SaveVendorCommand = new RelayCommand(SaveVendorAsync);
        ClearVendorFormCommand = new RelayCommand(ClearVendorForm);
        SaveItemMasterCommand = new RelayCommand(SaveItemMasterAsync);
        ClearItemMasterFormCommand = new RelayCommand(ClearItemMasterForm);
        SaveWarehouseMasterCommand = new RelayCommand(SaveWarehouseMasterAsync);
        ClearWarehouseMasterFormCommand = new RelayCommand(ClearWarehouseMasterForm);
        SaveVehicleMasterCommand = new RelayCommand(SaveVehicleMasterAsync);
        ClearVehicleMasterFormCommand = new RelayCommand(ClearVehicleMasterForm);
        SaveDriverMasterCommand = new RelayCommand(SaveDriverMasterAsync);
        ClearDriverMasterFormCommand = new RelayCommand(ClearDriverMasterForm);
        SaveWeighbridgeMasterCommand = new RelayCommand(SaveWeighbridgeMasterAsync);
        ClearWeighbridgeMasterFormCommand = new RelayCommand(ClearWeighbridgeMasterForm);
        SaveOperatorMasterCommand = new RelayCommand(SaveOperatorMasterAsync);
        ClearOperatorMasterFormCommand = new RelayCommand(ClearOperatorMasterForm);
        RefreshUsersCommand = new RelayCommand(LoadUsersAsync);
    }

    public ObservableCollection<string> ConnectionTypes { get; }
    public ObservableCollection<string> ParityOptions { get; }
    public ObservableCollection<string> StopBitsOptions { get; }
    public ObservableCollection<string> PartyTypes { get; }
    public ObservableCollection<Party> Parties { get; } = new();
    public ObservableCollection<Party> FilteredParties { get; } = new();
    public ObservableCollection<Material> Materials { get; } = new();
    public ObservableCollection<Vehicle> Vehicles { get; } = new();
    public ObservableCollection<Driver> Drivers { get; } = new();
    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<Vendor> Vendors { get; } = new();
    public ObservableCollection<ItemMaster> ItemMasters { get; } = new();
    public ObservableCollection<WarehouseMaster> WarehouseMasters { get; } = new();
    public ObservableCollection<Customer> FilteredCustomers { get; } = new();
    public ObservableCollection<Vendor> FilteredVendors { get; } = new();
    public ObservableCollection<ItemMaster> FilteredItemMasters { get; } = new();
    public ObservableCollection<WarehouseMaster> FilteredWarehouseMasters { get; } = new();
    public ObservableCollection<Vehicle> FilteredVehicles { get; } = new();
    public ObservableCollection<Driver> FilteredDrivers { get; } = new();
    public ObservableCollection<WeighbridgeMaster> WeighbridgeMasters { get; } = new();
    public ObservableCollection<WeighbridgeMaster> FilteredWeighbridgeMasters { get; } = new();
    public ObservableCollection<OperatorMaster> OperatorMasters { get; } = new();
    public ObservableCollection<OperatorMaster> FilteredOperatorMasters { get; } = new();
    public ObservableCollection<Weighment> OpenWeighments { get; } = new();
    public ObservableCollection<Weighment> CompletedToday { get; } = new();
    public ObservableCollection<Weighment> ReportRows { get; } = new();
    public ObservableCollection<Weighment> FilteredReportRows { get; } = new();
    public ObservableCollection<AppUser> Users { get; } = new();

    public RelayCommand ConnectCommand { get; }
    public RelayCommand DisconnectCommand { get; }
    public RelayCommand SaveSettingsCommand { get; }
    public RelayCommand SaveFirstWeightCommand { get; }
    public RelayCommand SaveSecondWeightCommand { get; }
    public RelayCommand LoadOpenTicketCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ClearCommand { get; }
    public RelayCommand AddPartyCommand { get; }
    public RelayCommand AddMaterialCommand { get; }
    public RelayCommand AddVehicleCommand { get; }
    public RelayCommand LoadReportCommand { get; }
    public RelayCommand ClearReportFiltersCommand { get; }
    public RelayCommand ExportReportCommand { get; }
    public RelayCommand PrintSlipCommand { get; }
    public RelayCommand SaveCompletedEditCommand { get; }
    public RelayCommand DeleteCompletedCommand { get; }
    public RelayCommand SaveReportEditCommand { get; }
    public RelayCommand DeleteReportRowCommand { get; }
    public RelayCommand SaveUserCommand { get; }
    public RelayCommand ClearUserFormCommand { get; }
    public RelayCommand SaveCustomerCommand { get; }
    public RelayCommand ClearCustomerFormCommand { get; }
    public RelayCommand SaveVendorCommand { get; }
    public RelayCommand ClearVendorFormCommand { get; }
    public RelayCommand SaveItemMasterCommand { get; }
    public RelayCommand ClearItemMasterFormCommand { get; }
    public RelayCommand SaveWarehouseMasterCommand { get; }
    public RelayCommand ClearWarehouseMasterFormCommand { get; }
    public RelayCommand SaveVehicleMasterCommand { get; }
    public RelayCommand ClearVehicleMasterFormCommand { get; }
    public RelayCommand SaveDriverMasterCommand { get; }
    public RelayCommand ClearDriverMasterFormCommand { get; }
    public RelayCommand SaveWeighbridgeMasterCommand { get; }
    public RelayCommand ClearWeighbridgeMasterFormCommand { get; }
    public RelayCommand SaveOperatorMasterCommand { get; }
    public RelayCommand ClearOperatorMasterFormCommand { get; }
    public RelayCommand RefreshUsersCommand { get; }

    public string CurrentUserDisplay => $"{_currentUser.FullName} ({_currentUser.Username})";
    public string CurrentUserId => _currentUser.UserId.ToString();
    public string CurrentUsername => _currentUser.Username;
    public string CurrentUserCompany => _currentUser.CompanyName;
    public bool CanAccessWeighment => _currentUser.CanAccessWeighment;
    public bool CanAccessSettings => _currentUser.CanAccessSettings;
    public bool CanAccessMasters => _currentUser.CanAccessMasters;
    public bool CanAccessReports => _currentUser.CanAccessReports;
    public bool CanAccessUserManagement => _currentUser.CanAccessUserManagement;
    public bool CanEditCompletedTransaction => _currentUser.CanEditCompletedTransaction;
    public bool CanDeleteCompletedTransaction => _currentUser.CanDeleteCompletedTransaction;
    public bool IsCompletedGridReadOnly => !_currentUser.CanEditCompletedTransaction;

    public bool CanSaveFirstWeight => CanAccessWeighment && _loadedOpenWeighmentId == null && !FirstWeight.HasValue;
    public bool CanSaveSecondWeight => CanAccessWeighment && _loadedOpenWeighmentId.HasValue && FirstWeight.HasValue && !SecondWeight.HasValue;

    public DeviceSettings Settings
    {
        get => _settings;
        set => SetProperty(ref _settings, value);
    }

    public string DatabaseFolderPath
    {
        get => _databaseFolderPath;
        set => SetProperty(ref _databaseFolderPath, value);
    }

    public WeighbridgeMaster? SelectedSettingsWeighbridge
    {
        get => _selectedSettingsWeighbridge;
        set
        {
            if (SetProperty(ref _selectedSettingsWeighbridge, value))
                ApplySelectedWeighbridgeToSettings();
        }
    }

    public decimal LiveWeight
    {
        get => _liveWeight;
        set
        {
            if (SetProperty(ref _liveWeight, value))
                OnPropertyChanged(nameof(LiveWeightText));
        }
    }

    public string LiveWeightText => $"{LiveWeight:N2} kg";

    public bool IsStable
    {
        get => _isStable;
        set
        {
            if (SetProperty(ref _isStable, value))
                OnPropertyChanged(nameof(StableText));
        }
    }

    public string StableText => IsStable ? "Yes" : "No";

    public string LiveRaw
    {
        get => _liveRaw;
        set => SetProperty(ref _liveRaw, value);
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (SetProperty(ref _isConnected, value))
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string TicketNo
    {
        get => _ticketNo;
        set => SetProperty(ref _ticketNo, value);
    }

    public string VehicleNo
    {
        get => _vehicleNo;
        set => SetProperty(ref _vehicleNo, value);
    }

    public string DriverName
    {
        get => _driverName;
        set => SetProperty(ref _driverName, value);
    }

    public string PartyAccount
    {
        get => _partyAccount;
        set => SetProperty(ref _partyAccount, value);
    }

    public string PartyName
    {
        get => _partyName;
        set => SetProperty(ref _partyName, value);
    }

    public string ItemNumber
    {
        get => _itemNumber;
        set => SetProperty(ref _itemNumber, value);
    }

    public string ItemName
    {
        get => _itemName;
        set => SetProperty(ref _itemName, value);
    }

    public string Remarks
    {
        get => _remarks;
        set => SetProperty(ref _remarks, value);
    }

    public Party? SelectedParty
    {
        get => _selectedParty;
        set => SetProperty(ref _selectedParty, value);
    }

    public Material? SelectedMaterial
    {
        get => _selectedMaterial;
        set => SetProperty(ref _selectedMaterial, value);
    }

    public Weighment? SelectedOpenWeighment
    {
        get => _selectedOpenWeighment;
        set => SetProperty(ref _selectedOpenWeighment, value);
    }

    public Weighment? SelectedCompletedWeighment
    {
        get => _selectedCompletedWeighment;
        set => SetProperty(ref _selectedCompletedWeighment, value);
    }

    public Weighment? SelectedReportWeighment
    {
        get => _selectedReportWeighment;
        set => SetProperty(ref _selectedReportWeighment, value);
    }

    public decimal? FirstWeight
    {
        get => _firstWeight;
        set
        {
            if (SetProperty(ref _firstWeight, value))
            {
                OnPropertyChanged(nameof(FirstWeightText));
                RecalculateNetWeight();
                NotifyWeighmentButtonStates();
            }
        }
    }

    public decimal? SecondWeight
    {
        get => _secondWeight;
        set
        {
            if (SetProperty(ref _secondWeight, value))
            {
                OnPropertyChanged(nameof(SecondWeightText));
                RecalculateNetWeight();
                NotifyWeighmentButtonStates();
            }
        }
    }

    public decimal? NetWeight
    {
        get => _netWeight;
        set
        {
            if (SetProperty(ref _netWeight, value))
                OnPropertyChanged(nameof(NetWeightText));
        }
    }

    public string FirstWeightText => FirstWeight.HasValue ? $"{FirstWeight.Value:N2}" : "-";
    public string SecondWeightText => SecondWeight.HasValue ? $"{SecondWeight.Value:N2}" : "-";
    public string NetWeightText => NetWeight.HasValue ? $"{NetWeight.Value:N2}" : "-";

    public DateTime ReportFrom
    {
        get => _reportFrom;
        set => SetProperty(ref _reportFrom, value);
    }

    public DateTime ReportTo
    {
        get => _reportTo;
        set => SetProperty(ref _reportTo, value);
    }

    public string ReportTicketFilter
    {
        get => _reportTicketFilter;
        set
        {
            if (SetProperty(ref _reportTicketFilter, value))
                ApplyReportFilter();
        }
    }

    public string ReportCompanyFilter
    {
        get => _reportCompanyFilter;
        set
        {
            if (SetProperty(ref _reportCompanyFilter, value))
                ApplyReportFilter();
        }
    }

    public string ReportVehicleFilter
    {
        get => _reportVehicleFilter;
        set
        {
            if (SetProperty(ref _reportVehicleFilter, value))
                ApplyReportFilter();
        }
    }

    public string ReportDriverFilter
    {
        get => _reportDriverFilter;
        set
        {
            if (SetProperty(ref _reportDriverFilter, value))
                ApplyReportFilter();
        }
    }

    public string ReportPartyFilter
    {
        get => _reportPartyFilter;
        set
        {
            if (SetProperty(ref _reportPartyFilter, value))
                ApplyReportFilter();
        }
    }

    public string ReportPartyTypeFilter
    {
        get => _reportPartyTypeFilter;
        set
        {
            if (SetProperty(ref _reportPartyTypeFilter, value))
                ApplyReportFilter();
        }
    }

    public string ReportItemFilter
    {
        get => _reportItemFilter;
        set
        {
            if (SetProperty(ref _reportItemFilter, value))
                ApplyReportFilter();
        }
    }

    public string ReportStatusFilter
    {
        get => _reportStatusFilter;
        set
        {
            if (SetProperty(ref _reportStatusFilter, value))
                ApplyReportFilter();
        }
    }

    public string NewPartyName
    {
        get => _newPartyName;
        set => SetProperty(ref _newPartyName, value);
    }

    public string NewPartyType
    {
        get => _newPartyType;
        set => SetProperty(ref _newPartyType, value);
    }

    public string NewMaterialName
    {
        get => _newMaterialName;
        set => SetProperty(ref _newMaterialName, value);
    }

    public string NewVehicleNo
    {
        get => _newVehicleNo;
        set => SetProperty(ref _newVehicleNo, value);
    }

    public string CustomerFilter
    {
        get => _customerFilter;
        set
        {
            if (SetProperty(ref _customerFilter, value))
                ApplyCustomerFilter();
        }
    }

    public string VendorFilter
    {
        get => _vendorFilter;
        set
        {
            if (SetProperty(ref _vendorFilter, value))
                ApplyVendorFilter();
        }
    }

    public string ItemMasterFilter
    {
        get => _itemMasterFilter;
        set
        {
            if (SetProperty(ref _itemMasterFilter, value))
                ApplyItemMasterFilter();
        }
    }

    public string WarehouseFilter
    {
        get => _warehouseFilter;
        set
        {
            if (SetProperty(ref _warehouseFilter, value))
                ApplyWarehouseFilter();
        }
    }

    public string VehicleFilter
    {
        get => _vehicleFilter;
        set
        {
            if (SetProperty(ref _vehicleFilter, value))
                ApplyVehicleFilter();
        }
    }

    public string DriverFilter
    {
        get => _driverFilter;
        set
        {
            if (SetProperty(ref _driverFilter, value))
                ApplyDriverFilter();
        }
    }

    public string CustomerAccountFilter
    {
        get => _customerAccountFilter;
        set
        {
            if (SetProperty(ref _customerAccountFilter, value))
                ApplyCustomerFilter();
        }
    }

    public string CustomerNameFilter
    {
        get => _customerNameFilter;
        set
        {
            if (SetProperty(ref _customerNameFilter, value))
                ApplyCustomerFilter();
        }
    }

    public string CustomerGroupFilter
    {
        get => _customerGroupFilter;
        set
        {
            if (SetProperty(ref _customerGroupFilter, value))
                ApplyCustomerFilter();
        }
    }

    public string CustomerStatusFilter
    {
        get => _customerStatusFilter;
        set
        {
            if (SetProperty(ref _customerStatusFilter, value))
                ApplyCustomerFilter();
        }
    }

    public string VendorAccountFilter
    {
        get => _vendorAccountFilter;
        set
        {
            if (SetProperty(ref _vendorAccountFilter, value))
                ApplyVendorFilter();
        }
    }

    public string VendorNameFilter
    {
        get => _vendorNameFilter;
        set
        {
            if (SetProperty(ref _vendorNameFilter, value))
                ApplyVendorFilter();
        }
    }

    public string VendorGroupFilter
    {
        get => _vendorGroupFilter;
        set
        {
            if (SetProperty(ref _vendorGroupFilter, value))
                ApplyVendorFilter();
        }
    }

    public string VendorStatusFilter
    {
        get => _vendorStatusFilter;
        set
        {
            if (SetProperty(ref _vendorStatusFilter, value))
                ApplyVendorFilter();
        }
    }

    public string ItemNumberFilter
    {
        get => _itemNumberFilter;
        set
        {
            if (SetProperty(ref _itemNumberFilter, value))
                ApplyItemMasterFilter();
        }
    }

    public string ItemProductNameFilter
    {
        get => _itemProductNameFilter;
        set
        {
            if (SetProperty(ref _itemProductNameFilter, value))
                ApplyItemMasterFilter();
        }
    }

    public string ItemSearchNameFilter
    {
        get => _itemSearchNameFilter;
        set
        {
            if (SetProperty(ref _itemSearchNameFilter, value))
                ApplyItemMasterFilter();
        }
    }

    public string ItemProductTypeFilter
    {
        get => _itemProductTypeFilter;
        set
        {
            if (SetProperty(ref _itemProductTypeFilter, value))
                ApplyItemMasterFilter();
        }
    }

    public string WarehouseCodeFilter
    {
        get => _warehouseCodeFilter;
        set
        {
            if (SetProperty(ref _warehouseCodeFilter, value))
                ApplyWarehouseFilter();
        }
    }

    public string WarehouseNameFilter
    {
        get => _warehouseNameFilter;
        set
        {
            if (SetProperty(ref _warehouseNameFilter, value))
                ApplyWarehouseFilter();
        }
    }

    public string WarehouseSiteFilter
    {
        get => _warehouseSiteFilter;
        set
        {
            if (SetProperty(ref _warehouseSiteFilter, value))
                ApplyWarehouseFilter();
        }
    }

    public string WarehouseTypeFilter
    {
        get => _warehouseTypeFilter;
        set
        {
            if (SetProperty(ref _warehouseTypeFilter, value))
                ApplyWarehouseFilter();
        }
    }

    public string VehicleIdFilter
    {
        get => _vehicleIdFilter;
        set
        {
            if (SetProperty(ref _vehicleIdFilter, value))
                ApplyVehicleFilter();
        }
    }

    public string VehicleNoFilter
    {
        get => _vehicleNoFilter;
        set
        {
            if (SetProperty(ref _vehicleNoFilter, value))
                ApplyVehicleFilter();
        }
    }

    public string VehicleEmirateFilter
    {
        get => _vehicleEmirateFilter;
        set
        {
            if (SetProperty(ref _vehicleEmirateFilter, value))
                ApplyVehicleFilter();
        }
    }

    public string VehicleCategoryFilter
    {
        get => _vehicleCategoryFilter;
        set
        {
            if (SetProperty(ref _vehicleCategoryFilter, value))
                ApplyVehicleFilter();
        }
    }

    public string VehicleTypeFilter
    {
        get => _vehicleTypeFilter;
        set
        {
            if (SetProperty(ref _vehicleTypeFilter, value))
                ApplyVehicleFilter();
        }
    }

    public string VehicleOwnerFilter
    {
        get => _vehicleOwnerFilter;
        set
        {
            if (SetProperty(ref _vehicleOwnerFilter, value))
                ApplyVehicleFilter();
        }
    }

    public string VehicleContactFilter
    {
        get => _vehicleContactFilter;
        set
        {
            if (SetProperty(ref _vehicleContactFilter, value))
                ApplyVehicleFilter();
        }
    }

    public string DriverIdFilter
    {
        get => _driverIdFilter;
        set
        {
            if (SetProperty(ref _driverIdFilter, value))
                ApplyDriverFilter();
        }
    }

    public string DriverNameFilter
    {
        get => _driverNameFilter;
        set
        {
            if (SetProperty(ref _driverNameFilter, value))
                ApplyDriverFilter();
        }
    }

    public string DriverTypeFilter
    {
        get => _driverTypeFilter;
        set
        {
            if (SetProperty(ref _driverTypeFilter, value))
                ApplyDriverFilter();
        }
    }

    public string DriverEmployerFilter
    {
        get => _driverEmployerFilter;
        set
        {
            if (SetProperty(ref _driverEmployerFilter, value))
                ApplyDriverFilter();
        }
    }

    public string DriverStatusFilter
    {
        get => _driverStatusFilter;
        set
        {
            if (SetProperty(ref _driverStatusFilter, value))
                ApplyDriverFilter();
        }
    }

    public string DriverCnicFilter
    {
        get => _driverCnicFilter;
        set
        {
            if (SetProperty(ref _driverCnicFilter, value))
                ApplyDriverFilter();
        }
    }

    public string DriverMobileFilter
    {
        get => _driverMobileFilter;
        set
        {
            if (SetProperty(ref _driverMobileFilter, value))
                ApplyDriverFilter();
        }
    }

    public string DriverLicenseFilter
    {
        get => _driverLicenseFilter;
        set
        {
            if (SetProperty(ref _driverLicenseFilter, value))
                ApplyDriverFilter();
        }
    }


    public string WeighbridgeCodeFilter
    {
        get => _weighbridgeCodeFilter;
        set { if (SetProperty(ref _weighbridgeCodeFilter, value)) ApplyWeighbridgeFilter(); }
    }

    public string WeighbridgeNameFilter
    {
        get => _weighbridgeNameFilter;
        set { if (SetProperty(ref _weighbridgeNameFilter, value)) ApplyWeighbridgeFilter(); }
    }

    public string WeighbridgeSiteFilter
    {
        get => _weighbridgeSiteFilter;
        set { if (SetProperty(ref _weighbridgeSiteFilter, value)) ApplyWeighbridgeFilter(); }
    }

    public string WeighbridgeWarehouseFilter
    {
        get => _weighbridgeWarehouseFilter;
        set { if (SetProperty(ref _weighbridgeWarehouseFilter, value)) ApplyWeighbridgeFilter(); }
    }

    public string WeighbridgeStatusFilter
    {
        get => _weighbridgeStatusFilter;
        set { if (SetProperty(ref _weighbridgeStatusFilter, value)) ApplyWeighbridgeFilter(); }
    }

    public string OperatorIdFilter
    {
        get => _operatorIdFilter;
        set { if (SetProperty(ref _operatorIdFilter, value)) ApplyOperatorFilter(); }
    }

    public string OperatorNameFilter
    {
        get => _operatorNameFilter;
        set { if (SetProperty(ref _operatorNameFilter, value)) ApplyOperatorFilter(); }
    }

    public string OperatorUsernameFilter
    {
        get => _operatorUsernameFilter;
        set { if (SetProperty(ref _operatorUsernameFilter, value)) ApplyOperatorFilter(); }
    }

    public string OperatorDesignationFilter
    {
        get => _operatorDesignationFilter;
        set { if (SetProperty(ref _operatorDesignationFilter, value)) ApplyOperatorFilter(); }
    }

    public string OperatorDepartmentFilter
    {
        get => _operatorDepartmentFilter;
        set { if (SetProperty(ref _operatorDepartmentFilter, value)) ApplyOperatorFilter(); }
    }

    public string OperatorWeighbridgeFilter
    {
        get => _operatorWeighbridgeFilter;
        set { if (SetProperty(ref _operatorWeighbridgeFilter, value)) ApplyOperatorFilter(); }
    }

    public string OperatorStatusFilter
    {
        get => _operatorStatusFilter;
        set { if (SetProperty(ref _operatorStatusFilter, value)) ApplyOperatorFilter(); }
    }

    public string SelectedPartyType
    {
        get => _selectedPartyType;
        set
        {
            if (SetProperty(ref _selectedPartyType, value))
                RefreshPartyLookup();
        }
    }

    public Party? SelectedWeighmentParty
    {
        get => _selectedWeighmentParty;
        set
        {
            if (SetProperty(ref _selectedWeighmentParty, value) && value != null)
            {
                PartyAccount = value.PartyAccount;
                PartyName = value.PartyName;
            }
        }
    }

    public ItemMaster? SelectedWeighmentItem
    {
        get => _selectedWeighmentItem;
        set
        {
            if (SetProperty(ref _selectedWeighmentItem, value) && value != null)
            {
                ItemNumber = value.ItemNumber;
                ItemName = value.ProductName;
            }
        }
    }

    public Vehicle VehicleMasterForm
    {
        get => _vehicleMasterForm;
        set => SetProperty(ref _vehicleMasterForm, value);
    }

    public Vehicle? SelectedVehicleMaster
    {
        get => _selectedVehicleMaster;
        set
        {
            if (SetProperty(ref _selectedVehicleMaster, value))
                LoadSelectedVehicleMasterToForm();
        }
    }

    public Driver DriverMasterForm
    {
        get => _driverMasterForm;
        set => SetProperty(ref _driverMasterForm, value);
    }

    public Driver? SelectedDriverMaster
    {
        get => _selectedDriverMaster;
        set
        {
            if (SetProperty(ref _selectedDriverMaster, value))
                LoadSelectedDriverMasterToForm();
        }
    }

    public Customer CustomerForm
    {
        get => _customerForm;
        set => SetProperty(ref _customerForm, value);
    }

    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (SetProperty(ref _selectedCustomer, value))
                LoadSelectedCustomerToForm();
        }
    }

    public Vendor VendorForm
    {
        get => _vendorForm;
        set => SetProperty(ref _vendorForm, value);
    }

    public Vendor? SelectedVendor
    {
        get => _selectedVendor;
        set
        {
            if (SetProperty(ref _selectedVendor, value))
                LoadSelectedVendorToForm();
        }
    }

    public ItemMaster ItemMasterForm
    {
        get => _itemMasterForm;
        set => SetProperty(ref _itemMasterForm, value);
    }

    public ItemMaster? SelectedItemMaster
    {
        get => _selectedItemMaster;
        set
        {
            if (SetProperty(ref _selectedItemMaster, value))
                LoadSelectedItemMasterToForm();
        }
    }

    public WarehouseMaster WarehouseMasterForm
    {
        get => _warehouseMasterForm;
        set => SetProperty(ref _warehouseMasterForm, value);
    }

    public WarehouseMaster? SelectedWarehouseMaster
    {
        get => _selectedWarehouseMaster;
        set
        {
            if (SetProperty(ref _selectedWarehouseMaster, value))
                LoadSelectedWarehouseMasterToForm();
        }
    }


    public WeighbridgeMaster WeighbridgeMasterForm
    {
        get => _weighbridgeMasterForm;
        set => SetProperty(ref _weighbridgeMasterForm, value);
    }

    public WeighbridgeMaster? SelectedWeighbridgeMaster
    {
        get => _selectedWeighbridgeMaster;
        set
        {
            if (SetProperty(ref _selectedWeighbridgeMaster, value))
                LoadSelectedWeighbridgeMasterToForm();
        }
    }

    public OperatorMaster OperatorMasterForm
    {
        get => _operatorMasterForm;
        set => SetProperty(ref _operatorMasterForm, value);
    }

    public OperatorMaster? SelectedOperatorMaster
    {
        get => _selectedOperatorMaster;
        set
        {
            if (SetProperty(ref _selectedOperatorMaster, value))
                LoadSelectedOperatorMasterToForm();
        }
    }

    public AppUser? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
                LoadSelectedUserToForm();
        }
    }

    public string UserUsername
    {
        get => _userUsername;
        set => SetProperty(ref _userUsername, value);
    }

    public string UserFullName
    {
        get => _userFullName;
        set => SetProperty(ref _userFullName, value);
    }

    public string UserCompanyName
    {
        get => _userCompanyName;
        set => SetProperty(ref _userCompanyName, value);
    }

    public string UserPassword
    {
        get => _userPassword;
        set => SetProperty(ref _userPassword, value);
    }

    public bool UserIsActive
    {
        get => _userIsActive;
        set => SetProperty(ref _userIsActive, value);
    }

    public bool UserCanAccessWeighment
    {
        get => _userCanAccessWeighment;
        set => SetProperty(ref _userCanAccessWeighment, value);
    }

    public bool UserCanAccessSettings
    {
        get => _userCanAccessSettings;
        set => SetProperty(ref _userCanAccessSettings, value);
    }

    public bool UserCanAccessMasters
    {
        get => _userCanAccessMasters;
        set => SetProperty(ref _userCanAccessMasters, value);
    }

    public bool UserCanAccessReports
    {
        get => _userCanAccessReports;
        set => SetProperty(ref _userCanAccessReports, value);
    }

    public bool UserCanAccessUserManagement
    {
        get => _userCanAccessUserManagement;
        set => SetProperty(ref _userCanAccessUserManagement, value);
    }

    public bool UserCanEditCompletedTransaction
    {
        get => _userCanEditCompletedTransaction;
        set => SetProperty(ref _userCanEditCompletedTransaction, value);
    }

    public bool UserCanDeleteCompletedTransaction
    {
        get => _userCanDeleteCompletedTransaction;
        set => SetProperty(ref _userCanDeleteCompletedTransaction, value);
    }

    public string UserFormModeText => _editingUserId.HasValue ? "Edit User" : "Create User";

    public async Task InitializeAsync()
    {
        try
        {
            DatabaseFolderPath = BridgeOneConfigService.GetDatabaseFolderPath();
            await _databaseService.InitializeAsync();
            Settings = await _databaseService.GetSettingsAsync();
            await LoadMastersAsync();
            SelectSettingsWeighbridgeFromSavedSettings();
            await RefreshWeighmentsAsync();
            if (CanAccessReports)
                await LoadReportAsync();
            if (CanAccessUserManagement)
                await LoadUsersAsync();
            if (!BridgeOneConfigService.IsDatabaseFolderConfigured())
            {
                System.Windows.MessageBox.Show("Database folder path is not defined. Please define it in Settings.", "BridgeOne", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                StatusMessage = "Database folder path is not defined. Please define it in Settings.";
            }
            else
            {
                StatusMessage = "Application loaded. Use Mock mode first, then test Serial/TCP with your indicator.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Initialization error: " + ex.Message;
        }
    }

    public async Task ConnectAsync()
    {
        try
        {
            await DisconnectAsync();

            if (SelectedSettingsWeighbridge == null)
            {
                System.Windows.MessageBox.Show("Please select a weighbridge in Settings before connecting.", "BridgeOne", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                StatusMessage = "Please select a weighbridge in Settings before connecting.";
                return;
            }

            ApplySelectedWeighbridgeToSettings();

            _weightReader = Settings.ConnectionType switch
            {
                "Serial" => new SerialWeightReader(),
                "TCP/IP" => new TcpWeightReader(),
                "TCP" => new TcpWeightReader(),
                _ => new MockWeightReader()
            };

            _weightReader.WeightReceived += WeightReader_WeightReceived;
            await _weightReader.ConnectAsync(Settings);

            IsConnected = true;
            ConnectionStatus = $"Connected ({Settings.ConnectionType})";
            StatusMessage = "Connected successfully.";
        }
        catch (Exception ex)
        {
            IsConnected = false;
            ConnectionStatus = "Disconnected";
            ClearLiveWeight();
            StatusMessage = "Connection error: " + ex.Message;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (_weightReader != null)
            {
                _weightReader.WeightReceived -= WeightReader_WeightReceived;
                await _weightReader.DisconnectAsync();
                _weightReader = null;
            }

            IsConnected = false;
            ConnectionStatus = "Disconnected";
            ClearLiveWeight();
        }
        catch (Exception ex)
        {
            ClearLiveWeight();
            StatusMessage = "Disconnect error: " + ex.Message;
        }
    }

    private void ClearLiveWeight()
    {
        LiveWeight = 0m;
        IsStable = false;
        LiveRaw = string.Empty;
    }

    private void WeightReader_WeightReceived(object? sender, WeightReadingEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            LiveWeight = e.Weight;
            IsStable = e.IsStable;
            LiveRaw = e.RawData;
        });
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            if (!CanAccessSettings)
            {
                StatusMessage = "You do not have access to Settings.";
                return;
            }

            var previousDatabaseFolderPath = BridgeOneConfigService.GetDatabaseFolderPath();
            var newDatabaseFilePath = BridgeOneConfigService.GetDatabaseFilePathForFolder(DatabaseFolderPath);

            ApplySelectedWeighbridgeToSettings();
            await _databaseService.SaveSettingsAsync(Settings);

            BridgeOneConfigService.SaveDatabaseFolderPath(DatabaseFolderPath);

            if (!string.Equals(previousDatabaseFolderPath, DatabaseFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                if (!System.IO.File.Exists(newDatabaseFilePath) && System.IO.File.Exists(_databaseService.DatabaseFilePath))
                    System.IO.File.Copy(_databaseService.DatabaseFilePath, newDatabaseFilePath);

                System.Windows.MessageBox.Show("Database folder path saved. Please restart BridgeOne to use the selected database path.", "BridgeOne", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                StatusMessage = "Database folder path saved. Restart BridgeOne to use the selected database path.";
            }
            else
            {
                StatusMessage = "Settings saved. Selected weighbridge configuration will be used for live weight.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Settings save error: " + ex.Message;
        }
    }

    private async Task SaveFirstWeightAsync()
    {
        try
        {
            if (!ValidateEntryBeforeFirstWeight())
                return;

            if (!CanSaveFirstWeight)
            {
                StatusMessage = "First weight is already saved for this ticket. Please click Save Second Weight, or click Clear to start a new ticket.";
                return;
            }

            TicketNo = await _databaseService.GenerateTicketNoAsync();
            FirstWeight = LiveWeight;

            var weighment = new Weighment
            {
                TicketNo = TicketNo,
                CompanyName = CurrentUserCompany.Trim(),
                VehicleNo = VehicleNo.Trim().ToUpperInvariant(),
                DriverName = DriverName.Trim(),
                PartyId = SelectedWeighmentParty?.PartyId,
                PartyAccount = PartyAccount.Trim(),
                PartyName = PartyName.Trim(),
                PartyType = SelectedPartyType,
                MaterialId = SelectedWeighmentItem?.ItemMasterId,
                ItemNumber = ItemNumber.Trim(),
                ItemName = ItemName.Trim(),
                MaterialName = ItemName.Trim(),
                FirstWeight = LiveWeight,
                FirstWeightTime = DateTime.Now,
                FirstWeightBy = CurrentUsername,
                Status = "Open",
                Remarks = Remarks.Trim(),
                CreatedAt = DateTime.Now
            };

            SetLoadedOpenWeighmentId(await _databaseService.InsertFirstWeightAsync(weighment));
            await _databaseService.AddVehicleAsync(weighment.VehicleNo);
            await _databaseService.AddDriverAsync(weighment.DriverName);
            await RefreshAllAsync();

            StatusMessage = $"First weight saved. Ticket: {TicketNo}";
        }
        catch (Exception ex)
        {
            StatusMessage = "First weight save error: " + ex.Message;
        }
    }

    private async Task SaveSecondWeightAsync()
    {
        try
        {
            if (!CanAccessWeighment)
            {
                StatusMessage = "You do not have access to Weighment.";
                return;
            }

            if (_loadedOpenWeighmentId == null)
            {
                StatusMessage = "Please select an open ticket and click Load Selected first.";
                return;
            }

            if (!CanSaveSecondWeight)
            {
                StatusMessage = "Second weight can be saved only after first weight is loaded.";
                return;
            }

            if (!IsStable)
            {
                StatusMessage = "Weight is unstable. Please wait until stable.";
                return;
            }

            SecondWeight = LiveWeight;
            await _databaseService.CompleteSecondWeightAsync(_loadedOpenWeighmentId.Value, LiveWeight, DateTime.Now, CurrentUsername);

            await RefreshAllAsync();
            StatusMessage = $"Second weight saved. Ticket completed: {TicketNo}";
            ClearEntry();
        }
        catch (Exception ex)
        {
            StatusMessage = "Second weight save error: " + ex.Message;
        }
    }

    private void LoadSelectedOpenTicket()
    {
        if (SelectedOpenWeighment == null)
        {
            StatusMessage = "Please select an open ticket first.";
            return;
        }

        SetLoadedOpenWeighmentId(SelectedOpenWeighment.WeighmentId);
        TicketNo = SelectedOpenWeighment.TicketNo;
        VehicleNo = SelectedOpenWeighment.VehicleNo;
        DriverName = SelectedOpenWeighment.DriverName;
        PartyAccount = SelectedOpenWeighment.PartyAccount;
        PartyName = SelectedOpenWeighment.PartyName;
        ItemNumber = SelectedOpenWeighment.ItemNumber;
        ItemName = string.IsNullOrWhiteSpace(SelectedOpenWeighment.ItemName) ? SelectedOpenWeighment.MaterialName : SelectedOpenWeighment.ItemName;
        Remarks = SelectedOpenWeighment.Remarks;
        FirstWeight = SelectedOpenWeighment.FirstWeight;
        SecondWeight = null;
        NetWeight = null;
        SelectedPartyType = string.IsNullOrWhiteSpace(SelectedOpenWeighment.PartyType) ? "Customer" : SelectedOpenWeighment.PartyType;
        RefreshPartyLookup();
        SelectedWeighmentParty = FilteredParties.FirstOrDefault(x => x.PartyId == SelectedOpenWeighment.PartyId);
        SelectedWeighmentItem = ItemMasters.FirstOrDefault(x => x.ItemMasterId == SelectedOpenWeighment.MaterialId);
        PartyAccount = SelectedOpenWeighment.PartyAccount;
        PartyName = SelectedOpenWeighment.PartyName;
        ItemNumber = SelectedOpenWeighment.ItemNumber;
        ItemName = string.IsNullOrWhiteSpace(SelectedOpenWeighment.ItemName) ? SelectedOpenWeighment.MaterialName : SelectedOpenWeighment.ItemName;

        StatusMessage = $"Open ticket loaded: {TicketNo}";
    }

    private async Task RefreshAllAsync()
    {
        await LoadMastersAsync();
        await RefreshWeighmentsAsync();
        if (CanAccessReports)
            await LoadReportAsync();
    }

    private async Task LoadMastersAsync()
    {
        var parties = await _databaseService.GetPartiesAsync();
        var materials = await _databaseService.GetMaterialsAsync();
        var vehicles = await _databaseService.GetVehiclesAsync();
        var drivers = await _databaseService.GetDriversAsync();
        var customers = await _databaseService.GetCustomersAsync();
        var vendors = await _databaseService.GetVendorsAsync();
        var itemMasters = await _databaseService.GetItemMastersAsync();
        var warehouseMasters = await _databaseService.GetWarehouseMastersAsync();
        var weighbridgeMasters = await _databaseService.GetWeighbridgeMastersAsync();
        var operatorMasters = await _databaseService.GetOperatorMastersAsync();

        ReplaceCollection(Parties, parties);
        ReplaceCollection(Materials, materials);
        ReplaceCollection(Vehicles, vehicles);
        ReplaceCollection(Drivers, drivers);
        ReplaceCollection(Customers, customers);
        ReplaceCollection(Vendors, vendors);
        ReplaceCollection(ItemMasters, itemMasters);
        ReplaceCollection(WarehouseMasters, warehouseMasters);
        ReplaceCollection(WeighbridgeMasters, weighbridgeMasters);
        ReplaceCollection(OperatorMasters, operatorMasters);
        if (SelectedSettingsWeighbridge == null)
            SelectSettingsWeighbridgeFromSavedSettings();

        ApplyMasterFilters();
        RefreshPartyLookup();
        SelectedWeighmentItem ??= ItemMasters.FirstOrDefault();
        SelectedMaterial ??= Materials.FirstOrDefault();
    }

    private async Task RefreshWeighmentsAsync()
    {
        ReplaceCollection(OpenWeighments, await _databaseService.GetOpenWeighmentsAsync());
        ReplaceCollection(CompletedToday, await _databaseService.GetCompletedTodayAsync());
    }

    private async Task AddPartyAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPartyName))
            {
                StatusMessage = "Please enter party name.";
                return;
            }

            await _databaseService.AddPartyAsync(NewPartyName, NewPartyType);
            NewPartyName = string.Empty;
            await LoadMastersAsync();
            StatusMessage = "Party saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Party save error: " + ex.Message;
        }
    }

    private async Task AddMaterialAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewMaterialName))
            {
                StatusMessage = "Please enter material name.";
                return;
            }

            await _databaseService.AddMaterialAsync(NewMaterialName);
            NewMaterialName = string.Empty;
            await LoadMastersAsync();
            StatusMessage = "Material saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Material save error: " + ex.Message;
        }
    }

    private async Task AddVehicleAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewVehicleNo))
            {
                StatusMessage = "Please enter vehicle number.";
                return;
            }

            await _databaseService.AddVehicleAsync(NewVehicleNo);
            NewVehicleNo = string.Empty;
            await LoadMastersAsync();
            StatusMessage = "Vehicle saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Vehicle save error: " + ex.Message;
        }
    }

    private async Task LoadReportAsync()
    {
        try
        {
            if (!CanAccessReports)
            {
                StatusMessage = "You do not have access to Reports.";
                return;
            }

            if (ReportTo < ReportFrom)
            {
                StatusMessage = "Report To date cannot be earlier than From date.";
                return;
            }

            var rows = await _databaseService.SearchWeighmentsAsync(ReportFrom, ReportTo);
            ReplaceCollection(ReportRows, rows);
            ApplyReportFilter();
            StatusMessage = $"Report loaded. Rows: {FilteredReportRows.Count} of {ReportRows.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Report load error: " + ex.Message;
        }
    }

    private Task ExportReportAsync()
    {
        try
        {
            if (!CanAccessReports)
            {
                StatusMessage = "You do not have access to Reports.";
                return Task.CompletedTask;
            }

            if (FilteredReportRows.Count == 0)
            {
                StatusMessage = "No report rows to export.";
                return Task.CompletedTask;
            }

            var filePath = CsvExportService.ExportWeighments(FilteredReportRows);
            StatusMessage = "Report exported: " + filePath;
        }
        catch (Exception ex)
        {
            StatusMessage = "Report export error: " + ex.Message;
        }

        return Task.CompletedTask;
    }

    private Task PrintSlipAsync()
    {
        try
        {
            if (!CanAccessReports)
            {
                StatusMessage = "You do not have access to Reports.";
                return Task.CompletedTask;
            }

            if (SelectedReportWeighment == null)
            {
                StatusMessage = "Please select a completed ticket from report first.";
                return Task.CompletedTask;
            }

            var printed = SlipService.PrintSlip(SelectedReportWeighment);
            StatusMessage = printed
                ? "Slip sent to printer."
                : "Print cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Slip print error: " + ex.Message;
        }

        return Task.CompletedTask;
    }

    private async Task SaveCompletedEditAsync()
    {
        await SaveCompletedTransactionEditAsync(SelectedCompletedWeighment);
    }

    private async Task SaveReportEditAsync()
    {
        await SaveCompletedTransactionEditAsync(SelectedReportWeighment);
    }

    private async Task SaveCompletedTransactionEditAsync(Weighment? weighment)
    {
        try
        {
            if (!CanEditCompletedTransaction)
            {
                StatusMessage = "You do not have edit access for completed transactions.";
                return;
            }

            if (weighment == null)
            {
                StatusMessage = "Please select a completed transaction first.";
                return;
            }

            await _databaseService.UpdateCompletedWeighmentAsync(weighment);
            await RefreshAllAsync();
            StatusMessage = $"Completed transaction updated: {weighment.TicketNo}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Completed transaction edit error: " + ex.Message;
        }
    }

    private async Task DeleteCompletedAsync()
    {
        await DeleteCompletedTransactionAsync(SelectedCompletedWeighment);
    }

    private async Task DeleteReportRowAsync()
    {
        await DeleteCompletedTransactionAsync(SelectedReportWeighment);
    }

    private async Task DeleteCompletedTransactionAsync(Weighment? weighment)
    {
        try
        {
            if (!CanDeleteCompletedTransaction)
            {
                StatusMessage = "You do not have delete access for completed transactions.";
                return;
            }

            if (weighment == null)
            {
                StatusMessage = "Please select a completed transaction first.";
                return;
            }

            var confirm = System.Windows.MessageBox.Show(
                $"Delete completed ticket {weighment.TicketNo}?",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (confirm != System.Windows.MessageBoxResult.Yes)
                return;

            await _databaseService.DeleteWeighmentAsync(weighment.WeighmentId);
            await RefreshAllAsync();
            StatusMessage = $"Completed transaction deleted: {weighment.TicketNo}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Completed transaction delete error: " + ex.Message;
        }
    }


    private async Task SaveCustomerAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            await _databaseService.SaveCustomerAsync(CustomerForm);
            await _databaseService.AddPartyAsync(string.IsNullOrWhiteSpace(CustomerForm.Name) ? CustomerForm.CustomerAccount : CustomerForm.Name, "Customer");
            await LoadMastersAsync();
            StatusMessage = "Customer saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Customer save error: " + ex.Message;
        }
    }

    private void ClearCustomerForm()
    {
        SelectedCustomer = null;
        CustomerForm = new Customer();
    }

    private void LoadSelectedCustomerToForm()
    {
        if (SelectedCustomer == null)
            return;

        CustomerForm = new Customer
        {
            CustomerId = SelectedCustomer.CustomerId,
            CustomerAccount = SelectedCustomer.CustomerAccount,
            Name = SelectedCustomer.Name,
            MethodOfPayment = SelectedCustomer.MethodOfPayment,
            TermsOfPayment = SelectedCustomer.TermsOfPayment,
            DeliveryTerms = SelectedCustomer.DeliveryTerms,
            AccountStatus = SelectedCustomer.AccountStatus,
            AccountStatusReason = SelectedCustomer.AccountStatusReason,
            CustomerGroup = SelectedCustomer.CustomerGroup,
            EmployeeResponsible = SelectedCustomer.EmployeeResponsible,
            Currency = SelectedCustomer.Currency,
            Telephone = SelectedCustomer.Telephone,
            OrganizationPerson = SelectedCustomer.OrganizationPerson,
            SearchName = SelectedCustomer.SearchName,
            ClassificationGroup = SelectedCustomer.ClassificationGroup,
            AddressNameDescription = SelectedCustomer.AddressNameDescription,
            Address = SelectedCustomer.Address,
            AddressPurpose = SelectedCustomer.AddressPurpose,
            ContactDescription = SelectedCustomer.ContactDescription,
            ContactType = SelectedCustomer.ContactType,
            ContactNumberAddress = SelectedCustomer.ContactNumberAddress,
            ContactExtension = SelectedCustomer.ContactExtension,
            InvoiceAccount = SelectedCustomer.InvoiceAccount,
            ModeOfDelivery = SelectedCustomer.ModeOfDelivery,
            SalesTaxGroup = SelectedCustomer.SalesTaxGroup
        };
        StatusMessage = $"Customer opened: {CustomerForm.CustomerAccount}";
    }

    private async Task SaveVendorAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            await _databaseService.SaveVendorAsync(VendorForm);
            await _databaseService.AddPartyAsync(string.IsNullOrWhiteSpace(VendorForm.Name) ? VendorForm.VendorAccount : VendorForm.Name, "Vendor");
            await LoadMastersAsync();
            StatusMessage = "Vendor saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Vendor save error: " + ex.Message;
        }
    }

    private void ClearVendorForm()
    {
        SelectedVendor = null;
        VendorForm = new Vendor();
    }

    private void LoadSelectedVendorToForm()
    {
        if (SelectedVendor == null)
            return;

        VendorForm = new Vendor
        {
            VendorId = SelectedVendor.VendorId,
            VendorAccount = SelectedVendor.VendorAccount,
            Name = SelectedVendor.Name,
            MethodOfPayment = SelectedVendor.MethodOfPayment,
            TermsOfPayment = SelectedVendor.TermsOfPayment,
            DeliveryTerms = SelectedVendor.DeliveryTerms,
            AccountStatus = SelectedVendor.AccountStatus,
            AccountStatusReason = SelectedVendor.AccountStatusReason,
            VendorGroup = SelectedVendor.VendorGroup,
            EmployeeResponsible = SelectedVendor.EmployeeResponsible,
            Currency = SelectedVendor.Currency,
            Telephone = SelectedVendor.Telephone,
            Type = SelectedVendor.Type,
            VendorClassificationGroup = SelectedVendor.VendorClassificationGroup,
            SearchName = SelectedVendor.SearchName,
            AddressNameDescription = SelectedVendor.AddressNameDescription,
            Address = SelectedVendor.Address,
            AddressPurpose = SelectedVendor.AddressPurpose,
            ContactDescription = SelectedVendor.ContactDescription,
            ContactType = SelectedVendor.ContactType,
            ContactNumberAddress = SelectedVendor.ContactNumberAddress,
            ContactExtension = SelectedVendor.ContactExtension,
            InvoiceAccount = SelectedVendor.InvoiceAccount,
            ModeOfDelivery = SelectedVendor.ModeOfDelivery,
            SalesTaxGroup = SelectedVendor.SalesTaxGroup
        };
        StatusMessage = $"Vendor opened: {VendorForm.VendorAccount}";
    }

    private async Task SaveItemMasterAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            await _databaseService.SaveItemMasterAsync(ItemMasterForm);
            await _databaseService.AddMaterialAsync(string.IsNullOrWhiteSpace(ItemMasterForm.ProductName) ? ItemMasterForm.ItemNumber : ItemMasterForm.ProductName);
            await LoadMastersAsync();
            StatusMessage = "Item saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Item save error: " + ex.Message;
        }
    }

    private void ClearItemMasterForm()
    {
        SelectedItemMaster = null;
        ItemMasterForm = new ItemMaster();
    }

    private void LoadSelectedItemMasterToForm()
    {
        if (SelectedItemMaster == null)
            return;

        ItemMasterForm = new ItemMaster
        {
            ItemMasterId = SelectedItemMaster.ItemMasterId,
            ItemNumber = SelectedItemMaster.ItemNumber,
            ProductName = SelectedItemMaster.ProductName,
            SearchName = SelectedItemMaster.SearchName,
            ProductType = SelectedItemMaster.ProductType,
            ProductSubtype = SelectedItemMaster.ProductSubtype,
            ProductNumber = SelectedItemMaster.ProductNumber,
            Description = SelectedItemMaster.Description,
            StorageDimensionGroup = SelectedItemMaster.StorageDimensionGroup,
            TrackingDimensionGroup = SelectedItemMaster.TrackingDimensionGroup,
            ItemModelGroup = SelectedItemMaster.ItemModelGroup,
            ReservationHierarchy = SelectedItemMaster.ReservationHierarchy,
            PurchaseUnit = SelectedItemMaster.PurchaseUnit,
            PurchaseOverDelivery = SelectedItemMaster.PurchaseOverDelivery,
            PurchaseUnderDelivery = SelectedItemMaster.PurchaseUnderDelivery,
            BuyerGroup = SelectedItemMaster.BuyerGroup,
            ItemPriceToleranceGroup = SelectedItemMaster.ItemPriceToleranceGroup,
            Vendor = SelectedItemMaster.Vendor,
            PurchaseItemSalesTaxGroup = SelectedItemMaster.PurchaseItemSalesTaxGroup,
            SellUnit = SelectedItemMaster.SellUnit,
            SellOverDelivery = SelectedItemMaster.SellOverDelivery,
            SellUnderDelivery = SelectedItemMaster.SellUnderDelivery,
            SellItemSalesTaxGroup = SelectedItemMaster.SellItemSalesTaxGroup,
            BatchNumberGroup = SelectedItemMaster.BatchNumberGroup,
            SerialNumberGroup = SelectedItemMaster.SerialNumberGroup,
            InventoryOverDelivery = SelectedItemMaster.InventoryOverDelivery,
            InventoryUnderDelivery = SelectedItemMaster.InventoryUnderDelivery,
            CatchWeightItem = SelectedItemMaster.CatchWeightItem,
            CWUnit = SelectedItemMaster.CWUnit,
            NominalQuantity = SelectedItemMaster.NominalQuantity,
            MinimumQuantity = SelectedItemMaster.MinimumQuantity,
            MaximumQuantity = SelectedItemMaster.MaximumQuantity,
            BOMUnit = SelectedItemMaster.BOMUnit,
            ConstantScrap = SelectedItemMaster.ConstantScrap,
            VariableScrap = SelectedItemMaster.VariableScrap,
            CostingLevel = SelectedItemMaster.CostingLevel,
            PlanningLevel = SelectedItemMaster.PlanningLevel,
            CostCalculationLevel = SelectedItemMaster.CostCalculationLevel,
            Phantom = SelectedItemMaster.Phantom,
            CalculationGroup = SelectedItemMaster.CalculationGroup,
            ProductionType = SelectedItemMaster.ProductionType,
            ItemGroup = SelectedItemMaster.ItemGroup,
            CostUnit = SelectedItemMaster.CostUnit,
            LastCostPrice = SelectedItemMaster.LastCostPrice,
            DateOfPrice = SelectedItemMaster.DateOfPrice,
            UnitSequenceGroupId = SelectedItemMaster.UnitSequenceGroupId
        };
        StatusMessage = $"Item opened: {ItemMasterForm.ItemNumber}";
    }

    private async Task SaveWarehouseMasterAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            await _databaseService.SaveWarehouseMasterAsync(WarehouseMasterForm);
            await LoadMastersAsync();
            StatusMessage = "Warehouse saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Warehouse save error: " + ex.Message;
        }
    }

    private void ClearWarehouseMasterForm()
    {
        SelectedWarehouseMaster = null;
        WarehouseMasterForm = new WarehouseMaster();
    }

    private void LoadSelectedWarehouseMasterToForm()
    {
        if (SelectedWarehouseMaster == null)
            return;

        WarehouseMasterForm = new WarehouseMaster
        {
            WarehouseMasterId = SelectedWarehouseMaster.WarehouseMasterId,
            Warehouse = SelectedWarehouseMaster.Warehouse,
            Name = SelectedWarehouseMaster.Name,
            Site = SelectedWarehouseMaster.Site,
            Type = SelectedWarehouseMaster.Type,
            QuarantineWarehouse = SelectedWarehouseMaster.QuarantineWarehouse,
            TransitWarehouse = SelectedWarehouseMaster.TransitWarehouse,
            GoodsInTransitWarehouse = SelectedWarehouseMaster.GoodsInTransitWarehouse,
            UnderDeliveryWarehouse = SelectedWarehouseMaster.UnderDeliveryWarehouse,
            VendorAccount = SelectedWarehouseMaster.VendorAccount,
            DefaultReceiptLocation = SelectedWarehouseMaster.DefaultReceiptLocation,
            DefaultIssueLocation = SelectedWarehouseMaster.DefaultIssueLocation,
            DefaultProductionFinishedGood = SelectedWarehouseMaster.DefaultProductionFinishedGood,
            AddressNameDescription = SelectedWarehouseMaster.AddressNameDescription,
            Address = SelectedWarehouseMaster.Address,
            Purpose = SelectedWarehouseMaster.Purpose
        };
        StatusMessage = $"Warehouse opened: {WarehouseMasterForm.Warehouse}";
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            if (!CanAccessUserManagement)
            {
                StatusMessage = "You do not have access to User Management.";
                return;
            }

            ReplaceCollection(Users, await _databaseService.GetUsersAsync());
            StatusMessage = $"Users loaded. Rows: {Users.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Users load error: " + ex.Message;
        }
    }

    private async Task SaveUserAsync()
    {
        try
        {
            if (!CanAccessUserManagement)
            {
                StatusMessage = "You do not have access to User Management.";
                return;
            }

            if (string.IsNullOrWhiteSpace(UserUsername))
            {
                StatusMessage = "Please enter username.";
                return;
            }

            if (string.IsNullOrWhiteSpace(UserCompanyName))
            {
                StatusMessage = "Please enter company name.";
                return;
            }

            if (!_editingUserId.HasValue && string.IsNullOrWhiteSpace(UserPassword))
            {
                StatusMessage = "Please enter password for new user.";
                return;
            }

            var user = new AppUser
            {
                UserId = _editingUserId ?? 0,
                Username = UserUsername.Trim(),
                FullName = string.IsNullOrWhiteSpace(UserFullName) ? UserUsername.Trim() : UserFullName.Trim(),
                CompanyName = UserCompanyName.Trim(),
                IsActive = UserIsActive,
                CanAccessWeighment = UserCanAccessWeighment,
                CanAccessSettings = UserCanAccessSettings,
                CanAccessMasters = UserCanAccessMasters,
                CanAccessReports = UserCanAccessReports,
                CanAccessUserManagement = UserCanAccessUserManagement,
                CanEditCompletedTransaction = UserCanEditCompletedTransaction,
                CanDeleteCompletedTransaction = UserCanDeleteCompletedTransaction
            };

            if (_editingUserId == _currentUser.UserId)
            {
                if (!user.IsActive)
                {
                    StatusMessage = "You cannot deactivate your own user.";
                    return;
                }

                if (!user.CanAccessUserManagement)
                {
                    StatusMessage = "You cannot remove your own User Management access.";
                    return;
                }
            }

            if (_editingUserId.HasValue)
            {
                await _databaseService.UpdateUserAsync(user, UserPassword);
                if (_editingUserId == _currentUser.UserId)
                    ApplyCurrentUserChanges(user);
                StatusMessage = "User updated.";
            }
            else
            {
                await _databaseService.AddUserAsync(user, UserPassword);
                StatusMessage = "User created.";
            }

            ClearUserForm();
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "User save error: " + ex.Message;
        }
    }

    private void ApplyCurrentUserChanges(AppUser user)
    {
        _currentUser.Username = user.Username;
        _currentUser.FullName = user.FullName;
        _currentUser.CompanyName = user.CompanyName;
        _currentUser.IsActive = user.IsActive;
        _currentUser.CanAccessWeighment = user.CanAccessWeighment;
        _currentUser.CanAccessSettings = user.CanAccessSettings;
        _currentUser.CanAccessMasters = user.CanAccessMasters;
        _currentUser.CanAccessReports = user.CanAccessReports;
        _currentUser.CanAccessUserManagement = user.CanAccessUserManagement;
        _currentUser.CanEditCompletedTransaction = user.CanEditCompletedTransaction;
        _currentUser.CanDeleteCompletedTransaction = user.CanDeleteCompletedTransaction;

        OnPropertyChanged(nameof(CurrentUserDisplay));
        OnPropertyChanged(nameof(CurrentUserCompany));
        OnPropertyChanged(nameof(CanAccessWeighment));
        OnPropertyChanged(nameof(CanAccessSettings));
        OnPropertyChanged(nameof(CanAccessMasters));
        OnPropertyChanged(nameof(CanAccessReports));
        OnPropertyChanged(nameof(CanAccessUserManagement));
        OnPropertyChanged(nameof(CanEditCompletedTransaction));
        OnPropertyChanged(nameof(CanDeleteCompletedTransaction));
        OnPropertyChanged(nameof(IsCompletedGridReadOnly));
        NotifyWeighmentButtonStates();
    }

    private void LoadSelectedUserToForm()
    {
        if (SelectedUser == null)
            return;

        _editingUserId = SelectedUser.UserId;
        UserUsername = SelectedUser.Username;
        UserFullName = SelectedUser.FullName;
        UserCompanyName = SelectedUser.CompanyName;
        UserPassword = string.Empty;
        UserIsActive = SelectedUser.IsActive;
        UserCanAccessWeighment = SelectedUser.CanAccessWeighment;
        UserCanAccessSettings = SelectedUser.CanAccessSettings;
        UserCanAccessMasters = SelectedUser.CanAccessMasters;
        UserCanAccessReports = SelectedUser.CanAccessReports;
        UserCanAccessUserManagement = SelectedUser.CanAccessUserManagement;
        UserCanEditCompletedTransaction = SelectedUser.CanEditCompletedTransaction;
        UserCanDeleteCompletedTransaction = SelectedUser.CanDeleteCompletedTransaction;
        OnPropertyChanged(nameof(UserFormModeText));
        StatusMessage = $"Selected user: {SelectedUser.Username}. Leave password blank if you do not want to change it.";
    }

    private void ClearUserForm()
    {
        _editingUserId = null;
        SelectedUser = null;
        UserUsername = string.Empty;
        UserFullName = string.Empty;
        UserCompanyName = string.Empty;
        UserPassword = string.Empty;
        UserIsActive = true;
        UserCanAccessWeighment = true;
        UserCanAccessSettings = false;
        UserCanAccessMasters = false;
        UserCanAccessReports = true;
        UserCanAccessUserManagement = false;
        UserCanEditCompletedTransaction = false;
        UserCanDeleteCompletedTransaction = false;
        OnPropertyChanged(nameof(UserFormModeText));
    }

    private void ClearEntry()
    {
        SetLoadedOpenWeighmentId(null);
        TicketNo = string.Empty;
        VehicleNo = string.Empty;
        DriverName = string.Empty;
        PartyAccount = string.Empty;
        PartyName = string.Empty;
        ItemNumber = string.Empty;
        ItemName = string.Empty;
        Remarks = string.Empty;
        FirstWeight = null;
        SecondWeight = null;
        NetWeight = null;
        SelectedOpenWeighment = null;
        SelectedWeighmentParty = FilteredParties.FirstOrDefault();
        SelectedWeighmentItem = ItemMasters.FirstOrDefault();
    }

    private bool ValidateEntryBeforeFirstWeight()
    {
        if (!CanAccessWeighment)
        {
            StatusMessage = "You do not have access to Weighment.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CurrentUserCompany))
        {
            StatusMessage = "Company is mandatory. Please update company in User Management for this user.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(VehicleNo))
        {
            StatusMessage = "Please enter vehicle number.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(PartyName))
        {
            StatusMessage = "Please select or enter party.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ItemName))
        {
            StatusMessage = "Please select or enter item.";
            return false;
        }

        if (!IsStable)
        {
            StatusMessage = "Weight is unstable. Please wait until stable.";
            return false;
        }

        return true;
    }

    private void RefreshPartyLookup()
    {
        var previousId = SelectedWeighmentParty?.PartyId;
        FilteredParties.Clear();

        IEnumerable<Party> lookup = SelectedPartyType == "Vendor"
            ? Vendors.Select(v => new Party { PartyId = v.VendorId, PartyAccount = v.VendorAccount, PartyName = v.Name, PartyType = "Vendor" })
            : Customers.Select(c => new Party { PartyId = c.CustomerId, PartyAccount = c.CustomerAccount, PartyName = c.Name, PartyType = "Customer" });

        foreach (var item in lookup)
            FilteredParties.Add(item);

        SelectedWeighmentParty = FilteredParties.FirstOrDefault(x => x.PartyId == previousId) ?? FilteredParties.FirstOrDefault();
    }

    private async Task SaveVehicleMasterAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            await _databaseService.SaveVehicleAsync(VehicleMasterForm);
            await LoadMastersAsync();
            StatusMessage = "Vehicle master saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Vehicle master save error: " + ex.Message;
        }
    }

    private void ClearVehicleMasterForm()
    {
        SelectedVehicleMaster = null;
        VehicleMasterForm = new Vehicle { IsActive = true, Status = "Active" };
    }

    private void LoadSelectedVehicleMasterToForm()
    {
        if (SelectedVehicleMaster == null)
            return;

        VehicleMasterForm = new Vehicle
        {
            VehicleId = SelectedVehicleMaster.VehicleId,
            PlateNumber = SelectedVehicleMaster.PlateNumber,
            PlateEmirate = SelectedVehicleMaster.PlateEmirate,
            PlateCategory = SelectedVehicleMaster.PlateCategory,
            VehicleType = SelectedVehicleMaster.VehicleType,
            OwnershipType = SelectedVehicleMaster.OwnershipType,
            OwnerPartyAccount = SelectedVehicleMaster.OwnerPartyAccount,
            Transporter = SelectedVehicleMaster.Transporter,
            Capacity = SelectedVehicleMaster.Capacity,
            DefaultDriver = SelectedVehicleMaster.DefaultDriver,
            RegistrationExpiryDate = SelectedVehicleMaster.RegistrationExpiryDate,
            LegalEntity = SelectedVehicleMaster.LegalEntity,
            Status = SelectedVehicleMaster.Status,
            IsActive = SelectedVehicleMaster.IsActive
        };
    }

    private async Task SaveDriverMasterAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            await _databaseService.SaveDriverAsync(DriverMasterForm);
            await LoadMastersAsync();
            StatusMessage = "Driver master saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Driver master save error: " + ex.Message;
        }
    }

    private void ClearDriverMasterForm()
    {
        SelectedDriverMaster = null;
        DriverMasterForm = new Driver { IsActive = true, Status = "Active", EffectiveFrom = DateTime.Today };
    }

    private void LoadSelectedDriverMasterToForm()
    {
        if (SelectedDriverMaster == null)
            return;

        DriverMasterForm = new Driver
        {
            DriverId = SelectedDriverMaster.DriverId,
            DriverName = SelectedDriverMaster.DriverName,
            MobileNumber = SelectedDriverMaster.MobileNumber,
            SecondaryMobile = SelectedDriverMaster.SecondaryMobile,
            Email = SelectedDriverMaster.Email,
            Nationality = SelectedDriverMaster.Nationality,
            DriverType = SelectedDriverMaster.DriverType,
            EmployerPartyType = SelectedDriverMaster.EmployerPartyType,
            EmployerAccount = SelectedDriverMaster.EmployerAccount,
            IdentificationType = SelectedDriverMaster.IdentificationType,
            IdentificationNumber = SelectedDriverMaster.IdentificationNumber,
            IdentificationExpiryDate = SelectedDriverMaster.IdentificationExpiryDate,
            EmiratesIdExpiryDate = SelectedDriverMaster.EmiratesIdExpiryDate,
            PassportNumber = SelectedDriverMaster.PassportNumber,
            PassportExpiryDate = SelectedDriverMaster.PassportExpiryDate,
            DrivingLicenceNumber = SelectedDriverMaster.DrivingLicenceNumber,
            DrivingLicenceIssuedBy = SelectedDriverMaster.DrivingLicenceIssuedBy,
            DrivingLicenceExpiryDate = SelectedDriverMaster.DrivingLicenceExpiryDate,
            LicenceCategories = SelectedDriverMaster.LicenceCategories,
            DefaultVehicle = SelectedDriverMaster.DefaultVehicle,
            Address = SelectedDriverMaster.Address,
            DriverPhoto = SelectedDriverMaster.DriverPhoto,
            EmiratesIdAttachment = SelectedDriverMaster.EmiratesIdAttachment,
            PassportAttachment = SelectedDriverMaster.PassportAttachment,
            DrivingLicenceAttachment = SelectedDriverMaster.DrivingLicenceAttachment,
            LegalEntity = SelectedDriverMaster.LegalEntity,
            Status = SelectedDriverMaster.Status,
            Blacklisted = SelectedDriverMaster.Blacklisted,
            BlacklistReason = SelectedDriverMaster.BlacklistReason,
            EffectiveFrom = SelectedDriverMaster.EffectiveFrom,
            IsActive = SelectedDriverMaster.IsActive,
            Remarks = SelectedDriverMaster.Remarks
        };
    }



    private void SelectSettingsWeighbridgeFromSavedSettings()
    {
        var selectedCode = Settings.SelectedWeighbridgeCode;
        SelectedSettingsWeighbridge = WeighbridgeMasters.FirstOrDefault(x =>
            string.Equals(x.WeighbridgeCode, selectedCode, StringComparison.OrdinalIgnoreCase));

        SelectedSettingsWeighbridge ??= WeighbridgeMasters.FirstOrDefault(x => x.IsActive);
        ApplySelectedWeighbridgeToSettings();
    }

    private void ApplySelectedWeighbridgeToSettings()
    {
        if (SelectedSettingsWeighbridge == null)
            return;

        Settings.SelectedWeighbridgeCode = SelectedSettingsWeighbridge.WeighbridgeCode;
        Settings.ConnectionType = string.IsNullOrWhiteSpace(SelectedSettingsWeighbridge.CommunicationType) ? "Mock" : SelectedSettingsWeighbridge.CommunicationType;
        Settings.ComPort = string.IsNullOrWhiteSpace(SelectedSettingsWeighbridge.ScaleComPort) ? "COM1" : SelectedSettingsWeighbridge.ScaleComPort;
        Settings.BaudRate = SelectedSettingsWeighbridge.BaudRate <= 0 ? 9600 : SelectedSettingsWeighbridge.BaudRate;
        Settings.Parity = string.IsNullOrWhiteSpace(SelectedSettingsWeighbridge.Parity) ? "None" : SelectedSettingsWeighbridge.Parity;
        Settings.DataBits = SelectedSettingsWeighbridge.DataBits <= 0 ? 8 : SelectedSettingsWeighbridge.DataBits;
        Settings.StopBits = string.IsNullOrWhiteSpace(SelectedSettingsWeighbridge.StopBits) ? "One" : SelectedSettingsWeighbridge.StopBits;
        Settings.IpAddress = string.IsNullOrWhiteSpace(SelectedSettingsWeighbridge.ScaleIpAddress) ? "192.168.1.100" : SelectedSettingsWeighbridge.ScaleIpAddress;
        Settings.TcpPort = SelectedSettingsWeighbridge.TcpPort <= 0 ? 4001 : SelectedSettingsWeighbridge.TcpPort;
        Settings.MinimumWeight = SelectedSettingsWeighbridge.MinimumWeight;
        Settings.WeightIncrement = SelectedSettingsWeighbridge.WeightIncrement;
        Settings.WeightStabilityTime = SelectedSettingsWeighbridge.WeightStabilityTime;
        Settings.CapacityUnit = string.IsNullOrWhiteSpace(SelectedSettingsWeighbridge.CapacityUnit) ? "kg" : SelectedSettingsWeighbridge.CapacityUnit;
        Settings.Printer = SelectedSettingsWeighbridge.Printer;

        OnPropertyChanged(nameof(Settings));
    }

    private async Task SaveWeighbridgeMasterAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            await _databaseService.SaveWeighbridgeMasterAsync(WeighbridgeMasterForm);
            await LoadMastersAsync();
            StatusMessage = "Weighbridge master saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Weighbridge master save error: " + ex.Message;
        }
    }

    private void ClearWeighbridgeMasterForm()
    {
        SelectedWeighbridgeMaster = null;
        WeighbridgeMasterForm = new WeighbridgeMaster
        {
            CapacityUnit = "kg",
            CommunicationType = "Mock",
            ScaleIpAddress = "192.168.1.100",
            TcpPort = 4001,
            ScaleComPort = "COM1",
            BaudRate = 9600,
            Parity = "None",
            DataBits = 8,
            StopBits = "One",
            OperatingStatus = "Active",
            EffectiveFrom = DateTime.Today,
            IsActive = true
        };
    }

    private void LoadSelectedWeighbridgeMasterToForm()
    {
        if (SelectedWeighbridgeMaster == null)
            return;

        WeighbridgeMasterForm = new WeighbridgeMaster
        {
            WeighbridgeId = SelectedWeighbridgeMaster.WeighbridgeId,
            WeighbridgeCode = SelectedWeighbridgeMaster.WeighbridgeCode,
            WeighbridgeName = SelectedWeighbridgeMaster.WeighbridgeName,
            Description = SelectedWeighbridgeMaster.Description,
            PlantSite = SelectedWeighbridgeMaster.PlantSite,
            Warehouse = SelectedWeighbridgeMaster.Warehouse,
            WarehouseAddress = SelectedWeighbridgeMaster.WarehouseAddress,
            WeighbridgeType = SelectedWeighbridgeMaster.WeighbridgeType,
            ScaleType = SelectedWeighbridgeMaster.ScaleType,
            ScaleCapacity = SelectedWeighbridgeMaster.ScaleCapacity,
            CapacityUnit = SelectedWeighbridgeMaster.CapacityUnit,
            MinimumWeight = SelectedWeighbridgeMaster.MinimumWeight,
            WeightIncrement = SelectedWeighbridgeMaster.WeightIncrement,
            WeightStabilityTime = SelectedWeighbridgeMaster.WeightStabilityTime,
            ScaleIpAddress = SelectedWeighbridgeMaster.ScaleIpAddress,
            TcpPort = SelectedWeighbridgeMaster.TcpPort,
            ScaleComPort = SelectedWeighbridgeMaster.ScaleComPort,
            BaudRate = SelectedWeighbridgeMaster.BaudRate,
            Parity = SelectedWeighbridgeMaster.Parity,
            DataBits = SelectedWeighbridgeMaster.DataBits,
            StopBits = SelectedWeighbridgeMaster.StopBits,
            CommunicationType = SelectedWeighbridgeMaster.CommunicationType,
            ScaleManufacturer = SelectedWeighbridgeMaster.ScaleManufacturer,
            ScaleModel = SelectedWeighbridgeMaster.ScaleModel,
            ScaleSerialNumber = SelectedWeighbridgeMaster.ScaleSerialNumber,
            CalibrationCertificateNo = SelectedWeighbridgeMaster.CalibrationCertificateNo,
            LastCalibrationDate = SelectedWeighbridgeMaster.LastCalibrationDate,
            NextCalibrationDate = SelectedWeighbridgeMaster.NextCalibrationDate,
            Printer = SelectedWeighbridgeMaster.Printer,
            CameraAvailable = SelectedWeighbridgeMaster.CameraAvailable,
            AnprAvailable = SelectedWeighbridgeMaster.AnprAvailable,
            TrafficLightAvailable = SelectedWeighbridgeMaster.TrafficLightAvailable,
            BoomBarrierAvailable = SelectedWeighbridgeMaster.BoomBarrierAvailable,
            CctvAvailable = SelectedWeighbridgeMaster.CctvAvailable,
            DefaultTicketTemplate = SelectedWeighbridgeMaster.DefaultTicketTemplate,
            DefaultCurrency = SelectedWeighbridgeMaster.DefaultCurrency,
            DefaultOperator = SelectedWeighbridgeMaster.DefaultOperator,
            AllowedOperators = SelectedWeighbridgeMaster.AllowedOperators,
            OperatingStatus = SelectedWeighbridgeMaster.OperatingStatus,
            EffectiveFrom = SelectedWeighbridgeMaster.EffectiveFrom,
            IsActive = SelectedWeighbridgeMaster.IsActive,
            Remarks = SelectedWeighbridgeMaster.Remarks
        };
    }

    private async Task SaveOperatorMasterAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            await _databaseService.SaveOperatorMasterAsync(OperatorMasterForm);
            await LoadMastersAsync();
            StatusMessage = "Operator master saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Operator master save error: " + ex.Message;
        }
    }

    private void ClearOperatorMasterForm()
    {
        SelectedOperatorMaster = null;
        OperatorMasterForm = new OperatorMaster
        {
            CanCaptureFirstWeight = true,
            CanCaptureSecondWeight = true,
            Status = "Active",
            EffectiveFrom = DateTime.Today,
            IsActive = true
        };
    }

    private void LoadSelectedOperatorMasterToForm()
    {
        if (SelectedOperatorMaster == null)
            return;

        OperatorMasterForm = new OperatorMaster
        {
            OperatorId = SelectedOperatorMaster.OperatorId,
            EmployeeId = SelectedOperatorMaster.EmployeeId,
            OperatorName = SelectedOperatorMaster.OperatorName,
            Username = SelectedOperatorMaster.Username,
            Email = SelectedOperatorMaster.Email,
            MobileNumber = SelectedOperatorMaster.MobileNumber,
            Designation = SelectedOperatorMaster.Designation,
            Department = SelectedOperatorMaster.Department,
            LegalEntity = SelectedOperatorMaster.LegalEntity,
            DefaultLegalEntity = SelectedOperatorMaster.DefaultLegalEntity,
            DefaultWeighbridge = SelectedOperatorMaster.DefaultWeighbridge,
            AssignedWeighbridges = SelectedOperatorMaster.AssignedWeighbridges,
            DefaultShift = SelectedOperatorMaster.DefaultShift,
            Role = SelectedOperatorMaster.Role,
            PermissionProfile = SelectedOperatorMaster.PermissionProfile,
            CanCaptureFirstWeight = SelectedOperatorMaster.CanCaptureFirstWeight,
            CanCaptureSecondWeight = SelectedOperatorMaster.CanCaptureSecondWeight,
            CanPerformManualWeightEntry = SelectedOperatorMaster.CanPerformManualWeightEntry,
            CanCorrectTransactions = SelectedOperatorMaster.CanCorrectTransactions,
            CanCancelTransactions = SelectedOperatorMaster.CanCancelTransactions,
            CanOverrideWeight = SelectedOperatorMaster.CanOverrideWeight,
            CanApproveQc = SelectedOperatorMaster.CanApproveQc,
            CanRetryIntegration = SelectedOperatorMaster.CanRetryIntegration,
            LastLogin = SelectedOperatorMaster.LastLogin,
            Status = SelectedOperatorMaster.Status,
            EffectiveFrom = SelectedOperatorMaster.EffectiveFrom,
            IsActive = SelectedOperatorMaster.IsActive,
            Remarks = SelectedOperatorMaster.Remarks
        };
    }

    private void ClearReportFilters()
    {
        ReportTicketFilter = string.Empty;
        ReportCompanyFilter = string.Empty;
        ReportVehicleFilter = string.Empty;
        ReportDriverFilter = string.Empty;
        ReportPartyFilter = string.Empty;
        ReportPartyTypeFilter = string.Empty;
        ReportItemFilter = string.Empty;
        ReportStatusFilter = string.Empty;
        ApplyReportFilter();
    }

    private void ApplyReportFilter()
    {
        ReplaceCollection(FilteredReportRows, ReportRows.Where(x =>
            MatchesFilter(x.TicketNo, ReportTicketFilter) &&
            MatchesFilter(x.CompanyName, ReportCompanyFilter) &&
            MatchesFilter(x.VehicleNo, ReportVehicleFilter) &&
            MatchesFilter(x.DriverName, ReportDriverFilter) &&
            (MatchesFilter(x.PartyAccount, ReportPartyFilter) || MatchesFilter(x.PartyName, ReportPartyFilter)) &&
            MatchesFilter(x.PartyType, ReportPartyTypeFilter) &&
            (MatchesFilter(x.ItemNumber, ReportItemFilter) || MatchesFilter(x.ItemName, ReportItemFilter) || MatchesFilter(x.MaterialName, ReportItemFilter)) &&
            MatchesFilter(x.Status, ReportStatusFilter)));
    }

    private void ApplyMasterFilters()
    {
        ApplyCustomerFilter();
        ApplyVendorFilter();
        ApplyItemMasterFilter();
        ApplyWarehouseFilter();
        ApplyVehicleFilter();
        ApplyDriverFilter();
        ApplyWeighbridgeFilter();
        ApplyOperatorFilter();
    }

    private void ApplyCustomerFilter()
    {
        ReplaceCollection(FilteredCustomers, Customers.Where(x =>
            MatchesFilter(x.CustomerAccount, CustomerAccountFilter) &&
            MatchesFilter(x.Name, CustomerNameFilter) &&
            MatchesFilter(x.CustomerGroup, CustomerGroupFilter) &&
            (MatchesFilter(x.AccountStatus, CustomerStatusFilter) || MatchesFilter(x.AccountStatusReason, CustomerStatusFilter))));
    }

    private void ApplyVendorFilter()
    {
        ReplaceCollection(FilteredVendors, Vendors.Where(x =>
            MatchesFilter(x.VendorAccount, VendorAccountFilter) &&
            MatchesFilter(x.Name, VendorNameFilter) &&
            MatchesFilter(x.VendorGroup, VendorGroupFilter) &&
            (MatchesFilter(x.AccountStatus, VendorStatusFilter) || MatchesFilter(x.AccountStatusReason, VendorStatusFilter))));
    }

    private void ApplyItemMasterFilter()
    {
        ReplaceCollection(FilteredItemMasters, ItemMasters.Where(x =>
            MatchesFilter(x.ItemNumber, ItemNumberFilter) &&
            MatchesFilter(x.ProductName, ItemProductNameFilter) &&
            MatchesFilter(x.SearchName, ItemSearchNameFilter) &&
            (MatchesFilter(x.ProductType, ItemProductTypeFilter) || MatchesFilter(x.ProductSubtype, ItemProductTypeFilter))));
    }

    private void ApplyWarehouseFilter()
    {
        ReplaceCollection(FilteredWarehouseMasters, WarehouseMasters.Where(x =>
            MatchesFilter(x.Warehouse, WarehouseCodeFilter) &&
            MatchesFilter(x.Name, WarehouseNameFilter) &&
            MatchesFilter(x.Site, WarehouseSiteFilter) &&
            MatchesFilter(x.Type, WarehouseTypeFilter)));
    }

    private void ApplyVehicleFilter()
    {
        ReplaceCollection(FilteredVehicles, Vehicles.Where(x =>
            MatchesFilter(x.VehicleId.ToString(), VehicleIdFilter) &&
            MatchesFilter(x.PlateNumber, VehicleNoFilter) &&
            MatchesFilter(x.PlateEmirate, VehicleEmirateFilter) &&
            MatchesFilter(x.PlateCategory, VehicleCategoryFilter) &&
            MatchesFilter(x.VehicleType, VehicleTypeFilter) &&
            MatchesFilter(x.OwnerPartyAccount, VehicleOwnerFilter) &&
            MatchesFilter(x.DefaultDriver, VehicleContactFilter)));
    }

    private void ApplyDriverFilter()
    {
        ReplaceCollection(FilteredDrivers, Drivers.Where(x =>
            MatchesFilter(x.DriverId.ToString(), DriverIdFilter) &&
            MatchesFilter(x.DriverName, DriverNameFilter) &&
            MatchesFilter(x.MobileNumber, DriverMobileFilter) &&
            MatchesFilter(x.DriverType, DriverTypeFilter) &&
            MatchesFilter(x.EmployerAccount, DriverEmployerFilter) &&
            MatchesFilter(x.IdentificationNumber, DriverCnicFilter) &&
            MatchesFilter(x.DrivingLicenceNumber, DriverLicenseFilter) &&
            MatchesFilter(x.Status, DriverStatusFilter)));
    }


    private void ApplyWeighbridgeFilter()
    {
        ReplaceCollection(FilteredWeighbridgeMasters, WeighbridgeMasters.Where(x =>
            MatchesFilter(x.WeighbridgeCode, WeighbridgeCodeFilter) &&
            MatchesFilter(x.WeighbridgeName, WeighbridgeNameFilter) &&
            MatchesFilter(x.PlantSite, WeighbridgeSiteFilter) &&
            MatchesFilter(x.Warehouse, WeighbridgeWarehouseFilter) &&
            (MatchesFilter(x.OperatingStatus, WeighbridgeStatusFilter) || MatchesFilter(x.CommunicationType, WeighbridgeStatusFilter))));
    }

    private void ApplyOperatorFilter()
    {
        ReplaceCollection(FilteredOperatorMasters, OperatorMasters.Where(x =>
            MatchesFilter(x.EmployeeId, OperatorIdFilter) &&
            MatchesFilter(x.OperatorName, OperatorNameFilter) &&
            MatchesFilter(x.Username, OperatorUsernameFilter) &&
            MatchesFilter(x.Designation, OperatorDesignationFilter) &&
            MatchesFilter(x.Department, OperatorDepartmentFilter) &&
            (MatchesFilter(x.DefaultWeighbridge, OperatorWeighbridgeFilter) || MatchesFilter(x.AssignedWeighbridges, OperatorWeighbridgeFilter)) &&
            MatchesFilter(x.Status, OperatorStatusFilter)));
    }

    private static bool IsFilterEmpty(string value) => string.IsNullOrWhiteSpace(value);

    private static bool HasText(string value, string filter)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Contains(filter?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesFilter(string value, string filter)
    {
        return string.IsNullOrWhiteSpace(filter) || HasText(value, filter);
    }

    private void RecalculateNetWeight()
    {
        if (FirstWeight.HasValue && SecondWeight.HasValue)
            NetWeight = Math.Abs(SecondWeight.Value - FirstWeight.Value);
        else
            NetWeight = null;
    }

    private void SetLoadedOpenWeighmentId(int? weighmentId)
    {
        _loadedOpenWeighmentId = weighmentId;
        NotifyWeighmentButtonStates();
    }

    private void NotifyWeighmentButtonStates()
    {
        OnPropertyChanged(nameof(CanSaveFirstWeight));
        OnPropertyChanged(nameof(CanSaveSecondWeight));
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;

namespace WeightBridgeApp.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly OperatorMaster _currentUser;
    private IWeightReader? _weightReader;
    private int? _loadedOpenWeighmentId;
    private bool _isRefreshingData;
    private const int MasterPageSize = 200;
    private int _customerPageIndex;
    private int _vendorPageIndex;
    private int _itemMasterPageIndex;
    private int _warehousePageIndex;
    private int _unitOfMeasurePageIndex;
    private bool _isLoadingCustomerPage;
    private bool _isLoadingVendorPage;
    private bool _isLoadingItemMasterPage;
    private bool _isLoadingWarehousePage;
    private bool _isLoadingUnitOfMeasurePage;

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
    private string _slipNumber = string.Empty;
    private TransactionTypeMaster? _selectedTransactionTypeMaster;
    private ScenarioMaster? _selectedScenarioMaster;
    private string _gatePassNumber = string.Empty;
    private string _externalReference = string.Empty;
    private string _operatorRemarks = string.Empty;
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
    private DateTime _transactionFrom = DateTime.Today;
    private DateTime _transactionTo = DateTime.Today;
    private string _transactionTicketFilter = string.Empty;
    private string _transactionCompanyFilter = string.Empty;
    private string _transactionVehicleFilter = string.Empty;
    private string _transactionDriverFilter = string.Empty;
    private string _transactionPartyFilter = string.Empty;
    private string _transactionPartyTypeFilter = string.Empty;
    private string _transactionItemFilter = string.Empty;
    private string _transactionStatusFilter = string.Empty;
    private int _selectedMainTabIndex;
    private Weighment? _selectedTransactionWeighment;
    private string _selectedTransactionReviewForm = string.Empty;
    private string _newPartyName = string.Empty;
    private string _newPartyType = "Customer";
    private string _newMaterialName = string.Empty;
    private string _newVehicleNo = string.Empty;
    private string _selectedPartyType = "Customer";
    private Party? _selectedWeighmentParty;
    private ItemMaster? _selectedWeighmentItem;
    private WeighmentMaterialLine? _selectedMaterialLine;
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
    private string _unitOfMeasureSymbolFilter = string.Empty;
    private string _unitOfMeasureSystemFilter = string.Empty;
    private string _unitOfMeasureClassFilter = string.Empty;
    private string _unitOfMeasureStateFilter = string.Empty;
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
    private string _selectedLegalEntityDataAreaId = string.Empty;
    private LegalEntityMaster? _selectedLegalEntityMaster;
    private LegalEntityMaster _legalEntityMasterForm = new();
    private string _legalEntityFilter = string.Empty;
    private ShiftMaster? _selectedShiftMaster;
    private ShiftMaster _shiftMasterForm = new();
    private ScenarioMaster? _selectedScenarioConfig;
    private ScenarioMaster _scenarioMasterForm = new();
    private ReasonMaster? _selectedReasonMaster;
    private ReasonMaster _reasonMasterForm = new();
    private ContractMaster? _selectedContractMaster;
    private ContractMaster _contractMasterForm = new();
    private ToleranceMaster? _selectedToleranceMaster;
    private ToleranceMaster _toleranceMasterForm = new();
    private ServiceChargeMaster? _selectedServiceChargeMaster;
    private ServiceChargeMaster _serviceChargeMasterForm = new();
    private TransactionTypeMaster? _selectedTransactionTypeConfig;
    private TransactionTypeMaster _transactionTypeMasterForm = new();
    private LocationMaster? _selectedLocationMaster;
    private LocationMaster _locationMasterForm = new();
    private WeighmentPurchaseDetails _purchaseDetailsForm = new();
    private WeighmentContractCollectionDetails _contractCollectionDetailsForm = new();
    private WeighmentTransferDetails _transferDetailsForm = new();
    private WeighmentSalesDispatchDetails _salesDispatchDetailsForm = new();
    private WeighmentProductionDetails _productionDetailsForm = new();
    private WeighmentReturnDetails _returnDetailsForm = new();
    private WeighmentDisposalDetails _disposalDetailsForm = new();
    private GatePass? _selectedGatePass;
    private GatePass? _selectedWeighmentGatePass;
    private GatePass _gatePassForm = new();
    private CancellationVoidRequest? _selectedCancellationVoidRequest;
    private CancellationVoidRequest _cancellationVoidForm = new();
    private WeighmentCorrection? _selectedCorrectionRequest;
    private string _selectedCancellationVoidType = "Cancel";
    private string _selectedCancellationReason = string.Empty;
    private LegalEntityMaster? _selectedOperatorLegalEntityToAdd;
    private OperatorLegalEntityAssignment? _selectedOperatorLegalEntityAssignment;
    private Customer? _selectedCustomer;
    private Customer _customerForm = new();
    private Vendor? _selectedVendor;
    private Vendor _vendorForm = new();
    private ItemMaster? _selectedItemMaster;
    private ItemMaster _itemMasterForm = new();
    private WarehouseMaster? _selectedWarehouseMaster;
    private WarehouseMaster _warehouseMasterForm = new();
    private UnitOfMeasureMaster? _selectedUnitOfMeasureMaster;
    private UnitOfMeasureMaster _unitOfMeasureMasterForm = new();
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

    public MainViewModel(DatabaseService databaseService, OperatorMaster currentUser)
    {
        _databaseService = databaseService;
        _currentUser = currentUser;
        CorrectionWorkspace = new CorrectionWorkspaceViewModel(
            _databaseService,
            _currentUser,
            () => CurrentUserCompany,
            async () =>
            {
                await RefreshWeighmentsAsync();
                if (CanAccessTransactions) await LoadTransactionsAsync();
                if (CanAccessReports) await LoadReportAsync();
            });

        ConnectionTypes = new ObservableCollection<string> { "Mock", "TCP/IP", "Serial", "USB", "OPC", "API" };
        ParityOptions = new ObservableCollection<string> { "None", "Odd", "Even", "Mark", "Space" };
        StopBitsOptions = new ObservableCollection<string> { "One", "Two", "OnePointFive" };
        PartyTypes = new ObservableCollection<string> { "Customer", "Vendor" };
        GatePassTypes = new ObservableCollection<string> { "Inbound", "Outbound" };
        GatePassStatuses = new ObservableCollection<string> { "Open", "Linked", "Closed", "Cancelled" };
        TransactionFormValues = new ObservableCollection<string>
        {
            "Purchase / Receipt / Collection",
            "Contract Collection",
            "Transfer Form",
            "Sales / Dispatch",
            "Production Weighing",
            "Return",
            "Disposal / Waste Movement"
        };
        LocationTypeValues = new ObservableCollection<string>
        {
            "Site",
            "Yard",
            "Warehouse",
            "Warehouse Location",
            "Factory",
            "Plant",
            "Project Site",
            "Disposal Site",
            "Collection Source",
            "Other"
        };
        BillingBasisValues = new ObservableCollection<string>
        {
            "Net",
            "Accepted Quantity",
            "Trip",
            "Fixed Fee"
        };
        TransferDirectionValues = new ObservableCollection<string> { "Transfer In", "Transfer Out", "Return" };
        SalesSubtypeValues = new ObservableCollection<string> { "Credit", "Cash", "Dispatch-only" };
        PaymentStatusValues = new ObservableCollection<string> { "Unpaid", "Paid", "Credit" };
        ProductionMovementValues = new ObservableCollection<string> { "Receipt", "Issue", "Return", "Dispatch" };
        ReturnTypeValues = new ObservableCollection<string> { "Purchase Return", "Sales Return", "Intercompany Return" };
        DisposalTypeValues = new ObservableCollection<string> { "Landfill", "Rejected Material Disposal", "Internal Waste Movement" };
        CancellationVoidTypes = new ObservableCollection<string> { "Cancel", "Void", "Reverse" };

        ConnectCommand = new RelayCommand(ConnectAsync, () => !IsConnected);
        DisconnectCommand = new RelayCommand(DisconnectAsync, () => IsConnected);
        SaveSettingsCommand = new RelayCommand(SaveSettingsAsync);
        SaveFirstWeightCommand = new RelayCommand(SaveFirstWeightAsync);
        SaveSecondWeightCommand = new RelayCommand(SaveSecondWeightAsync);
        LoadOpenTicketCommand = new RelayCommand(LoadSelectedOpenTicket); // kept for compatibility; open ticket loads automatically on row selection
        RefreshCommand = new RelayCommand(RefreshAllAsync);
        ClearCommand = new RelayCommand(ClearEntry);
        OpenVehicleLookupCommand = new RelayCommand(OpenVehicleLookupAsync);
        OpenDriverLookupCommand = new RelayCommand(OpenDriverLookupAsync);
        OpenPartyLookupCommand = new RelayCommand(OpenPartyLookupAsync);
        OpenItemLookupCommand = new RelayCommand(OpenItemLookupAsync, () => IsHeaderAndLinesEditable);
        AddMaterialLineCommand = new RelayCommand(AddMaterialLineAsync, () => IsHeaderAndLinesEditable);
        DeleteMaterialLineCommand = new RelayCommand(DeleteMaterialLine, () => IsHeaderAndLinesEditable && SelectedMaterialLine != null);
        OpenTransactionTypeLookupCommand = new RelayCommand(OpenTransactionTypeLookupAsync);
        OpenScenarioLookupCommand = new RelayCommand(OpenScenarioLookupAsync);
        OpenWeighmentGatePassLookupCommand = new RelayCommand(OpenWeighmentGatePassLookupAsync);
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
        LoadTransactionsCommand = new RelayCommand(LoadTransactionsAsync);
        ClearTransactionFiltersCommand = new RelayCommand(ClearTransactionFilters);
        CorrectTransactionCommand = new RelayCommand(CorrectTransactionAsync);
        CancelTransactionCommand = new RelayCommand(CancelTransactionAsync);
        StartCancellationFromTransactionCommand = new RelayCommand(StartCancellationFromSelectedTransactionAsync, () => CanInitiateCancellationFromTransaction);
        NewCorrectionCommand = new RelayCommand(NewCorrectionAsync);
        OpenSelectedCorrectionCommand = new RelayCommand(OpenSelectedCorrectionAsync, () => CanOpenSelectedCorrection);
        SaveUserCommand = new RelayCommand(SaveUserAsync);
        ClearUserFormCommand = new RelayCommand(ClearUserForm);
        SaveCustomerCommand = new RelayCommand(SaveCustomerAsync);
        ClearCustomerFormCommand = new RelayCommand(ClearCustomerForm);
        SaveVendorCommand = new RelayCommand(SaveVendorAsync);
        ClearVendorFormCommand = new RelayCommand(ClearVendorForm);
        SaveItemMasterCommand = new RelayCommand(SaveItemMasterAsync);
        ClearItemMasterFormCommand = new RelayCommand(ClearItemMasterForm);
        OpenItemUnitConversionsCommand = new RelayCommand(OpenItemUnitConversions, () => SelectedItemMaster != null && !string.IsNullOrWhiteSpace(SelectedItemMaster.ProductNumber));
        SaveWarehouseMasterCommand = new RelayCommand(SaveWarehouseMasterAsync);
        ClearWarehouseMasterFormCommand = new RelayCommand(ClearWarehouseMasterForm);
        CustomerPreviousPageCommand = new RelayCommand(async () => { if (_customerPageIndex > 0) _customerPageIndex--; await LoadCustomerPageAsync(); }, () => _customerPageIndex > 0 && !_isLoadingCustomerPage);
        CustomerNextPageCommand = new RelayCommand(async () => { _customerPageIndex++; await LoadCustomerPageAsync(); }, () => !_isLoadingCustomerPage);
        VendorPreviousPageCommand = new RelayCommand(async () => { if (_vendorPageIndex > 0) _vendorPageIndex--; await LoadVendorPageAsync(); }, () => _vendorPageIndex > 0 && !_isLoadingVendorPage);
        VendorNextPageCommand = new RelayCommand(async () => { _vendorPageIndex++; await LoadVendorPageAsync(); }, () => !_isLoadingVendorPage);
        ItemMasterPreviousPageCommand = new RelayCommand(async () => { if (_itemMasterPageIndex > 0) _itemMasterPageIndex--; await LoadItemMasterPageAsync(); }, () => _itemMasterPageIndex > 0 && !_isLoadingItemMasterPage);
        ItemMasterNextPageCommand = new RelayCommand(async () => { _itemMasterPageIndex++; await LoadItemMasterPageAsync(); }, () => !_isLoadingItemMasterPage);
        WarehousePreviousPageCommand = new RelayCommand(async () => { if (_warehousePageIndex > 0) _warehousePageIndex--; await LoadWarehousePageAsync(); }, () => _warehousePageIndex > 0 && !_isLoadingWarehousePage);
        WarehouseNextPageCommand = new RelayCommand(async () => { _warehousePageIndex++; await LoadWarehousePageAsync(); }, () => !_isLoadingWarehousePage);
        UnitOfMeasurePreviousPageCommand = new RelayCommand(async () => { if (_unitOfMeasurePageIndex > 0) _unitOfMeasurePageIndex--; await LoadUnitOfMeasurePageAsync(); }, () => _unitOfMeasurePageIndex > 0 && !_isLoadingUnitOfMeasurePage);
        UnitOfMeasureNextPageCommand = new RelayCommand(async () => { _unitOfMeasurePageIndex++; await LoadUnitOfMeasurePageAsync(); }, () => !_isLoadingUnitOfMeasurePage);
        SaveVehicleMasterCommand = new RelayCommand(SaveVehicleMasterAsync);
        ClearVehicleMasterFormCommand = new RelayCommand(ClearVehicleMasterForm);
        SaveDriverMasterCommand = new RelayCommand(SaveDriverMasterAsync);
        ClearDriverMasterFormCommand = new RelayCommand(ClearDriverMasterForm);
        SaveWeighbridgeMasterCommand = new RelayCommand(SaveWeighbridgeMasterAsync);
        ClearWeighbridgeMasterFormCommand = new RelayCommand(ClearWeighbridgeMasterForm);
        SaveOperatorMasterCommand = new RelayCommand(SaveOperatorMasterAsync);
        ClearOperatorMasterFormCommand = new RelayCommand(ClearOperatorMasterForm);
        RefreshUsersCommand = new RelayCommand(LoadUsersAsync);
        SaveLegalEntityCommand = new RelayCommand(SaveLegalEntityAsync);
        ClearLegalEntityFormCommand = new RelayCommand(ClearLegalEntityForm);
        AddOperatorLegalEntityCommand = new RelayCommand(AddOperatorLegalEntityToForm);
        RemoveOperatorLegalEntityCommand = new RelayCommand(RemoveOperatorLegalEntityFromForm);
        SetDefaultOperatorLegalEntityCommand = new RelayCommand(SetDefaultOperatorLegalEntity);
        SaveShiftMasterCommand = new RelayCommand(SaveShiftMasterAsync);
        ClearShiftMasterFormCommand = new RelayCommand(ClearShiftMasterForm);
        SaveScenarioMasterCommand = new RelayCommand(SaveScenarioMasterAsync);
        ClearScenarioMasterFormCommand = new RelayCommand(ClearScenarioMasterForm);
        SaveReasonMasterCommand = new RelayCommand(SaveReasonMasterAsync);
        ClearReasonMasterFormCommand = new RelayCommand(ClearReasonMasterForm);
        SaveContractMasterCommand = new RelayCommand(SaveContractMasterAsync);
        ClearContractMasterFormCommand = new RelayCommand(ClearContractMasterForm);
        SaveToleranceMasterCommand = new RelayCommand(SaveToleranceMasterAsync);
        ClearToleranceMasterFormCommand = new RelayCommand(ClearToleranceMasterForm);
        SaveServiceChargeMasterCommand = new RelayCommand(SaveServiceChargeMasterAsync);
        ClearServiceChargeMasterFormCommand = new RelayCommand(ClearServiceChargeMasterForm);
        SaveTransactionTypeMasterCommand = new RelayCommand(SaveTransactionTypeMasterAsync);
        ClearTransactionTypeMasterFormCommand = new RelayCommand(ClearTransactionTypeMasterForm);
        SaveLocationMasterCommand = new RelayCommand(SaveLocationMasterAsync);
        ClearLocationMasterFormCommand = new RelayCommand(ClearLocationMasterForm);
        OpenPurchaseVendorLookupCommand = new RelayCommand(OpenPurchaseVendorLookupAsync, () => IsPurchaseVendorSelectable);
        OpenPurchaseSourceLookupCommand = new RelayCommand(OpenPurchaseSourceLookupAsync, () => IsPurchaseDetailsEditable);
        OpenPurchaseDestinationLookupCommand = new RelayCommand(OpenPurchaseDestinationLookupAsync, () => IsPurchaseDetailsEditable);
        OpenContractCollectionVendorLookupCommand = new RelayCommand(OpenContractCollectionVendorLookupAsync, () => IsContractCollectionDetailsEditable);
        OpenContractCollectionInvoiceAccountLookupCommand = new RelayCommand(OpenContractCollectionInvoiceAccountLookupAsync, () => IsContractCollectionDetailsEditable);
        OpenContractCollectionContractLookupCommand = new RelayCommand(OpenContractCollectionContractLookupAsync, () => IsContractCollectionDetailsEditable);
        OpenContractCollectionLocationLookupCommand = new RelayCommand(OpenContractCollectionLocationLookupAsync, () => IsContractCollectionDetailsEditable);
        OpenContractCollectionDestinationLookupCommand = new RelayCommand(OpenContractCollectionDestinationLookupAsync, () => IsContractCollectionDetailsEditable);
        OpenTransferFromLocationLookupCommand = new RelayCommand(OpenTransferFromLocationLookupAsync, () => IsTransferDetailsEditable);
        OpenTransferToLocationLookupCommand = new RelayCommand(OpenTransferToLocationLookupAsync, () => IsTransferDetailsEditable);
        OpenSalesCustomerLookupCommand = new RelayCommand(OpenSalesCustomerLookupAsync, () => IsSalesDispatchDetailsEditable);
        OpenSalesSourceLookupCommand = new RelayCommand(OpenSalesSourceLookupAsync, () => IsSalesDispatchDetailsEditable);
        OpenProductionWarehouseLocationLookupCommand = new RelayCommand(OpenProductionWarehouseLocationLookupAsync, () => IsProductionDetailsEditable);
        OpenReturnVendorLookupCommand = new RelayCommand(OpenReturnVendorLookupAsync, () => IsReturnDetailsEditable);
        OpenReturnCustomerLookupCommand = new RelayCommand(OpenReturnCustomerLookupAsync, () => IsReturnDetailsEditable);
        OpenReturnSourceLookupCommand = new RelayCommand(OpenReturnSourceLookupAsync, () => IsReturnDetailsEditable);
        OpenReturnDestinationLookupCommand = new RelayCommand(OpenReturnDestinationLookupAsync, () => IsReturnDetailsEditable);
        OpenDisposalSourceLookupCommand = new RelayCommand(OpenDisposalSourceLookupAsync, () => IsDisposalDetailsEditable);
        OpenDisposalDestinationLookupCommand = new RelayCommand(OpenDisposalDestinationLookupAsync, () => IsDisposalDetailsEditable);
        SaveGatePassCommand = new RelayCommand(SaveGatePassAsync);
        ClearGatePassCommand = new RelayCommand(ClearGatePassForm);
        CloseGatePassCommand = new RelayCommand(CloseGatePassAsync);
        CancelGatePassCommand = new RelayCommand(CancelGatePassAsync);
        PrintGatePassCommand = new RelayCommand(PrintGatePassAsync);
        OpenGatePassVehicleLookupCommand = new RelayCommand(OpenGatePassVehicleLookupAsync);
        OpenGatePassDriverLookupCommand = new RelayCommand(OpenGatePassDriverLookupAsync);
        OpenGatePassPartyLookupCommand = new RelayCommand(OpenGatePassPartyLookupAsync);
        OpenGatePassItemLookupCommand = new RelayCommand(OpenGatePassItemLookupAsync);
        OpenCancellationSlipLookupCommand = new RelayCommand(OpenCancellationSlipLookupAsync, () => IsCancellationVoidFormEditable);
        SubmitCancellationVoidCommand = new RelayCommand(SubmitCancellationVoidAsync, () => CanSubmitCancellationVoid);
        ApproveCancellationVoidCommand = new RelayCommand(ApproveCancellationVoidAsync, () => CanApproveCancellationVoid);
        RejectCancellationVoidCommand = new RelayCommand(RejectCancellationVoidAsync, () => CanRejectCancellationVoid);
        NewCancellationVoidCommand = new RelayCommand(NewCancellationVoidAsync, () => CanCreateCancellationVoidRequest);

        PurchaseDetailsForm.PropertyChanged += PurchaseDetailsForm_PropertyChanged;
        ContractCollectionDetailsForm.PropertyChanged += ContractCollectionDetailsForm_PropertyChanged;
    }

    public ObservableCollection<string> ConnectionTypes { get; }
    public ObservableCollection<string> ParityOptions { get; }
    public ObservableCollection<string> StopBitsOptions { get; }
    public ObservableCollection<string> PartyTypes { get; }
    public ObservableCollection<Party> Parties { get; } = new();
    public ObservableCollection<Party> FilteredParties { get; } = new();
    public ObservableCollection<Material> Materials { get; } = new();
    public ObservableCollection<Vehicle> Vehicles { get; } = new();
    public ObservableCollection<Vehicle> ActiveVehicles { get; } = new();
    public ObservableCollection<Driver> Drivers { get; } = new();
    public ObservableCollection<Driver> ActiveDrivers { get; } = new();
    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<Vendor> Vendors { get; } = new();
    public ObservableCollection<ItemMaster> ItemMasters { get; } = new();
    public ObservableCollection<WarehouseMaster> WarehouseMasters { get; } = new();
    public ObservableCollection<UnitOfMeasureMaster> UnitOfMeasureMasters { get; } = new();
    // Complete UOM list used by Material Lines. This is separate from the paged UOM master grid.
    public ObservableCollection<string> MaterialLineUomSymbols { get; } = new();
    public ObservableCollection<Customer> FilteredCustomers { get; } = new();
    public ObservableCollection<Vendor> FilteredVendors { get; } = new();
    public ObservableCollection<ItemMaster> FilteredItemMasters { get; } = new();
    public ObservableCollection<WarehouseMaster> FilteredWarehouseMasters { get; } = new();
    public ObservableCollection<UnitOfMeasureMaster> FilteredUnitOfMeasureMasters { get; } = new();
    public ObservableCollection<Vehicle> FilteredVehicles { get; } = new();
    public ObservableCollection<Driver> FilteredDrivers { get; } = new();
    public ObservableCollection<WeighbridgeMaster> WeighbridgeMasters { get; } = new();
    public ObservableCollection<WeighbridgeMaster> ActiveWeighbridgeMasters { get; } = new();
    public ObservableCollection<WeighbridgeMaster> FilteredWeighbridgeMasters { get; } = new();
    public ObservableCollection<OperatorMaster> OperatorMasters { get; } = new();
    public ObservableCollection<OperatorMaster> FilteredOperatorMasters { get; } = new();
    public ObservableCollection<LegalEntityMaster> LegalEntities { get; } = new();
    public ObservableCollection<LegalEntityMaster> FilteredLegalEntities { get; } = new();
    public ObservableCollection<LegalEntityMaster> AllowedLegalEntities { get; } = new();
    public ObservableCollection<OperatorLegalEntityAssignment> OperatorLegalEntityAssignments { get; } = new();
    public ObservableCollection<Weighment> OpenWeighments { get; } = new();
    public ObservableCollection<WeighmentMaterialLine> MaterialLines { get; } = new();
    public ObservableCollection<Weighment> CompletedToday { get; } = new();
    public ObservableCollection<Weighment> ReportRows { get; } = new();
    public ObservableCollection<Weighment> FilteredReportRows { get; } = new();
    public ObservableCollection<Weighment> TransactionRows { get; } = new();
    public ObservableCollection<Weighment> FilteredTransactionRows { get; } = new();
    public ObservableCollection<TransactionReviewField> TransactionReviewCommonFields { get; } = new();
    public ObservableCollection<TransactionReviewField> TransactionReviewDynamicFields { get; } = new();
    public ObservableCollection<WeighmentMaterialLine> TransactionReviewMaterialLines { get; } = new();
    public ObservableCollection<AppUser> Users { get; } = new();
    public ObservableCollection<ShiftMaster> ShiftMasters { get; } = new();
    public ObservableCollection<ScenarioMaster> ScenarioMasters { get; } = new();
    public ObservableCollection<ReasonMaster> ReasonMasters { get; } = new();
    public ObservableCollection<ContractMaster> ContractMasters { get; } = new();
    public ObservableCollection<ToleranceMaster> ToleranceMasters { get; } = new();
    public ObservableCollection<ServiceChargeMaster> ServiceChargeMasters { get; } = new();
    public ObservableCollection<TransactionTypeMaster> TransactionTypeMasters { get; } = new();
    public ObservableCollection<LocationMaster> LocationMasters { get; } = new();
    public ObservableCollection<GatePass> GatePasses { get; } = new();
    public ObservableCollection<GatePass> OpenGatePasses { get; } = new();
    public ObservableCollection<CancellationVoidRequest> CancellationVoidRequests { get; } = new();
    public ObservableCollection<WeighmentCorrection> CorrectionRequests { get; } = new();
    public CorrectionWorkspaceViewModel CorrectionWorkspace { get; }
    public ObservableCollection<string> CancellationVoidTypes { get; }
    public ObservableCollection<string> CancellationReasons { get; } = new();
    public ObservableCollection<string> GatePassTypes { get; }
    public ObservableCollection<string> GatePassStatuses { get; }
    public ObservableCollection<string> TransactionFormValues { get; }
    public ObservableCollection<string> LocationTypeValues { get; }
    public ObservableCollection<string> BillingBasisValues { get; }
    public ObservableCollection<string> TransferDirectionValues { get; }
    public ObservableCollection<string> SalesSubtypeValues { get; }
    public ObservableCollection<string> PaymentStatusValues { get; }
    public ObservableCollection<string> ProductionMovementValues { get; }
    public ObservableCollection<string> ReturnTypeValues { get; }
    public ObservableCollection<string> DisposalTypeValues { get; }

    public RelayCommand ConnectCommand { get; }
    public RelayCommand DisconnectCommand { get; }
    public RelayCommand SaveSettingsCommand { get; }
    public RelayCommand SaveFirstWeightCommand { get; }
    public RelayCommand SaveSecondWeightCommand { get; }
    public RelayCommand LoadOpenTicketCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ClearCommand { get; }
    public RelayCommand OpenVehicleLookupCommand { get; }
    public RelayCommand OpenDriverLookupCommand { get; }
    public RelayCommand OpenPartyLookupCommand { get; }
    public RelayCommand OpenItemLookupCommand { get; }
    public RelayCommand AddMaterialLineCommand { get; }
    public RelayCommand DeleteMaterialLineCommand { get; }
    public RelayCommand OpenTransactionTypeLookupCommand { get; }
    public RelayCommand OpenScenarioLookupCommand { get; }
    public RelayCommand OpenWeighmentGatePassLookupCommand { get; }
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
    public RelayCommand LoadTransactionsCommand { get; }
    public RelayCommand ClearTransactionFiltersCommand { get; }
    public RelayCommand CorrectTransactionCommand { get; }
    public RelayCommand CancelTransactionCommand { get; } // legacy; direct cancellation is no longer exposed
    public RelayCommand StartCancellationFromTransactionCommand { get; }
    public RelayCommand NewCorrectionCommand { get; }
    public RelayCommand OpenSelectedCorrectionCommand { get; }
    public RelayCommand SaveUserCommand { get; }
    public RelayCommand ClearUserFormCommand { get; }
    public RelayCommand SaveCustomerCommand { get; }
    public RelayCommand ClearCustomerFormCommand { get; }
    public RelayCommand SaveVendorCommand { get; }
    public RelayCommand ClearVendorFormCommand { get; }
    public RelayCommand SaveItemMasterCommand { get; }
    public RelayCommand ClearItemMasterFormCommand { get; }
    public RelayCommand OpenItemUnitConversionsCommand { get; }
    public RelayCommand SaveWarehouseMasterCommand { get; }
    public RelayCommand ClearWarehouseMasterFormCommand { get; }
    public RelayCommand CustomerPreviousPageCommand { get; }
    public RelayCommand CustomerNextPageCommand { get; }
    public RelayCommand VendorPreviousPageCommand { get; }
    public RelayCommand VendorNextPageCommand { get; }
    public RelayCommand ItemMasterPreviousPageCommand { get; }
    public RelayCommand ItemMasterNextPageCommand { get; }
    public RelayCommand WarehousePreviousPageCommand { get; }
    public RelayCommand WarehouseNextPageCommand { get; }
    public RelayCommand UnitOfMeasurePreviousPageCommand { get; }
    public RelayCommand UnitOfMeasureNextPageCommand { get; }
    public RelayCommand SaveVehicleMasterCommand { get; }
    public RelayCommand ClearVehicleMasterFormCommand { get; }
    public RelayCommand SaveDriverMasterCommand { get; }
    public RelayCommand ClearDriverMasterFormCommand { get; }
    public RelayCommand SaveWeighbridgeMasterCommand { get; }
    public RelayCommand ClearWeighbridgeMasterFormCommand { get; }
    public RelayCommand SaveOperatorMasterCommand { get; }
    public RelayCommand ClearOperatorMasterFormCommand { get; }
    public RelayCommand RefreshUsersCommand { get; }
    public RelayCommand SaveLegalEntityCommand { get; }
    public RelayCommand ClearLegalEntityFormCommand { get; }
    public RelayCommand AddOperatorLegalEntityCommand { get; }
    public RelayCommand RemoveOperatorLegalEntityCommand { get; }
    public RelayCommand SetDefaultOperatorLegalEntityCommand { get; }
    public RelayCommand SaveShiftMasterCommand { get; }
    public RelayCommand ClearShiftMasterFormCommand { get; }
    public RelayCommand SaveScenarioMasterCommand { get; }
    public RelayCommand ClearScenarioMasterFormCommand { get; }
    public RelayCommand SaveReasonMasterCommand { get; }
    public RelayCommand ClearReasonMasterFormCommand { get; }
    public RelayCommand SaveContractMasterCommand { get; }
    public RelayCommand ClearContractMasterFormCommand { get; }
    public RelayCommand SaveToleranceMasterCommand { get; }
    public RelayCommand ClearToleranceMasterFormCommand { get; }
    public RelayCommand SaveServiceChargeMasterCommand { get; }
    public RelayCommand ClearServiceChargeMasterFormCommand { get; }
    public RelayCommand SaveTransactionTypeMasterCommand { get; }
    public RelayCommand ClearTransactionTypeMasterFormCommand { get; }
    public RelayCommand SaveLocationMasterCommand { get; }
    public RelayCommand ClearLocationMasterFormCommand { get; }
    public RelayCommand OpenPurchaseVendorLookupCommand { get; }
    public RelayCommand OpenPurchaseSourceLookupCommand { get; }
    public RelayCommand OpenPurchaseDestinationLookupCommand { get; }
    public RelayCommand OpenContractCollectionVendorLookupCommand { get; }
    public RelayCommand OpenContractCollectionInvoiceAccountLookupCommand { get; }
    public RelayCommand OpenContractCollectionContractLookupCommand { get; }
    public RelayCommand OpenContractCollectionLocationLookupCommand { get; }
    public RelayCommand OpenContractCollectionDestinationLookupCommand { get; }
    public RelayCommand OpenTransferFromLocationLookupCommand { get; }
    public RelayCommand OpenTransferToLocationLookupCommand { get; }
    public RelayCommand OpenSalesCustomerLookupCommand { get; }
    public RelayCommand OpenSalesSourceLookupCommand { get; }
    public RelayCommand OpenProductionWarehouseLocationLookupCommand { get; }
    public RelayCommand OpenReturnVendorLookupCommand { get; }
    public RelayCommand OpenReturnCustomerLookupCommand { get; }
    public RelayCommand OpenReturnSourceLookupCommand { get; }
    public RelayCommand OpenReturnDestinationLookupCommand { get; }
    public RelayCommand OpenDisposalSourceLookupCommand { get; }
    public RelayCommand OpenDisposalDestinationLookupCommand { get; }
    public RelayCommand SaveGatePassCommand { get; }
    public RelayCommand ClearGatePassCommand { get; }
    public RelayCommand CloseGatePassCommand { get; }
    public RelayCommand CancelGatePassCommand { get; }
    public RelayCommand PrintGatePassCommand { get; }
    public RelayCommand OpenGatePassVehicleLookupCommand { get; }
    public RelayCommand OpenGatePassDriverLookupCommand { get; }
    public RelayCommand OpenGatePassPartyLookupCommand { get; }
    public RelayCommand OpenGatePassItemLookupCommand { get; }
    public RelayCommand OpenCancellationSlipLookupCommand { get; }
    public RelayCommand SubmitCancellationVoidCommand { get; }
    public RelayCommand ApproveCancellationVoidCommand { get; }
    public RelayCommand RejectCancellationVoidCommand { get; }
    public RelayCommand NewCancellationVoidCommand { get; }


    public string SlipNumber
    {
        get => _slipNumber;
        set => SetProperty(ref _slipNumber, value);
    }

    public TransactionTypeMaster? SelectedTransactionTypeMaster
    {
        get => _selectedTransactionTypeMaster;
        set
        {
            if (SetProperty(ref _selectedTransactionTypeMaster, value))
            {
                OnPropertyChanged(nameof(TransactionTypeDisplay));
                OnPropertyChanged(nameof(SelectedTransactionForm));
                OnPropertyChanged(nameof(IsPurchaseReceiptCollectionForm));
                OnPropertyChanged(nameof(IsContractCollectionForm));
                OnPropertyChanged(nameof(IsTransferForm));
                OnPropertyChanged(nameof(IsSalesDispatchForm));
                OnPropertyChanged(nameof(IsProductionWeighingForm));
                OnPropertyChanged(nameof(IsReturnForm));
                OnPropertyChanged(nameof(IsDisposalWasteMovementForm));
                OnPropertyChanged(nameof(IsPurchaseDetailsEditable));
                OnPropertyChanged(nameof(IsPurchaseDetailsReadOnly));
                OnPropertyChanged(nameof(IsPurchaseVendorSelectable));
                OnPropertyChanged(nameof(IsPurchaseRateAmountEditable));
                OnPropertyChanged(nameof(IsContractCollectionDetailsEditable));
                OnPropertyChanged(nameof(IsContractCollectionDetailsReadOnly));
                OnPropertyChanged(nameof(IsTransferDetailsEditable));
                OnPropertyChanged(nameof(IsSalesDispatchDetailsEditable));
                OnPropertyChanged(nameof(IsProductionDetailsEditable));
                OnPropertyChanged(nameof(IsReturnDetailsEditable));
                OnPropertyChanged(nameof(IsDisposalDetailsEditable));
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string TransactionTypeDisplay => SelectedTransactionTypeMaster?.Type ?? string.Empty;
    public string SelectedTransactionForm => SelectedTransactionTypeMaster?.Form ?? string.Empty;
    public bool IsPurchaseReceiptCollectionForm => string.Equals(SelectedTransactionForm, "Purchase / Receipt / Collection", StringComparison.OrdinalIgnoreCase);
    public bool IsContractCollectionForm => string.Equals(SelectedTransactionForm, "Contract Collection", StringComparison.OrdinalIgnoreCase);
    public bool IsTransferForm => string.Equals(SelectedTransactionForm, "Transfer Form", StringComparison.OrdinalIgnoreCase);
    public bool IsSalesDispatchForm => string.Equals(SelectedTransactionForm, "Sales / Dispatch", StringComparison.OrdinalIgnoreCase);
    public bool IsProductionWeighingForm => string.Equals(SelectedTransactionForm, "Production Weighing", StringComparison.OrdinalIgnoreCase);
    public bool IsReturnForm => string.Equals(SelectedTransactionForm, "Return", StringComparison.OrdinalIgnoreCase);
    public bool IsDisposalWasteMovementForm => string.Equals(SelectedTransactionForm, "Disposal / Waste Movement", StringComparison.OrdinalIgnoreCase);
    public bool IsPurchaseDetailsEditable => IsHeaderAndLinesEditable && IsPurchaseReceiptCollectionForm;
    public bool IsPurchaseDetailsReadOnly => !IsPurchaseDetailsEditable;
    public bool IsContractCollectionDetailsEditable => IsHeaderAndLinesEditable && IsContractCollectionForm;
    public bool IsContractCollectionDetailsReadOnly => !IsContractCollectionDetailsEditable;
    public bool IsTransferDetailsEditable => IsHeaderAndLinesEditable && IsTransferForm;
    public bool IsSalesDispatchDetailsEditable => IsHeaderAndLinesEditable && IsSalesDispatchForm;
    public bool IsProductionDetailsEditable => IsHeaderAndLinesEditable && IsProductionWeighingForm;
    public bool IsReturnDetailsEditable => IsHeaderAndLinesEditable && IsReturnForm;
    public bool IsDisposalDetailsEditable => IsHeaderAndLinesEditable && IsDisposalWasteMovementForm;

    public ScenarioMaster? SelectedScenarioMaster
    {
        get => _selectedScenarioMaster;
        set
        {
            if (SetProperty(ref _selectedScenarioMaster, value))
                OnPropertyChanged(nameof(ScenarioDisplay));
        }
    }

    public string ScenarioDisplay => SelectedScenarioMaster?.Form ?? string.Empty;

    public string GatePassNumber
    {
        get => _gatePassNumber;
        set => SetProperty(ref _gatePassNumber, value);
    }

    public string ExternalReference
    {
        get => _externalReference;
        set => SetProperty(ref _externalReference, value);
    }

    public string OperatorRemarks
    {
        get => _operatorRemarks;
        set => SetProperty(ref _operatorRemarks, value);
    }

    public ShiftMaster? SelectedShiftMaster { get => _selectedShiftMaster; set { if (SetProperty(ref _selectedShiftMaster, value) && value != null) ShiftMasterForm = new ShiftMaster { ShiftMasterId = value.ShiftMasterId, Code = value.Code, StartTime = value.StartTime, EndTime = value.EndTime, CrossingMidnightRule = value.CrossingMidnightRule }; } }
    public ShiftMaster ShiftMasterForm { get => _shiftMasterForm; set => SetProperty(ref _shiftMasterForm, value); }
    public ScenarioMaster? SelectedScenarioConfig { get => _selectedScenarioConfig; set { if (SetProperty(ref _selectedScenarioConfig, value) && value != null) ScenarioMasterForm = new ScenarioMaster { ScenarioMasterId = value.ScenarioMasterId, Form = value.Form, DataAreaId = value.DataAreaId, Movement = value.Movement, Formula = value.Formula, PartyRule = value.PartyRule, QC = value.QC, MultiItem = value.MultiItem, Print = value.Print }; } }
    public ScenarioMaster ScenarioMasterForm { get => _scenarioMasterForm; set => SetProperty(ref _scenarioMasterForm, value); }
    public ReasonMaster? SelectedReasonMaster { get => _selectedReasonMaster; set { if (SetProperty(ref _selectedReasonMaster, value) && value != null) ReasonMasterForm = new ReasonMaster { ReasonMasterId = value.ReasonMasterId, Code = value.Code, Description = value.Description }; } }
    public ReasonMaster ReasonMasterForm { get => _reasonMasterForm; set => SetProperty(ref _reasonMasterForm, value); }
    public ContractMaster? SelectedContractMaster { get => _selectedContractMaster; set { if (SetProperty(ref _selectedContractMaster, value) && value != null) ContractMasterForm = new ContractMaster { ContractMasterId = value.ContractMasterId, ContractNumber = value.ContractNumber, Parties = value.Parties, Locations = value.Locations, BillingBasis = value.BillingBasis, Validity = value.Validity }; } }
    public ContractMaster ContractMasterForm { get => _contractMasterForm; set => SetProperty(ref _contractMasterForm, value); }
    public ToleranceMaster? SelectedToleranceMaster { get => _selectedToleranceMaster; set { if (SetProperty(ref _selectedToleranceMaster, value) && value != null) ToleranceMasterForm = new ToleranceMaster { ToleranceMasterId = value.ToleranceMasterId, AbsoluteTolerance = value.AbsoluteTolerance, PercentageTolerance = value.PercentageTolerance, AllocationTolerance = value.AllocationTolerance, ApprovalThreshold = value.ApprovalThreshold }; } }
    public ToleranceMaster ToleranceMasterForm { get => _toleranceMasterForm; set => SetProperty(ref _toleranceMasterForm, value); }
    public ServiceChargeMaster? SelectedServiceChargeMaster { get => _selectedServiceChargeMaster; set { if (SetProperty(ref _selectedServiceChargeMaster, value) && value != null) ServiceChargeMasterForm = new ServiceChargeMaster { ServiceChargeMasterId = value.ServiceChargeMasterId, DataAreaId = value.DataAreaId, ServiceMode = value.ServiceMode, Amount = value.Amount, Currency = value.Currency, Validity = value.Validity }; } }
    public ServiceChargeMaster ServiceChargeMasterForm { get => _serviceChargeMasterForm; set => SetProperty(ref _serviceChargeMasterForm, value); }
    public TransactionTypeMaster? SelectedTransactionTypeConfig { get => _selectedTransactionTypeConfig; set { if (SetProperty(ref _selectedTransactionTypeConfig, value) && value != null) TransactionTypeMasterForm = new TransactionTypeMaster { TransactionTypeMasterId = value.TransactionTypeMasterId, Type = value.Type, Description = value.Description, Form = value.Form }; } }
    public TransactionTypeMaster TransactionTypeMasterForm { get => _transactionTypeMasterForm; set => SetProperty(ref _transactionTypeMasterForm, value); }

    public LocationMaster? SelectedLocationMaster { get => _selectedLocationMaster; set { if (SetProperty(ref _selectedLocationMaster, value) && value != null) LocationMasterForm = new LocationMaster { LocationMasterId = value.LocationMasterId, DataAreaId = value.DataAreaId, LocationCode = value.LocationCode, LocationName = value.LocationName, LocationType = value.LocationType, Warehouse = value.Warehouse, Site = value.Site, Status = value.Status }; } }
    public LocationMaster LocationMasterForm { get => _locationMasterForm; set => SetProperty(ref _locationMasterForm, value); }

    public WeighmentTransferDetails TransferDetailsForm { get => _transferDetailsForm; set => SetProperty(ref _transferDetailsForm, value); }
    public WeighmentSalesDispatchDetails SalesDispatchDetailsForm { get => _salesDispatchDetailsForm; set => SetProperty(ref _salesDispatchDetailsForm, value); }
    public WeighmentProductionDetails ProductionDetailsForm { get => _productionDetailsForm; set => SetProperty(ref _productionDetailsForm, value); }
    public WeighmentReturnDetails ReturnDetailsForm { get => _returnDetailsForm; set => SetProperty(ref _returnDetailsForm, value); }
    public WeighmentDisposalDetails DisposalDetailsForm { get => _disposalDetailsForm; set => SetProperty(ref _disposalDetailsForm, value); }

    public WeighmentPurchaseDetails PurchaseDetailsForm
    {
        get => _purchaseDetailsForm;
        set
        {
            if (ReferenceEquals(_purchaseDetailsForm, value))
                return;

            if (_purchaseDetailsForm != null)
                _purchaseDetailsForm.PropertyChanged -= PurchaseDetailsForm_PropertyChanged;

            if (SetProperty(ref _purchaseDetailsForm, value))
            {
                if (_purchaseDetailsForm != null)
                    _purchaseDetailsForm.PropertyChanged += PurchaseDetailsForm_PropertyChanged;

                RaisePurchaseDetailsDependentProperties();
            }
        }
    }

    public string PurchaseVendorDisplay => BuildMergedDisplay(PurchaseDetailsForm.VendorAccount, PurchaseDetailsForm.VendorName);
    public string PurchaseSourceDisplay => PurchaseDetailsForm.Source;
    public string PurchaseDestinationDisplay => PurchaseDetailsForm.Destination;
    public bool IsPurchaseVendorSelectable => IsPurchaseDetailsEditable && !PurchaseDetailsForm.WalkInVendor;
    public bool IsPurchaseRateAmountEditable => IsPurchaseDetailsEditable && !PurchaseDetailsForm.FocFlag;

    private void PurchaseDetailsForm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WeighmentPurchaseDetails.WalkInVendor) ||
            e.PropertyName == nameof(WeighmentPurchaseDetails.VendorAccount) ||
            e.PropertyName == nameof(WeighmentPurchaseDetails.VendorName) ||
            e.PropertyName == nameof(WeighmentPurchaseDetails.FocFlag) ||
            e.PropertyName == nameof(WeighmentPurchaseDetails.RateAmount) ||
            e.PropertyName == nameof(WeighmentPurchaseDetails.Source) ||
            e.PropertyName == nameof(WeighmentPurchaseDetails.Destination))
        {
            RaisePurchaseDetailsDependentProperties();
        }
    }

    private void RaisePurchaseDetailsDependentProperties()
    {
        OnPropertyChanged(nameof(PurchaseVendorDisplay));
        OnPropertyChanged(nameof(PurchaseSourceDisplay));
        OnPropertyChanged(nameof(PurchaseDestinationDisplay));
        OnPropertyChanged(nameof(IsPurchaseVendorSelectable));
        OnPropertyChanged(nameof(IsPurchaseRateAmountEditable));
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    public WeighmentContractCollectionDetails ContractCollectionDetailsForm
    {
        get => _contractCollectionDetailsForm;
        set
        {
            if (ReferenceEquals(_contractCollectionDetailsForm, value))
                return;

            if (_contractCollectionDetailsForm != null)
                _contractCollectionDetailsForm.PropertyChanged -= ContractCollectionDetailsForm_PropertyChanged;

            if (SetProperty(ref _contractCollectionDetailsForm, value))
            {
                if (_contractCollectionDetailsForm != null)
                    _contractCollectionDetailsForm.PropertyChanged += ContractCollectionDetailsForm_PropertyChanged;

                RaiseContractCollectionDetailsDependentProperties();
            }
        }
    }

    public string ContractCollectionVendorDisplay => BuildMergedDisplay(ContractCollectionDetailsForm.VendorAccount, ContractCollectionDetailsForm.VendorName);
    public string ContractCollectionInvoiceAccountDisplay => BuildMergedDisplay(ContractCollectionDetailsForm.InvoiceAccount, ContractCollectionDetailsForm.InvoiceAccountName);
    public string ContractCollectionContractDisplay => ContractCollectionDetailsForm.ContractNumber;
    public string ContractCollectionLocationDisplay => ContractCollectionDetailsForm.CollectionLocation;
    public string ContractCollectionDestinationDisplay => ContractCollectionDetailsForm.Destination;

    private void ContractCollectionDetailsForm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WeighmentContractCollectionDetails.VendorAccount) ||
            e.PropertyName == nameof(WeighmentContractCollectionDetails.VendorName) ||
            e.PropertyName == nameof(WeighmentContractCollectionDetails.InvoiceAccount) ||
            e.PropertyName == nameof(WeighmentContractCollectionDetails.InvoiceAccountName) ||
            e.PropertyName == nameof(WeighmentContractCollectionDetails.ContractNumber) ||
            e.PropertyName == nameof(WeighmentContractCollectionDetails.CollectionLocation) ||
            e.PropertyName == nameof(WeighmentContractCollectionDetails.Destination) ||
            e.PropertyName == nameof(WeighmentContractCollectionDetails.BillingBasis))
        {
            RaiseContractCollectionDetailsDependentProperties();
        }
    }

    private void RaiseContractCollectionDetailsDependentProperties()
    {
        OnPropertyChanged(nameof(ContractCollectionVendorDisplay));
        OnPropertyChanged(nameof(ContractCollectionInvoiceAccountDisplay));
        OnPropertyChanged(nameof(ContractCollectionContractDisplay));
        OnPropertyChanged(nameof(ContractCollectionLocationDisplay));
        OnPropertyChanged(nameof(ContractCollectionDestinationDisplay));
        OnPropertyChanged(nameof(IsContractCollectionDetailsEditable));
        OnPropertyChanged(nameof(IsContractCollectionDetailsReadOnly));
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    public GatePass? SelectedGatePass
    {
        get => _selectedGatePass;
        set
        {
            if (SetProperty(ref _selectedGatePass, value) && value != null)
            {
                GatePassForm = new GatePass
                {
                    GatePassId = value.GatePassId,
                    DataAreaId = value.DataAreaId,
                    GatePassNumber = value.GatePassNumber,
                    Type = value.Type,
                    EntryDateTime = value.EntryDateTime,
                    VehiclePlate = value.VehiclePlate,
                    DriverName = value.DriverName,
                    DriverMobile = value.DriverMobile,
                    PartyType = value.PartyType,
                    PartyAccount = value.PartyAccount,
                    PartyName = value.PartyName,
                    ExpectedTransactionType = value.ExpectedTransactionType,
                    ExpectedItemNumber = value.ExpectedItemNumber,
                    ExpectedItem = value.ExpectedItem,
                    Source = value.Source,
                    Destination = value.Destination,
                    SecurityOfficer = value.SecurityOfficer,
                    ExitDateTime = value.ExitDateTime,
                    ClosedBy = value.ClosedBy,
                    LinkedTicketNo = value.LinkedTicketNo,
                    Status = value.Status,
                    Remarks = value.Remarks,
                    CreatedAt = value.CreatedAt
                };
                OnPropertyChanged(nameof(IsGatePassFormReadOnly));
                OnPropertyChanged(nameof(IsGatePassFormEditable));
                OnPropertyChanged(nameof(CanLinkSelectedGatePass));
            }
        }
    }

    public GatePass GatePassForm
    {
        get => _gatePassForm;
        set
        {
            if (SetProperty(ref _gatePassForm, value))
            {
                OnPropertyChanged(nameof(IsGatePassFormReadOnly));
                OnPropertyChanged(nameof(IsGatePassFormEditable));
                OnPropertyChanged(nameof(CanLinkSelectedGatePass));
                OnPropertyChanged(nameof(GatePassPartyDisplay));
                OnPropertyChanged(nameof(GatePassExpectedItemDisplay));
            }
        }
    }

    public bool IsGatePassFormReadOnly => GatePassForm.GatePassId > 0;
    public bool IsGatePassFormEditable => !IsGatePassFormReadOnly;
    public bool CanLinkSelectedGatePass => SelectedGatePass != null && string.Equals(SelectedGatePass.Status, "Open", StringComparison.OrdinalIgnoreCase);

    public GatePass? SelectedWeighmentGatePass
    {
        get => _selectedWeighmentGatePass;
        set
        {
            if (SetProperty(ref _selectedWeighmentGatePass, value) && value != null)
            {
                ApplyGatePassToWeighment(value);
            }
        }
    }

    public string GatePassPartyDisplay => BuildMergedDisplay(GatePassForm.PartyAccount, GatePassForm.PartyName);
    public string GatePassExpectedItemDisplay => BuildMergedDisplay(GatePassForm.ExpectedItemNumber, GatePassForm.ExpectedItem);


    public string CurrentUserDisplay => $"{_currentUser.OperatorName} ({_currentUser.Username})";
    public string CurrentUserId => _currentUser.OperatorId.ToString();
    public string CurrentUsername => _currentUser.Username;
    public string CurrentUserCompany => string.IsNullOrWhiteSpace(SelectedLegalEntityDataAreaId) ? (string.IsNullOrWhiteSpace(_currentUser.DataAreaId) ? "DAT" : _currentUser.DataAreaId) : SelectedLegalEntityDataAreaId;
    public string CustomerPageText => $"Page {_customerPageIndex + 1} | Loaded {FilteredCustomers.Count:N0} rows | Page size {MasterPageSize:N0}";
    public string VendorPageText => $"Page {_vendorPageIndex + 1} | Loaded {FilteredVendors.Count:N0} rows | Page size {MasterPageSize:N0}";
    public string ItemMasterPageText => $"Page {_itemMasterPageIndex + 1} | Loaded {FilteredItemMasters.Count:N0} rows | Page size {MasterPageSize:N0}";
    public string WarehousePageText => $"Page {_warehousePageIndex + 1} | Loaded {FilteredWarehouseMasters.Count:N0} rows | Page size {MasterPageSize:N0}";
    public string UnitOfMeasurePageText => $"Page {_unitOfMeasurePageIndex + 1} | Loaded {FilteredUnitOfMeasureMasters.Count:N0} rows | Page size {MasterPageSize:N0}";

    public int SelectedMainTabIndex
    {
        get => _selectedMainTabIndex;
        set => SetProperty(ref _selectedMainTabIndex, value);
    }

    public string SelectedLegalEntityDataAreaId
    {
        get => _selectedLegalEntityDataAreaId;
        set
        {
            if (SetProperty(ref _selectedLegalEntityDataAreaId, value))
            {
                OnPropertyChanged(nameof(CurrentUserCompany));
                if (!_isRefreshingData)
                    _ = RefreshDataForSelectedLegalEntityAsync();
            }
        }
    }
    public bool CanAccessWeighment => _currentUser.CanAccessWeighment;
    public bool CanAccessSettings => _currentUser.CanAccessSettings;
    public bool CanAccessMasters => _currentUser.CanAccessMasters;
    public bool CanAccessReports => _currentUser.CanAccessReports;
    public bool CanAccessTransactions => _currentUser.CanAccessTransactions;
    public bool CanAccessGatePass => _currentUser.CanAccessGatePass;
    public bool CanAccessCancellationVoid => _currentUser.CanAccessCancellationVoid;
    public bool CanAccessCorrection => _currentUser.CanAccessCorrection;
    public bool CanAccessUserManagement => false;
    public bool CanCorrectTransactions => CanAccessCorrection && (_currentUser.CanSubmitCorrection || _currentUser.CanApproveRejectCorrection);
    public bool CanEditCompletedTransaction => false;

    // Legacy direct-cancel authorization is intentionally disabled.
    public bool CanCancelTransactions => false;
    public bool CanDeleteCompletedTransaction => false;
    public bool CanCorrectSelectedTransaction => CanCorrectTransactions && SelectedTransactionWeighment != null && string.Equals(SelectedTransactionWeighment.Status, "Completed", StringComparison.OrdinalIgnoreCase);
    public bool CanCancelSelectedTransaction => false;
    public bool CanCreateCorrectionRequest => CanAccessCorrection && _currentUser.CanSubmitCorrection;
    public bool CanOpenSelectedCorrection => CanAccessCorrection && SelectedCorrectionRequest != null;

    public bool CanCreateCancellationVoidRequest => CanAccessCancellationVoid && _currentUser.CanSubmitCancellationVoid;
    public bool CanApproveRejectCancellationVoidAccess => CanAccessCancellationVoid && _currentUser.CanApproveRejectCancellationVoid;
    public bool IsCancellationVoidFormEditable => CanCreateCancellationVoidRequest && CancellationVoidForm.CancellationVoidId == 0;
    public bool CanSubmitCancellationVoid => IsCancellationVoidFormEditable && CancellationVoidForm.WeighmentId > 0;
    public bool CanApproveCancellationVoid => CanAccessCancellationVoid
                                              && _currentUser.CanApproveRejectCancellationVoid
                                              && CancellationVoidForm.CancellationVoidId > 0
                                              && string.Equals(CancellationVoidForm.Status, "Draft", StringComparison.OrdinalIgnoreCase);
    public bool CanRejectCancellationVoid => CanApproveCancellationVoid;
    public bool CanInitiateCancellationFromTransaction => CanCreateCancellationVoidRequest
                                                         && SelectedTransactionWeighment != null
                                                         && (string.Equals(SelectedTransactionWeighment.Status, "Open", StringComparison.OrdinalIgnoreCase)
                                                             || string.Equals(SelectedTransactionWeighment.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                                                         && !string.Equals(SelectedTransactionWeighment.CancellationVoidStatus, "Draft", StringComparison.OrdinalIgnoreCase)
                                                         && !string.Equals(SelectedTransactionWeighment.CancellationVoidStatus, "Approved", StringComparison.OrdinalIgnoreCase);
    public bool IsCompletedGridReadOnly => true;

    public bool CanSaveFirstWeight => CanAccessWeighment && _currentUser.CanCaptureFirstWeight && _loadedOpenWeighmentId == null && !FirstWeight.HasValue;
    public bool CanSaveSecondWeight => CanAccessWeighment && _currentUser.CanCaptureSecondWeight && _loadedOpenWeighmentId.HasValue && FirstWeight.HasValue && !SecondWeight.HasValue;
    public bool IsHeaderAndLinesEditable => _loadedOpenWeighmentId == null && !FirstWeight.HasValue;
    public bool IsHeaderAndLinesLocked => !IsHeaderAndLinesEditable;


    public LegalEntityMaster? SelectedLegalEntityMaster
    {
        get => _selectedLegalEntityMaster;
        set
        {
            if (SetProperty(ref _selectedLegalEntityMaster, value))
                LoadSelectedLegalEntityToForm();
        }
    }

    public LegalEntityMaster LegalEntityMasterForm
    {
        get => _legalEntityMasterForm;
        set => SetProperty(ref _legalEntityMasterForm, value);
    }

    public string LegalEntityFilter
    {
        get => _legalEntityFilter;
        set
        {
            if (SetProperty(ref _legalEntityFilter, value))
                ApplyLegalEntityFilter();
        }
    }

    public LegalEntityMaster? SelectedOperatorLegalEntityToAdd
    {
        get => _selectedOperatorLegalEntityToAdd;
        set => SetProperty(ref _selectedOperatorLegalEntityToAdd, value);
    }

    public OperatorLegalEntityAssignment? SelectedOperatorLegalEntityAssignment
    {
        get => _selectedOperatorLegalEntityAssignment;
        set => SetProperty(ref _selectedOperatorLegalEntityAssignment, value);
    }

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
            {
                OnPropertyChanged(nameof(LiveWeightText));
                OnPropertyChanged(nameof(LiveScaleWeightText));
            }
        }
    }

    public string LiveWeightText => $"{LiveWeight:N2} kg";
    public string LiveScaleWeightText => LiveWeight.ToString("N3");

    public bool IsStable
    {
        get => _isStable;
        set
        {
            if (SetProperty(ref _isStable, value))
            {
                OnPropertyChanged(nameof(StableText));
                OnPropertyChanged(nameof(LiveScaleStatusText));
            }
        }
    }

    public string StableText => IsStable ? "Yes" : "No";
    public string LiveScaleStatusText => IsStable ? "STABLE" : "UNSTABLE";

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
        set
        {
            if (SetProperty(ref _partyAccount, value))
                OnPropertyChanged(nameof(PartyDisplay));
        }
    }

    public string PartyName
    {
        get => _partyName;
        set
        {
            if (SetProperty(ref _partyName, value))
                OnPropertyChanged(nameof(PartyDisplay));
        }
    }

    public string PartyDisplay => BuildMergedDisplay(PartyAccount, PartyName);

    public string ItemNumber
    {
        get => _itemNumber;
        set
        {
            if (SetProperty(ref _itemNumber, value))
                OnPropertyChanged(nameof(ItemDisplay));
        }
    }

    public string ItemName
    {
        get => _itemName;
        set
        {
            if (SetProperty(ref _itemName, value))
                OnPropertyChanged(nameof(ItemDisplay));
        }
    }

    public string ItemDisplay => BuildMergedDisplay(ItemNumber, ItemName);

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

    public WeighmentMaterialLine? SelectedMaterialLine
    {
        get => _selectedMaterialLine;
        set
        {
            if (SetProperty(ref _selectedMaterialLine, value))
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    public Weighment? SelectedOpenWeighment
    {
        get => _selectedOpenWeighment;
        set
        {
            if (SetProperty(ref _selectedOpenWeighment, value) && value != null)
            {
                LoadSelectedOpenTicket();
            }
        }
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

    public Weighment? SelectedTransactionWeighment
    {
        get => _selectedTransactionWeighment;
        set
        {
            if (SetProperty(ref _selectedTransactionWeighment, value))
            {
                OnPropertyChanged(nameof(CanCorrectSelectedTransaction));
                OnPropertyChanged(nameof(CanCancelSelectedTransaction));
                OnPropertyChanged(nameof(CanInitiateCancellationFromTransaction));
                OnPropertyChanged(nameof(HasSelectedTransactionReview));
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                if (value != null)
                    _ = LoadTransactionReviewAsync(value);
                else
                    ClearTransactionReview();
            }
        }
    }

    public string SelectedTransactionReviewForm
    {
        get => _selectedTransactionReviewForm;
        private set => SetProperty(ref _selectedTransactionReviewForm, value);
    }

    public bool HasSelectedTransactionReview => SelectedTransactionWeighment != null;

    public WeighmentCorrection? SelectedCorrectionRequest
    {
        get => _selectedCorrectionRequest;
        set
        {
            if (SetProperty(ref _selectedCorrectionRequest, value))
            {
                OnPropertyChanged(nameof(CanOpenSelectedCorrection));
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                if (value != null)
                    StatusMessage = $"Correction selected: {value.CorrectionNumber}";
            }
        }
    }

    public CancellationVoidRequest? SelectedCancellationVoidRequest
    {
        get => _selectedCancellationVoidRequest;
        set
        {
            if (SetProperty(ref _selectedCancellationVoidRequest, value) && value != null)
            {
                CancellationVoidForm = CloneCancellationVoidRequest(value);
                StatusMessage = $"Cancellation/Void request selected: {value.CancellationVoidNumber}";
            }
        }
    }

    public CancellationVoidRequest CancellationVoidForm
    {
        get => _cancellationVoidForm;
        set
        {
            if (SetProperty(ref _cancellationVoidForm, value))
            {
                // Keep the ComboBox selection properties explicitly synchronized with the form.
                // Reason is intentionally not defaulted: the user must select a Reason Master code.
                _selectedCancellationVoidType = value?.Type?.Trim() ?? string.Empty;
                var formReason = value?.Reason?.Trim() ?? string.Empty;
                _selectedCancellationReason = formReason;
                OnPropertyChanged(nameof(SelectedCancellationVoidType));
                OnPropertyChanged(nameof(SelectedCancellationReason));
                OnPropertyChanged(nameof(IsCancellationVoidFormEditable));
                OnPropertyChanged(nameof(CanCreateCancellationVoidRequest));
                OnPropertyChanged(nameof(CanSubmitCancellationVoid));
                OnPropertyChanged(nameof(CanApproveCancellationVoid));
                OnPropertyChanged(nameof(CanRejectCancellationVoid));
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string SelectedCancellationVoidType
    {
        get => _selectedCancellationVoidType;
        set
        {
            var normalized = value?.Trim() ?? string.Empty;
            if (SetProperty(ref _selectedCancellationVoidType, normalized))
            {
                CancellationVoidForm.Type = normalized;
                OnPropertyChanged(nameof(CanSubmitCancellationVoid));
            }
        }
    }

    public string SelectedCancellationReason
    {
        get => _selectedCancellationReason;
        set
        {
            var normalized = value?.Trim() ?? string.Empty;
            if (SetProperty(ref _selectedCancellationReason, normalized))
            {
                CancellationVoidForm.Reason = normalized;
                OnPropertyChanged(nameof(CanSubmitCancellationVoid));
            }
        }
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

    public DateTime TransactionFrom
    {
        get => _transactionFrom;
        set => SetProperty(ref _transactionFrom, value);
    }

    public DateTime TransactionTo
    {
        get => _transactionTo;
        set => SetProperty(ref _transactionTo, value);
    }

    public string TransactionTicketFilter
    {
        get => _transactionTicketFilter;
        set
        {
            if (SetProperty(ref _transactionTicketFilter, value))
                ApplyTransactionFilter();
        }
    }

    public string TransactionCompanyFilter
    {
        get => _transactionCompanyFilter;
        set
        {
            if (SetProperty(ref _transactionCompanyFilter, value))
                ApplyTransactionFilter();
        }
    }

    public string TransactionVehicleFilter
    {
        get => _transactionVehicleFilter;
        set
        {
            if (SetProperty(ref _transactionVehicleFilter, value))
                ApplyTransactionFilter();
        }
    }

    public string TransactionDriverFilter
    {
        get => _transactionDriverFilter;
        set
        {
            if (SetProperty(ref _transactionDriverFilter, value))
                ApplyTransactionFilter();
        }
    }

    public string TransactionPartyFilter
    {
        get => _transactionPartyFilter;
        set
        {
            if (SetProperty(ref _transactionPartyFilter, value))
                ApplyTransactionFilter();
        }
    }

    public string TransactionPartyTypeFilter
    {
        get => _transactionPartyTypeFilter;
        set
        {
            if (SetProperty(ref _transactionPartyTypeFilter, value))
                ApplyTransactionFilter();
        }
    }

    public string TransactionItemFilter
    {
        get => _transactionItemFilter;
        set
        {
            if (SetProperty(ref _transactionItemFilter, value))
                ApplyTransactionFilter();
        }
    }

    public string TransactionStatusFilter
    {
        get => _transactionStatusFilter;
        set
        {
            if (SetProperty(ref _transactionStatusFilter, value))
                ApplyTransactionFilter();
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
                _ = LoadCustomerPageAsync(resetPage: true);
        }
    }

    public string VendorFilter
    {
        get => _vendorFilter;
        set
        {
            if (SetProperty(ref _vendorFilter, value))
                _ = LoadVendorPageAsync(resetPage: true);
        }
    }

    public string ItemMasterFilter
    {
        get => _itemMasterFilter;
        set
        {
            if (SetProperty(ref _itemMasterFilter, value))
                _ = LoadItemMasterPageAsync(resetPage: true);
        }
    }

    public string WarehouseFilter
    {
        get => _warehouseFilter;
        set
        {
            if (SetProperty(ref _warehouseFilter, value))
                _ = LoadWarehousePageAsync(resetPage: true);
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
                _ = LoadCustomerPageAsync(resetPage: true);
        }
    }

    public string CustomerNameFilter
    {
        get => _customerNameFilter;
        set
        {
            if (SetProperty(ref _customerNameFilter, value))
                _ = LoadCustomerPageAsync(resetPage: true);
        }
    }

    public string CustomerGroupFilter
    {
        get => _customerGroupFilter;
        set
        {
            if (SetProperty(ref _customerGroupFilter, value))
                _ = LoadCustomerPageAsync(resetPage: true);
        }
    }

    public string CustomerStatusFilter
    {
        get => _customerStatusFilter;
        set
        {
            if (SetProperty(ref _customerStatusFilter, value))
                _ = LoadCustomerPageAsync(resetPage: true);
        }
    }

    public string VendorAccountFilter
    {
        get => _vendorAccountFilter;
        set
        {
            if (SetProperty(ref _vendorAccountFilter, value))
                _ = LoadVendorPageAsync(resetPage: true);
        }
    }

    public string VendorNameFilter
    {
        get => _vendorNameFilter;
        set
        {
            if (SetProperty(ref _vendorNameFilter, value))
                _ = LoadVendorPageAsync(resetPage: true);
        }
    }

    public string VendorGroupFilter
    {
        get => _vendorGroupFilter;
        set
        {
            if (SetProperty(ref _vendorGroupFilter, value))
                _ = LoadVendorPageAsync(resetPage: true);
        }
    }

    public string VendorStatusFilter
    {
        get => _vendorStatusFilter;
        set
        {
            if (SetProperty(ref _vendorStatusFilter, value))
                _ = LoadVendorPageAsync(resetPage: true);
        }
    }

    public string ItemNumberFilter
    {
        get => _itemNumberFilter;
        set
        {
            if (SetProperty(ref _itemNumberFilter, value))
                _ = LoadItemMasterPageAsync(resetPage: true);
        }
    }

    public string ItemProductNameFilter
    {
        get => _itemProductNameFilter;
        set
        {
            if (SetProperty(ref _itemProductNameFilter, value))
                _ = LoadItemMasterPageAsync(resetPage: true);
        }
    }

    public string ItemSearchNameFilter
    {
        get => _itemSearchNameFilter;
        set
        {
            if (SetProperty(ref _itemSearchNameFilter, value))
                _ = LoadItemMasterPageAsync(resetPage: true);
        }
    }

    public string ItemProductTypeFilter
    {
        get => _itemProductTypeFilter;
        set
        {
            if (SetProperty(ref _itemProductTypeFilter, value))
                _ = LoadItemMasterPageAsync(resetPage: true);
        }
    }

    public string WarehouseCodeFilter
    {
        get => _warehouseCodeFilter;
        set
        {
            if (SetProperty(ref _warehouseCodeFilter, value))
                _ = LoadWarehousePageAsync(resetPage: true);
        }
    }

    public string WarehouseNameFilter
    {
        get => _warehouseNameFilter;
        set
        {
            if (SetProperty(ref _warehouseNameFilter, value))
                _ = LoadWarehousePageAsync(resetPage: true);
        }
    }

    public string WarehouseSiteFilter
    {
        get => _warehouseSiteFilter;
        set
        {
            if (SetProperty(ref _warehouseSiteFilter, value))
                _ = LoadWarehousePageAsync(resetPage: true);
        }
    }

    public string WarehouseTypeFilter
    {
        get => _warehouseTypeFilter;
        set
        {
            if (SetProperty(ref _warehouseTypeFilter, value))
                _ = LoadWarehousePageAsync(resetPage: true);
        }
    }

    public string UnitOfMeasureSymbolFilter
    {
        get => _unitOfMeasureSymbolFilter;
        set
        {
            if (SetProperty(ref _unitOfMeasureSymbolFilter, value))
                _ = LoadUnitOfMeasurePageAsync(resetPage: true);
        }
    }

    public string UnitOfMeasureSystemFilter
    {
        get => _unitOfMeasureSystemFilter;
        set
        {
            if (SetProperty(ref _unitOfMeasureSystemFilter, value))
                _ = LoadUnitOfMeasurePageAsync(resetPage: true);
        }
    }

    public string UnitOfMeasureClassFilter
    {
        get => _unitOfMeasureClassFilter;
        set
        {
            if (SetProperty(ref _unitOfMeasureClassFilter, value))
                _ = LoadUnitOfMeasurePageAsync(resetPage: true);
        }
    }

    public string UnitOfMeasureStateFilter
    {
        get => _unitOfMeasureStateFilter;
        set
        {
            if (SetProperty(ref _unitOfMeasureStateFilter, value))
                _ = LoadUnitOfMeasurePageAsync(resetPage: true);
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


    public UnitOfMeasureMaster UnitOfMeasureMasterForm
    {
        get => _unitOfMeasureMasterForm;
        set => SetProperty(ref _unitOfMeasureMasterForm, value);
    }

    public UnitOfMeasureMaster? SelectedUnitOfMeasureMaster
    {
        get => _selectedUnitOfMeasureMaster;
        set
        {
            if (SetProperty(ref _selectedUnitOfMeasureMaster, value))
                LoadSelectedUnitOfMeasureMasterToForm();
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
            await LoadAllowedLegalEntitiesAsync();
            await LoadMastersAsync();
            SelectSettingsWeighbridgeFromSavedSettings();
            await RefreshWeighmentsAsync();
            if (CanAccessCancellationVoid)
            {
                await LoadCancellationVoidRequestsAsync();
                if (CanCreateCancellationVoidRequest)
                    await PrepareNewCancellationVoidAsync();
            }
            if (CanAccessCorrection)
            {
                await LoadCorrectionRequestsAsync();
                await CorrectionWorkspace.InitializeAsync();
            }
            if (CanAccessReports)
                await LoadReportAsync();
            StatusMessage = $"Application loaded. Database: {DatabaseFolderPath}. Use Mock mode first, then test Serial/TCP with your indicator.";
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
            var normalizedNewDatabaseFolderPath = System.IO.Path.GetFullPath(DatabaseFolderPath.Trim());
            var newDatabaseFilePath = BridgeOneConfigService.GetDatabaseFilePathForFolder(normalizedNewDatabaseFolderPath);
            var wasNewDatabaseCreated = !System.IO.File.Exists(newDatabaseFilePath);

            ApplySelectedWeighbridgeToSettings();
            await _databaseService.SaveSettingsAsync(Settings);

            BridgeOneConfigService.SaveDatabaseFolderPath(normalizedNewDatabaseFolderPath);

            if (!string.Equals(previousDatabaseFolderPath, normalizedNewDatabaseFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                var targetDatabaseService = new DatabaseService(newDatabaseFilePath);
                await targetDatabaseService.InitializeAsync();
                await targetDatabaseService.SaveSettingsAsync(Settings);

                var message = wasNewDatabaseCreated
                    ? "Database folder path saved. No database was found in the selected folder, so a new BridgeOne database was created with seed data. Default login: admin / admin123. Please restart BridgeOne."
                    : "Database folder path saved. Existing database found in the selected folder. Please restart BridgeOne to use it.";

                System.Windows.MessageBox.Show(message, "BridgeOne", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                StatusMessage = message;
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

            if (!_currentUser.CanCaptureFirstWeight)
            {
                StatusMessage = "You do not have permission to capture First Weight.";
                return;
            }

            if (!CanSaveFirstWeight)
            {
                StatusMessage = "First weight is already saved for this slip. Please click Second Weight, or click Clear to start a new slip.";
                return;
            }

            var currentWeighbridgeCode = SelectedSettingsWeighbridge?.WeighbridgeCode ?? Settings.SelectedWeighbridgeCode;
            SlipNumber = await _databaseService.GenerateSlipNumberAsync(currentWeighbridgeCode, CurrentUserCompany);
            TicketNo = SlipNumber;
            if (MaterialLines.Count == 0)
            {
                StatusMessage = "Please add at least one material line before saving First Weight.";
                return;
            }

            var materialLineSnapshot = MaterialLines.Select(line => new WeighmentMaterialLine
            {
                MaterialLineId = line.MaterialLineId,
                WeighmentId = line.WeighmentId,
                SlipNumber = line.SlipNumber,
                DataAreaId = line.DataAreaId,
                LineNo = line.LineNo,
                ItemMasterId = line.ItemMasterId,
                ItemNumber = line.ItemNumber,
                ItemName = line.ItemName,
                ExpectedQty = line.ExpectedQty,
                Uom = line.Uom,
                Remarks = line.Remarks,
                CreatedBy = line.CreatedBy,
                CreatedAt = line.CreatedAt
            }).ToList();

            var primaryMaterialLine = materialLineSnapshot.First();
            FirstWeight = LiveWeight;
            var transactionDateTime = DateTime.Now;

            var weighment = new Weighment
            {
                TicketNo = TicketNo,
                SlipNumber = SlipNumber,
                TransactionType = SelectedTransactionTypeMaster?.Type ?? string.Empty,
                Scenario = SelectedScenarioMaster?.Form ?? string.Empty,
                GatePassNumber = GatePassNumber.Trim(),
                WeighbridgeCode = currentWeighbridgeCode,
                TransactionDateTime = transactionDateTime,
                ShiftCode = DeriveShiftCode(transactionDateTime),
                OperatorUsername = CurrentUsername,
                ExternalReference = ExternalReference.Trim(),
                OperatorRemarks = OperatorRemarks.Trim(),
                DataAreaId = CurrentUserCompany.Trim(),
                CompanyName = CurrentUserCompany.Trim(),
                VehicleNo = VehicleNo.Trim().ToUpperInvariant(),
                DriverName = DriverName.Trim(),
                MaterialId = primaryMaterialLine.ItemMasterId,
                ItemNumber = primaryMaterialLine.ItemNumber.Trim(),
                ItemName = primaryMaterialLine.ItemName.Trim(),
                MaterialName = primaryMaterialLine.ItemName.Trim(),
                FirstWeight = LiveWeight,
                FirstWeightTime = DateTime.Now,
                FirstWeightBy = CurrentUsername,
                Status = "Open",
                Remarks = Remarks.Trim(),
                CreatedAt = DateTime.Now
            };

            var savedSlipNumber = SlipNumber;
            var savedWeighmentId = await _databaseService.InsertFirstWeightAsync(weighment);
            await _databaseService.SaveWeighmentMaterialLinesAsync(savedWeighmentId, savedSlipNumber, CurrentUserCompany, materialLineSnapshot, CurrentUsername);
            if (IsPurchaseReceiptCollectionForm)
            {
                var purchaseDetailsSnapshot = new WeighmentPurchaseDetails
                {
                    WeighmentId = savedWeighmentId,
                    SlipNumber = savedSlipNumber,
                    DataAreaId = CurrentUserCompany,
                    PurchaseSubtype = PurchaseDetailsForm.PurchaseSubtype?.Trim() ?? string.Empty,
                    VendorAccount = PurchaseDetailsForm.VendorAccount?.Trim() ?? string.Empty,
                    VendorName = PurchaseDetailsForm.VendorName?.Trim() ?? string.Empty,
                    WalkInVendor = PurchaseDetailsForm.WalkInVendor,
                    SupplierDriverName = PurchaseDetailsForm.SupplierDriverName?.Trim() ?? string.Empty,
                    PurchaseContractReference = PurchaseDetailsForm.PurchaseContractReference?.Trim() ?? string.Empty,
                    Source = PurchaseDetailsForm.Source?.Trim() ?? string.Empty,
                    Destination = PurchaseDetailsForm.Destination?.Trim() ?? string.Empty,
                    FocFlag = PurchaseDetailsForm.FocFlag,
                    RateAmount = PurchaseDetailsForm.FocFlag ? 0 : PurchaseDetailsForm.RateAmount
                };
                await _databaseService.SaveWeighmentPurchaseDetailsAsync(purchaseDetailsSnapshot);
            }
            if (IsContractCollectionForm)
            {
                var contractCollectionDetailsSnapshot = new WeighmentContractCollectionDetails
                {
                    WeighmentId = savedWeighmentId,
                    SlipNumber = savedSlipNumber,
                    DataAreaId = CurrentUserCompany,
                    VendorAccount = ContractCollectionDetailsForm.VendorAccount?.Trim() ?? string.Empty,
                    VendorName = ContractCollectionDetailsForm.VendorName?.Trim() ?? string.Empty,
                    InvoiceAccount = ContractCollectionDetailsForm.InvoiceAccount?.Trim() ?? string.Empty,
                    InvoiceAccountName = ContractCollectionDetailsForm.InvoiceAccountName?.Trim() ?? string.Empty,
                    ContractNumber = ContractCollectionDetailsForm.ContractNumber?.Trim() ?? string.Empty,
                    CollectionLocation = ContractCollectionDetailsForm.CollectionLocation?.Trim() ?? string.Empty,
                    Destination = ContractCollectionDetailsForm.Destination?.Trim() ?? string.Empty,
                    BillingBasis = ContractCollectionDetailsForm.BillingBasis?.Trim() ?? string.Empty
                };
                await _databaseService.SaveWeighmentContractCollectionDetailsAsync(contractCollectionDetailsSnapshot);
            }
            if (IsTransferForm)
            {
                await _databaseService.SaveWeighmentTransferDetailsAsync(new WeighmentTransferDetails
                {
                    WeighmentId = savedWeighmentId,
                    SlipNumber = savedSlipNumber,
                    DataAreaId = CurrentUserCompany,
                    TransferDirection = TransferDetailsForm.TransferDirection?.Trim() ?? string.Empty,
                    FromLegalEntity = TransferDetailsForm.FromLegalEntity?.Trim() ?? string.Empty,
                    ToLegalEntity = TransferDetailsForm.ToLegalEntity?.Trim() ?? string.Empty,
                    FromLocation = TransferDetailsForm.FromLocation?.Trim() ?? string.Empty,
                    ToLocation = TransferDetailsForm.ToLocation?.Trim() ?? string.Empty,
                    TransferReference = TransferDetailsForm.TransferReference?.Trim() ?? string.Empty,
                    SendingSlipReference = TransferDetailsForm.SendingSlipReference?.Trim() ?? string.Empty
                });
            }
            if (IsSalesDispatchForm)
            {
                await _databaseService.SaveWeighmentSalesDispatchDetailsAsync(new WeighmentSalesDispatchDetails
                {
                    WeighmentId = savedWeighmentId,
                    SlipNumber = savedSlipNumber,
                    DataAreaId = CurrentUserCompany,
                    SalesSubtype = SalesDispatchDetailsForm.SalesSubtype?.Trim() ?? string.Empty,
                    CustomerAccount = SalesDispatchDetailsForm.CustomerAccount?.Trim() ?? string.Empty,
                    CustomerName = SalesDispatchDetailsForm.CustomerName?.Trim() ?? string.Empty,
                    WalkInCustomer = SalesDispatchDetailsForm.WalkInCustomer?.Trim() ?? string.Empty,
                    SalesReference = SalesDispatchDetailsForm.SalesReference?.Trim() ?? string.Empty,
                    Source = SalesDispatchDetailsForm.Source?.Trim() ?? string.Empty,
                    Destination = SalesDispatchDetailsForm.Destination?.Trim() ?? string.Empty,
                    PaymentStatus = SalesDispatchDetailsForm.PaymentStatus?.Trim() ?? string.Empty,
                    ReceiptNumber = SalesDispatchDetailsForm.ReceiptNumber?.Trim() ?? string.Empty
                });
            }
            if (IsProductionWeighingForm)
            {
                await _databaseService.SaveWeighmentProductionDetailsAsync(new WeighmentProductionDetails
                {
                    WeighmentId = savedWeighmentId,
                    SlipNumber = savedSlipNumber,
                    DataAreaId = CurrentUserCompany,
                    ProductionMovement = ProductionDetailsForm.ProductionMovement?.Trim() ?? string.Empty,
                    ProductionOrderReference = ProductionDetailsForm.ProductionOrderReference?.Trim() ?? string.Empty,
                    ProductionLine = ProductionDetailsForm.ProductionLine?.Trim() ?? string.Empty,
                    WarehouseLocation = ProductionDetailsForm.WarehouseLocation?.Trim() ?? string.Empty,
                    BatchNumber = ProductionDetailsForm.BatchNumber?.Trim() ?? string.Empty,
                    NumberOfRollsUnits = Math.Max(0, ProductionDetailsForm.NumberOfRollsUnits),
                    GradeGsmWidth = ProductionDetailsForm.GradeGsmWidth?.Trim() ?? string.Empty
                });
            }
            if (IsReturnForm)
            {
                await _databaseService.SaveWeighmentReturnDetailsAsync(new WeighmentReturnDetails
                {
                    WeighmentId = savedWeighmentId,
                    SlipNumber = savedSlipNumber,
                    DataAreaId = CurrentUserCompany,
                    ReturnType = ReturnDetailsForm.ReturnType?.Trim() ?? string.Empty,
                    VendorAccount = ReturnDetailsForm.VendorAccount?.Trim() ?? string.Empty,
                    VendorName = ReturnDetailsForm.VendorName?.Trim() ?? string.Empty,
                    CustomerAccount = ReturnDetailsForm.CustomerAccount?.Trim() ?? string.Empty,
                    CustomerName = ReturnDetailsForm.CustomerName?.Trim() ?? string.Empty,
                    FromLegalEntity = ReturnDetailsForm.FromLegalEntity?.Trim() ?? string.Empty,
                    ToLegalEntity = ReturnDetailsForm.ToLegalEntity?.Trim() ?? string.Empty,
                    OriginalSlipNumber = ReturnDetailsForm.OriginalSlipNumber?.Trim() ?? string.Empty,
                    ReturnReference = ReturnDetailsForm.ReturnReference?.Trim() ?? string.Empty,
                    ReturnReason = ReturnDetailsForm.ReturnReason?.Trim() ?? string.Empty,
                    Source = ReturnDetailsForm.Source?.Trim() ?? string.Empty,
                    Destination = ReturnDetailsForm.Destination?.Trim() ?? string.Empty
                });
            }
            if (IsDisposalWasteMovementForm)
            {
                await _databaseService.SaveWeighmentDisposalDetailsAsync(new WeighmentDisposalDetails
                {
                    WeighmentId = savedWeighmentId,
                    SlipNumber = savedSlipNumber,
                    DataAreaId = CurrentUserCompany,
                    DisposalType = DisposalDetailsForm.DisposalType?.Trim() ?? string.Empty,
                    Source = DisposalDetailsForm.Source?.Trim() ?? string.Empty,
                    DisposalDestination = DisposalDetailsForm.DisposalDestination?.Trim() ?? string.Empty,
                    Reason = DisposalDetailsForm.Reason?.Trim() ?? string.Empty,
                    PermitManifestNumber = DisposalDetailsForm.PermitManifestNumber?.Trim() ?? string.Empty,
                    AuthorizedBy = DisposalDetailsForm.AuthorizedBy?.Trim() ?? string.Empty
                });
            }
            if (!string.IsNullOrWhiteSpace(weighment.GatePassNumber))
                await _databaseService.LinkGatePassAsync(weighment.GatePassNumber, savedSlipNumber);
            await _databaseService.AddVehicleAsync(weighment.VehicleNo);
            await _databaseService.AddDriverAsync(weighment.DriverName);
            await RefreshAllAsync();

            // Safety control: after first weight is saved, clear the entry screen and do not keep
            // the open slip loaded. The operator must select the slip row from Open Slips
            // before saving the second weight.
            ClearEntry();

            StatusMessage = $"First weight saved. Slip: {savedSlipNumber}. Select the slip from Open Slips before saving Second Weight.";
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
                StatusMessage = "Please select an open slip from Open Slips first.";
                return;
            }

            if (!_currentUser.CanCaptureSecondWeight)
            {
                StatusMessage = "You do not have permission to capture Second Weight.";
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
            StatusMessage = $"Second weight saved. Slip completed: {SlipNumber}";
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
            StatusMessage = "Please select an open slip first.";
            return;
        }

        SetLoadedOpenWeighmentId(SelectedOpenWeighment.WeighmentId);
        TicketNo = SelectedOpenWeighment.TicketNo;
        SlipNumber = SelectedOpenWeighment.SlipNumber;
        GatePassNumber = SelectedOpenWeighment.GatePassNumber;
        ExternalReference = SelectedOpenWeighment.ExternalReference;
        OperatorRemarks = SelectedOpenWeighment.OperatorRemarks;
        SelectedTransactionTypeMaster = TransactionTypeMasters.FirstOrDefault(x => string.Equals(x.Type, SelectedOpenWeighment.TransactionType, StringComparison.OrdinalIgnoreCase));
        SelectedScenarioMaster = ScenarioMasters.FirstOrDefault(x => string.Equals(x.Form, SelectedOpenWeighment.Scenario, StringComparison.OrdinalIgnoreCase));
        VehicleNo = SelectedOpenWeighment.VehicleNo;
        DriverName = SelectedOpenWeighment.DriverName;
        ItemNumber = SelectedOpenWeighment.ItemNumber;
        ItemName = string.IsNullOrWhiteSpace(SelectedOpenWeighment.ItemName) ? SelectedOpenWeighment.MaterialName : SelectedOpenWeighment.ItemName;
        Remarks = SelectedOpenWeighment.Remarks;
        FirstWeight = SelectedOpenWeighment.FirstWeight;
        SecondWeight = null;
        NetWeight = null;
        RefreshPartyLookup();
        SelectedWeighmentItem = ItemMasters.FirstOrDefault(x => x.ItemMasterId == SelectedOpenWeighment.MaterialId);
        ItemNumber = SelectedOpenWeighment.ItemNumber;
        ItemName = string.IsNullOrWhiteSpace(SelectedOpenWeighment.ItemName) ? SelectedOpenWeighment.MaterialName : SelectedOpenWeighment.ItemName;

        var savedLines = _databaseService.GetWeighmentMaterialLinesAsync(SelectedOpenWeighment.WeighmentId).GetAwaiter().GetResult();
        MaterialLines.Clear();
        foreach (var line in savedLines)
            MaterialLines.Add(line);

        if (MaterialLines.Count == 0 && !string.IsNullOrWhiteSpace(ItemNumber))
        {
            MaterialLines.Add(new WeighmentMaterialLine
            {
                WeighmentId = SelectedOpenWeighment.WeighmentId,
                SlipNumber = SlipNumber,
                DataAreaId = CurrentUserCompany,
                LineNo = 1,
                ItemMasterId = SelectedOpenWeighment.MaterialId,
                ItemNumber = ItemNumber,
                ItemName = ItemName,
                ExpectedQty = 0,
                Uom = string.Empty,
                Remarks = SelectedOpenWeighment.Remarks ?? string.Empty,
                CreatedBy = SelectedOpenWeighment.FirstWeightBy,
                CreatedAt = SelectedOpenWeighment.CreatedAt
            });
        }

        var savedPurchaseDetails = _databaseService.GetWeighmentPurchaseDetailsAsync(SelectedOpenWeighment.WeighmentId).GetAwaiter().GetResult();
        PurchaseDetailsForm = savedPurchaseDetails ?? new WeighmentPurchaseDetails { WeighmentId = SelectedOpenWeighment.WeighmentId, SlipNumber = SlipNumber, DataAreaId = CurrentUserCompany };
        var savedContractCollectionDetails = _databaseService.GetWeighmentContractCollectionDetailsAsync(SelectedOpenWeighment.WeighmentId).GetAwaiter().GetResult();
        ContractCollectionDetailsForm = savedContractCollectionDetails ?? new WeighmentContractCollectionDetails { WeighmentId = SelectedOpenWeighment.WeighmentId, SlipNumber = SlipNumber, DataAreaId = CurrentUserCompany };
        var savedTransferDetails = _databaseService.GetWeighmentTransferDetailsAsync(SelectedOpenWeighment.WeighmentId).GetAwaiter().GetResult();
        TransferDetailsForm = savedTransferDetails ?? new WeighmentTransferDetails { WeighmentId = SelectedOpenWeighment.WeighmentId, SlipNumber = SlipNumber, DataAreaId = CurrentUserCompany };
        var savedSalesDispatchDetails = _databaseService.GetWeighmentSalesDispatchDetailsAsync(SelectedOpenWeighment.WeighmentId).GetAwaiter().GetResult();
        SalesDispatchDetailsForm = savedSalesDispatchDetails ?? new WeighmentSalesDispatchDetails { WeighmentId = SelectedOpenWeighment.WeighmentId, SlipNumber = SlipNumber, DataAreaId = CurrentUserCompany };
        var savedProductionDetails = _databaseService.GetWeighmentProductionDetailsAsync(SelectedOpenWeighment.WeighmentId).GetAwaiter().GetResult();
        ProductionDetailsForm = savedProductionDetails ?? new WeighmentProductionDetails { WeighmentId = SelectedOpenWeighment.WeighmentId, SlipNumber = SlipNumber, DataAreaId = CurrentUserCompany };
        var savedReturnDetails = _databaseService.GetWeighmentReturnDetailsAsync(SelectedOpenWeighment.WeighmentId).GetAwaiter().GetResult();
        ReturnDetailsForm = savedReturnDetails ?? new WeighmentReturnDetails { WeighmentId = SelectedOpenWeighment.WeighmentId, SlipNumber = SlipNumber, DataAreaId = CurrentUserCompany };
        var savedDisposalDetails = _databaseService.GetWeighmentDisposalDetailsAsync(SelectedOpenWeighment.WeighmentId).GetAwaiter().GetResult();
        DisposalDetailsForm = savedDisposalDetails ?? new WeighmentDisposalDetails { WeighmentId = SelectedOpenWeighment.WeighmentId, SlipNumber = SlipNumber, DataAreaId = CurrentUserCompany };

        OnPropertyChanged(nameof(IsHeaderAndLinesEditable));
        OnPropertyChanged(nameof(IsHeaderAndLinesLocked));
        OnPropertyChanged(nameof(IsPurchaseDetailsEditable));
        OnPropertyChanged(nameof(IsPurchaseDetailsReadOnly));
        OnPropertyChanged(nameof(IsPurchaseVendorSelectable));
        OnPropertyChanged(nameof(IsPurchaseRateAmountEditable));
        OnPropertyChanged(nameof(IsContractCollectionDetailsEditable));
        OnPropertyChanged(nameof(IsContractCollectionDetailsReadOnly));
        OnPropertyChanged(nameof(IsTransferDetailsEditable));
        OnPropertyChanged(nameof(IsSalesDispatchDetailsEditable));
        OnPropertyChanged(nameof(IsProductionDetailsEditable));
        OnPropertyChanged(nameof(IsReturnDetailsEditable));
        OnPropertyChanged(nameof(IsDisposalDetailsEditable));
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        StatusMessage = $"Open slip loaded: {SlipNumber}";
    }

    private async Task RefreshAllAsync()
    {
        var legalEntityBeforeRefresh = SelectedLegalEntityDataAreaId;

        try
        {
            _isRefreshingData = true;
            Settings = await _databaseService.GetSettingsAsync();
            await LoadAllowedLegalEntitiesAsync(legalEntityBeforeRefresh);
            await LoadMastersAsync();
            SelectSettingsWeighbridgeFromSavedSettings();
            await RefreshWeighmentsAsync();
            if (CanAccessReports)
                await LoadReportAsync();
            if (CanAccessTransactions)
                await LoadTransactionsAsync();
            if (CanAccessCancellationVoid)
                await LoadCancellationVoidRequestsAsync();
            if (CanAccessCorrection)
                await LoadCorrectionRequestsAsync();
            StatusMessage = $"Data refreshed successfully for Legal Entity {CurrentUserCompany}.";
        }
        finally
        {
            _isRefreshingData = false;
        }
    }

    private async Task RefreshDataForSelectedLegalEntityAsync()
    {
        try
        {
            ResetLargeMasterPageIndexes();
            ClearCompanyScopedFormsForCurrentLegalEntity();
            ClearEntry();
            await LoadMastersAsync();
            SelectSettingsWeighbridgeFromSavedSettings();
            await RefreshWeighmentsAsync();
            if (CanAccessReports)
                await LoadReportAsync();
            if (CanAccessTransactions)
                await LoadTransactionsAsync();
            if (CanAccessCancellationVoid)
            {
                await LoadCancellationVoidRequestsAsync();
                if (CanCreateCancellationVoidRequest)
                    await PrepareNewCancellationVoidAsync();
            }
            if (CanAccessCorrection)
            {
                await LoadCorrectionRequestsAsync();
                await CorrectionWorkspace.RefreshForCompanyAsync();
            }
            StatusMessage = $"Legal Entity changed to {CurrentUserCompany}.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Legal Entity change error: " + ex.Message;
        }
    }

    private async Task LoadAllowedLegalEntitiesAsync(string? preferredDataAreaId = null)
    {
        var allLegalEntities = await _databaseService.GetLegalEntitiesAsync();
        ReplaceCollection(LegalEntities, allLegalEntities);
        ApplyLegalEntityFilter();

        var assignments = _currentUser.OperatorId > 0
            ? await _databaseService.GetOperatorLegalEntitiesAsync(_currentUser.OperatorId)
            : new List<OperatorLegalEntityAssignment>();

        if (assignments.Count == 0 && !string.IsNullOrWhiteSpace(_currentUser.DataAreaId))
        {
            assignments.Add(new OperatorLegalEntityAssignment
            {
                OperatorId = _currentUser.OperatorId,
                DataAreaId = _currentUser.DataAreaId,
                LegalEntityName = _currentUser.DataAreaId,
                IsDefault = true
            });
        }

        var allowed = assignments
            .Select(a => allLegalEntities.FirstOrDefault(le => IsSameDataArea(le.DataAreaId, a.DataAreaId))
                ?? new LegalEntityMaster { DataAreaId = a.DataAreaId, LegalEntityName = a.LegalEntityName })
            .Where(x => !string.IsNullOrWhiteSpace(x.DataAreaId))
            .GroupBy(x => x.DataAreaId, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (allowed.Count == 0)
            allowed.Add(new LegalEntityMaster { DataAreaId = "DAT", LegalEntityName = "Default Legal Entity" });

        ReplaceCollection(AllowedLegalEntities, allowed);

        var defaultAssignment = assignments.FirstOrDefault(x => x.IsDefault) ?? assignments.FirstOrDefault();
        var defaultDataAreaId = defaultAssignment?.DataAreaId;
        if (string.IsNullOrWhiteSpace(defaultDataAreaId) || !allowed.Any(x => IsSameDataArea(x.DataAreaId, defaultDataAreaId)))
            defaultDataAreaId = allowed.First().DataAreaId;

        var legalEntityToSelect = preferredDataAreaId;
        if (string.IsNullOrWhiteSpace(legalEntityToSelect))
            legalEntityToSelect = _selectedLegalEntityDataAreaId;

        if (string.IsNullOrWhiteSpace(legalEntityToSelect) || !allowed.Any(x => IsSameDataArea(x.DataAreaId, legalEntityToSelect)))
            legalEntityToSelect = defaultDataAreaId ?? allowed.First().DataAreaId;

        _selectedLegalEntityDataAreaId = legalEntityToSelect ?? "DAT";
        OnPropertyChanged(nameof(SelectedLegalEntityDataAreaId));
        OnPropertyChanged(nameof(CurrentUserCompany));
    }

    private void ClearCompanyScopedFormsForCurrentLegalEntity()
    {
        SelectedVehicleMaster = null;
        VehicleMasterForm = new Vehicle { DataAreaId = CurrentUserCompany, Status = "Active" };

        SelectedDriverMaster = null;
        DriverMasterForm = new Driver
        {
            DataAreaId = CurrentUserCompany,
            Status = "Active",
            EffectiveFrom = DateTime.Today
        };

        ClearGatePassForm();

        SelectedWeighbridgeMaster = null;
        WeighbridgeMasterForm = new WeighbridgeMaster
        {
            DataAreaId = CurrentUserCompany,
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
            EffectiveFrom = DateTime.Today
        };
    }

    private async Task LoadMastersAsync()
    {
        var currentDataAreaId = CurrentUserCompany;
        var parties = await _databaseService.GetPartiesAsync();
        var materials = await _databaseService.GetMaterialsAsync();
        var vehicles = await _databaseService.GetVehiclesAsync();
        var drivers = await _databaseService.GetDriversAsync();
        var weighbridgeMasters = await _databaseService.GetWeighbridgeMastersAsync();
        var operatorMasters = await _databaseService.GetOperatorMastersAsync();
        var legalEntities = await _databaseService.GetLegalEntitiesAsync();
        var shiftMasters = await _databaseService.GetShiftMastersAsync();
        var scenarioMasters = await _databaseService.GetScenarioMastersAsync(currentDataAreaId);
        var reasonMasters = await _databaseService.GetReasonMastersAsync();
        var contractMasters = await _databaseService.GetContractMastersAsync();
        var toleranceMasters = await _databaseService.GetToleranceMastersAsync();
        var serviceChargeMasters = await _databaseService.GetServiceChargeMastersAsync(currentDataAreaId);
        var transactionTypeMasters = await _databaseService.GetTransactionTypeMastersAsync();
        var locationMasters = await _databaseService.GetLocationMastersAsync(currentDataAreaId);
        // Load the complete global UOM master for the Material Lines drop-down.
        var materialLineUoms = await _databaseService.GetUnitOfMeasureMastersAsync();
        var gatePasses = CanAccessGatePass ? await _databaseService.GetGatePassesAsync(currentDataAreaId) : new List<GatePass>();

        ReplaceCollection(LegalEntities, legalEntities);
        ApplyLegalEntityFilter();
        ReplaceCollection(ShiftMasters, shiftMasters);
        ReplaceCollection(ScenarioMasters, scenarioMasters);
        ReplaceCollection(ReasonMasters, reasonMasters);
        ReplaceCollection(CancellationReasons, reasonMasters
            .Select(x => x.Code?.Trim() ?? string.Empty)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        ReplaceCollection(ContractMasters, contractMasters);
        ReplaceCollection(ToleranceMasters, toleranceMasters);
        ReplaceCollection(ServiceChargeMasters, serviceChargeMasters);
        ReplaceCollection(TransactionTypeMasters, transactionTypeMasters);
        ReplaceCollection(LocationMasters, locationMasters);
        ReplaceCollection(MaterialLineUomSymbols, materialLineUoms
            .Where(x => !string.IsNullOrWhiteSpace(x.symbol)
                && !string.Equals(x.IsDelete?.Trim(), "1", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x.IsDelete?.Trim(), "true", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.symbol.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        ReplaceCollection(GatePasses, gatePasses);
        ReplaceCollection(OpenGatePasses, gatePasses.Where(x => string.Equals(x.Status, "Open", StringComparison.OrdinalIgnoreCase)));

        var dataAreaVehicles = vehicles.Where(x => IsSameDataArea(x.DataAreaId, currentDataAreaId)).ToList();
        var dataAreaDrivers = drivers.Where(x => IsSameDataArea(x.DataAreaId, currentDataAreaId)).ToList();
        var dataAreaWeighbridges = weighbridgeMasters.Where(x => IsSameDataArea(x.DataAreaId, currentDataAreaId)).ToList();
        // Operator Master is global for login/security, so it is not filtered by Legal Entity.
        var globalOperators = operatorMasters.ToList();

        ReplaceCollection(Parties, parties);
        ReplaceCollection(Materials, materials);
        ReplaceCollection(Vehicles, dataAreaVehicles);
        ReplaceCollection(ActiveVehicles, dataAreaVehicles.Where(x => IsStatusActive(x.Status)));
        ReplaceCollection(Drivers, dataAreaDrivers);
        ReplaceCollection(ActiveDrivers, dataAreaDrivers.Where(x => IsStatusActive(x.Status)));
        ReplaceCollection(WeighbridgeMasters, dataAreaWeighbridges);
        ReplaceCollection(ActiveWeighbridgeMasters, dataAreaWeighbridges.Where(x => IsStatusActive(x.OperatingStatus)));
        ReplaceCollection(OperatorMasters, globalOperators);
        if (SelectedSettingsWeighbridge == null)
            SelectSettingsWeighbridgeFromSavedSettings();

        await LoadLargeMasterPagesAsync(resetPages: false);
        ApplyVehicleFilter();
        ApplyDriverFilter();
        ApplyWeighbridgeFilter();
        ApplyOperatorFilter();
        RefreshPartyLookup();
        SelectedWeighmentItem ??= null;
        SelectedMaterial ??= Materials.FirstOrDefault();
    }

    private void ResetLargeMasterPageIndexes()
    {
        _customerPageIndex = 0;
        _vendorPageIndex = 0;
        _itemMasterPageIndex = 0;
        _warehousePageIndex = 0;
        _unitOfMeasurePageIndex = 0;
    }

    private async Task LoadLargeMasterPagesAsync(bool resetPages)
    {
        await LoadCustomerPageAsync(resetPages);
        await LoadVendorPageAsync(resetPages);
        await LoadItemMasterPageAsync(resetPages);
        await LoadWarehousePageAsync(resetPages);
        await LoadUnitOfMeasurePageAsync(resetPages);
    }

    private async Task LoadCustomerPageAsync(bool resetPage = false)
    {
        if (_isLoadingCustomerPage)
            return;

        try
        {
            _isLoadingCustomerPage = true;
            if (resetPage)
                _customerPageIndex = 0;

            var rows = await _databaseService.GetCustomersPageAsync(CurrentUserCompany, CustomerAccountFilter, CustomerNameFilter, CustomerGroupFilter, CustomerStatusFilter, MasterPageSize, _customerPageIndex * MasterPageSize);
            ReplaceCollection(Customers, rows);
            ReplaceCollection(FilteredCustomers, rows);
            OnPropertyChanged(nameof(CustomerPageText));
        }
        finally
        {
            _isLoadingCustomerPage = false;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    private async Task LoadVendorPageAsync(bool resetPage = false)
    {
        if (_isLoadingVendorPage)
            return;

        try
        {
            _isLoadingVendorPage = true;
            if (resetPage)
                _vendorPageIndex = 0;

            var rows = await _databaseService.GetVendorsPageAsync(CurrentUserCompany, VendorAccountFilter, VendorNameFilter, VendorGroupFilter, VendorStatusFilter, MasterPageSize, _vendorPageIndex * MasterPageSize);
            ReplaceCollection(Vendors, rows);
            ReplaceCollection(FilteredVendors, rows);
            OnPropertyChanged(nameof(VendorPageText));
        }
        finally
        {
            _isLoadingVendorPage = false;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    private async Task LoadItemMasterPageAsync(bool resetPage = false)
    {
        if (_isLoadingItemMasterPage)
            return;

        try
        {
            _isLoadingItemMasterPage = true;
            if (resetPage)
                _itemMasterPageIndex = 0;

            var rows = await _databaseService.GetItemMastersPageAsync(CurrentUserCompany, ItemNumberFilter, ItemProductNameFilter, ItemSearchNameFilter, ItemProductTypeFilter, MasterPageSize, _itemMasterPageIndex * MasterPageSize);
            ReplaceCollection(ItemMasters, rows);
            ReplaceCollection(FilteredItemMasters, rows);
            OnPropertyChanged(nameof(ItemMasterPageText));
        }
        finally
        {
            _isLoadingItemMasterPage = false;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    private async Task LoadWarehousePageAsync(bool resetPage = false)
    {
        if (_isLoadingWarehousePage)
            return;

        try
        {
            _isLoadingWarehousePage = true;
            if (resetPage)
                _warehousePageIndex = 0;

            var rows = await _databaseService.GetWarehouseMastersPageAsync(CurrentUserCompany, WarehouseCodeFilter, WarehouseNameFilter, WarehouseSiteFilter, WarehouseTypeFilter, MasterPageSize, _warehousePageIndex * MasterPageSize);
            ReplaceCollection(WarehouseMasters, rows);
            ReplaceCollection(FilteredWarehouseMasters, rows);
            OnPropertyChanged(nameof(WarehousePageText));
        }
        finally
        {
            _isLoadingWarehousePage = false;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    private async Task LoadUnitOfMeasurePageAsync(bool resetPage = false)
    {
        if (_isLoadingUnitOfMeasurePage)
            return;

        try
        {
            _isLoadingUnitOfMeasurePage = true;
            if (resetPage)
                _unitOfMeasurePageIndex = 0;

            // Unit of Measure is a global synchronized master; do not filter it by the selected legal entity.
            var rows = await _databaseService.GetUnitOfMeasureMastersPageAsync(UnitOfMeasureSymbolFilter, UnitOfMeasureSystemFilter, UnitOfMeasureClassFilter, UnitOfMeasureStateFilter, MasterPageSize, _unitOfMeasurePageIndex * MasterPageSize);
            ReplaceCollection(UnitOfMeasureMasters, rows);
            ReplaceCollection(FilteredUnitOfMeasureMasters, rows);
            OnPropertyChanged(nameof(UnitOfMeasurePageText));
        }
        finally
        {
            _isLoadingUnitOfMeasurePage = false;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    private async Task RefreshWeighmentsAsync()
    {
        var openRows = (await _databaseService.GetOpenWeighmentsAsync()).Where(x => IsSameDataArea(x.DataAreaId, CurrentUserCompany));
        var completedRows = (await _databaseService.GetCompletedTodayAsync()).Where(x => IsSameDataArea(x.DataAreaId, CurrentUserCompany));
        ReplaceCollection(OpenWeighments, openRows);
        ReplaceCollection(CompletedToday, completedRows);
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

            var rows = (await _databaseService.SearchWeighmentsAsync(ReportFrom, ReportTo))
                .Where(x => IsSameDataArea(x.DataAreaId, CurrentUserCompany));
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
                StatusMessage = "Please select a completed slip from report first.";
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

    private Task SaveCompletedTransactionEditAsync(Weighment? weighment)
    {
        // Completed transactions are immutable outside the controlled Correction workflow.
        StatusMessage = "Completed transactions cannot be edited directly. Use Create / View Correction.";
        return Task.CompletedTask;
    }

    private async Task DeleteCompletedAsync()
    {
        await DeleteCompletedTransactionAsync(SelectedCompletedWeighment);
    }

    private async Task DeleteReportRowAsync()
    {
        await DeleteCompletedTransactionAsync(SelectedReportWeighment);
    }

    private Task DeleteCompletedTransactionAsync(Weighment? weighment)
    {
        // Completed transactions are never directly deleted/cancelled. Use Cancellation / Void.
        StatusMessage = "Completed transactions cannot be cancelled directly. Use Create Cancellation / Void.";
        return Task.CompletedTask;
    }

    private async Task LoadCorrectionRequestsAsync()
    {
        try
        {
            if (!CanAccessCorrection)
                return;

            var selectedId = SelectedCorrectionRequest?.CorrectionId ?? 0;
            var rows = await _databaseService.GetWeighmentCorrectionsAsync(CurrentUserCompany);
            ReplaceCollection(CorrectionRequests, rows);

            if (selectedId > 0)
                SelectedCorrectionRequest = CorrectionRequests.FirstOrDefault(x => x.CorrectionId == selectedId);
        }
        catch (Exception ex)
        {
            StatusMessage = "Correction load error: " + ex.Message;
        }
    }

    private async Task NewCorrectionAsync()
    {
        try
        {
            if (!CanCreateCorrectionRequest)
            {
                StatusMessage = "You do not have permission to create/submit corrections.";
                return;
            }

            SelectedMainTabIndex = 6;
            var lookup = new WeightBridgeApp.CorrectionSlipLookupWindow(_databaseService, CurrentUserCompany)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (lookup.ShowDialog() != true || lookup.SelectedWeighment == null)
                return;

            await OpenCorrectionWindowAsync(lookup.SelectedWeighment);
        }
        catch (Exception ex)
        {
            StatusMessage = "New correction error: " + ex.Message;
        }
    }

    private async Task OpenSelectedCorrectionAsync()
    {
        try
        {
            if (!CanAccessCorrection)
            {
                StatusMessage = "You do not have access to corrections.";
                return;
            }

            var request = SelectedCorrectionRequest;
            if (request == null)
            {
                StatusMessage = "Please select a correction request first.";
                return;
            }

            var weighment = await _databaseService.GetWeighmentByIdAsync(request.WeighmentId, CurrentUserCompany);
            if (weighment == null)
            {
                StatusMessage = "The original transaction for this correction could not be found.";
                return;
            }

            await OpenCorrectionWindowAsync(weighment, request);
        }
        catch (Exception ex)
        {
            StatusMessage = "Open correction error: " + ex.Message;
        }
    }

    private async Task OpenCorrectionWindowAsync(Weighment transaction, WeighmentCorrection? correctionToOpen = null)
    {
        if (!string.Equals(transaction.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = "Correction workflow is available only for Completed transactions.";
            return;
        }

        var window = new WeightBridgeApp.TransactionCorrectionWindow(_databaseService, _currentUser, transaction, correctionToOpen)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        window.ShowDialog();

        await RefreshWeighmentsAsync();
        await LoadCorrectionRequestsAsync();
        if (CanAccessTransactions)
            await LoadTransactionsAsync();

        var correctionId = correctionToOpen?.CorrectionId ?? 0;
        if (correctionId > 0)
            SelectedCorrectionRequest = CorrectionRequests.FirstOrDefault(x => x.CorrectionId == correctionId);
        else
            SelectedCorrectionRequest = CorrectionRequests.FirstOrDefault(x => x.WeighmentId == transaction.WeighmentId);

        StatusMessage = $"Correction screen closed for {(string.IsNullOrWhiteSpace(transaction.SlipNumber) ? transaction.TicketNo : transaction.SlipNumber)}.";
    }

    private async Task LoadCancellationVoidRequestsAsync()
    {
        try
        {
            if (!CanAccessCancellationVoid)
                return;

            var rows = await _databaseService.GetCancellationVoidRequestsAsync(CurrentUserCompany);
            ReplaceCollection(CancellationVoidRequests, rows);
        }
        catch (Exception ex)
        {
            StatusMessage = "Cancellation/Void load error: " + ex.Message;
        }
    }

    private async Task PrepareNewCancellationVoidAsync()
    {
        SelectedCancellationVoidRequest = null;
        CancellationVoidForm = new CancellationVoidRequest
        {
            DataAreaId = CurrentUserCompany,
            Type = "Cancel",
            Reason = string.Empty,
            Status = "Draft",
            CancellationVoidNumber = await _databaseService.GenerateCancellationVoidNumberAsync(CurrentUserCompany)
        };

        // Reason must be explicitly selected from Reason Master.
        SelectedCancellationReason = string.Empty;
    }

    private async Task NewCancellationVoidAsync()
    {
        if (!CanCreateCancellationVoidRequest)
        {
            StatusMessage = "You do not have permission to submit Cancellation/Void requests.";
            return;
        }

        await PrepareNewCancellationVoidAsync();
        SelectedMainTabIndex = 5;
        StatusMessage = "New Cancellation/Void request ready.";
    }

    private async Task StartCancellationFromSelectedTransactionAsync()
    {
        try
        {
            if (!CanCreateCancellationVoidRequest)
            {
                StatusMessage = "You do not have permission to submit Cancellation/Void requests.";
                return;
            }

            var selected = SelectedTransactionWeighment;
            if (selected == null)
            {
                StatusMessage = "Please select a completed transaction first.";
                return;
            }

            if (!string.Equals(selected.Status, "Open", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(selected.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "Cancellation/Void requests can only be created for Open or Completed transactions.";
                return;
            }

            if (string.Equals(selected.CancellationVoidStatus, "Draft", StringComparison.OrdinalIgnoreCase)
                || string.Equals(selected.CancellationVoidStatus, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = $"This transaction is already linked to Cancellation/Void request {selected.CancellationVoidNumber}.";
                return;
            }

            await PrepareNewCancellationVoidAsync();
            CancellationVoidForm.WeighmentId = selected.WeighmentId;
            CancellationVoidForm.SlipNumber = string.IsNullOrWhiteSpace(selected.SlipNumber) ? selected.TicketNo : selected.SlipNumber;
            CancellationVoidForm.GatePassNumber = selected.GatePassNumber ?? string.Empty;

            OnPropertyChanged(nameof(CancellationVoidForm));
            OnPropertyChanged(nameof(CanSubmitCancellationVoid));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            SelectedMainTabIndex = 5;
            StatusMessage = $"Cancellation/Void request started for {selected.Status} slip {CancellationVoidForm.SlipNumber}.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Cancellation/Void request start error: " + ex.Message;
        }
    }

    private async Task OpenCancellationSlipLookupAsync()
    {
        try
        {
            if (!IsCancellationVoidFormEditable)
            {
                StatusMessage = "Approved/rejected/submitted requests are read-only. Click New to create another request.";
                return;
            }

            var window = new WeightBridgeApp.CancellationSlipLookupWindow(_databaseService, CurrentUserCompany)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (window.ShowDialog() != true || window.SelectedWeighment == null)
                return;

            var selected = window.SelectedWeighment;
            CancellationVoidForm.WeighmentId = selected.WeighmentId;
            CancellationVoidForm.SlipNumber = string.IsNullOrWhiteSpace(selected.SlipNumber) ? selected.TicketNo : selected.SlipNumber;
            CancellationVoidForm.GatePassNumber = selected.GatePassNumber ?? string.Empty;
            if (string.IsNullOrWhiteSpace(CancellationVoidForm.CancellationVoidNumber))
                CancellationVoidForm.CancellationVoidNumber = await _databaseService.GenerateCancellationVoidNumberAsync(CurrentUserCompany);

            OnPropertyChanged(nameof(CancellationVoidForm));
            OnPropertyChanged(nameof(CanSubmitCancellationVoid));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            StatusMessage = $"Original slip selected: {CancellationVoidForm.SlipNumber}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Original slip lookup error: " + ex.Message;
        }
    }

    private async Task SubmitCancellationVoidAsync()
    {
        try
        {
            if (!CanCreateCancellationVoidRequest)
            {
                StatusMessage = "You do not have permission to submit cancellation/void requests.";
                return;
            }

            if (CancellationVoidForm.WeighmentId <= 0 || string.IsNullOrWhiteSpace(CancellationVoidForm.SlipNumber))
            {
                StatusMessage = "Please select the original slip.";
                return;
            }

            // Explicitly synchronize the selected Reason Master code before validation.
            CancellationVoidForm.Type = SelectedCancellationVoidType?.Trim() ?? string.Empty;

            var effectiveReason = SelectedCancellationReason?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(effectiveReason))
                effectiveReason = CancellationVoidForm.Reason?.Trim() ?? string.Empty;

            CancellationVoidForm.Reason = effectiveReason;
            if (!string.Equals(_selectedCancellationReason, effectiveReason, StringComparison.Ordinal))
            {
                _selectedCancellationReason = effectiveReason;
                OnPropertyChanged(nameof(SelectedCancellationReason));
            }

            if (string.IsNullOrWhiteSpace(CancellationVoidForm.Type))
            {
                StatusMessage = "Type is mandatory.";
                return;
            }

            if (string.IsNullOrWhiteSpace(CancellationVoidForm.Reason))
            {
                StatusMessage = "Reason is mandatory.";
                return;
            }

            CancellationVoidForm.DataAreaId = CurrentUserCompany;
            CancellationVoidForm.Status = "Draft";
            CancellationVoidForm.SubmittedBy = CurrentUsername;
            CancellationVoidForm.SubmittedDateTime = DateTime.Now;
            CancellationVoidForm.ApprovedRejectedBy = string.Empty;
            CancellationVoidForm.ApprovalRejectedDateTime = null;
            CancellationVoidForm.CreatedAt = DateTime.Now;
            if (string.IsNullOrWhiteSpace(CancellationVoidForm.CancellationVoidNumber))
                CancellationVoidForm.CancellationVoidNumber = await _databaseService.GenerateCancellationVoidNumberAsync(CurrentUserCompany);

            CancellationVoidForm.CancellationVoidId = await _databaseService.SubmitCancellationVoidAsync(CancellationVoidForm);
            await LoadCancellationVoidRequestsAsync();
            var saved = CancellationVoidRequests.FirstOrDefault(x => x.CancellationVoidId == CancellationVoidForm.CancellationVoidId);
            if (saved != null)
                SelectedCancellationVoidRequest = saved;
            else
                CancellationVoidForm = CloneCancellationVoidRequest(CancellationVoidForm);

            OnPropertyChanged(nameof(IsCancellationVoidFormEditable));
            OnPropertyChanged(nameof(CanSubmitCancellationVoid));
            OnPropertyChanged(nameof(CanApproveCancellationVoid));
            OnPropertyChanged(nameof(CanRejectCancellationVoid));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            StatusMessage = $"Cancellation/Void request submitted: {CancellationVoidForm.CancellationVoidNumber}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Cancellation/Void submit error: " + ex.Message;
        }
    }

    private async Task ApproveCancellationVoidAsync()
    {
        try
        {
            if (!CanApproveCancellationVoid)
            {
                StatusMessage = "Please select a submitted Draft cancellation/void request.";
                return;
            }

            var confirm = System.Windows.MessageBox.Show(
                $"Approve {CancellationVoidForm.Type} request {CancellationVoidForm.CancellationVoidNumber} for slip {CancellationVoidForm.SlipNumber}?\n\nThe original transaction will be cancelled and its linked Gate Pass will be closed.",
                "Approve Cancellation / Void",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (confirm != System.Windows.MessageBoxResult.Yes)
                return;

            var id = CancellationVoidForm.CancellationVoidId;
            await _databaseService.ApproveCancellationVoidAsync(id, CurrentUsername);
            await RefreshWeighmentsAsync();
            await LoadCancellationVoidRequestsAsync();
            if (CanAccessTransactions)
                await LoadTransactionsAsync();
            await LoadMastersAsync();

            var approved = CancellationVoidRequests.FirstOrDefault(x => x.CancellationVoidId == id);
            if (approved != null)
                SelectedCancellationVoidRequest = approved;

            StatusMessage = $"Cancellation/Void request approved. Slip {CancellationVoidForm.SlipNumber} is cancelled and the linked Gate Pass is closed.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Cancellation/Void approval error: " + ex.Message;
        }
    }

    private async Task RejectCancellationVoidAsync()
    {
        try
        {
            if (!CanRejectCancellationVoid)
            {
                StatusMessage = "Please select a submitted Draft cancellation/void request.";
                return;
            }

            var confirm = System.Windows.MessageBox.Show(
                $"Reject cancellation/void request {CancellationVoidForm.CancellationVoidNumber}?",
                "Reject Cancellation / Void",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (confirm != System.Windows.MessageBoxResult.Yes)
                return;

            var id = CancellationVoidForm.CancellationVoidId;
            await _databaseService.RejectCancellationVoidAsync(id, CurrentUsername);
            await LoadCancellationVoidRequestsAsync();
            var rejected = CancellationVoidRequests.FirstOrDefault(x => x.CancellationVoidId == id);
            if (rejected != null)
                SelectedCancellationVoidRequest = rejected;
            StatusMessage = $"Cancellation/Void request rejected: {CancellationVoidForm.CancellationVoidNumber}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Cancellation/Void rejection error: " + ex.Message;
        }
    }

    private static CancellationVoidRequest CloneCancellationVoidRequest(CancellationVoidRequest source) => new()
    {
        CancellationVoidId = source.CancellationVoidId,
        DataAreaId = source.DataAreaId,
        WeighmentId = source.WeighmentId,
        SlipNumber = source.SlipNumber,
        GatePassNumber = source.GatePassNumber,
        CancellationVoidNumber = source.CancellationVoidNumber,
        Type = source.Type,
        Reason = source.Reason,
        Comment = source.Comment,
        Status = source.Status,
        SubmittedBy = source.SubmittedBy,
        SubmittedDateTime = source.SubmittedDateTime,
        ApprovedRejectedBy = source.ApprovedRejectedBy,
        ApprovalRejectedDateTime = source.ApprovalRejectedDateTime,
        CreatedAt = source.CreatedAt
    };

    private void ClearTransactionReview()
    {
        SelectedTransactionReviewForm = string.Empty;
        TransactionReviewCommonFields.Clear();
        TransactionReviewDynamicFields.Clear();
        TransactionReviewMaterialLines.Clear();
    }

    private async Task LoadTransactionReviewAsync(Weighment transaction)
    {
        try
        {
            var selectedId = transaction.WeighmentId;
            var transactionConfig = TransactionTypeMasters.FirstOrDefault(x =>
                string.Equals(x.Type?.Trim(), transaction.TransactionType?.Trim(), StringComparison.OrdinalIgnoreCase));
            var form = transactionConfig?.Form?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(form))
                form = transaction.TransactionType?.Trim() ?? string.Empty;

            var common = new List<TransactionReviewField>
            {
                ReviewField("Slip Number", transaction.SlipNumber),
                ReviewField("Status", transaction.Status),
                ReviewField("Transaction Type", transaction.TransactionType),
                ReviewField("Mapped Form", form),
                ReviewField("Scenario", transaction.Scenario),
                ReviewField("Legal Entity", transaction.DataAreaId),
                ReviewField("Company", transaction.CompanyName),
                ReviewField("Transaction Date/Time", transaction.TransactionDateTime),
                ReviewField("Gate Pass", transaction.GatePassNumber),
                ReviewField("Weighbridge", transaction.WeighbridgeCode),
                ReviewField("Shift", transaction.ShiftCode),
                ReviewField("Vehicle", transaction.VehicleNo),
                ReviewField("Driver", transaction.DriverName),
                ReviewField("Item Number", transaction.ItemNumber),
                ReviewField("Item Name", transaction.ItemName),
                ReviewField("Operator", transaction.OperatorUsername),
                ReviewField("First Weight", transaction.FirstWeight),
                ReviewField("First Weight Date/Time", transaction.FirstWeightTime),
                ReviewField("First Weight By", string.IsNullOrWhiteSpace(transaction.FirstWeightByDisplay) ? transaction.FirstWeightBy : transaction.FirstWeightByDisplay),
                ReviewField("Second Weight", transaction.SecondWeight),
                ReviewField("Second Weight Date/Time", transaction.SecondWeightTime),
                ReviewField("Second Weight By", string.IsNullOrWhiteSpace(transaction.SecondWeightByDisplay) ? transaction.SecondWeightBy : transaction.SecondWeightByDisplay),
                ReviewField("Net Weight", transaction.NetWeight),
                ReviewField("External Reference", transaction.ExternalReference),
                ReviewField("Operator Remarks", transaction.OperatorRemarks),
                ReviewField("Remarks", transaction.Remarks),
                ReviewField("Corrected", transaction.IsCorrected),
                ReviewField("Correction Version", transaction.CorrectionVersion),
                ReviewField("Last Correction", transaction.LastCorrectionNumber),
                ReviewField("Last Corrected Date/Time", transaction.LastCorrectedDateTime),
                ReviewField("Last Corrected By", transaction.LastCorrectedBy),
                ReviewField("Cancellation/Void Number", transaction.CancellationVoidNumber),
                ReviewField("Cancellation/Void Status", transaction.CancellationVoidStatus)
            };

            var dynamicFields = new List<TransactionReviewField>();
            switch (form)
            {
                case "Purchase / Receipt / Collection":
                {
                    var d = await _databaseService.GetWeighmentPurchaseDetailsAsync(selectedId);
                    if (d != null)
                    {
                        dynamicFields.Add(ReviewField("Purchase Subtype", d.PurchaseSubtype));
                        dynamicFields.Add(ReviewField("Vendor Account", d.VendorAccount));
                        dynamicFields.Add(ReviewField("Vendor Name", d.VendorName));
                        dynamicFields.Add(ReviewField("Walk-in Vendor", d.WalkInVendor));
                        dynamicFields.Add(ReviewField("Supplier Driver Name", d.SupplierDriverName));
                        dynamicFields.Add(ReviewField("Purchase Contract Reference", d.PurchaseContractReference));
                        dynamicFields.Add(ReviewField("Source", d.Source));
                        dynamicFields.Add(ReviewField("Destination", d.Destination));
                        dynamicFields.Add(ReviewField("FOC", d.FocFlag));
                        dynamicFields.Add(ReviewField("Rate Amount", d.RateAmount));
                    }
                    break;
                }
                case "Contract Collection":
                {
                    var d = await _databaseService.GetWeighmentContractCollectionDetailsAsync(selectedId);
                    if (d != null)
                    {
                        dynamicFields.Add(ReviewField("Vendor Account", d.VendorAccount));
                        dynamicFields.Add(ReviewField("Vendor Name", d.VendorName));
                        dynamicFields.Add(ReviewField("Invoice Account", d.InvoiceAccount));
                        dynamicFields.Add(ReviewField("Invoice Account Name", d.InvoiceAccountName));
                        dynamicFields.Add(ReviewField("Contract Number", d.ContractNumber));
                        dynamicFields.Add(ReviewField("Collection Location", d.CollectionLocation));
                        dynamicFields.Add(ReviewField("Destination", d.Destination));
                        dynamicFields.Add(ReviewField("Billing Basis", d.BillingBasis));
                    }
                    break;
                }
                case "Transfer Form":
                {
                    var d = await _databaseService.GetWeighmentTransferDetailsAsync(selectedId);
                    if (d != null)
                    {
                        dynamicFields.Add(ReviewField("Transfer Direction", d.TransferDirection));
                        dynamicFields.Add(ReviewField("From Legal Entity", d.FromLegalEntity));
                        dynamicFields.Add(ReviewField("To Legal Entity", d.ToLegalEntity));
                        dynamicFields.Add(ReviewField("From Location", d.FromLocation));
                        dynamicFields.Add(ReviewField("To Location", d.ToLocation));
                        dynamicFields.Add(ReviewField("Transfer Reference", d.TransferReference));
                        dynamicFields.Add(ReviewField("Sending Slip Reference", d.SendingSlipReference));
                    }
                    break;
                }
                case "Sales / Dispatch":
                {
                    var d = await _databaseService.GetWeighmentSalesDispatchDetailsAsync(selectedId);
                    if (d != null)
                    {
                        dynamicFields.Add(ReviewField("Sales Subtype", d.SalesSubtype));
                        dynamicFields.Add(ReviewField("Customer Account", d.CustomerAccount));
                        dynamicFields.Add(ReviewField("Customer Name", d.CustomerName));
                        dynamicFields.Add(ReviewField("Walk-in Customer", d.WalkInCustomer));
                        dynamicFields.Add(ReviewField("Sales Reference", d.SalesReference));
                        dynamicFields.Add(ReviewField("Source", d.Source));
                        dynamicFields.Add(ReviewField("Destination", d.Destination));
                        dynamicFields.Add(ReviewField("Payment Status", d.PaymentStatus));
                        dynamicFields.Add(ReviewField("Receipt Number", d.ReceiptNumber));
                    }
                    break;
                }
                case "Production Weighing":
                {
                    var d = await _databaseService.GetWeighmentProductionDetailsAsync(selectedId);
                    if (d != null)
                    {
                        dynamicFields.Add(ReviewField("Production Movement", d.ProductionMovement));
                        dynamicFields.Add(ReviewField("Production Order Reference", d.ProductionOrderReference));
                        dynamicFields.Add(ReviewField("Production Line", d.ProductionLine));
                        dynamicFields.Add(ReviewField("Warehouse Location", d.WarehouseLocation));
                        dynamicFields.Add(ReviewField("Batch Number", d.BatchNumber));
                        dynamicFields.Add(ReviewField("Number of Rolls / Units", d.NumberOfRollsUnits));
                        dynamicFields.Add(ReviewField("Grade / GSM / Width", d.GradeGsmWidth));
                    }
                    break;
                }
                case "Return":
                {
                    var d = await _databaseService.GetWeighmentReturnDetailsAsync(selectedId);
                    if (d != null)
                    {
                        dynamicFields.Add(ReviewField("Return Type", d.ReturnType));
                        dynamicFields.Add(ReviewField("Vendor Account", d.VendorAccount));
                        dynamicFields.Add(ReviewField("Vendor Name", d.VendorName));
                        dynamicFields.Add(ReviewField("Customer Account", d.CustomerAccount));
                        dynamicFields.Add(ReviewField("Customer Name", d.CustomerName));
                        dynamicFields.Add(ReviewField("From Legal Entity", d.FromLegalEntity));
                        dynamicFields.Add(ReviewField("To Legal Entity", d.ToLegalEntity));
                        dynamicFields.Add(ReviewField("Original Slip Number", d.OriginalSlipNumber));
                        dynamicFields.Add(ReviewField("Return Reference", d.ReturnReference));
                        dynamicFields.Add(ReviewField("Return Reason", d.ReturnReason));
                        dynamicFields.Add(ReviewField("Source", d.Source));
                        dynamicFields.Add(ReviewField("Destination", d.Destination));
                    }
                    break;
                }
                case "Disposal / Waste Movement":
                {
                    var d = await _databaseService.GetWeighmentDisposalDetailsAsync(selectedId);
                    if (d != null)
                    {
                        dynamicFields.Add(ReviewField("Disposal Type", d.DisposalType));
                        dynamicFields.Add(ReviewField("Source", d.Source));
                        dynamicFields.Add(ReviewField("Disposal Destination", d.DisposalDestination));
                        dynamicFields.Add(ReviewField("Reason", d.Reason));
                        dynamicFields.Add(ReviewField("Permit / Manifest Number", d.PermitManifestNumber));
                        dynamicFields.Add(ReviewField("Authorized By", d.AuthorizedBy));
                    }
                    break;
                }
            }

            if (dynamicFields.Count == 0)
                dynamicFields.Add(ReviewField("Information", "No transaction-specific detail record is available."));

            var materialLines = await _databaseService.GetWeighmentMaterialLinesAsync(selectedId);
            if (SelectedTransactionWeighment?.WeighmentId != selectedId)
                return;

            SelectedTransactionReviewForm = form;
            ReplaceCollection(TransactionReviewCommonFields, common);
            ReplaceCollection(TransactionReviewDynamicFields, dynamicFields);
            ReplaceCollection(TransactionReviewMaterialLines, materialLines);
            StatusMessage = $"Reviewing transaction {transaction.SlipNumber}.";
        }
        catch (Exception ex)
        {
            if (SelectedTransactionWeighment?.WeighmentId == transaction.WeighmentId)
            {
                ClearTransactionReview();
                StatusMessage = "Transaction review load error: " + ex.Message;
            }
        }
    }

    private static TransactionReviewField ReviewField(string label, object? value)
    {
        string text = value switch
        {
            null => string.Empty,
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            decimal number => number.ToString("0.###"),
            double number => number.ToString("0.###"),
            bool flag => flag ? "Yes" : "No",
            _ => Convert.ToString(value) ?? string.Empty
        };
        return new TransactionReviewField { Label = label, Value = text };
    }

    private async Task LoadTransactionsAsync()
    {
        try
        {
            if (!CanAccessTransactions)
            {
                StatusMessage = "You do not have access to Transactions.";
                return;
            }

            if (TransactionTo < TransactionFrom)
            {
                StatusMessage = "Transaction To date cannot be earlier than From date.";
                return;
            }

            var rows = (await _databaseService.SearchWeighmentsAsync(TransactionFrom, TransactionTo))
                .Where(x => IsSameDataArea(x.DataAreaId, CurrentUserCompany));
            SelectedTransactionWeighment = null;
            ReplaceCollection(TransactionRows, rows);
            ApplyTransactionFilter();
            StatusMessage = $"Transactions loaded. Rows: {FilteredTransactionRows.Count} of {TransactionRows.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Transaction load error: " + ex.Message;
        }
    }

    private void ClearTransactionFilters()
    {
        TransactionTicketFilter = string.Empty;
        TransactionCompanyFilter = string.Empty;
        TransactionVehicleFilter = string.Empty;
        TransactionDriverFilter = string.Empty;
        TransactionPartyFilter = string.Empty;
        TransactionPartyTypeFilter = string.Empty;
        TransactionItemFilter = string.Empty;
        TransactionStatusFilter = string.Empty;
        ApplyTransactionFilter();
    }

    private async Task CorrectTransactionAsync()
    {
        try
        {
            if (!CanAccessCorrection || (!_currentUser.CanSubmitCorrection && !_currentUser.CanApproveRejectCorrection))
            {
                StatusMessage = "You do not have permission to access transaction corrections.";
                return;
            }

            if (SelectedTransactionWeighment == null)
            {
                StatusMessage = "Please select a completed transaction first.";
                return;
            }

            if (!string.Equals(SelectedTransactionWeighment.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "Correction workflow is available only for Completed transactions.";
                return;
            }

            SelectedMainTabIndex = 6;
            await CorrectionWorkspace.StartForTransactionAsync(SelectedTransactionWeighment);
        }
        catch (Exception ex)
        {
            StatusMessage = "Transaction correction error: " + ex.Message;
        }
    }

    private Task CancelTransactionAsync()
    {
        // Direct cancellation is intentionally blocked. Use the controlled Cancellation / Void workflow.
        StatusMessage = "Transactions cannot be cancelled directly. Use Create Cancellation / Void.";
        return Task.CompletedTask;
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
        CustomerForm = new Customer { DataAreaId = CurrentUserCompany };
    }

    private void LoadSelectedCustomerToForm()
    {
        if (SelectedCustomer == null)
            return;

        CustomerForm = new Customer
        {
            CustomerId = SelectedCustomer.CustomerId,
            DataAreaId = SelectedCustomer.DataAreaId,
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
        VendorForm = new Vendor { DataAreaId = CurrentUserCompany };
    }

    private void LoadSelectedVendorToForm()
    {
        if (SelectedVendor == null)
            return;

        VendorForm = new Vendor
        {
            VendorId = SelectedVendor.VendorId,
            DataAreaId = SelectedVendor.DataAreaId,
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

    private void OpenItemUnitConversions()
    {
        if (SelectedItemMaster == null)
        {
            StatusMessage = "Select an item first.";
            return;
        }

        var productNumber = SelectedItemMaster.ProductNumber?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(productNumber))
        {
            StatusMessage = "The selected item does not have a Product Number, so unit conversions cannot be linked.";
            return;
        }

        var window = new WeightBridgeApp.ProductUnitOfMeasureConversionWindow(
            _databaseService,
            SelectedItemMaster.ItemNumber,
            productNumber)
        {
            Owner = Application.Current?.MainWindow
        };
        window.ShowDialog();
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
        ItemMasterForm = new ItemMaster { DataAreaId = CurrentUserCompany };
    }

    private void LoadSelectedItemMasterToForm()
    {
        if (SelectedItemMaster == null)
            return;

        ItemMasterForm = new ItemMaster
        {
            ItemMasterId = SelectedItemMaster.ItemMasterId,
            DataAreaId = SelectedItemMaster.DataAreaId,
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
        WarehouseMasterForm = new WarehouseMaster { DataAreaId = CurrentUserCompany };
    }

    private void LoadSelectedWarehouseMasterToForm()
    {
        if (SelectedWarehouseMaster == null)
            return;

        WarehouseMasterForm = new WarehouseMaster
        {
            WarehouseMasterId = SelectedWarehouseMaster.WarehouseMasterId,
            DataAreaId = SelectedWarehouseMaster.DataAreaId,
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

    private void LoadSelectedUnitOfMeasureMasterToForm()
    {
        if (SelectedUnitOfMeasureMaster == null)
            return;

        UnitOfMeasureMasterForm = new UnitOfMeasureMaster
        {
            UnitOfMeasureMasterId = SelectedUnitOfMeasureMaster.UnitOfMeasureMasterId,
            symbol = SelectedUnitOfMeasureMaster.symbol,
            isbaseunit = SelectedUnitOfMeasureMaster.isbaseunit,
            issystemunit = SelectedUnitOfMeasureMaster.issystemunit,
            systemofunits = SelectedUnitOfMeasureMaster.systemofunits,
            unitofmeasureclass = SelectedUnitOfMeasureMaster.unitofmeasureclass,
            sysdatastatecode = SelectedUnitOfMeasureMaster.sysdatastatecode,
            decimalprecision = SelectedUnitOfMeasureMaster.decimalprecision,
            Id = SelectedUnitOfMeasureMaster.Id,
            SinkCreatedOn = SelectedUnitOfMeasureMaster.SinkCreatedOn,
            SinkModifiedOn = SelectedUnitOfMeasureMaster.SinkModifiedOn,
            modifieddatetime = SelectedUnitOfMeasureMaster.modifieddatetime,
            modifiedby = SelectedUnitOfMeasureMaster.modifiedby,
            modifiedtransactionid = SelectedUnitOfMeasureMaster.modifiedtransactionid,
            createddatetime = SelectedUnitOfMeasureMaster.createddatetime,
            createdby = SelectedUnitOfMeasureMaster.createdby,
            createdtransactionid = SelectedUnitOfMeasureMaster.createdtransactionid,
            dataareaid = SelectedUnitOfMeasureMaster.dataareaid,
            recversion = SelectedUnitOfMeasureMaster.recversion,
            partition = SelectedUnitOfMeasureMaster.partition,
            sysrowversion = SelectedUnitOfMeasureMaster.sysrowversion,
            recid = SelectedUnitOfMeasureMaster.recid,
            tableid = SelectedUnitOfMeasureMaster.tableid,
            versionnumber = SelectedUnitOfMeasureMaster.versionnumber,
            createdon = SelectedUnitOfMeasureMaster.createdon,
            modifiedon = SelectedUnitOfMeasureMaster.modifiedon,
            IsDelete = SelectedUnitOfMeasureMaster.IsDelete,
            PartitionId = SelectedUnitOfMeasureMaster.PartitionId
        };
        StatusMessage = $"Unit of Measure opened: {UnitOfMeasureMasterForm.symbol}";
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
        OnPropertyChanged(nameof(CanAccessCorrection));
        OnPropertyChanged(nameof(CanCreateCorrectionRequest));
        OnPropertyChanged(nameof(CanOpenSelectedCorrection));
        OnPropertyChanged(nameof(CanAccessUserManagement));
        OnPropertyChanged(nameof(CanCorrectTransactions));
        OnPropertyChanged(nameof(CanCancelTransactions));
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
        SlipNumber = string.Empty;
        SelectedTransactionTypeMaster = null;
        SelectedScenarioMaster = null;
        PurchaseDetailsForm = new WeighmentPurchaseDetails { DataAreaId = CurrentUserCompany };
        ContractCollectionDetailsForm = new WeighmentContractCollectionDetails { DataAreaId = CurrentUserCompany };
        TransferDetailsForm = new WeighmentTransferDetails { DataAreaId = CurrentUserCompany, FromLegalEntity = CurrentUserCompany };
        SalesDispatchDetailsForm = new WeighmentSalesDispatchDetails { DataAreaId = CurrentUserCompany };
        ProductionDetailsForm = new WeighmentProductionDetails { DataAreaId = CurrentUserCompany };
        ReturnDetailsForm = new WeighmentReturnDetails { DataAreaId = CurrentUserCompany, FromLegalEntity = CurrentUserCompany };
        DisposalDetailsForm = new WeighmentDisposalDetails { DataAreaId = CurrentUserCompany };
        GatePassNumber = string.Empty;
        SelectedWeighmentGatePass = null;
        ExternalReference = string.Empty;
        OperatorRemarks = string.Empty;
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
        SelectedWeighmentParty = null;
        SelectedWeighmentItem = null;
        SelectedMaterialLine = null;
        MaterialLines.Clear();
        OnPropertyChanged(nameof(IsHeaderAndLinesEditable));
        OnPropertyChanged(nameof(IsHeaderAndLinesLocked));
        OnPropertyChanged(nameof(IsPurchaseDetailsEditable));
        OnPropertyChanged(nameof(IsPurchaseDetailsReadOnly));
        OnPropertyChanged(nameof(IsPurchaseVendorSelectable));
        OnPropertyChanged(nameof(IsPurchaseRateAmountEditable));
        OnPropertyChanged(nameof(IsContractCollectionDetailsEditable));
        OnPropertyChanged(nameof(IsContractCollectionDetailsReadOnly));
        OnPropertyChanged(nameof(IsTransferDetailsEditable));
        OnPropertyChanged(nameof(IsSalesDispatchDetailsEditable));
        OnPropertyChanged(nameof(IsProductionDetailsEditable));
        OnPropertyChanged(nameof(IsReturnDetailsEditable));
        OnPropertyChanged(nameof(IsDisposalDetailsEditable));
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    private bool ValidateEntryBeforeFirstWeight()
    {
        if (!CanAccessWeighment)
        {
            StatusMessage = "You do not have access to Weighment.";
            return false;
        }

        if (!_currentUser.CanCaptureFirstWeight)
        {
            StatusMessage = "You do not have permission to capture First Weight.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CurrentUserCompany))
        {
            StatusMessage = "Company / Legal Entity is mandatory. Please update Legal Entity in Operator Master for this operator.";
            return false;
        }

        var missingFields = new List<string>();

        if (SelectedTransactionTypeMaster == null)
            missingFields.Add("Form / Transaction Type");

        if (SelectedScenarioMaster == null)
            missingFields.Add("Scenario");

        if (string.IsNullOrWhiteSpace(VehicleNo))
            missingFields.Add("Vehicle No");

        if (string.IsNullOrWhiteSpace(DriverName))
            missingFields.Add("Driver Name");

        if (MaterialLines.Count == 0)
            missingFields.Add("Material Line");

        if (IsPurchaseReceiptCollectionForm)
        {
            if (string.IsNullOrWhiteSpace(PurchaseDetailsForm.PurchaseSubtype))
                missingFields.Add("Purchase Subtype");

            if (string.IsNullOrWhiteSpace(PurchaseDetailsForm.Source))
                missingFields.Add("Source");

            if (string.IsNullOrWhiteSpace(PurchaseDetailsForm.Destination))
                missingFields.Add("Destination");

            if (!PurchaseDetailsForm.WalkInVendor && string.IsNullOrWhiteSpace(PurchaseDetailsForm.VendorAccount))
                missingFields.Add("Vendor");

            if (PurchaseDetailsForm.WalkInVendor && string.IsNullOrWhiteSpace(PurchaseDetailsForm.SupplierDriverName))
                missingFields.Add("Supplier / Driver Name");
        }

        if (IsContractCollectionForm)
        {
            if (string.IsNullOrWhiteSpace(ContractCollectionDetailsForm.VendorAccount))
                missingFields.Add("Contract Collection Vendor Account");

            if (string.IsNullOrWhiteSpace(ContractCollectionDetailsForm.ContractNumber))
                missingFields.Add("Contract Number");

            if (string.IsNullOrWhiteSpace(ContractCollectionDetailsForm.CollectionLocation))
                missingFields.Add("Collection Location");

            if (string.IsNullOrWhiteSpace(ContractCollectionDetailsForm.Destination))
                missingFields.Add("Destination");

            if (string.IsNullOrWhiteSpace(ContractCollectionDetailsForm.BillingBasis))
                missingFields.Add("Billing Basis");
        }

        if (IsTransferForm)
        {
            if (string.IsNullOrWhiteSpace(TransferDetailsForm.TransferDirection)) missingFields.Add("Transfer Direction");
            if (string.IsNullOrWhiteSpace(TransferDetailsForm.FromLegalEntity)) missingFields.Add("From Legal Entity");
            if (string.IsNullOrWhiteSpace(TransferDetailsForm.ToLegalEntity)) missingFields.Add("To Legal Entity");
            if (string.IsNullOrWhiteSpace(TransferDetailsForm.FromLocation)) missingFields.Add("From Location");
            if (string.IsNullOrWhiteSpace(TransferDetailsForm.ToLocation)) missingFields.Add("To Location");
            if (!string.IsNullOrWhiteSpace(TransferDetailsForm.FromLegalEntity) &&
                string.Equals(TransferDetailsForm.FromLegalEntity, TransferDetailsForm.ToLegalEntity, StringComparison.OrdinalIgnoreCase))
                missingFields.Add("Different From/To Legal Entity");
        }

        if (IsSalesDispatchForm)
        {
            if (string.IsNullOrWhiteSpace(SalesDispatchDetailsForm.SalesSubtype)) missingFields.Add("Sales Subtype");
            if (string.IsNullOrWhiteSpace(SalesDispatchDetailsForm.Source)) missingFields.Add("Sales Source");

            var isCashSale = string.Equals(SalesDispatchDetailsForm.SalesSubtype, "Cash", StringComparison.OrdinalIgnoreCase);
            if (isCashSale)
            {
                if (string.IsNullOrWhiteSpace(SalesDispatchDetailsForm.CustomerAccount) && string.IsNullOrWhiteSpace(SalesDispatchDetailsForm.WalkInCustomer))
                    missingFields.Add("Customer / Walk-in Customer");
                if (string.IsNullOrWhiteSpace(SalesDispatchDetailsForm.PaymentStatus))
                    missingFields.Add("Payment Status");
                if (string.Equals(SalesDispatchDetailsForm.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(SalesDispatchDetailsForm.ReceiptNumber))
                    missingFields.Add("Receipt Number");
            }
            else if (string.IsNullOrWhiteSpace(SalesDispatchDetailsForm.CustomerAccount))
            {
                missingFields.Add("Customer");
            }
        }

        if (IsProductionWeighingForm)
        {
            if (string.IsNullOrWhiteSpace(ProductionDetailsForm.ProductionMovement)) missingFields.Add("Production Movement");
            if (string.IsNullOrWhiteSpace(ProductionDetailsForm.ProductionOrderReference)) missingFields.Add("Production Order Reference");
            if (string.IsNullOrWhiteSpace(ProductionDetailsForm.WarehouseLocation)) missingFields.Add("Warehouse / Location");
            if (ProductionDetailsForm.NumberOfRollsUnits < 0) missingFields.Add("Number of Rolls / Units must be >= 0");
        }

        if (IsReturnForm)
        {
            if (string.IsNullOrWhiteSpace(ReturnDetailsForm.ReturnType)) missingFields.Add("Return Type");
            if (string.IsNullOrWhiteSpace(ReturnDetailsForm.ReturnReason)) missingFields.Add("Return Reason");
            if (string.IsNullOrWhiteSpace(ReturnDetailsForm.Source)) missingFields.Add("Return Source");

            if (string.Equals(ReturnDetailsForm.ReturnType, "Purchase Return", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(ReturnDetailsForm.VendorAccount))
                missingFields.Add("Vendor");
            if (string.Equals(ReturnDetailsForm.ReturnType, "Sales Return", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(ReturnDetailsForm.CustomerAccount))
                missingFields.Add("Customer");
            if (string.Equals(ReturnDetailsForm.ReturnType, "Intercompany Return", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(ReturnDetailsForm.FromLegalEntity)) missingFields.Add("From Legal Entity");
                if (string.IsNullOrWhiteSpace(ReturnDetailsForm.ToLegalEntity)) missingFields.Add("To Legal Entity");
                if (!string.IsNullOrWhiteSpace(ReturnDetailsForm.FromLegalEntity) && string.Equals(ReturnDetailsForm.FromLegalEntity, ReturnDetailsForm.ToLegalEntity, StringComparison.OrdinalIgnoreCase))
                    missingFields.Add("Different From/To Legal Entity");
                if (string.IsNullOrWhiteSpace(ReturnDetailsForm.Destination)) missingFields.Add("Return Destination");
            }
        }

        if (IsDisposalWasteMovementForm)
        {
            if (string.IsNullOrWhiteSpace(DisposalDetailsForm.DisposalType)) missingFields.Add("Disposal Type");
            if (string.IsNullOrWhiteSpace(DisposalDetailsForm.Source)) missingFields.Add("Disposal Source");
            if (string.IsNullOrWhiteSpace(DisposalDetailsForm.DisposalDestination)) missingFields.Add("Disposal Destination");
            if (string.IsNullOrWhiteSpace(DisposalDetailsForm.Reason)) missingFields.Add("Disposal Reason");
        }

        if (missingFields.Count > 0)
        {
            StatusMessage = "Mandatory field(s) missing: " + string.Join(", ", missingFields) + ".";
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
        // Party selection is handled through the search lookup window. Do not pre-load
        // Customers/Vendors into an editable ComboBox because those masters can be very large.
        FilteredParties.Clear();
        SelectedWeighmentParty = null;
        PartyAccount = string.Empty;
        PartyName = string.Empty;
    }

    private static string BuildMergedDisplay(string code, string name)
    {
        var cleanCode = code?.Trim() ?? string.Empty;
        var cleanName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cleanCode))
            return cleanName;

        if (string.IsNullOrWhiteSpace(cleanName))
            return cleanCode;

        return cleanCode + " - " + cleanName;
    }

    private Task OpenVehicleLookupAsync()
    {
        var lookupWindow = new WeightBridgeApp.VehicleLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedVehicle != null)
        {
            VehicleNo = lookupWindow.SelectedVehicle.VehicleNo;
            StatusMessage = $"Selected Vehicle: {VehicleNo}";
        }

        return Task.CompletedTask;
    }

    private Task OpenDriverLookupAsync()
    {
        var lookupWindow = new WeightBridgeApp.DriverLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedDriver != null)
        {
            DriverName = lookupWindow.SelectedDriver.DriverName;
            StatusMessage = $"Selected Driver: {DriverName}";
        }

        return Task.CompletedTask;
    }

    private Task OpenPartyLookupAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedPartyType))
        {
            StatusMessage = "Please select Party Type first.";
            return Task.CompletedTask;
        }

        var lookupWindow = new WeightBridgeApp.PartyLookupWindow(_databaseService, CurrentUserCompany, SelectedPartyType)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedParty != null)
        {
            SelectedWeighmentParty = lookupWindow.SelectedParty;
            PartyAccount = lookupWindow.SelectedParty.PartyAccount;
            PartyName = lookupWindow.SelectedParty.PartyName;
            StatusMessage = $"Selected {SelectedPartyType}: {PartyAccount} - {PartyName}";
        }

        return Task.CompletedTask;
    }

    private Task OpenItemLookupAsync()
    {
        var lookupWindow = new WeightBridgeApp.ItemLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedItemMaster != null)
        {
            if (MaterialLines.Count == 0)
            {
                var selectedItem = lookupWindow.SelectedItemMaster;
                var uom = !string.IsNullOrWhiteSpace(selectedItem.PurchaseUnit)
                    ? selectedItem.PurchaseUnit
                    : !string.IsNullOrWhiteSpace(selectedItem.SellUnit)
                        ? selectedItem.SellUnit
                        : selectedItem.CostUnit;
                MaterialLines.Add(new WeighmentMaterialLine
                {
                    DataAreaId = CurrentUserCompany,
                    LineNo = 1,
                    ItemMasterId = selectedItem.ItemMasterId,
                    ItemNumber = selectedItem.ItemNumber,
                    ItemName = selectedItem.ProductName,
                    ExpectedQty = 0,
                    Uom = uom,
                    CreatedBy = CurrentUsername,
                    CreatedAt = DateTime.Now
                });
            }
            UpdatePrimaryItemFromMaterialLines();
            StatusMessage = $"Selected Item: {ItemNumber} - {ItemName}";
        }

        return Task.CompletedTask;
    }


    private Task AddMaterialLineAsync()
    {
        if (!IsHeaderAndLinesEditable)
        {
            StatusMessage = "Material lines cannot be changed after First Weight is saved.";
            return Task.CompletedTask;
        }

        var lookupWindow = new WeightBridgeApp.ItemLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedItemMaster != null)
        {
            var selectedItem = lookupWindow.SelectedItemMaster;
            var uom = !string.IsNullOrWhiteSpace(selectedItem.PurchaseUnit)
                ? selectedItem.PurchaseUnit
                : !string.IsNullOrWhiteSpace(selectedItem.SellUnit)
                    ? selectedItem.SellUnit
                    : selectedItem.CostUnit;

            MaterialLines.Add(new WeighmentMaterialLine
            {
                DataAreaId = CurrentUserCompany,
                LineNo = MaterialLines.Count + 1,
                ItemMasterId = selectedItem.ItemMasterId,
                ItemNumber = selectedItem.ItemNumber,
                ItemName = selectedItem.ProductName,
                ExpectedQty = 0,
                Uom = uom,
                Remarks = string.Empty,
                CreatedBy = CurrentUsername,
                CreatedAt = DateTime.Now
            });

            UpdatePrimaryItemFromMaterialLines();
            StatusMessage = $"Material line added: {selectedItem.ItemNumber} - {selectedItem.ProductName}";
        }

        return Task.CompletedTask;
    }

    private void DeleteMaterialLine()
    {
        if (!IsHeaderAndLinesEditable)
        {
            StatusMessage = "Material lines cannot be changed after First Weight is saved.";
            return;
        }

        if (SelectedMaterialLine == null)
        {
            StatusMessage = "Please select a material line to delete.";
            return;
        }

        MaterialLines.Remove(SelectedMaterialLine);
        SelectedMaterialLine = null;
        ResequenceMaterialLines();
        UpdatePrimaryItemFromMaterialLines();
        StatusMessage = "Material line removed.";
    }

    private void ResequenceMaterialLines()
    {
        var lineNo = 1;
        foreach (var line in MaterialLines)
            line.LineNo = lineNo++;
    }

    private void UpdatePrimaryItemFromMaterialLines()
    {
        var firstLine = MaterialLines.FirstOrDefault();
        SelectedWeighmentItem = firstLine?.ItemMasterId == null
            ? null
            : ItemMasters.FirstOrDefault(x => x.ItemMasterId == firstLine.ItemMasterId.Value);
        ItemNumber = firstLine?.ItemNumber ?? string.Empty;
        ItemName = firstLine?.ItemName ?? string.Empty;
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

            VehicleMasterForm.DataAreaId = CurrentUserCompany;
            VehicleMasterForm.LegalEntity = CurrentUserCompany;
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
        VehicleMasterForm = new Vehicle { DataAreaId = CurrentUserCompany, Status = "Active" };
    }

    private void LoadSelectedVehicleMasterToForm()
    {
        if (SelectedVehicleMaster == null)
            return;

        VehicleMasterForm = new Vehicle
        {
            VehicleId = SelectedVehicleMaster.VehicleId,
            DataAreaId = SelectedVehicleMaster.DataAreaId,
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
            Status = SelectedVehicleMaster.Status
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

            DriverMasterForm.DataAreaId = CurrentUserCompany;
            DriverMasterForm.LegalEntity = CurrentUserCompany;
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
        DriverMasterForm = new Driver { DataAreaId = CurrentUserCompany, Status = "Active", EffectiveFrom = DateTime.Today };
    }

    private void LoadSelectedDriverMasterToForm()
    {
        if (SelectedDriverMaster == null)
            return;

        DriverMasterForm = new Driver
        {
            DriverId = SelectedDriverMaster.DriverId,
            DataAreaId = SelectedDriverMaster.DataAreaId,
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
            Remarks = SelectedDriverMaster.Remarks
        };
    }



    private void SelectSettingsWeighbridgeFromSavedSettings()
    {
        var selectedCode = Settings.SelectedWeighbridgeCode;
        SelectedSettingsWeighbridge = ActiveWeighbridgeMasters.FirstOrDefault(x =>
            string.Equals(x.WeighbridgeCode, selectedCode, StringComparison.OrdinalIgnoreCase));

        SelectedSettingsWeighbridge ??= ActiveWeighbridgeMasters.FirstOrDefault();
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

            WeighbridgeMasterForm.DataAreaId = CurrentUserCompany;
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
        ClearGatePassForm();

        SelectedWeighbridgeMaster = null;
        WeighbridgeMasterForm = new WeighbridgeMaster
        {
            DataAreaId = CurrentUserCompany,
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
            EffectiveFrom = DateTime.Today
        };
    }

    private void LoadSelectedWeighbridgeMasterToForm()
    {
        if (SelectedWeighbridgeMaster == null)
            return;

        WeighbridgeMasterForm = new WeighbridgeMaster
        {
            WeighbridgeId = SelectedWeighbridgeMaster.WeighbridgeId,
            DataAreaId = SelectedWeighbridgeMaster.DataAreaId,
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

            NormalizeOperatorLegalEntityAssignmentsForSave();

            // Workflow action permissions require access to their corresponding screen.
            if (OperatorMasterForm.CanSubmitCancellationVoid || OperatorMasterForm.CanApproveRejectCancellationVoid)
                OperatorMasterForm.CanAccessCancellationVoid = true;
            if (OperatorMasterForm.CanSubmitCorrection || OperatorMasterForm.CanApproveRejectCorrection || OperatorMasterForm.CanCorrectWeight)
                OperatorMasterForm.CanAccessCorrection = true;

            await _databaseService.SaveOperatorMasterAsync(OperatorMasterForm);
            var savedOperator = await _databaseService.GetOperatorByUsernameAsync(OperatorMasterForm.Username);
            if (savedOperator == null)
                throw new InvalidOperationException("Operator was saved but could not be reloaded.");

            await _databaseService.SaveOperatorLegalEntitiesAsync(savedOperator.OperatorId, OperatorLegalEntityAssignments);
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
        OperatorLegalEntityAssignments.Clear();
        var defaultLegalEntity = AllowedLegalEntities.FirstOrDefault(x => IsSameDataArea(x.DataAreaId, CurrentUserCompany));
        if (defaultLegalEntity != null)
        {
            OperatorLegalEntityAssignments.Add(new OperatorLegalEntityAssignment
            {
                DataAreaId = defaultLegalEntity.DataAreaId,
                LegalEntityName = defaultLegalEntity.LegalEntityName,
                IsDefault = true
            });
        }
        OperatorMasterForm = new OperatorMaster
        {
            DataAreaId = CurrentUserCompany,
            CanAccessWeighment = true,
            CanAccessReports = true,
            CanAccessTransactions = true,
            CanCaptureFirstWeight = true,
            CanCaptureSecondWeight = true,
            Status = "Active",
            EffectiveFrom = DateTime.Today
        };
    }

    private void LoadSelectedOperatorMasterToForm()
    {
        if (SelectedOperatorMaster == null)
            return;

        OperatorMasterForm = new OperatorMaster
        {
            OperatorId = SelectedOperatorMaster.OperatorId,
            DataAreaId = SelectedOperatorMaster.DataAreaId,
            EmployeeId = SelectedOperatorMaster.EmployeeId,
            OperatorName = SelectedOperatorMaster.OperatorName,
            Username = SelectedOperatorMaster.Username,
            PasswordHash = SelectedOperatorMaster.PasswordHash,
            PasswordSalt = SelectedOperatorMaster.PasswordSalt,
            Password = string.Empty,
            ConfirmPassword = string.Empty,
            Email = SelectedOperatorMaster.Email,
            MobileNumber = SelectedOperatorMaster.MobileNumber,
            Designation = SelectedOperatorMaster.Designation,
            Department = SelectedOperatorMaster.Department,
            DefaultWeighbridge = SelectedOperatorMaster.DefaultWeighbridge,
            AssignedWeighbridges = SelectedOperatorMaster.AssignedWeighbridges,
            DefaultShift = SelectedOperatorMaster.DefaultShift,
            Role = SelectedOperatorMaster.Role,
            PermissionProfile = SelectedOperatorMaster.PermissionProfile,
            CanAccessWeighment = SelectedOperatorMaster.CanAccessWeighment,
            CanAccessMasters = SelectedOperatorMaster.CanAccessMasters,
            CanAccessReports = SelectedOperatorMaster.CanAccessReports,
            CanAccessTransactions = SelectedOperatorMaster.CanAccessTransactions,
            CanAccessGatePass = SelectedOperatorMaster.CanAccessGatePass,
            CanAccessCancellationVoid = SelectedOperatorMaster.CanAccessCancellationVoid,
            CanAccessCorrection = SelectedOperatorMaster.CanAccessCorrection,
            CanAccessSettings = SelectedOperatorMaster.CanAccessSettings,
            CanCaptureFirstWeight = SelectedOperatorMaster.CanCaptureFirstWeight,
            CanCaptureSecondWeight = SelectedOperatorMaster.CanCaptureSecondWeight,
            CanCorrectTransactions = SelectedOperatorMaster.CanCorrectTransactions,
            CanSubmitCorrection = SelectedOperatorMaster.CanSubmitCorrection,
            CanApproveRejectCorrection = SelectedOperatorMaster.CanApproveRejectCorrection,
            CanCorrectWeight = SelectedOperatorMaster.CanCorrectWeight,
            CanSubmitCancellationVoid = SelectedOperatorMaster.CanSubmitCancellationVoid,
            CanApproveRejectCancellationVoid = SelectedOperatorMaster.CanApproveRejectCancellationVoid,
            LastLogin = SelectedOperatorMaster.LastLogin,
            Status = SelectedOperatorMaster.Status,
            EffectiveFrom = SelectedOperatorMaster.EffectiveFrom,
            Remarks = SelectedOperatorMaster.Remarks
        };
        _ = LoadOperatorLegalEntitiesForFormAsync(OperatorMasterForm.OperatorId);
    }


    private async Task SaveLegalEntityAsync()
    {
        try
        {
            if (!CanAccessMasters)
            {
                StatusMessage = "You do not have access to Masters.";
                return;
            }

            await _databaseService.SaveLegalEntityAsync(LegalEntityMasterForm);
            await LoadAllowedLegalEntitiesAsync();
            await LoadMastersAsync();
            StatusMessage = "Legal Entity saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Legal Entity save error: " + ex.Message;
        }
    }

    private void ClearLegalEntityForm()
    {
        SelectedLegalEntityMaster = null;
        LegalEntityMasterForm = new LegalEntityMaster();
    }

    private void LoadSelectedLegalEntityToForm()
    {
        if (SelectedLegalEntityMaster == null)
            return;

        LegalEntityMasterForm = new LegalEntityMaster
        {
            LegalEntityId = SelectedLegalEntityMaster.LegalEntityId,
            DataAreaId = SelectedLegalEntityMaster.DataAreaId,
            LegalEntityName = SelectedLegalEntityMaster.LegalEntityName,
            Remarks = SelectedLegalEntityMaster.Remarks
        };
    }

    private async Task LoadOperatorLegalEntitiesForFormAsync(int operatorId)
    {
        OperatorLegalEntityAssignments.Clear();
        if (operatorId <= 0)
            return;

        var lines = await _databaseService.GetOperatorLegalEntitiesAsync(operatorId);
        foreach (var line in lines)
            OperatorLegalEntityAssignments.Add(line);
    }

    private void AddOperatorLegalEntityToForm()
    {
        var legalEntityToAdd = SelectedOperatorLegalEntityToAdd
            ?? LegalEntities.FirstOrDefault(x => !OperatorLegalEntityAssignments.Any(a => IsSameDataArea(a.DataAreaId, x.DataAreaId)));

        if (legalEntityToAdd == null)
        {
            StatusMessage = "No Legal Entity is available to assign.";
            return;
        }

        if (OperatorLegalEntityAssignments.Any(x => IsSameDataArea(x.DataAreaId, legalEntityToAdd.DataAreaId)))
        {
            StatusMessage = "Legal Entity is already assigned to this operator.";
            return;
        }

        var isDefault = OperatorLegalEntityAssignments.Count == 0;
        OperatorLegalEntityAssignments.Add(new OperatorLegalEntityAssignment
        {
            OperatorId = OperatorMasterForm.OperatorId,
            DataAreaId = legalEntityToAdd.DataAreaId,
            LegalEntityName = legalEntityToAdd.LegalEntityName,
            IsDefault = isDefault
        });

        if (isDefault)
            OperatorMasterForm.DataAreaId = legalEntityToAdd.DataAreaId;

        StatusMessage = "Legal Entity assigned to operator.";
    }

    private void NormalizeOperatorLegalEntityAssignmentsForSave()
    {
        if (OperatorLegalEntityAssignments.Count == 0)
            throw new InvalidOperationException("At least one Legal Entity must be assigned to the operator.");

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var assignment in OperatorLegalEntityAssignments)
        {
            assignment.DataAreaId = assignment.DataAreaId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(assignment.DataAreaId))
                throw new InvalidOperationException("Legal Entity is mandatory in Assigned Legal Entities grid.");

            if (!seen.Add(assignment.DataAreaId))
                throw new InvalidOperationException($"Legal Entity {assignment.DataAreaId} is assigned more than once.");

            var master = LegalEntities.FirstOrDefault(x => IsSameDataArea(x.DataAreaId, assignment.DataAreaId));
            if (master == null)
                throw new InvalidOperationException($"Legal Entity {assignment.DataAreaId} is not available in Legal Entity Master.");

            assignment.LegalEntityName = master.LegalEntityName;
        }

        if (!OperatorLegalEntityAssignments.Any(x => x.IsDefault))
            OperatorLegalEntityAssignments[0].IsDefault = true;

        var defaultLine = OperatorLegalEntityAssignments.First(x => x.IsDefault);
        foreach (var assignment in OperatorLegalEntityAssignments.Where(x => !ReferenceEquals(x, defaultLine)))
            assignment.IsDefault = false;

        OperatorMasterForm.DataAreaId = defaultLine.DataAreaId;
        ReplaceCollection(OperatorLegalEntityAssignments, OperatorLegalEntityAssignments.ToList());
    }

    private void RemoveOperatorLegalEntityFromForm()
    {
        if (SelectedOperatorLegalEntityAssignment == null)
        {
            StatusMessage = "Please select assigned Legal Entity to remove.";
            return;
        }

        var removedDefault = SelectedOperatorLegalEntityAssignment.IsDefault;
        OperatorLegalEntityAssignments.Remove(SelectedOperatorLegalEntityAssignment);
        SelectedOperatorLegalEntityAssignment = null;

        if (removedDefault && OperatorLegalEntityAssignments.Count > 0)
        {
            OperatorLegalEntityAssignments[0].IsDefault = true;
            OperatorMasterForm.DataAreaId = OperatorLegalEntityAssignments[0].DataAreaId;
        }

        StatusMessage = "Assigned Legal Entity removed.";
    }

    private void SetDefaultOperatorLegalEntity()
    {
        if (SelectedOperatorLegalEntityAssignment == null)
        {
            StatusMessage = "Please select assigned Legal Entity first.";
            return;
        }

        foreach (var line in OperatorLegalEntityAssignments)
            line.IsDefault = false;

        SelectedOperatorLegalEntityAssignment.IsDefault = true;
        OperatorMasterForm.DataAreaId = SelectedOperatorLegalEntityAssignment.DataAreaId;
        ReplaceCollection(OperatorLegalEntityAssignments, OperatorLegalEntityAssignments.ToList());
        StatusMessage = "Default Legal Entity updated.";
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
            MatchesFilter(x.SlipNumber, ReportTicketFilter) &&
            MatchesFilter(x.CompanyName, ReportCompanyFilter) &&
            MatchesFilter(x.VehicleNo, ReportVehicleFilter) &&
            MatchesFilter(x.DriverName, ReportDriverFilter) &&
            (MatchesFilter(x.ItemNumber, ReportItemFilter) || MatchesFilter(x.ItemName, ReportItemFilter) || MatchesFilter(x.MaterialName, ReportItemFilter)) &&
            MatchesFilter(x.Status, ReportStatusFilter)));
    }

    private void ApplyTransactionFilter()
    {
        ReplaceCollection(FilteredTransactionRows, TransactionRows.Where(x =>
            MatchesFilter(x.SlipNumber, TransactionTicketFilter) &&
            MatchesFilter(x.CompanyName, TransactionCompanyFilter) &&
            MatchesFilter(x.VehicleNo, TransactionVehicleFilter) &&
            MatchesFilter(x.DriverName, TransactionDriverFilter) &&
            (MatchesFilter(x.ItemNumber, TransactionItemFilter) || MatchesFilter(x.ItemName, TransactionItemFilter) || MatchesFilter(x.MaterialName, TransactionItemFilter)) &&
            MatchesFilter(x.Status, TransactionStatusFilter)));
    }

    private void ApplyMasterFilters()
    {
        ApplyLegalEntityFilter();
        ApplyCustomerFilter();
        ApplyVendorFilter();
        ApplyItemMasterFilter();
        ApplyWarehouseFilter();
        ApplyVehicleFilter();
        ApplyDriverFilter();
        ApplyWeighbridgeFilter();
        ApplyOperatorFilter();
    }

    private void ApplyLegalEntityFilter()
    {
        ReplaceCollection(FilteredLegalEntities, LegalEntities.Where(x =>
            MatchesFilter(x.DataAreaId, LegalEntityFilter) ||
            MatchesFilter(x.LegalEntityName, LegalEntityFilter) ||
            MatchesFilter(x.Remarks, LegalEntityFilter)));
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



    public void EnsureMasterTabReady(string? masterHeader)
    {
        if (string.IsNullOrWhiteSpace(masterHeader))
            return;

        if (masterHeader.Contains("Scenario", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(ScenarioMasterForm.DataAreaId))
            ScenarioMasterForm = new ScenarioMaster { DataAreaId = CurrentUserCompany };

        if (masterHeader.Contains("Service Charge", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(ServiceChargeMasterForm.DataAreaId) || !IsSameDataArea(ServiceChargeMasterForm.DataAreaId, CurrentUserCompany))
                ServiceChargeMasterForm = new ServiceChargeMaster { DataAreaId = CurrentUserCompany, Currency = "PKR" };
        }

        if (masterHeader.Contains("Location", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(LocationMasterForm.DataAreaId))
            LocationMasterForm = new LocationMaster { DataAreaId = CurrentUserCompany, Status = string.IsNullOrWhiteSpace(LocationMasterForm.Status) ? "Active" : LocationMasterForm.Status };

        if (masterHeader.Contains("Transaction Type", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(TransactionTypeMasterForm.Form))
            TransactionTypeMasterForm = new TransactionTypeMaster { Type = TransactionTypeMasterForm.Type, Description = TransactionTypeMasterForm.Description, Form = TransactionFormValues.FirstOrDefault() ?? string.Empty };

        OnPropertyChanged(nameof(ScenarioMasterForm));
        OnPropertyChanged(nameof(ServiceChargeMasterForm));
        OnPropertyChanged(nameof(LocationMasterForm));
        OnPropertyChanged(nameof(TransactionTypeMasterForm));
        StatusMessage = $"{masterHeader} opened.";
    }

    private async Task SaveShiftMasterAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ShiftMasterForm.Code))
            {
                StatusMessage = "Shift Master save error: Code is required.";
                return;
            }

            await _databaseService.SaveShiftMasterAsync(ShiftMasterForm);
            ClearShiftMasterForm();
            await LoadMastersAsync();
            StatusMessage = "Shift Master saved.";
        }
        catch (Exception ex) { StatusMessage = "Shift Master save error: " + ex.Message; }
    }

    private void ClearShiftMasterForm() { SelectedShiftMaster = null; ShiftMasterForm = new ShiftMaster(); }

    private async Task SaveScenarioMasterAsync()
    {
        try { if (string.IsNullOrWhiteSpace(ScenarioMasterForm.DataAreaId)) ScenarioMasterForm.DataAreaId = CurrentUserCompany; await _databaseService.SaveScenarioMasterAsync(ScenarioMasterForm); ClearScenarioMasterForm(); await LoadMastersAsync(); StatusMessage = "Scenario Master saved."; }
        catch (Exception ex) { StatusMessage = "Scenario Master save error: " + ex.Message; }
    }

    private void ClearScenarioMasterForm() { SelectedScenarioConfig = null; ScenarioMasterForm = new ScenarioMaster { DataAreaId = CurrentUserCompany }; }

    private async Task SaveReasonMasterAsync()
    {
        try { await _databaseService.SaveReasonMasterAsync(ReasonMasterForm); ClearReasonMasterForm(); await LoadMastersAsync(); StatusMessage = "Reason Master saved."; }
        catch (Exception ex) { StatusMessage = "Reason Master save error: " + ex.Message; }
    }

    private void ClearReasonMasterForm() { SelectedReasonMaster = null; ReasonMasterForm = new ReasonMaster(); }

    private async Task SaveContractMasterAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ContractMasterForm.ContractNumber))
            {
                StatusMessage = "Contract Master save error: Contract Number is required.";
                return;
            }

            await _databaseService.SaveContractMasterAsync(ContractMasterForm);
            ClearContractMasterForm();
            await LoadMastersAsync();
            StatusMessage = "Contract Master saved.";
        }
        catch (Exception ex) { StatusMessage = "Contract Master save error: " + ex.Message; }
    }

    private void ClearContractMasterForm() { SelectedContractMaster = null; ContractMasterForm = new ContractMaster(); }

    private async Task SaveToleranceMasterAsync()
    {
        try { await _databaseService.SaveToleranceMasterAsync(ToleranceMasterForm); ClearToleranceMasterForm(); await LoadMastersAsync(); StatusMessage = "Tolerance Master saved."; }
        catch (Exception ex) { StatusMessage = "Tolerance Master save error: " + ex.Message; }
    }

    private void ClearToleranceMasterForm() { SelectedToleranceMaster = null; ToleranceMasterForm = new ToleranceMaster(); }

    private async Task SaveServiceChargeMasterAsync()
    {
        try
        {
            ServiceChargeMasterForm.DataAreaId = CurrentUserCompany;

            if (string.IsNullOrWhiteSpace(ServiceChargeMasterForm.ServiceMode))
            {
                StatusMessage = "Service Charge Master save error: Service Mode is required.";
                return;
            }

            await _databaseService.SaveServiceChargeMasterAsync(ServiceChargeMasterForm);
            ClearServiceChargeMasterForm();
            await LoadMastersAsync();
            StatusMessage = "Service Charge Master saved.";
        }
        catch (Exception ex) { StatusMessage = "Service Charge Master save error: " + ex.Message; }
    }

    private void ClearServiceChargeMasterForm() { SelectedServiceChargeMaster = null; ServiceChargeMasterForm = new ServiceChargeMaster { DataAreaId = CurrentUserCompany, Currency = "PKR" }; }

    private async Task SaveTransactionTypeMasterAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TransactionTypeMasterForm.Type))
            {
                StatusMessage = "Transaction Type Master save error: Type is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(TransactionTypeMasterForm.Form))
                TransactionTypeMasterForm.Form = TransactionFormValues.FirstOrDefault() ?? string.Empty;

            await _databaseService.SaveTransactionTypeMasterAsync(TransactionTypeMasterForm);
            ClearTransactionTypeMasterForm();
            await LoadMastersAsync();
            StatusMessage = "Transaction Type Master saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Transaction Type Master save error: " + ex.Message;
        }
    }

    private void ClearTransactionTypeMasterForm()
    {
        SelectedTransactionTypeConfig = null;
        TransactionTypeMasterForm = new TransactionTypeMaster { Form = TransactionFormValues.FirstOrDefault() ?? string.Empty };
    }

    private async Task SaveLocationMasterAsync()
    {
        try
        {
            LocationMasterForm.DataAreaId = CurrentUserCompany;

            await _databaseService.SaveLocationMasterAsync(LocationMasterForm);
            ClearLocationMasterForm();
            await LoadMastersAsync();
            StatusMessage = "Location Master saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Location Master save error: " + ex.Message;
        }
    }

    private void ClearLocationMasterForm() { SelectedLocationMaster = null; LocationMasterForm = new LocationMaster { DataAreaId = CurrentUserCompany, Status = "Active" }; }

    public void ApplyLocationWarehouse(WarehouseMaster? warehouse)
    {
        if (warehouse == null)
            return;

        LocationMasterForm = new LocationMaster
        {
            LocationMasterId = LocationMasterForm.LocationMasterId,
            DataAreaId = CurrentUserCompany,
            LocationCode = LocationMasterForm.LocationCode,
            LocationName = LocationMasterForm.LocationName,
            LocationType = LocationMasterForm.LocationType,
            Warehouse = warehouse.Warehouse,
            Site = warehouse.Site,
            Status = LocationMasterForm.Status
        };
    }

    private Task OpenPurchaseVendorLookupAsync()
    {
        if (!IsPurchaseVendorSelectable)
            return Task.CompletedTask;

        var lookupWindow = new WeightBridgeApp.PartyLookupWindow(_databaseService, CurrentUserCompany, "Vendor")
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedParty != null)
        {
            PurchaseDetailsForm.VendorAccount = lookupWindow.SelectedParty.PartyAccount;
            PurchaseDetailsForm.VendorName = lookupWindow.SelectedParty.PartyName;
            OnPropertyChanged(nameof(PurchaseDetailsForm));
            OnPropertyChanged(nameof(PurchaseVendorDisplay));
            StatusMessage = $"Selected purchase vendor: {PurchaseVendorDisplay}";
        }

        return Task.CompletedTask;
    }

    private Task OpenPurchaseSourceLookupAsync()
    {
        var lookupWindow = new WeightBridgeApp.LocationLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedLocation != null)
        {
            PurchaseDetailsForm.Source = BuildMergedDisplay(lookupWindow.SelectedLocation.LocationCode, lookupWindow.SelectedLocation.LocationName);
            OnPropertyChanged(nameof(PurchaseDetailsForm));
            OnPropertyChanged(nameof(PurchaseSourceDisplay));
            StatusMessage = $"Selected source: {PurchaseDetailsForm.Source}";
        }

        return Task.CompletedTask;
    }

    private Task OpenPurchaseDestinationLookupAsync()
    {
        var lookupWindow = new WeightBridgeApp.LocationLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedLocation != null)
        {
            PurchaseDetailsForm.Destination = BuildMergedDisplay(lookupWindow.SelectedLocation.LocationCode, lookupWindow.SelectedLocation.LocationName);
            OnPropertyChanged(nameof(PurchaseDetailsForm));
            OnPropertyChanged(nameof(PurchaseDestinationDisplay));
            StatusMessage = $"Selected destination: {PurchaseDetailsForm.Destination}";
        }

        return Task.CompletedTask;
    }


    private Task OpenContractCollectionVendorLookupAsync()
    {
        if (!IsContractCollectionDetailsEditable)
            return Task.CompletedTask;

        var lookupWindow = new WeightBridgeApp.PartyLookupWindow(_databaseService, CurrentUserCompany, "Vendor")
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedParty != null)
        {
            ContractCollectionDetailsForm.VendorAccount = lookupWindow.SelectedParty.PartyAccount;
            ContractCollectionDetailsForm.VendorName = lookupWindow.SelectedParty.PartyName;
            OnPropertyChanged(nameof(ContractCollectionDetailsForm));
            RaiseContractCollectionDetailsDependentProperties();
            StatusMessage = $"Selected contract vendor: {ContractCollectionVendorDisplay}";
        }

        return Task.CompletedTask;
    }

    private Task OpenContractCollectionInvoiceAccountLookupAsync()
    {
        if (!IsContractCollectionDetailsEditable)
            return Task.CompletedTask;

        var lookupWindow = new WeightBridgeApp.PartyLookupWindow(_databaseService, CurrentUserCompany, "Customer")
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedParty != null)
        {
            ContractCollectionDetailsForm.InvoiceAccount = lookupWindow.SelectedParty.PartyAccount;
            ContractCollectionDetailsForm.InvoiceAccountName = lookupWindow.SelectedParty.PartyName;
            OnPropertyChanged(nameof(ContractCollectionDetailsForm));
            RaiseContractCollectionDetailsDependentProperties();
            StatusMessage = $"Selected invoice account: {ContractCollectionInvoiceAccountDisplay}";
        }

        return Task.CompletedTask;
    }

    private Task OpenContractCollectionContractLookupAsync()
    {
        if (!IsContractCollectionDetailsEditable)
            return Task.CompletedTask;

        var lookupWindow = new WeightBridgeApp.ContractLookupWindow(_databaseService)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedContract != null)
        {
            ContractCollectionDetailsForm.ContractNumber = lookupWindow.SelectedContract.ContractNumber;
            ContractCollectionDetailsForm.BillingBasis = lookupWindow.SelectedContract.BillingBasis;
            OnPropertyChanged(nameof(ContractCollectionDetailsForm));
            RaiseContractCollectionDetailsDependentProperties();
            StatusMessage = $"Selected contract: {ContractCollectionDetailsForm.ContractNumber}";
        }

        return Task.CompletedTask;
    }

    private Task OpenContractCollectionLocationLookupAsync()
    {
        if (!IsContractCollectionDetailsEditable)
            return Task.CompletedTask;

        var lookupWindow = new WeightBridgeApp.LocationLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedLocation != null)
        {
            ContractCollectionDetailsForm.CollectionLocation = BuildMergedDisplay(lookupWindow.SelectedLocation.LocationCode, lookupWindow.SelectedLocation.LocationName);
            OnPropertyChanged(nameof(ContractCollectionDetailsForm));
            RaiseContractCollectionDetailsDependentProperties();
            StatusMessage = $"Selected collection location: {ContractCollectionDetailsForm.CollectionLocation}";
        }

        return Task.CompletedTask;
    }

    private Task OpenContractCollectionDestinationLookupAsync()
    {
        if (!IsContractCollectionDetailsEditable)
            return Task.CompletedTask;

        var lookupWindow = new WeightBridgeApp.LocationLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedLocation != null)
        {
            ContractCollectionDetailsForm.Destination = BuildMergedDisplay(lookupWindow.SelectedLocation.LocationCode, lookupWindow.SelectedLocation.LocationName);
            OnPropertyChanged(nameof(ContractCollectionDetailsForm));
            RaiseContractCollectionDetailsDependentProperties();
            StatusMessage = $"Selected contract destination: {ContractCollectionDetailsForm.Destination}";
        }

        return Task.CompletedTask;
    }


    private Task OpenDetailLocationLookupAsync(string? dataAreaId, Action<string> applySelection, string label)
    {
        var effectiveDataAreaId = string.IsNullOrWhiteSpace(dataAreaId) ? CurrentUserCompany : dataAreaId.Trim();
        var lookupWindow = new WeightBridgeApp.LocationLookupWindow(_databaseService, effectiveDataAreaId)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedLocation != null)
        {
            applySelection(BuildMergedDisplay(lookupWindow.SelectedLocation.LocationCode, lookupWindow.SelectedLocation.LocationName));
            StatusMessage = $"Selected {label}: {lookupWindow.SelectedLocation.LocationCode} - {lookupWindow.SelectedLocation.LocationName}";
        }
        return Task.CompletedTask;
    }

    private Task OpenTransferFromLocationLookupAsync() =>
        OpenDetailLocationLookupAsync(TransferDetailsForm.FromLegalEntity, x => TransferDetailsForm.FromLocation = x, "from location");

    private Task OpenTransferToLocationLookupAsync() =>
        OpenDetailLocationLookupAsync(TransferDetailsForm.ToLegalEntity, x => TransferDetailsForm.ToLocation = x, "to location");

    private Task OpenSalesCustomerLookupAsync()
    {
        var lookupWindow = new WeightBridgeApp.PartyLookupWindow(_databaseService, CurrentUserCompany, "Customer")
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedParty != null)
        {
            SalesDispatchDetailsForm.CustomerAccount = lookupWindow.SelectedParty.PartyAccount;
            SalesDispatchDetailsForm.CustomerName = lookupWindow.SelectedParty.PartyName;
            StatusMessage = $"Selected sales customer: {SalesDispatchDetailsForm.CustomerAccount} - {SalesDispatchDetailsForm.CustomerName}";
        }
        return Task.CompletedTask;
    }

    private Task OpenSalesSourceLookupAsync() =>
        OpenDetailLocationLookupAsync(CurrentUserCompany, x => SalesDispatchDetailsForm.Source = x, "sales source");

    private Task OpenProductionWarehouseLocationLookupAsync() =>
        OpenDetailLocationLookupAsync(CurrentUserCompany, x => ProductionDetailsForm.WarehouseLocation = x, "warehouse / location");

    private Task OpenReturnVendorLookupAsync()
    {
        var lookupWindow = new WeightBridgeApp.PartyLookupWindow(_databaseService, CurrentUserCompany, "Vendor")
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedParty != null)
        {
            ReturnDetailsForm.VendorAccount = lookupWindow.SelectedParty.PartyAccount;
            ReturnDetailsForm.VendorName = lookupWindow.SelectedParty.PartyName;
            StatusMessage = $"Selected return vendor: {ReturnDetailsForm.VendorAccount} - {ReturnDetailsForm.VendorName}";
        }
        return Task.CompletedTask;
    }

    private Task OpenReturnCustomerLookupAsync()
    {
        var lookupWindow = new WeightBridgeApp.PartyLookupWindow(_databaseService, CurrentUserCompany, "Customer")
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedParty != null)
        {
            ReturnDetailsForm.CustomerAccount = lookupWindow.SelectedParty.PartyAccount;
            ReturnDetailsForm.CustomerName = lookupWindow.SelectedParty.PartyName;
            StatusMessage = $"Selected return customer: {ReturnDetailsForm.CustomerAccount} - {ReturnDetailsForm.CustomerName}";
        }
        return Task.CompletedTask;
    }

    private Task OpenReturnSourceLookupAsync() =>
        OpenDetailLocationLookupAsync(ReturnDetailsForm.FromLegalEntity, x => ReturnDetailsForm.Source = x, "return source");

    private Task OpenReturnDestinationLookupAsync() =>
        OpenDetailLocationLookupAsync(ReturnDetailsForm.ToLegalEntity, x => ReturnDetailsForm.Destination = x, "return destination");

    private Task OpenDisposalSourceLookupAsync() =>
        OpenDetailLocationLookupAsync(CurrentUserCompany, x => DisposalDetailsForm.Source = x, "disposal source");

    private Task OpenDisposalDestinationLookupAsync() =>
        OpenDetailLocationLookupAsync(CurrentUserCompany, x => DisposalDetailsForm.DisposalDestination = x, "disposal destination");


    private async Task SaveGatePassAsync()
    {
        try
        {
            if (GatePassForm.GatePassId > 0)
                throw new InvalidOperationException("Saved Gate Pass cannot be edited. Cancel and create a new Gate Pass if correction is required.");

            GatePassForm.DataAreaId = CurrentUserCompany;
            GatePassForm.SecurityOfficer = CurrentUsername;
            GatePassForm.EntryDateTime ??= DateTime.Now;
            GatePassForm.CreatedAt = DateTime.Now;
            GatePassForm.Status = "Open";
            if (string.IsNullOrWhiteSpace(GatePassForm.GatePassNumber))
                GatePassForm.GatePassNumber = await _databaseService.GenerateGatePassNumberAsync(CurrentUserCompany);

            await _databaseService.SaveGatePassAsync(GatePassForm);
            GatePassNumber = GatePassForm.GatePassNumber;
            ClearGatePassForm();
            await LoadMastersAsync();
            StatusMessage = "Gate Pass saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Gate Pass save error: " + ex.Message;
        }
    }

    private void ClearGatePassForm()
    {
        SelectedGatePass = null;
        GatePassForm = new GatePass
        {
            DataAreaId = CurrentUserCompany,
            Type = "Inbound",
            PartyType = "Customer",
            EntryDateTime = DateTime.Now,
            SecurityOfficer = CurrentUsername,
            Status = "Open"
        };
    }

    private async Task CloseGatePassAsync()
    {
        try
        {
            if (SelectedGatePass == null)
                throw new InvalidOperationException("Please select a Gate Pass first.");
            await _databaseService.CloseGatePassAsync(SelectedGatePass.GatePassId, CurrentUsername);
            ClearGatePassForm();
            await LoadMastersAsync();
            StatusMessage = "Gate Pass closed.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Gate Pass close error: " + ex.Message;
        }
    }

    private async Task CancelGatePassAsync()
    {
        try
        {
            if (SelectedGatePass == null)
                throw new InvalidOperationException("Please select a Gate Pass first.");
            await _databaseService.CancelGatePassAsync(SelectedGatePass.GatePassId, CurrentUsername);
            ClearGatePassForm();
            await LoadMastersAsync();
            StatusMessage = "Gate Pass cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Gate Pass cancel error: " + ex.Message;
        }
    }

    private void ApplyGatePassToWeighment(GatePass gatePass)
    {
        if (!string.Equals(gatePass.Status, "Open", StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = "Only open Gate Pass can be selected for weighment.";
            return;
        }

        GatePassNumber = gatePass.GatePassNumber;
        if (!string.IsNullOrWhiteSpace(gatePass.VehiclePlate))
            VehicleNo = gatePass.VehiclePlate;
        if (!string.IsNullOrWhiteSpace(gatePass.DriverName))
            DriverName = gatePass.DriverName;
        if (!string.IsNullOrWhiteSpace(gatePass.ExpectedTransactionType))
            SelectedTransactionTypeMaster = TransactionTypeMasters.FirstOrDefault(x => string.Equals(x.Type, gatePass.ExpectedTransactionType, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(gatePass.PartyType))
            SelectedPartyType = gatePass.PartyType;
        if (!string.IsNullOrWhiteSpace(gatePass.PartyAccount))
            PartyAccount = gatePass.PartyAccount;
        if (!string.IsNullOrWhiteSpace(gatePass.PartyName))
            PartyName = gatePass.PartyName;
        if (!string.IsNullOrWhiteSpace(gatePass.ExpectedItemNumber))
            ItemNumber = gatePass.ExpectedItemNumber;
        if (!string.IsNullOrWhiteSpace(gatePass.ExpectedItem))
            ItemName = gatePass.ExpectedItem;

        StatusMessage = $"Gate Pass {GatePassNumber} selected for weighment.";
    }

    private Task OpenTransactionTypeLookupAsync()
    {
        var lookupWindow = new WeightBridgeApp.TransactionTypeLookupWindow(TransactionTypeMasters)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedTransactionType != null)
            SelectedTransactionTypeMaster = lookupWindow.SelectedTransactionType;

        return Task.CompletedTask;
    }

    private Task OpenScenarioLookupAsync()
    {
        var lookupWindow = new WeightBridgeApp.ScenarioLookupWindow(ScenarioMasters)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedScenario != null)
            SelectedScenarioMaster = lookupWindow.SelectedScenario;

        return Task.CompletedTask;
    }

    private Task OpenWeighmentGatePassLookupAsync()
    {
        var lookupWindow = new WeightBridgeApp.GatePassLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedGatePass != null)
        {
            SelectedWeighmentGatePass = lookupWindow.SelectedGatePass;
        }

        return Task.CompletedTask;
    }

    private Task OpenGatePassVehicleLookupAsync()
    {
        if (IsGatePassFormReadOnly)
            return Task.CompletedTask;

        var lookupWindow = new WeightBridgeApp.VehicleLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedVehicle != null)
        {
            GatePassForm.VehiclePlate = lookupWindow.SelectedVehicle.VehicleNo;
            OnPropertyChanged(nameof(GatePassForm));
            StatusMessage = $"Selected Gate Pass Vehicle: {GatePassForm.VehiclePlate}";
        }

        return Task.CompletedTask;
    }

    private Task OpenGatePassDriverLookupAsync()
    {
        if (IsGatePassFormReadOnly)
            return Task.CompletedTask;

        var lookupWindow = new WeightBridgeApp.DriverLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedDriver != null)
        {
            GatePassForm.DriverName = lookupWindow.SelectedDriver.DriverName;
            GatePassForm.DriverMobile = lookupWindow.SelectedDriver.MobileNumber;
            OnPropertyChanged(nameof(GatePassForm));
            StatusMessage = $"Selected Gate Pass Driver: {GatePassForm.DriverName}";
        }

        return Task.CompletedTask;
    }

    private Task OpenGatePassPartyLookupAsync()
    {
        if (IsGatePassFormReadOnly)
            return Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(GatePassForm.PartyType))
        {
            StatusMessage = "Please select Gate Pass Party Type first.";
            return Task.CompletedTask;
        }

        var lookupWindow = new WeightBridgeApp.PartyLookupWindow(_databaseService, CurrentUserCompany, GatePassForm.PartyType)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedParty != null)
        {
            GatePassForm.PartyAccount = lookupWindow.SelectedParty.PartyAccount;
            GatePassForm.PartyName = lookupWindow.SelectedParty.PartyName;
            OnPropertyChanged(nameof(GatePassForm));
            OnPropertyChanged(nameof(GatePassPartyDisplay));
            StatusMessage = $"Selected Gate Pass Party: {GatePassForm.PartyAccount} - {GatePassForm.PartyName}";
        }

        return Task.CompletedTask;
    }

    private Task OpenGatePassItemLookupAsync()
    {
        if (IsGatePassFormReadOnly)
            return Task.CompletedTask;

        var lookupWindow = new WeightBridgeApp.ItemLookupWindow(_databaseService, CurrentUserCompany)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (lookupWindow.ShowDialog() == true && lookupWindow.SelectedItemMaster != null)
        {
            GatePassForm.ExpectedItemNumber = lookupWindow.SelectedItemMaster.ItemNumber;
            GatePassForm.ExpectedItem = lookupWindow.SelectedItemMaster.ProductName;
            OnPropertyChanged(nameof(GatePassForm));
            OnPropertyChanged(nameof(GatePassExpectedItemDisplay));
            StatusMessage = $"Selected Gate Pass Expected Item: {GatePassForm.ExpectedItemNumber} - {GatePassForm.ExpectedItem}";
        }

        return Task.CompletedTask;
    }

    private Task PrintGatePassAsync()
    {
        try
        {
            var gatePass = SelectedGatePass ?? (GatePassForm.GatePassId > 0 ? GatePassForm : null);
            if (gatePass == null || string.IsNullOrWhiteSpace(gatePass.GatePassNumber))
            {
                StatusMessage = "Please select a saved Gate Pass to print.";
                return Task.CompletedTask;
            }

            var printed = SlipService.PrintGatePass(gatePass);
            StatusMessage = printed
                ? "Gate Pass sent to printer."
                : "Gate Pass print cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Gate Pass print error: " + ex.Message;
        }

        return Task.CompletedTask;
    }

    private string DeriveShiftCode(DateTime transactionDateTime)
    {
        foreach (var shift in ShiftMasters)
        {
            if (!TimeSpan.TryParse(shift.StartTime, out var start) || !TimeSpan.TryParse(shift.EndTime, out var end))
                continue;

            var current = transactionDateTime.TimeOfDay;
            var crossesMidnight = string.Equals(shift.CrossingMidnightRule, "Yes", StringComparison.OrdinalIgnoreCase) || start > end;
            var inShift = crossesMidnight ? current >= start || current <= end : current >= start && current <= end;
            if (inShift)
                return shift.Code;
        }

        return string.Empty;
    }

    private static bool IsFilterEmpty(string value) => string.IsNullOrWhiteSpace(value);

    private static bool HasText(string value, string filter)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Contains(filter?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSameDataArea(string? recordDataAreaId, string? currentDataAreaId)
    {
        var record = string.IsNullOrWhiteSpace(recordDataAreaId) ? "DAT" : recordDataAreaId.Trim();
        var current = string.IsNullOrWhiteSpace(currentDataAreaId) ? "DAT" : currentDataAreaId.Trim();
        return string.Equals(record, current, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsStatusActive(string? status) => string.Equals(status?.Trim(), "Active", StringComparison.OrdinalIgnoreCase);

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
        OnPropertyChanged(nameof(IsHeaderAndLinesEditable));
        OnPropertyChanged(nameof(IsHeaderAndLinesLocked));
        OnPropertyChanged(nameof(IsPurchaseDetailsEditable));
        OnPropertyChanged(nameof(IsPurchaseDetailsReadOnly));
        OnPropertyChanged(nameof(IsPurchaseVendorSelectable));
        OnPropertyChanged(nameof(IsPurchaseRateAmountEditable));
        OnPropertyChanged(nameof(IsContractCollectionDetailsEditable));
        OnPropertyChanged(nameof(IsContractCollectionDetailsReadOnly));
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    private void NotifyWeighmentButtonStates()
    {
        OnPropertyChanged(nameof(CanSaveFirstWeight));
        OnPropertyChanged(nameof(CanSaveSecondWeight));
        OnPropertyChanged(nameof(IsHeaderAndLinesEditable));
        OnPropertyChanged(nameof(IsHeaderAndLinesLocked));
        OnPropertyChanged(nameof(IsPurchaseDetailsEditable));
        OnPropertyChanged(nameof(IsPurchaseDetailsReadOnly));
        OnPropertyChanged(nameof(IsPurchaseVendorSelectable));
        OnPropertyChanged(nameof(IsPurchaseRateAmountEditable));
        OnPropertyChanged(nameof(IsContractCollectionDetailsEditable));
        OnPropertyChanged(nameof(IsContractCollectionDetailsReadOnly));
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }
}

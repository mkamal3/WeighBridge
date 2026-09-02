using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;

namespace WeightBridgeApp.ViewModels;

public sealed class CorrectionWorkspaceViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly OperatorMaster _currentUser;
    private readonly Func<string> _companyProvider;
    private readonly Func<Task>? _afterChange;

    private WeighmentCorrection _correctionForm = new();
    private Weighment _originalWeighment = new();
    private Weighment _correctedWeighment = new();
    private WeighmentCorrection? _selectedRequest;
    private CorrectionMaterialLineEdit? _selectedMaterialLine;
    private decimal _correctedFirstWeight;
    private decimal? _correctedSecondWeight;
    private string _statusMessage = string.Empty;
    private bool _isLoadingSelection;

    public CorrectionWorkspaceViewModel(DatabaseService databaseService, OperatorMaster currentUser, Func<string> companyProvider, Func<Task>? afterChange = null)
    {
        _databaseService = databaseService;
        _currentUser = currentUser;
        _companyProvider = companyProvider;
        _afterChange = afterChange;

        NewRequestCommand = new RelayCommand(NewRequestAsync, () => CanCreateRequest);
        OpenSlipLookupCommand = new RelayCommand(OpenSlipLookupAsync, () => IsDraftEditable);
        SaveDraftCommand = new RelayCommand(() => SaveAsync(false), () => CanSaveDraft);
        SubmitCommand = new RelayCommand(() => SaveAsync(true), () => CanSubmit);
        ApproveCommand = new RelayCommand(ApproveAsync, () => CanApprove);
        RejectCommand = new RelayCommand(RejectAsync, () => CanReject);
        AddLineCommand = new RelayCommand(AddLine, () => IsDraftEditable && CorrectionForm.WeighmentId > 0);
        RemoveAddedLineCommand = new RelayCommand(RemoveAddedLine, () => IsDraftEditable && SelectedMaterialLine?.IsAddedLine == true);
        RefreshCommand = new RelayCommand(RefreshAsync);
    }

    public ObservableCollection<WeighmentCorrection> Requests { get; } = new();
    public ObservableCollection<CorrectionMaterialLineEdit> MaterialLines { get; } = new();
    public ObservableCollection<ItemMaster> ItemMasters { get; } = new();
    public ObservableCollection<string> UomSymbols { get; } = new();
    public ObservableCollection<ReasonMaster> CorrectionReasons { get; } = new();
    public ObservableCollection<string> CorrectionTypes { get; } = new() { "General", "Weight", "Material", "Vehicle", "Driver", "Other" };
    public ObservableCollection<string> LineActionTypes { get; } = new() { "Modify", "Replace", "Remove", "Add" };

    public RelayCommand NewRequestCommand { get; }
    public RelayCommand OpenSlipLookupCommand { get; }
    public RelayCommand SaveDraftCommand { get; }
    public RelayCommand SubmitCommand { get; }
    public RelayCommand ApproveCommand { get; }
    public RelayCommand RejectCommand { get; }
    public RelayCommand AddLineCommand { get; }
    public RelayCommand RemoveAddedLineCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public WeighmentCorrection CorrectionForm
    {
        get => _correctionForm;
        private set
        {
            if (SetProperty(ref _correctionForm, value)) NotifyState();
        }
    }

    public Weighment OriginalWeighment
    {
        get => _originalWeighment;
        private set => SetProperty(ref _originalWeighment, value);
    }

    public Weighment CorrectedWeighment
    {
        get => _correctedWeighment;
        private set => SetProperty(ref _correctedWeighment, value);
    }

    public WeighmentCorrection? SelectedRequest
    {
        get => _selectedRequest;
        set
        {
            if (SetProperty(ref _selectedRequest, value) && value != null && !_isLoadingSelection)
                _ = LoadRequestAsync(value);
        }
    }

    public CorrectionMaterialLineEdit? SelectedMaterialLine
    {
        get => _selectedMaterialLine;
        set
        {
            if (SetProperty(ref _selectedMaterialLine, value))
            {
                OnPropertyChanged(nameof(CanRemoveAddedLine));
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public decimal CorrectedFirstWeight
    {
        get => _correctedFirstWeight;
        set
        {
            if (SetProperty(ref _correctedFirstWeight, value))
                OnPropertyChanged(nameof(CorrectedNetWeight));
        }
    }

    public decimal? CorrectedSecondWeight
    {
        get => _correctedSecondWeight;
        set
        {
            if (SetProperty(ref _correctedSecondWeight, value))
                OnPropertyChanged(nameof(CorrectedNetWeight));
        }
    }

    public decimal? CorrectedNetWeight => CorrectedSecondWeight.HasValue ? Math.Abs(CorrectedSecondWeight.Value - CorrectedFirstWeight) : null;

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool CanCreateRequest => _currentUser.CanAccessCorrection && _currentUser.CanSubmitCorrection;
    public bool IsDraftEditable => CanCreateRequest && string.Equals(CorrectionForm.Status, "Draft", StringComparison.OrdinalIgnoreCase);
    public bool IsFormReadOnly => !IsDraftEditable;
    public bool IsWeightReadOnly => !IsDraftEditable || !_currentUser.CanCorrectWeight;
    public bool CanSaveDraft => IsDraftEditable && CorrectionForm.WeighmentId > 0;
    public bool CanSubmit => CanSaveDraft;
    // Approval / rejection is controlled by the dedicated Operator Master permission.
    // A user with "Can Approve / Reject Correction" can act on any Submitted correction,
    // including one they originally submitted. No separate maker-checker permission is required.
    public bool CanApprove => _currentUser.CanAccessCorrection
                              && _currentUser.CanApproveRejectCorrection
                              && CorrectionForm.CorrectionId > 0
                              && string.Equals(CorrectionForm.Status, "Submitted", StringComparison.OrdinalIgnoreCase);
    public bool CanReject => CanApprove;
    public bool CanRemoveAddedLine => IsDraftEditable && SelectedMaterialLine?.IsAddedLine == true;

    public async Task InitializeAsync()
    {
        await LoadReferenceDataAsync();
        await LoadRequestsAsync();
        if (CanCreateRequest)
            await NewRequestAsync();
    }

    public async Task RefreshForCompanyAsync()
    {
        await LoadReferenceDataAsync();
        await LoadRequestsAsync();
        if (CanCreateRequest)
            await NewRequestAsync();
        else
            ClearForm();
    }

    public async Task StartForTransactionAsync(Weighment transaction)
    {
        if (!CanCreateRequest)
        {
            StatusMessage = "You do not have permission to create correction requests.";
            return;
        }
        await NewRequestAsync();
        await ApplySelectedTransactionAsync(transaction);
    }

    private async Task RefreshAsync()
    {
        await LoadRequestsAsync();
        StatusMessage = "Correction requests refreshed.";
    }

    private async Task LoadReferenceDataAsync()
    {
        CorrectionReasons.Clear();
        var reasons = await _databaseService.GetReasonMastersAsync();
        foreach (var reason in reasons
                     .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                     .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase))
            CorrectionReasons.Add(reason);

        ItemMasters.Clear();
        var company = CurrentCompany;
        var items = await _databaseService.GetItemMastersAsync();
        foreach (var item in items.Where(x => string.Equals(x.DataAreaId?.Trim(), company, StringComparison.OrdinalIgnoreCase)
                                           && !IsDeleted(x.IsDelete))
                                  .OrderBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase))
            ItemMasters.Add(item);

        UomSymbols.Clear();
        var uoms = await _databaseService.GetUnitOfMeasureMastersAsync();
        foreach (var symbol in uoms.Where(x => !string.IsNullOrWhiteSpace(x.symbol) && !IsDeleted(x.IsDelete))
                                   .Select(x => x.symbol.Trim())
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            UomSymbols.Add(symbol);
    }

    private static bool IsDeleted(string? value) => string.Equals(value?.Trim(), "1", StringComparison.OrdinalIgnoreCase)
                                                    || string.Equals(value?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

    private string CurrentCompany => string.IsNullOrWhiteSpace(_companyProvider()) ? "DAT" : _companyProvider().Trim();

    private async Task LoadRequestsAsync()
    {
        var selectedId = SelectedRequest?.CorrectionId ?? CorrectionForm.CorrectionId;
        var rows = await _databaseService.GetWeighmentCorrectionsAsync(CurrentCompany);
        Requests.Clear();
        foreach (var row in rows) Requests.Add(row);

        if (selectedId > 0)
        {
            _isLoadingSelection = true;
            SelectedRequest = Requests.FirstOrDefault(x => x.CorrectionId == selectedId);
            _isLoadingSelection = false;
        }
    }

    private async Task NewRequestAsync()
    {
        try
        {
            if (!CanCreateRequest)
            {
                StatusMessage = "You do not have permission to create correction requests.";
                return;
            }

            _isLoadingSelection = true;
            SelectedRequest = null;
            _isLoadingSelection = false;

            CorrectionForm = new WeighmentCorrection
            {
                DataAreaId = CurrentCompany,
                CorrectionNumber = await _databaseService.GenerateCorrectionNumberAsync(CurrentCompany),
                CorrectionType = "General",
                Reason = string.Empty,
                Status = "Draft",
                CreatedAt = DateTime.Now
            };
            OriginalWeighment = new Weighment { DataAreaId = CurrentCompany };
            CorrectedWeighment = new Weighment { DataAreaId = CurrentCompany };
            MaterialLines.Clear();
            CorrectedFirstWeight = 0m;
            CorrectedSecondWeight = null;
            StatusMessage = "New correction request created. Select a completed transaction.";
            NotifyState();
        }
        catch (Exception ex)
        {
            StatusMessage = "New correction error: " + ex.Message;
        }
    }

    private async Task OpenSlipLookupAsync()
    {
        try
        {
            if (!IsDraftEditable) return;
            var lookup = new CorrectionSlipLookupWindow(_databaseService, CurrentCompany);
            var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (owner != null) lookup.Owner = owner;
            if (lookup.ShowDialog() != true || lookup.SelectedWeighment == null) return;

            await ApplySelectedTransactionAsync(lookup.SelectedWeighment);
        }
        catch (Exception ex)
        {
            StatusMessage = "Transaction selection error: " + ex.Message;
        }
    }

    private async Task ApplySelectedTransactionAsync(Weighment transaction)
    {
        if (!string.Equals(transaction.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Completed transactions can be corrected.");

        var active = await _databaseService.GetActiveCorrectionAsync(transaction.WeighmentId);
        if (active != null)
            throw new InvalidOperationException($"An active correction request already exists for this transaction: {active.CorrectionNumber}.");

        CorrectionForm.WeighmentId = transaction.WeighmentId;
        CorrectionForm.DataAreaId = transaction.DataAreaId;
        CorrectionForm.SlipNumber = string.IsNullOrWhiteSpace(transaction.SlipNumber) ? transaction.TicketNo : transaction.SlipNumber;
        OriginalWeighment = CloneWeighment(transaction);
        CorrectedWeighment = CloneWeighment(transaction);
        CorrectedFirstWeight = transaction.FirstWeight;
        CorrectedSecondWeight = transaction.SecondWeight;
        var lines = await _databaseService.GetWeighmentMaterialLinesAsync(transaction.WeighmentId);
        BuildMaterialLines(lines);
        OnPropertyChanged(nameof(CorrectionForm));
        StatusMessage = $"Selected completed slip {CorrectionForm.SlipNumber}. Enter header and/or material line corrections.";
        NotifyState();
    }

    private void BuildMaterialLines(IEnumerable<WeighmentMaterialLine> lines)
    {
        MaterialLines.Clear();
        foreach (var line in lines.OrderBy(x => x.LineNo))
        {
            MaterialLines.Add(new CorrectionMaterialLineEdit
            {
                LineNo = line.LineNo,
                OriginalMaterialLineId = line.MaterialLineId,
                OriginalItemMasterId = line.ItemMasterId,
                OriginalItemNumber = line.ItemNumber,
                OriginalItemName = line.ItemName,
                OriginalUom = line.Uom,
                OriginalExpectedQty = line.ExpectedQty,
                OriginalRemarks = line.Remarks,
                ActionType = "Modify",
                CorrectedItemMasterId = line.ItemMasterId,
                CorrectedItemNumber = line.ItemNumber,
                CorrectedItemName = line.ItemName,
                CorrectedUom = line.Uom,
                CorrectedExpectedQty = line.ExpectedQty,
                CorrectedRemarks = line.Remarks
            });
        }
    }

    private async Task LoadRequestAsync(WeighmentCorrection request)
    {
        try
        {
            var transaction = await _databaseService.GetWeighmentByIdAsync(request.WeighmentId, request.DataAreaId);
            if (transaction == null)
            {
                StatusMessage = "The original transaction could not be found.";
                return;
            }

            CorrectionForm = CloneCorrection(request);
            OriginalWeighment = CloneWeighment(transaction);
            CorrectedWeighment = CloneWeighment(transaction);
            CorrectedFirstWeight = transaction.FirstWeight;
            CorrectedSecondWeight = transaction.SecondWeight;
            BuildMaterialLines(await _databaseService.GetWeighmentMaterialLinesAsync(transaction.WeighmentId));
            ApplyExistingDetails(await _databaseService.GetCorrectionDetailsAsync(request.CorrectionId));
            StatusMessage = $"Loaded correction {request.CorrectionNumber} ({request.Status}).";
            NotifyState();
        }
        catch (Exception ex)
        {
            StatusMessage = "Correction load error: " + ex.Message;
        }
    }

    private void ApplyExistingDetails(IEnumerable<WeighmentCorrectionDetail> details)
    {
        foreach (var detail in details.Where(x => string.Equals(x.DetailType, "Header", StringComparison.OrdinalIgnoreCase)))
        {
            switch (detail.FieldName)
            {
                case nameof(Weighment.VehicleNo): OriginalWeighment.VehicleNo = detail.OriginalValue; CorrectedWeighment.VehicleNo = detail.CorrectedValue; break;
                case nameof(Weighment.DriverName): OriginalWeighment.DriverName = detail.OriginalValue; CorrectedWeighment.DriverName = detail.CorrectedValue; break;
                case nameof(Weighment.ItemNumber): OriginalWeighment.ItemNumber = detail.OriginalValue; CorrectedWeighment.ItemNumber = detail.CorrectedValue; break;
                case nameof(Weighment.ItemName): OriginalWeighment.ItemName = detail.OriginalValue; CorrectedWeighment.ItemName = detail.CorrectedValue; break;
                case nameof(Weighment.FirstWeight):
                    if (decimal.TryParse(detail.OriginalValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var ofw)) OriginalWeighment.FirstWeight = ofw;
                    if (decimal.TryParse(detail.CorrectedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var cfw)) CorrectedFirstWeight = cfw;
                    break;
                case nameof(Weighment.SecondWeight):
                    OriginalWeighment.SecondWeight = decimal.TryParse(detail.OriginalValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var osw) ? osw : null;
                    CorrectedSecondWeight = decimal.TryParse(detail.CorrectedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var csw) ? csw : null;
                    break;
                case nameof(Weighment.Remarks): OriginalWeighment.Remarks = detail.OriginalValue; CorrectedWeighment.Remarks = detail.CorrectedValue; break;
            }
        }

        foreach (var detail in details.Where(x => string.Equals(x.DetailType, "MaterialLine", StringComparison.OrdinalIgnoreCase)))
        {
            var edit = detail.OriginalMaterialLineId.HasValue
                ? MaterialLines.FirstOrDefault(x => x.OriginalMaterialLineId == detail.OriginalMaterialLineId)
                : null;
            if (edit == null)
            {
                edit = new CorrectionMaterialLineEdit { LineNo = detail.LineNo, OriginalMaterialLineId = detail.OriginalMaterialLineId };
                MaterialLines.Add(edit);
            }
            edit.OriginalItemMasterId = detail.OriginalItemMasterId;
            edit.OriginalItemNumber = detail.OriginalItemNumber;
            edit.OriginalItemName = detail.OriginalItemName;
            edit.OriginalUom = detail.OriginalUom;
            edit.OriginalExpectedQty = detail.OriginalExpectedQty;
            edit.OriginalRemarks = detail.OriginalRemarks;
            edit.ActionType = detail.ActionType;
            edit.CorrectedItemMasterId = detail.CorrectedItemMasterId;
            edit.CorrectedItemNumber = detail.CorrectedItemNumber;
            edit.CorrectedItemName = detail.CorrectedItemName;
            edit.CorrectedUom = detail.CorrectedUom;
            edit.CorrectedExpectedQty = detail.CorrectedExpectedQty;
            edit.CorrectedRemarks = detail.CorrectedRemarks;
        }

        var ordered = MaterialLines.OrderBy(x => x.LineNo).ToList();
        MaterialLines.Clear();
        foreach (var line in ordered) MaterialLines.Add(line);
        OnPropertyChanged(nameof(OriginalWeighment));
        OnPropertyChanged(nameof(CorrectedWeighment));
        OnPropertyChanged(nameof(CorrectedNetWeight));
    }

    private void AddLine()
    {
        var next = MaterialLines.Count == 0 ? 1 : MaterialLines.Max(x => x.LineNo) + 1;
        MaterialLines.Add(new CorrectionMaterialLineEdit
        {
            LineNo = next,
            ActionType = "Add",
            CorrectedUom = UomSymbols.FirstOrDefault() ?? string.Empty,
            CorrectedExpectedQty = 0m
        });
        SelectedMaterialLine = MaterialLines.Last();
        StatusMessage = "New material correction line added.";
    }

    private void RemoveAddedLine()
    {
        if (SelectedMaterialLine?.IsAddedLine != true) return;
        MaterialLines.Remove(SelectedMaterialLine);
        SelectedMaterialLine = null;
        StatusMessage = "Unsaved added line removed.";
    }

    private List<WeighmentCorrectionDetail> BuildDetails()
    {
        if (CorrectionForm.WeighmentId <= 0) throw new InvalidOperationException("Please select a completed transaction first.");
        if (!_currentUser.CanCorrectWeight && (CorrectedFirstWeight != OriginalWeighment.FirstWeight || CorrectedSecondWeight != OriginalWeighment.SecondWeight))
            throw new InvalidOperationException("You do not have permission to correct weight values.");

        CorrectedWeighment.FirstWeight = CorrectedFirstWeight;
        CorrectedWeighment.SecondWeight = CorrectedSecondWeight;
        CorrectedWeighment.NetWeight = CorrectedNetWeight;

        var details = new List<WeighmentCorrectionDetail>();
        AddHeaderChange(details, nameof(Weighment.VehicleNo), OriginalWeighment.VehicleNo, CorrectedWeighment.VehicleNo);
        AddHeaderChange(details, nameof(Weighment.DriverName), OriginalWeighment.DriverName, CorrectedWeighment.DriverName);
        AddHeaderChange(details, nameof(Weighment.FirstWeight), OriginalWeighment.FirstWeight.ToString(CultureInfo.InvariantCulture), CorrectedFirstWeight.ToString(CultureInfo.InvariantCulture));
        AddHeaderChange(details, nameof(Weighment.SecondWeight), OriginalWeighment.SecondWeight?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, CorrectedSecondWeight?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
        AddHeaderChange(details, nameof(Weighment.Remarks), OriginalWeighment.Remarks, CorrectedWeighment.Remarks);

        foreach (var line in MaterialLines)
        {
            var action = line.IsAddedLine ? "Add" : (line.ActionType?.Trim() ?? "Modify");
            if (!line.IsAddedLine && string.Equals(action, "Add", StringComparison.OrdinalIgnoreCase)) action = "Modify";

            var changed = line.IsAddedLine
                          || string.Equals(action, "Remove", StringComparison.OrdinalIgnoreCase)
                          || !string.Equals(line.OriginalItemNumber ?? string.Empty, line.CorrectedItemNumber ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                          || !string.Equals(line.OriginalUom ?? string.Empty, line.CorrectedUom ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                          || line.OriginalExpectedQty != line.CorrectedExpectedQty
                          || !string.Equals(line.OriginalRemarks ?? string.Empty, line.CorrectedRemarks ?? string.Empty, StringComparison.Ordinal);
            if (!changed) continue;

            ItemMaster? correctedItem = null;
            if (!string.Equals(action, "Remove", StringComparison.OrdinalIgnoreCase))
            {
                correctedItem = ItemMasters.FirstOrDefault(x => string.Equals(x.ItemNumber, line.CorrectedItemNumber, StringComparison.OrdinalIgnoreCase));
                if (correctedItem == null) throw new InvalidOperationException($"Material line {line.LineNo}: select a valid Item from Item Master.");
                if (string.IsNullOrWhiteSpace(line.CorrectedUom) || !UomSymbols.Any(x => string.Equals(x, line.CorrectedUom, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"Material line {line.LineNo}: select a valid UOM from UOM Master.");
                if (!line.CorrectedExpectedQty.HasValue || line.CorrectedExpectedQty.Value < 0)
                    throw new InvalidOperationException($"Material line {line.LineNo}: Corrected Qty must be zero or greater.");
                line.CorrectedItemMasterId = correctedItem.ItemMasterId;
                line.CorrectedItemName = correctedItem.ProductName;
            }

            if (!line.IsAddedLine && !string.Equals(action, "Remove", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(line.OriginalItemNumber, line.CorrectedItemNumber, StringComparison.OrdinalIgnoreCase)) action = "Replace";

            details.Add(new WeighmentCorrectionDetail
            {
                DetailType = "MaterialLine",
                LineNo = line.LineNo,
                OriginalMaterialLineId = line.OriginalMaterialLineId,
                ActionType = action,
                OriginalItemMasterId = line.OriginalItemMasterId,
                CorrectedItemMasterId = line.CorrectedItemMasterId,
                OriginalItemNumber = line.OriginalItemNumber,
                CorrectedItemNumber = line.CorrectedItemNumber,
                OriginalItemName = line.OriginalItemName,
                CorrectedItemName = line.CorrectedItemName,
                OriginalUom = line.OriginalUom,
                CorrectedUom = line.CorrectedUom,
                OriginalExpectedQty = line.OriginalExpectedQty,
                CorrectedExpectedQty = line.CorrectedExpectedQty,
                OriginalRemarks = line.OriginalRemarks,
                CorrectedRemarks = line.CorrectedRemarks
            });
        }

        var existingLineCount = MaterialLines.Count(x => !x.IsAddedLine);
        if (existingLineCount > 0)
        {
            var remaining = MaterialLines.Count(x => !x.IsAddedLine && !string.Equals(x.ActionType?.Trim(), "Remove", StringComparison.OrdinalIgnoreCase));
            var added = MaterialLines.Count(x => x.IsAddedLine);
            if (remaining + added == 0) throw new InvalidOperationException("At least one material line must remain after correction.");
        }

        return details;
    }

    private static void AddHeaderChange(List<WeighmentCorrectionDetail> details, string fieldName, string? original, string? corrected)
    {
        original ??= string.Empty;
        corrected ??= string.Empty;
        if (string.Equals(original, corrected, StringComparison.Ordinal)) return;
        details.Add(new WeighmentCorrectionDetail { DetailType = "Header", FieldName = fieldName, OriginalValue = original, CorrectedValue = corrected });
    }

    private async Task SaveAsync(bool submit)
    {
        try
        {
            if (!IsDraftEditable) throw new InvalidOperationException("This correction is not editable.");
            var details = BuildDetails();
            CorrectionForm.DataAreaId = CurrentCompany;
            if (string.IsNullOrWhiteSpace(CorrectionForm.CorrectionNumber))
                CorrectionForm.CorrectionNumber = await _databaseService.GenerateCorrectionNumberAsync(CurrentCompany);

            CorrectionForm.CorrectionId = await _databaseService.SaveCorrectionAsync(CorrectionForm, details, submit, _currentUser.Username);
            await LoadRequestsAsync();
            _isLoadingSelection = true;
            SelectedRequest = Requests.FirstOrDefault(x => x.CorrectionId == CorrectionForm.CorrectionId);
            _isLoadingSelection = false;
            if (SelectedRequest != null) CorrectionForm = CloneCorrection(SelectedRequest);
            StatusMessage = submit ? $"Correction {CorrectionForm.CorrectionNumber} submitted for approval." : $"Correction {CorrectionForm.CorrectionNumber} saved as Draft.";
            NotifyState();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            MessageBox.Show(ex.Message, "Correction", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task ApproveAsync()
    {
        try
        {
            if (!CanApprove) throw new InvalidOperationException("Select a Submitted correction that you are allowed to approve.");
            var confirm = MessageBox.Show($"Approve correction {CorrectionForm.CorrectionNumber}?\n\nThe corrected header/material values will replace the active transaction values while the original values remain in correction history.", "Approve Correction", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;
            var id = CorrectionForm.CorrectionId;
            await _databaseService.ApproveCorrectionAsync(id, _currentUser.Username);
            await LoadRequestsAsync();
            var row = Requests.FirstOrDefault(x => x.CorrectionId == id);
            if (row != null) await LoadRequestAsync(row);
            if (_afterChange != null) await _afterChange();
            StatusMessage = $"Correction {CorrectionForm.CorrectionNumber} approved and applied.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Correction approval error: " + ex.Message;
            MessageBox.Show(ex.Message, "Correction", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task RejectAsync()
    {
        try
        {
            if (!CanReject) throw new InvalidOperationException("Select a Submitted correction that you are allowed to reject.");
            var confirm = MessageBox.Show($"Reject correction {CorrectionForm.CorrectionNumber}?", "Reject Correction", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;
            var id = CorrectionForm.CorrectionId;
            await _databaseService.RejectCorrectionAsync(id, _currentUser.Username);
            await LoadRequestsAsync();
            var row = Requests.FirstOrDefault(x => x.CorrectionId == id);
            if (row != null) await LoadRequestAsync(row);
            StatusMessage = $"Correction {CorrectionForm.CorrectionNumber} rejected.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Correction rejection error: " + ex.Message;
            MessageBox.Show(ex.Message, "Correction", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void NotifyState()
    {
        OnPropertyChanged(nameof(IsDraftEditable));
        OnPropertyChanged(nameof(IsFormReadOnly));
        OnPropertyChanged(nameof(IsWeightReadOnly));
        OnPropertyChanged(nameof(CanSaveDraft));
        OnPropertyChanged(nameof(CanSubmit));
        OnPropertyChanged(nameof(CanApprove));
        OnPropertyChanged(nameof(CanReject));
        OnPropertyChanged(nameof(CanRemoveAddedLine));
        OnPropertyChanged(nameof(CorrectedNetWeight));
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    private void ClearForm()
    {
        CorrectionForm = new WeighmentCorrection { DataAreaId = CurrentCompany, Status = "Draft" };
        OriginalWeighment = new Weighment { DataAreaId = CurrentCompany };
        CorrectedWeighment = new Weighment { DataAreaId = CurrentCompany };
        MaterialLines.Clear();
        NotifyState();
    }

    private static WeighmentCorrection CloneCorrection(WeighmentCorrection source) => new()
    {
        CorrectionId = source.CorrectionId,
        DataAreaId = source.DataAreaId,
        WeighmentId = source.WeighmentId,
        SlipNumber = source.SlipNumber,
        CorrectionNumber = source.CorrectionNumber,
        CorrectionType = source.CorrectionType,
        Reason = source.Reason,
        Comment = source.Comment,
        Status = source.Status,
        SubmittedBy = source.SubmittedBy,
        SubmittedDateTime = source.SubmittedDateTime,
        ApprovedRejectedBy = source.ApprovedRejectedBy,
        ApprovalRejectedDateTime = source.ApprovalRejectedDateTime,
        CreatedAt = source.CreatedAt
    };

    private static Weighment CloneWeighment(Weighment source) => new()
    {
        WeighmentId = source.WeighmentId,
        DataAreaId = source.DataAreaId,
        TicketNo = source.TicketNo,
        SlipNumber = source.SlipNumber,
        TransactionType = source.TransactionType,
        Scenario = source.Scenario,
        GatePassNumber = source.GatePassNumber,
        WeighbridgeCode = source.WeighbridgeCode,
        TransactionDateTime = source.TransactionDateTime,
        ShiftCode = source.ShiftCode,
        OperatorUsername = source.OperatorUsername,
        ExternalReference = source.ExternalReference,
        OperatorRemarks = source.OperatorRemarks,
        VehicleNo = source.VehicleNo,
        DriverName = source.DriverName,
        MaterialId = source.MaterialId,
        ItemNumber = source.ItemNumber,
        ItemName = source.ItemName,
        MaterialName = source.MaterialName,
        FirstWeight = source.FirstWeight,
        FirstWeightTime = source.FirstWeightTime,
        FirstWeightBy = source.FirstWeightBy,
        FirstWeightByDisplay = source.FirstWeightByDisplay,
        SecondWeight = source.SecondWeight,
        SecondWeightTime = source.SecondWeightTime,
        SecondWeightBy = source.SecondWeightBy,
        SecondWeightByDisplay = source.SecondWeightByDisplay,
        NetWeight = source.NetWeight,
        Status = source.Status,
        IsCorrected = source.IsCorrected,
        CorrectionVersion = source.CorrectionVersion,
        LastCorrectionNumber = source.LastCorrectionNumber,
        LastCorrectedDateTime = source.LastCorrectedDateTime,
        LastCorrectedBy = source.LastCorrectedBy,
        Remarks = source.Remarks,
        CreatedAt = source.CreatedAt
    };
}

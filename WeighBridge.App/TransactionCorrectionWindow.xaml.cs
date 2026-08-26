using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;

namespace WeightBridgeApp;

public partial class TransactionCorrectionWindow : Window, INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private readonly OperatorMaster _currentUser;
    private decimal _correctedFirstWeight;
    private decimal? _correctedSecondWeight;
    private string _statusMessage = "Loading correction...";
    private CorrectionMaterialLineEdit? _selectedMaterialLineEdit;
    private readonly WeighmentCorrection? _correctionToOpen;

    public TransactionCorrectionWindow(DatabaseService databaseService, OperatorMaster currentUser, Weighment transaction, WeighmentCorrection? correctionToOpen = null)
    {
        _databaseService = databaseService;
        _currentUser = currentUser;
        _correctionToOpen = correctionToOpen;
        OriginalWeighment = CloneWeighment(transaction);
        CorrectedWeighment = CloneWeighment(transaction);
        if (string.IsNullOrWhiteSpace(OriginalWeighment.SlipNumber))
            OriginalWeighment.SlipNumber = OriginalWeighment.TicketNo;
        if (string.IsNullOrWhiteSpace(CorrectedWeighment.SlipNumber))
            CorrectedWeighment.SlipNumber = CorrectedWeighment.TicketNo;
        _correctedFirstWeight = transaction.FirstWeight;
        _correctedSecondWeight = transaction.SecondWeight;
        Correction = new WeighmentCorrection
        {
            DataAreaId = transaction.DataAreaId,
            WeighmentId = transaction.WeighmentId,
            SlipNumber = string.IsNullOrWhiteSpace(transaction.SlipNumber) ? transaction.TicketNo : transaction.SlipNumber,
            CorrectionType = "General",
            Status = "Draft"
        };
        InitializeComponent();
        DataContext = this;
    }

    public Weighment OriginalWeighment { get; }
    public Weighment CorrectedWeighment { get; }
    public WeighmentCorrection Correction { get; private set; }
    public ObservableCollection<CorrectionMaterialLineEdit> MaterialLineEdits { get; } = new();
    public ObservableCollection<ItemMaster> ItemMasters { get; } = new();
    public ObservableCollection<string> UomSymbols { get; } = new();
    public ObservableCollection<ReasonMaster> CorrectionReasons { get; } = new();
    public ObservableCollection<string> CorrectionTypes { get; } = new() { "General", "Weight", "Material", "Vehicle", "Driver", "Other" };
    public ObservableCollection<string> LineActionTypes { get; } = new() { "Modify", "Replace", "Remove", "Add" };

    public CorrectionMaterialLineEdit? SelectedMaterialLineEdit
    {
        get => _selectedMaterialLineEdit;
        set { _selectedMaterialLineEdit = value; OnPropertyChanged(); }
    }

    public decimal CorrectedFirstWeight
    {
        get => _correctedFirstWeight;
        set { _correctedFirstWeight = value; OnPropertyChanged(); OnPropertyChanged(nameof(CorrectedNetWeight)); }
    }

    public decimal? CorrectedSecondWeight
    {
        get => _correctedSecondWeight;
        set { _correctedSecondWeight = value; OnPropertyChanged(); OnPropertyChanged(nameof(CorrectedNetWeight)); }
    }

    public decimal? CorrectedNetWeight => CorrectedSecondWeight.HasValue ? Math.Abs(CorrectedSecondWeight.Value - CorrectedFirstWeight) : null;

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsDraftEditable => _currentUser.CanAccessCorrection
                                   && _currentUser.CanSubmitCorrection
                                   && string.Equals(Correction.Status, "Draft", StringComparison.OrdinalIgnoreCase);
    public bool IsFormReadOnly => !IsDraftEditable;
    public bool IsWeightReadOnly => !IsDraftEditable || !_currentUser.CanCorrectWeight;
    public bool CanSaveDraft => IsDraftEditable;
    public bool CanSubmit => IsDraftEditable;
    // Approval / rejection is controlled by the dedicated Operator Master permission.
    // No additional maker-checker restriction is applied.
    public bool CanApprove => _currentUser.CanAccessCorrection
                              && _currentUser.CanApproveRejectCorrection
                              && Correction.CorrectionId > 0
                              && string.Equals(Correction.Status, "Submitted", StringComparison.OrdinalIgnoreCase);
    public bool CanReject => CanApprove;

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.Equals(OriginalWeighment.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Correction workflow is available only for Completed transactions. Open transactions must be edited before completion.");
            if (!_currentUser.CanAccessCorrection)
                throw new InvalidOperationException("You do not have access to the Correction screen.");

            var reasons = await _databaseService.GetReasonMastersAsync();
            foreach (var reason in reasons
                         .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                         .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase))
                CorrectionReasons.Add(reason);

            var items = await _databaseService.GetItemMastersAsync();
            foreach (var item in items.Where(x => string.Equals(x.DataAreaId?.Trim(), OriginalWeighment.DataAreaId?.Trim(), StringComparison.OrdinalIgnoreCase)
                                               && !string.Equals(x.IsDelete?.Trim(), "1", StringComparison.OrdinalIgnoreCase)
                                               && !string.Equals(x.IsDelete?.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                                      .OrderBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase))
                ItemMasters.Add(item);

            var uoms = await _databaseService.GetUnitOfMeasureMastersAsync();
            foreach (var symbol in uoms.Where(x => !string.IsNullOrWhiteSpace(x.symbol)
                                                && !string.Equals(x.IsDelete?.Trim(), "1", StringComparison.OrdinalIgnoreCase)
                                                && !string.Equals(x.IsDelete?.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                                       .Select(x => x.symbol.Trim())
                                       .Distinct(StringComparer.OrdinalIgnoreCase)
                                       .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                UomSymbols.Add(symbol);

            var originalLines = await _databaseService.GetWeighmentMaterialLinesAsync(OriginalWeighment.WeighmentId);
            BuildMaterialLineEdits(originalLines);

            var correctionToLoad = _correctionToOpen ?? await _databaseService.GetActiveCorrectionAsync(OriginalWeighment.WeighmentId);
            if (correctionToLoad != null)
            {
                Correction = CloneCorrection(correctionToLoad);
                var details = await _databaseService.GetCorrectionDetailsAsync(correctionToLoad.CorrectionId);
                ApplyExistingDetails(details);
                StatusMessage = $"Loaded {correctionToLoad.Status} correction {correctionToLoad.CorrectionNumber}.";
            }
            else
            {
                Correction.CorrectionNumber = await _databaseService.GenerateCorrectionNumberAsync(OriginalWeighment.DataAreaId);
                Correction.Reason = string.Empty;
                StatusMessage = _currentUser.CanSubmitCorrection
                    ? "New correction is ready. Enter changes, save as Draft or Submit for approval."
                    : "No active correction exists for this transaction. You have approval access only.";
            }

            NotifyAll();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            MessageBox.Show(this, ex.Message, "Correction", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BuildMaterialLineEdits(IEnumerable<WeighmentMaterialLine> lines)
    {
        MaterialLineEdits.Clear();
        foreach (var line in lines.OrderBy(x => x.LineNo))
        {
            MaterialLineEdits.Add(new CorrectionMaterialLineEdit
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

    private void ApplyExistingDetails(IEnumerable<WeighmentCorrectionDetail> details)
    {
        foreach (var detail in details.Where(x => string.Equals(x.DetailType, "Header", StringComparison.OrdinalIgnoreCase)))
        {
            switch (detail.FieldName)
            {
                case nameof(Weighment.VehicleNo):
                    OriginalWeighment.VehicleNo = detail.OriginalValue;
                    CorrectedWeighment.VehicleNo = detail.CorrectedValue;
                    break;
                case nameof(Weighment.DriverName):
                    OriginalWeighment.DriverName = detail.OriginalValue;
                    CorrectedWeighment.DriverName = detail.CorrectedValue;
                    break;
                case nameof(Weighment.ItemNumber):
                    OriginalWeighment.ItemNumber = detail.OriginalValue;
                    CorrectedWeighment.ItemNumber = detail.CorrectedValue;
                    break;
                case nameof(Weighment.ItemName):
                    OriginalWeighment.ItemName = detail.OriginalValue;
                    CorrectedWeighment.ItemName = detail.CorrectedValue;
                    break;
                case nameof(Weighment.FirstWeight):
                    if (decimal.TryParse(detail.OriginalValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var originalFirst))
                        OriginalWeighment.FirstWeight = originalFirst;
                    if (decimal.TryParse(detail.CorrectedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var correctedFirst))
                        _correctedFirstWeight = correctedFirst;
                    break;
                case nameof(Weighment.SecondWeight):
                    OriginalWeighment.SecondWeight = decimal.TryParse(detail.OriginalValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var originalSecond)
                        ? originalSecond
                        : null;
                    _correctedSecondWeight = decimal.TryParse(detail.CorrectedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var correctedSecond)
                        ? correctedSecond
                        : null;
                    break;
                case nameof(Weighment.Remarks):
                    OriginalWeighment.Remarks = detail.OriginalValue;
                    CorrectedWeighment.Remarks = detail.CorrectedValue;
                    break;
            }
        }

        foreach (var detail in details.Where(x => string.Equals(x.DetailType, "MaterialLine", StringComparison.OrdinalIgnoreCase)))
        {
            CorrectionMaterialLineEdit? edit = null;
            if (detail.OriginalMaterialLineId.HasValue)
                edit = MaterialLineEdits.FirstOrDefault(x => x.OriginalMaterialLineId == detail.OriginalMaterialLineId);

            if (edit == null)
            {
                edit = new CorrectionMaterialLineEdit
                {
                    LineNo = detail.LineNo,
                    OriginalMaterialLineId = detail.OriginalMaterialLineId,
                    OriginalItemMasterId = detail.OriginalItemMasterId,
                    OriginalItemNumber = detail.OriginalItemNumber,
                    OriginalItemName = detail.OriginalItemName,
                    OriginalUom = detail.OriginalUom,
                    OriginalExpectedQty = detail.OriginalExpectedQty,
                    OriginalRemarks = detail.OriginalRemarks,
                    ActionType = detail.ActionType
                };
                MaterialLineEdits.Add(edit);
            }
            else
            {
                // For approved historical corrections the active transaction line already contains
                // the corrected values. Restore the original side from the audit detail.
                edit.OriginalItemMasterId = detail.OriginalItemMasterId;
                edit.OriginalItemNumber = detail.OriginalItemNumber;
                edit.OriginalItemName = detail.OriginalItemName;
                edit.OriginalUom = detail.OriginalUom;
                edit.OriginalExpectedQty = detail.OriginalExpectedQty;
                edit.OriginalRemarks = detail.OriginalRemarks;
            }

            edit.ActionType = detail.ActionType;
            edit.CorrectedItemMasterId = detail.CorrectedItemMasterId;
            edit.CorrectedItemNumber = detail.CorrectedItemNumber;
            edit.CorrectedItemName = detail.CorrectedItemName;
            edit.CorrectedUom = detail.CorrectedUom;
            edit.CorrectedExpectedQty = detail.CorrectedExpectedQty;
            edit.CorrectedRemarks = detail.CorrectedRemarks;
        }

        if (OriginalWeighment.SecondWeight.HasValue)
            OriginalWeighment.NetWeight = Math.Abs(OriginalWeighment.SecondWeight.Value - OriginalWeighment.FirstWeight);

        // Keep the line order stable after reconstructing historical Remove/Add rows.
        var ordered = MaterialLineEdits.OrderBy(x => x.LineNo).ToList();
        MaterialLineEdits.Clear();
        foreach (var line in ordered) MaterialLineEdits.Add(line);
    }

    private List<WeighmentCorrectionDetail> BuildDetails()
    {
        var details = new List<WeighmentCorrectionDetail>();
        AddHeaderChange(details, nameof(Weighment.VehicleNo), OriginalWeighment.VehicleNo, CorrectedWeighment.VehicleNo);
        AddHeaderChange(details, nameof(Weighment.DriverName), OriginalWeighment.DriverName, CorrectedWeighment.DriverName);
        AddHeaderChange(details, nameof(Weighment.FirstWeight), OriginalWeighment.FirstWeight.ToString(CultureInfo.InvariantCulture), CorrectedFirstWeight.ToString(CultureInfo.InvariantCulture));
        AddHeaderChange(details, nameof(Weighment.SecondWeight), OriginalWeighment.SecondWeight?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, CorrectedSecondWeight?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
        AddHeaderChange(details, nameof(Weighment.Remarks), OriginalWeighment.Remarks, CorrectedWeighment.Remarks);

        foreach (var line in MaterialLineEdits)
        {
            var action = line.ActionType?.Trim() ?? "Modify";
            if (line.IsAddedLine)
                action = "Add";
            else if (string.Equals(action, "Add", StringComparison.OrdinalIgnoreCase))
                action = "Modify"; // An existing line can never be inserted again as a new line.

            var changed = line.IsAddedLine
                          || string.Equals(action, "Remove", StringComparison.OrdinalIgnoreCase)
                          || !string.Equals(line.OriginalItemNumber ?? string.Empty, line.CorrectedItemNumber ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                          || !string.Equals(line.OriginalItemName ?? string.Empty, line.CorrectedItemName ?? string.Empty, StringComparison.Ordinal)
                          || !string.Equals(line.OriginalUom ?? string.Empty, line.CorrectedUom ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                          || line.OriginalExpectedQty != line.CorrectedExpectedQty
                          || !string.Equals(line.OriginalRemarks ?? string.Empty, line.CorrectedRemarks ?? string.Empty, StringComparison.Ordinal);
            if (!changed) continue;

            if (!string.Equals(action, "Remove", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(line.CorrectedItemNumber))
                    throw new InvalidOperationException($"Material line {line.LineNo}: Item Number is mandatory.");
                if (string.IsNullOrWhiteSpace(line.CorrectedUom))
                    throw new InvalidOperationException($"Material line {line.LineNo}: UOM is mandatory.");
                if (!line.CorrectedExpectedQty.HasValue || line.CorrectedExpectedQty.Value < 0)
                    throw new InvalidOperationException($"Material line {line.LineNo}: Corrected Qty must be zero or greater.");
                if (!ItemMasters.Any(x => string.Equals(x.ItemNumber, line.CorrectedItemNumber, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"Material line {line.LineNo}: Corrected Item must be selected from Item Master.");
                if (!UomSymbols.Any(x => string.Equals(x, line.CorrectedUom, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"Material line {line.LineNo}: Corrected UOM must be selected from UOM Master.");
            }

            if (!line.IsAddedLine && !string.Equals(action, "Remove", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(line.OriginalItemNumber, line.CorrectedItemNumber, StringComparison.OrdinalIgnoreCase))
                action = "Replace";

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

        // A completed weighment that already has material lines must retain at least one active line after correction.
        var existingLineCount = MaterialLineEdits.Count(x => !x.IsAddedLine);
        if (existingLineCount > 0)
        {
            var remainingExisting = MaterialLineEdits.Count(x => !x.IsAddedLine && !string.Equals(x.ActionType?.Trim(), "Remove", StringComparison.OrdinalIgnoreCase));
            var addedLines = MaterialLineEdits.Count(x => x.IsAddedLine);
            if (remainingExisting + addedLines == 0)
                throw new InvalidOperationException("At least one material line must remain after correction.");
        }

        return details;
    }

    private static void AddHeaderChange(List<WeighmentCorrectionDetail> details, string fieldName, string? original, string? corrected)
    {
        original ??= string.Empty;
        corrected ??= string.Empty;
        if (string.Equals(original, corrected, StringComparison.Ordinal)) return;
        details.Add(new WeighmentCorrectionDetail
        {
            DetailType = "Header",
            FieldName = fieldName,
            OriginalValue = original,
            CorrectedValue = corrected
        });
    }

    private async void SaveDraft_Click(object sender, RoutedEventArgs e)
    {
        await SaveAsync(false);
    }

    private async void Submit_Click(object sender, RoutedEventArgs e)
    {
        await SaveAsync(true);
    }

    private async Task SaveAsync(bool submit)
    {
        try
        {
            if (!IsDraftEditable) throw new InvalidOperationException("You do not have permission to edit or submit this correction.");
            if (!_currentUser.CanCorrectWeight
                && (CorrectedFirstWeight != OriginalWeighment.FirstWeight || CorrectedSecondWeight != OriginalWeighment.SecondWeight))
                throw new InvalidOperationException("You do not have permission to correct weight values.");

            CorrectedWeighment.FirstWeight = CorrectedFirstWeight;
            CorrectedWeighment.SecondWeight = CorrectedSecondWeight;
            CorrectedWeighment.NetWeight = CorrectedNetWeight;
            var details = BuildDetails();
            Correction.CorrectionId = await _databaseService.SaveCorrectionAsync(Correction, details, submit, _currentUser.Username);
            StatusMessage = submit ? "Correction submitted for approval." : "Correction draft saved.";
            NotifyAll();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            MessageBox.Show(this, ex.Message, "Correction", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void Approve_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!CanApprove) throw new InvalidOperationException("You do not have permission to approve this correction.");
            var confirm = MessageBox.Show(this, $"Approve correction {Correction.CorrectionNumber}? The corrected values will become the active transaction values.", "Approve Correction", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;
            await _databaseService.ApproveCorrectionAsync(Correction.CorrectionId, _currentUser.Username);
            Correction.Status = "Approved";
            Correction.ApprovedRejectedBy = _currentUser.Username;
            Correction.ApprovalRejectedDateTime = DateTime.Now;
            StatusMessage = "Correction approved and applied to the transaction. Original values remain in correction history.";
            NotifyAll();
            DialogResult = true;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            MessageBox.Show(this, ex.Message, "Correction", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void Reject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!CanReject) throw new InvalidOperationException("You do not have permission to reject this correction.");
            var confirm = MessageBox.Show(this, $"Reject correction {Correction.CorrectionNumber}?", "Reject Correction", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;
            await _databaseService.RejectCorrectionAsync(Correction.CorrectionId, _currentUser.Username);
            Correction.Status = "Rejected";
            Correction.ApprovedRejectedBy = _currentUser.Username;
            Correction.ApprovalRejectedDateTime = DateTime.Now;
            StatusMessage = "Correction rejected. The transaction was not changed.";
            NotifyAll();
            DialogResult = true;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            MessageBox.Show(this, ex.Message, "Correction", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void AddLine_Click(object sender, RoutedEventArgs e)
    {
        if (!IsDraftEditable) return;
        var defaultUom = UomSymbols.FirstOrDefault() ?? string.Empty;
        var nextLine = MaterialLineEdits.Count == 0 ? 1 : MaterialLineEdits.Max(x => x.LineNo) + 1;
        var line = new CorrectionMaterialLineEdit
        {
            LineNo = nextLine,
            ActionType = "Add",
            CorrectedUom = defaultUom,
            CorrectedExpectedQty = 0m
        };
        MaterialLineEdits.Add(line);
        SelectedMaterialLineEdit = line;
    }

    private void RemoveAddedLine_Click(object sender, RoutedEventArgs e)
    {
        if (!IsDraftEditable || SelectedMaterialLineEdit == null) return;
        if (SelectedMaterialLineEdit.IsAddedLine)
            MaterialLineEdits.Remove(SelectedMaterialLineEdit);
        else
            SelectedMaterialLineEdit.ActionType = "Remove";
    }

    private void MaterialItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox combo || combo.DataContext is not CorrectionMaterialLineEdit line) return;
        if (combo.SelectedItem is not ItemMaster item) return;
        line.CorrectedItemMasterId = item.ItemMasterId;
        line.CorrectedItemNumber = item.ItemNumber;
        line.CorrectedItemName = item.ProductName;

        // When the item changes, start from the item's default UOM. The user can still override it from UOM Master.
        var defaultUom = !string.IsNullOrWhiteSpace(item.PurchaseUnit) ? item.PurchaseUnit
            : !string.IsNullOrWhiteSpace(item.SellUnit) ? item.SellUnit
            : !string.IsNullOrWhiteSpace(item.BOMUnit) ? item.BOMUnit
            : item.CostUnit;
        if (!string.IsNullOrWhiteSpace(defaultUom) && UomSymbols.Any(x => string.Equals(x, defaultUom, StringComparison.OrdinalIgnoreCase)))
            line.CorrectedUom = UomSymbols.First(x => string.Equals(x, defaultUom, StringComparison.OrdinalIgnoreCase));
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void NotifyAll()
    {
        OnPropertyChanged(nameof(Correction));
        OnPropertyChanged(nameof(CorrectedWeighment));
        OnPropertyChanged(nameof(CorrectedFirstWeight));
        OnPropertyChanged(nameof(CorrectedSecondWeight));
        OnPropertyChanged(nameof(CorrectedNetWeight));
        OnPropertyChanged(nameof(IsDraftEditable));
        OnPropertyChanged(nameof(IsFormReadOnly));
        OnPropertyChanged(nameof(IsWeightReadOnly));
        OnPropertyChanged(nameof(CanSaveDraft));
        OnPropertyChanged(nameof(CanSubmit));
        OnPropertyChanged(nameof(CanApprove));
        OnPropertyChanged(nameof(CanReject));
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
        CompanyName = source.CompanyName,
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

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

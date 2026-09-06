using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;

namespace WeightBridgeApp.ViewModels;

public sealed class QualityInspectionWorkspaceViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly OperatorMaster _currentUser;
    private readonly Func<string> _companyProvider;
    private readonly Func<Task>? _afterChange;
    private QualityInspection _inspectionForm = new();
    private QualityInspection? _selectedInspection;
    private Weighment _originalWeighment = new();
    private string _statusMessage = string.Empty;
    private bool _isLoadingSelection;

    public QualityInspectionWorkspaceViewModel(DatabaseService databaseService, OperatorMaster currentUser,
        Func<string> companyProvider, Func<Task>? afterChange = null)
    {
        _databaseService = databaseService;
        _currentUser = currentUser;
        _companyProvider = companyProvider;
        _afterChange = afterChange;

        NewInspectionCommand = new RelayCommand(NewInspectionAsync, () => CanProcessQualityInspection);
        LoadSlipCommand = new RelayCommand(OpenSlipLookupAsync, () => IsEditable);
        SaveCommand = new RelayCommand(() => SaveAsync(false), () => CanSave);
        CompleteCommand = new RelayCommand(() => SaveAsync(true), () => CanComplete);
        PrintCommand = new RelayCommand(PrintAsync, () => CanPrint);
        ReopenCommand = new RelayCommand(ReopenAsync, () => CanReopen);
        CancelCommand = new RelayCommand(CancelAsync, () => CanProcessQualityInspection);
        RefreshCommand = new RelayCommand(RefreshAsync);
    }

    public ObservableCollection<QualityInspection> Inspections { get; } = new();
    public ObservableCollection<QualityInspectionLine> Lines { get; } = new();
    public ObservableCollection<ReasonMaster> RejectionReasons { get; } = new();

    public RelayCommand NewInspectionCommand { get; }
    public RelayCommand LoadSlipCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand CompleteCommand { get; }
    public RelayCommand PrintCommand { get; }
    public RelayCommand ReopenCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public QualityInspection InspectionForm
    {
        get => _inspectionForm;
        private set
        {
            if (ReferenceEquals(_inspectionForm, value)) return;
            _inspectionForm.PropertyChanged -= InspectionForm_PropertyChanged;
            _inspectionForm = value;
            _inspectionForm.PropertyChanged += InspectionForm_PropertyChanged;
            OnPropertyChanged();
            NotifyState();
        }
    }

    public QualityInspection? SelectedInspection
    {
        get => _selectedInspection;
        set
        {
            if (SetProperty(ref _selectedInspection, value) && value != null && !_isLoadingSelection)
                _ = LoadInspectionAsync(value);
        }
    }

    public Weighment OriginalWeighment
    {
        get => _originalWeighment;
        private set => SetProperty(ref _originalWeighment, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public decimal AcceptedQtyTotal => Lines.Sum(x => x.AcceptedQty);
    public decimal RejectedQtyTotal => Lines.Sum(x => x.RejectedQty);
    public string InspectionMode => RejectedQtyTotal <= 0m
        ? "Quality Inspection"
        : AcceptedQtyTotal <= 0m ? "Full Rejection" : "Partial Rejection";

    public bool CanProcessQualityInspection => _currentUser.CanAccessQualityInspection && _currentUser.CanProcessQualityInspection;
    public bool IsEditable => CanProcessQualityInspection && string.Equals(InspectionForm.Status, "Draft", StringComparison.OrdinalIgnoreCase);
    public bool IsReadOnly => !IsEditable;
    public bool CanSave => IsEditable && InspectionForm.WeighmentId > 0 && Lines.Count > 0;
    public bool CanComplete => CanSave;
    public bool CanPrint => InspectionForm.QualityInspectionId > 0;
    public bool CanReopen => IsSupervisor
                             && CanProcessQualityInspection
                             && InspectionForm.QualityInspectionId > 0
                             && string.Equals(InspectionForm.Status, "Completed", StringComparison.OrdinalIgnoreCase);

    private bool IsSupervisor => ContainsRole(_currentUser.Role, "supervisor")
                                 || ContainsRole(_currentUser.Role, "administrator")
                                 || ContainsRole(_currentUser.Designation, "supervisor")
                                 || ContainsRole(_currentUser.Designation, "administrator");
    private static bool ContainsRole(string? value, string role) =>
        value?.Contains(role, StringComparison.OrdinalIgnoreCase) == true;
    private string CurrentCompany => string.IsNullOrWhiteSpace(_companyProvider()) ? "DAT" : _companyProvider().Trim();

    public async Task InitializeAsync()
    {
        await LoadReferenceDataAsync();
        await LoadInspectionsAsync();
        await PrepareInitialFormAsync();
    }

    public async Task RefreshForCompanyAsync()
    {
        await LoadReferenceDataAsync();
        await LoadInspectionsAsync();
        await PrepareInitialFormAsync();
    }

    public async Task RefreshListAsync()
    {
        await LoadReferenceDataAsync();
        await LoadInspectionsAsync();
    }

    public async Task StartForTransactionAsync(Weighment transaction)
    {
        if (!CanProcessQualityInspection)
            throw new InvalidOperationException("You do not have permission to process Quality Inspection transactions.");
        await NewInspectionAsync();
        await ApplySelectedTransactionAsync(transaction);
    }

    private async Task PrepareInitialFormAsync()
    {
        if (CanProcessQualityInspection)
        {
            await NewInspectionAsync();
            return;
        }

        var first = Inspections.FirstOrDefault();
        if (first != null)
        {
            _isLoadingSelection = true;
            SelectedInspection = first;
            _isLoadingSelection = false;
            await LoadInspectionAsync(first);
        }
        else
        {
            StatusMessage = "Quality Inspection access is read-only. No inspections are available.";
        }
    }

    private async Task LoadReferenceDataAsync()
    {
        RejectionReasons.Clear();
        var reasons = await _databaseService.GetReasonMastersAsync();
        foreach (var reason in reasons.Where(x => !string.IsNullOrWhiteSpace(x.Code))
                                      .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase))
            RejectionReasons.Add(reason);
    }

    private async Task LoadInspectionsAsync()
    {
        var selectedId = SelectedInspection?.QualityInspectionId ?? InspectionForm.QualityInspectionId;
        var rows = await _databaseService.GetQualityInspectionsAsync(CurrentCompany);
        Inspections.Clear();
        foreach (var row in rows) Inspections.Add(row);

        if (selectedId > 0)
        {
            _isLoadingSelection = true;
            SelectedInspection = Inspections.FirstOrDefault(x => x.QualityInspectionId == selectedId);
            _isLoadingSelection = false;
        }
    }

    private Task NewInspectionAsync()
    {
        _isLoadingSelection = true;
        SelectedInspection = null;
        _isLoadingSelection = false;
        ClearLines();
        OriginalWeighment = new Weighment { DataAreaId = CurrentCompany };
        InspectionForm = new QualityInspection
        {
            DataAreaId = CurrentCompany,
            QcUser = _currentUser.Username,
            InspectionDateTime = DateTime.Now,
            InspectionMode = "Quality Inspection",
            Status = "Draft",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        StatusMessage = "New QC inspection. Load a QC-enabled completed slip.";
        return Task.CompletedTask;
    }

    private async Task OpenSlipLookupAsync()
    {
        try
        {
            var lookup = new QualityInspectionSlipLookupWindow(_databaseService, CurrentCompany);
            var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (owner != null) lookup.Owner = owner;
            if (lookup.ShowDialog() == true && lookup.SelectedWeighment != null)
                await ApplySelectedTransactionAsync(lookup.SelectedWeighment);
        }
        catch (Exception ex)
        {
            ShowError("Slip selection error", ex);
        }
    }

    private async Task ApplySelectedTransactionAsync(Weighment transaction)
    {
        try
        {
            if (!await _databaseService.IsWeighmentQcEligibleAsync(transaction.WeighmentId, CurrentCompany))
                throw new InvalidOperationException("Only a QC-enabled Completed slip without an existing QC record can be loaded.");

            var materialLines = await _databaseService.GetWeighmentMaterialLinesAsync(transaction.WeighmentId);
            if (materialLines.Count == 0)
                throw new InvalidOperationException("The selected slip has no active material lines to inspect.");

            OriginalWeighment = CloneWeighment(transaction);
            InspectionForm.WeighmentId = transaction.WeighmentId;
            InspectionForm.DataAreaId = transaction.DataAreaId;
            InspectionForm.SlipNumber = string.IsNullOrWhiteSpace(transaction.SlipNumber) ? transaction.TicketNo : transaction.SlipNumber;
            InspectionForm.NetWeight = transaction.NetWeight ?? 0m;
            InspectionForm.InspectionMode = "Quality Inspection";
            ClearLines();
            foreach (var source in materialLines.OrderBy(x => x.LineNo))
            {
                AddLine(new QualityInspectionLine
                {
                    OriginalMaterialLineId = source.MaterialLineId,
                    LineNo = source.LineNo,
                    ItemMasterId = source.ItemMasterId,
                    ItemNumber = source.ItemNumber,
                    ItemName = source.ItemName,
                    Uom = source.Uom,
                    OriginalQty = source.ExpectedQty,
                    AcceptedQty = source.ExpectedQty
                });
            }

            OnPropertyChanged(nameof(InspectionForm));
            NotifyLineTotals();
            StatusMessage = $"Loaded QC-enabled slip {InspectionForm.SlipNumber}. Enter Accepted Qty for each line; Rejected Qty is calculated automatically.";
            NotifyState();
        }
        catch (Exception ex)
        {
            ShowError("Quality Inspection", ex);
        }
    }

    private async Task LoadInspectionAsync(QualityInspection selected)
    {
        try
        {
            var weighment = await _databaseService.GetWeighmentByIdAsync(selected.WeighmentId, selected.DataAreaId);
            if (weighment == null) throw new InvalidOperationException("The original slip could not be found.");

            InspectionForm = CloneInspection(selected);
            OriginalWeighment = CloneWeighment(weighment);
            ClearLines();
            var lines = await _databaseService.GetQualityInspectionLinesAsync(selected.QualityInspectionId);
            foreach (var line in lines) AddLine(line);
            NotifyLineTotals();
            StatusMessage = $"Loaded QC {InspectionForm.QcNumber} ({InspectionForm.Status}).";
            NotifyState();
        }
        catch (Exception ex)
        {
            ShowError("QC load error", ex);
        }
    }

    private async Task SaveAsync(bool complete)
    {
        try
        {
            if (!IsEditable) throw new InvalidOperationException("This QC inspection is read-only.");
            InspectionForm.DataAreaId = CurrentCompany;
            InspectionForm.InspectionMode = InspectionMode;
            InspectionForm.QualityInspectionId = await _databaseService.SaveQualityInspectionAsync(
                InspectionForm, Lines, complete, _currentUser.Username);

            await LoadInspectionsAsync();
            var saved = Inspections.FirstOrDefault(x => x.QualityInspectionId == InspectionForm.QualityInspectionId);
            if (saved != null)
            {
                _isLoadingSelection = true;
                SelectedInspection = saved;
                _isLoadingSelection = false;
                await LoadInspectionAsync(saved);
            }
            if (_afterChange != null) await _afterChange();
            StatusMessage = complete
                ? $"QC {InspectionForm.QcNumber} completed as {InspectionForm.InspectionMode}."
                : $"QC {InspectionForm.QcNumber} saved as Draft.";
        }
        catch (Exception ex)
        {
            ShowError("Quality Inspection", ex);
        }
    }

    private async Task ReopenAsync()
    {
        try
        {
            if (!CanReopen) throw new InvalidOperationException("Only a Supervisor or Administrator can reopen a Completed QC inspection.");
            var result = MessageBox.Show(
                $"Reopen QC {InspectionForm.QcNumber}?\n\nThis supervisor action is written to the QC audit trail.",
                "Reopen Quality Inspection", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            var id = InspectionForm.QualityInspectionId;
            await _databaseService.ReopenQualityInspectionAsync(id, _currentUser.Username);
            await LoadInspectionsAsync();
            var reopened = Inspections.FirstOrDefault(x => x.QualityInspectionId == id);
            if (reopened != null) await LoadInspectionAsync(reopened);
            StatusMessage = $"QC {InspectionForm.QcNumber} reopened by supervisor. Remarks are required when completing the override.";
        }
        catch (Exception ex)
        {
            ShowError("Reopen Quality Inspection", ex);
        }
    }

    private Task PrintAsync()
    {
        try
        {
            if (!CanPrint) throw new InvalidOperationException("Save the QC inspection before printing.");
            var document = BuildPrintDocument();
            var dialog = new PrintDialog();
            if (dialog.ShowDialog() == true)
                dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, $"QC {InspectionForm.QcNumber}");
        }
        catch (Exception ex)
        {
            ShowError("Print QC Slip", ex);
        }
        return Task.CompletedTask;
    }

    private FlowDocument BuildPrintDocument()
    {
        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 11,
            PagePadding = new Thickness(42),
            ColumnWidth = double.PositiveInfinity
        };
        document.Blocks.Add(new Paragraph(new Run("QUALITY INSPECTION SLIP"))
        {
            FontSize = 19,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center
        });
        document.Blocks.Add(new Paragraph(new Run(
            $"QC Number: {InspectionForm.QcNumber}\nSlip Number: {InspectionForm.SlipNumber}\n" +
            $"QC User: {InspectionForm.QcUser}\nInspection Date/Time: {InspectionForm.InspectionDateTime:dd/MM/yyyy HH:mm:ss}\n" +
            $"Mode: {InspectionForm.InspectionMode}\nNet Weight: {InspectionForm.NetWeight:N3}\nStatus: {InspectionForm.Status}")));

        var table = new Table { CellSpacing = 0 };
        foreach (var width in new[] { 45d, 90d, 150d, 75d, 85d, 85d, 105d })
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });
        var group = new TableRowGroup();
        table.RowGroups.Add(group);
        group.Rows.Add(PrintRow(true, "Line", "Item", "Item Name", "Original", "Accepted", "Rejected", "Reason"));
        foreach (var line in Lines.OrderBy(x => x.LineNo))
            group.Rows.Add(PrintRow(false, line.LineNo.ToString(), line.ItemNumber, line.ItemName,
                line.OriginalQty.ToString("N3"), line.AcceptedQty.ToString("N3"), line.RejectedQty.ToString("N3"), line.RejectionReason));
        document.Blocks.Add(table);
        document.Blocks.Add(new Paragraph(new Run("QC Remarks: " + (InspectionForm.QcRemarks ?? string.Empty))) { Margin = new Thickness(0, 14, 0, 0) });
        return document;
    }

    private static TableRow PrintRow(bool header, params string[] values)
    {
        var row = new TableRow { FontWeight = header ? FontWeights.SemiBold : FontWeights.Normal };
        foreach (var value in values)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(value ?? string.Empty)) { Margin = new Thickness(3) })
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(2)
            });
        }
        return row;
    }

    private async Task CancelAsync()
    {
        if (InspectionForm.WeighmentId > 0 && IsEditable)
        {
            var result = MessageBox.Show("Clear the current QC form? Unsaved changes will be discarded.",
                "Cancel Quality Inspection", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
        }
        await NewInspectionAsync();
    }

    private async Task RefreshAsync()
    {
        await RefreshListAsync();
        StatusMessage = "QC inspections refreshed.";
    }

    private void AddLine(QualityInspectionLine line)
    {
        line.PropertyChanged += Line_PropertyChanged;
        Lines.Add(line);
    }

    private void ClearLines()
    {
        foreach (var line in Lines) line.PropertyChanged -= Line_PropertyChanged;
        Lines.Clear();
        NotifyLineTotals();
    }

    private void Line_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(QualityInspectionLine.AcceptedQty) or nameof(QualityInspectionLine.RejectedQty))
        {
            InspectionForm.InspectionMode = InspectionMode;
            NotifyLineTotals();
        }
    }

    private void InspectionForm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    private void NotifyLineTotals()
    {
        OnPropertyChanged(nameof(AcceptedQtyTotal));
        OnPropertyChanged(nameof(RejectedQtyTotal));
        OnPropertyChanged(nameof(InspectionMode));
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    private void NotifyState()
    {
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(IsReadOnly));
        OnPropertyChanged(nameof(CanProcessQualityInspection));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanComplete));
        OnPropertyChanged(nameof(CanPrint));
        OnPropertyChanged(nameof(CanReopen));
        NotifyLineTotals();
    }

    private void ShowError(string title, Exception ex)
    {
        StatusMessage = ex.Message;
        MessageBox.Show(ex.Message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private static QualityInspection CloneInspection(QualityInspection source) => new()
    {
        QualityInspectionId = source.QualityInspectionId,
        DataAreaId = source.DataAreaId,
        WeighmentId = source.WeighmentId,
        SlipNumber = source.SlipNumber,
        QcNumber = source.QcNumber,
        QcUser = source.QcUser,
        InspectionDateTime = source.InspectionDateTime,
        InspectionMode = source.InspectionMode,
        NetWeight = source.NetWeight,
        QcRemarks = source.QcRemarks,
        Status = source.Status,
        CompletedBy = source.CompletedBy,
        CompletedDateTime = source.CompletedDateTime,
        ReopenCount = source.ReopenCount,
        LastReopenedBy = source.LastReopenedBy,
        LastReopenedDateTime = source.LastReopenedDateTime,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
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
        VehicleNo = source.VehicleNo,
        DriverName = source.DriverName,
        ItemNumber = source.ItemNumber,
        ItemName = source.ItemName,
        NetWeight = source.NetWeight,
        Status = source.Status,
        TransactionDateTime = source.TransactionDateTime
    };
}

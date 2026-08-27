using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class CorrectionMaterialLineEdit : INotifyPropertyChanged
{
    private string _actionType = "Modify";
    private int? _correctedItemMasterId;
    private string _correctedItemNumber = string.Empty;
    private string _correctedItemName = string.Empty;
    private string _correctedUom = string.Empty;
    private decimal? _correctedExpectedQty;
    private string _correctedRemarks = string.Empty;

    public int LineNo { get; set; }
    public int? OriginalMaterialLineId { get; set; }
    public int? OriginalItemMasterId { get; set; }
    public string OriginalItemNumber { get; set; } = string.Empty;
    public string OriginalItemName { get; set; } = string.Empty;
    public string OriginalUom { get; set; } = string.Empty;
    public decimal? OriginalExpectedQty { get; set; }
    public string OriginalRemarks { get; set; } = string.Empty;

    public string ActionType { get => _actionType; set { _actionType = value; OnPropertyChanged(); } }
    public int? CorrectedItemMasterId { get => _correctedItemMasterId; set { _correctedItemMasterId = value; OnPropertyChanged(); } }
    public string CorrectedItemNumber { get => _correctedItemNumber; set { _correctedItemNumber = value; OnPropertyChanged(); } }
    public string CorrectedItemName { get => _correctedItemName; set { _correctedItemName = value; OnPropertyChanged(); } }
    public string CorrectedUom { get => _correctedUom; set { _correctedUom = value; OnPropertyChanged(); } }
    public decimal? CorrectedExpectedQty { get => _correctedExpectedQty; set { _correctedExpectedQty = value; OnPropertyChanged(); } }
    public string CorrectedRemarks { get => _correctedRemarks; set { _correctedRemarks = value; OnPropertyChanged(); } }

    public bool IsAddedLine => !OriginalMaterialLineId.HasValue;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

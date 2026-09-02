using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class QualityInspectionLine : INotifyPropertyChanged
{
    private decimal _acceptedQty;
    private decimal? _moisturePercent;
    private decimal? _contaminationPercent;
    private string _rejectionReason = string.Empty;
    private string _remarks = string.Empty;

    public int QualityInspectionLineId { get; set; }
    public int QualityInspectionId { get; set; }
    public int? OriginalMaterialLineId { get; set; }
    public int LineNo { get; set; }
    public int? ItemMasterId { get; set; }
    public string ItemNumber { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Uom { get; set; } = string.Empty;
    public decimal OriginalQty { get; set; }

    public decimal AcceptedQty
    {
        get => _acceptedQty;
        set
        {
            if (_acceptedQty == value) return;
            _acceptedQty = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RejectedQty));
            OnPropertyChanged(nameof(HasRejection));
        }
    }

    public decimal RejectedQty => OriginalQty - AcceptedQty;
    public bool HasRejection => RejectedQty > 0m;

    public decimal? MoisturePercent
    {
        get => _moisturePercent;
        set { if (_moisturePercent != value) { _moisturePercent = value; OnPropertyChanged(); } }
    }

    public decimal? ContaminationPercent
    {
        get => _contaminationPercent;
        set { if (_contaminationPercent != value) { _contaminationPercent = value; OnPropertyChanged(); } }
    }

    public string RejectionReason
    {
        get => _rejectionReason;
        set
        {
            if (string.Equals(_rejectionReason, value, StringComparison.Ordinal)) return;
            _rejectionReason = value;
            OnPropertyChanged();
        }
    }

    public string Remarks
    {
        get => _remarks;
        set
        {
            if (string.Equals(_remarks, value, StringComparison.Ordinal)) return;
            _remarks = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

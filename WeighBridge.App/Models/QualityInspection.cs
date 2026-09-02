using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class QualityInspection : INotifyPropertyChanged
{
    private string _qcRemarks = string.Empty;

    public int QualityInspectionId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string QcNumber { get; set; } = string.Empty;
    public string QcUser { get; set; } = string.Empty;
    public DateTime? InspectionDateTime { get; set; }
    public string InspectionMode { get; set; } = "Quality Inspection";
    public decimal NetWeight { get; set; }
    public string QcRemarks
    {
        get => _qcRemarks;
        set
        {
            if (string.Equals(_qcRemarks, value, StringComparison.Ordinal)) return;
            _qcRemarks = value;
            OnPropertyChanged();
        }
    }
    public string Status { get; set; } = "Draft";
    public string CompletedBy { get; set; } = string.Empty;
    public DateTime? CompletedDateTime { get; set; }
    public int ReopenCount { get; set; }
    public string LastReopenedBy { get; set; } = string.Empty;
    public DateTime? LastReopenedDateTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class WeighmentReturnDetails : INotifyPropertyChanged
{
    private string _returnType = string.Empty;
    private string _vendorAccount = string.Empty;
    private string _vendorName = string.Empty;
    private string _customerAccount = string.Empty;
    private string _customerName = string.Empty;
    private string _fromLegalEntity = string.Empty;
    private string _toLegalEntity = string.Empty;
    private string _originalSlipNumber = string.Empty;
    private string _returnReference = string.Empty;
    private string _returnReason = string.Empty;
    private string _source = string.Empty;
    private string _destination = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int ReturnDetailsId { get; set; }
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string DataAreaId { get; set; } = string.Empty;
    public string ReturnType { get => _returnType; set => SetProperty(ref _returnType, value ?? string.Empty); }
    public string VendorAccount { get => _vendorAccount; set => SetProperty(ref _vendorAccount, value ?? string.Empty); }
    public string VendorName { get => _vendorName; set => SetProperty(ref _vendorName, value ?? string.Empty); }
    public string CustomerAccount { get => _customerAccount; set => SetProperty(ref _customerAccount, value ?? string.Empty); }
    public string CustomerName { get => _customerName; set => SetProperty(ref _customerName, value ?? string.Empty); }
    public string FromLegalEntity { get => _fromLegalEntity; set => SetProperty(ref _fromLegalEntity, value ?? string.Empty); }
    public string ToLegalEntity { get => _toLegalEntity; set => SetProperty(ref _toLegalEntity, value ?? string.Empty); }
    public string OriginalSlipNumber { get => _originalSlipNumber; set => SetProperty(ref _originalSlipNumber, value ?? string.Empty); }
    public string ReturnReference { get => _returnReference; set => SetProperty(ref _returnReference, value ?? string.Empty); }
    public string ReturnReason { get => _returnReason; set => SetProperty(ref _returnReason, value ?? string.Empty); }
    public string Source { get => _source; set => SetProperty(ref _source, value ?? string.Empty); }
    public string Destination { get => _destination; set => SetProperty(ref _destination, value ?? string.Empty); }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

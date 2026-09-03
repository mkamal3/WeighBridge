using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class WeighmentGeneralWeighingServiceDetails : INotifyPropertyChanged
{
    private string _externalPartyName = string.Empty;
    private string _customerAccount = string.Empty;
    private string _customerName = string.Empty;
    private string _mobileNumber = string.Empty;
    private string _materialDescription = string.Empty;
    private string _serviceMode = string.Empty;
    private decimal _serviceCharge;
    private string _currency = string.Empty;
    private string _paymentStatus = string.Empty;
    private string _receiptNumber = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int GeneralWeighingServiceDetailsId { get; set; }
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string DataAreaId { get; set; } = string.Empty;

    public string ExternalPartyName { get => _externalPartyName; set => SetProperty(ref _externalPartyName, value ?? string.Empty); }
    public string CustomerAccount { get => _customerAccount; set => SetProperty(ref _customerAccount, value ?? string.Empty); }
    public string CustomerName { get => _customerName; set => SetProperty(ref _customerName, value ?? string.Empty); }
    public string MobileNumber { get => _mobileNumber; set => SetProperty(ref _mobileNumber, value ?? string.Empty); }
    public string MaterialDescription { get => _materialDescription; set => SetProperty(ref _materialDescription, value ?? string.Empty); }
    public string ServiceMode { get => _serviceMode; set => SetProperty(ref _serviceMode, value ?? string.Empty); }
    public decimal ServiceCharge { get => _serviceCharge; set => SetProperty(ref _serviceCharge, value); }
    public string Currency { get => _currency; set => SetProperty(ref _currency, value ?? string.Empty); }
    public string PaymentStatus { get => _paymentStatus; set => SetProperty(ref _paymentStatus, value ?? string.Empty); }
    public string ReceiptNumber { get => _receiptNumber; set => SetProperty(ref _receiptNumber, value ?? string.Empty); }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

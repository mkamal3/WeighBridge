using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class WeighmentSalesDispatchDetails : INotifyPropertyChanged
{
    private string _salesSubtype = string.Empty;
    private string _customerAccount = string.Empty;
    private string _customerName = string.Empty;
    private string _walkInCustomer = string.Empty;
    private string _salesReference = string.Empty;
    private string _source = string.Empty;
    private string _destination = string.Empty;
    private string _paymentStatus = string.Empty;
    private string _receiptNumber = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int SalesDispatchDetailsId { get; set; }
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string DataAreaId { get; set; } = string.Empty;
    public string SalesSubtype { get => _salesSubtype; set => SetProperty(ref _salesSubtype, value ?? string.Empty); }
    public string CustomerAccount { get => _customerAccount; set => SetProperty(ref _customerAccount, value ?? string.Empty); }
    public string CustomerName { get => _customerName; set => SetProperty(ref _customerName, value ?? string.Empty); }
    public string WalkInCustomer { get => _walkInCustomer; set => SetProperty(ref _walkInCustomer, value ?? string.Empty); }
    public string SalesReference { get => _salesReference; set => SetProperty(ref _salesReference, value ?? string.Empty); }
    public string Source { get => _source; set => SetProperty(ref _source, value ?? string.Empty); }
    public string Destination { get => _destination; set => SetProperty(ref _destination, value ?? string.Empty); }
    public string PaymentStatus { get => _paymentStatus; set => SetProperty(ref _paymentStatus, value ?? string.Empty); }
    public string ReceiptNumber { get => _receiptNumber; set => SetProperty(ref _receiptNumber, value ?? string.Empty); }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

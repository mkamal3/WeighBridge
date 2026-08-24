using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class WeighmentContractCollectionDetails : INotifyPropertyChanged
{
    private string _vendorAccount = string.Empty;
    private string _vendorName = string.Empty;
    private string _invoiceAccount = string.Empty;
    private string _invoiceAccountName = string.Empty;
    private string _contractNumber = string.Empty;
    private string _collectionLocation = string.Empty;
    private string _destination = string.Empty;
    private string _billingBasis = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int ContractCollectionDetailsId { get; set; }
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string DataAreaId { get; set; } = string.Empty;

    public string VendorAccount
    {
        get => _vendorAccount;
        set => SetProperty(ref _vendorAccount, value ?? string.Empty);
    }

    public string VendorName
    {
        get => _vendorName;
        set => SetProperty(ref _vendorName, value ?? string.Empty);
    }

    public string InvoiceAccount
    {
        get => _invoiceAccount;
        set => SetProperty(ref _invoiceAccount, value ?? string.Empty);
    }

    public string InvoiceAccountName
    {
        get => _invoiceAccountName;
        set => SetProperty(ref _invoiceAccountName, value ?? string.Empty);
    }

    public string ContractNumber
    {
        get => _contractNumber;
        set => SetProperty(ref _contractNumber, value ?? string.Empty);
    }

    public string CollectionLocation
    {
        get => _collectionLocation;
        set => SetProperty(ref _collectionLocation, value ?? string.Empty);
    }

    public string Destination
    {
        get => _destination;
        set => SetProperty(ref _destination, value ?? string.Empty);
    }

    public string BillingBasis
    {
        get => _billingBasis;
        set => SetProperty(ref _billingBasis, value ?? string.Empty);
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

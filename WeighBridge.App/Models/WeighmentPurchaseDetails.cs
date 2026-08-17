using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class WeighmentPurchaseDetails : INotifyPropertyChanged
{
    private string _purchaseSubtype = string.Empty;
    private string _vendorAccount = string.Empty;
    private string _vendorName = string.Empty;
    private bool _walkInVendor;
    private string _supplierDriverName = string.Empty;
    private string _purchaseContractReference = string.Empty;
    private string _source = string.Empty;
    private string _destination = string.Empty;
    private bool _focFlag;
    private decimal _rateAmount;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int PurchaseDetailsId { get; set; }
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string DataAreaId { get; set; } = string.Empty;

    public string PurchaseSubtype
    {
        get => _purchaseSubtype;
        set => SetProperty(ref _purchaseSubtype, value ?? string.Empty);
    }

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

    public bool WalkInVendor
    {
        get => _walkInVendor;
        set
        {
            if (!SetProperty(ref _walkInVendor, value))
                return;

            if (value)
            {
                VendorAccount = "WALK-IN";
                VendorName = "Walk-in Vendor";
            }
            else if (string.Equals(VendorAccount, "WALK-IN", StringComparison.OrdinalIgnoreCase))
            {
                VendorAccount = string.Empty;
                VendorName = string.Empty;
            }

            OnPropertyChanged(nameof(IsVendorSelectable));
        }
    }

    public bool IsVendorSelectable => !WalkInVendor;

    public string SupplierDriverName
    {
        get => _supplierDriverName;
        set => SetProperty(ref _supplierDriverName, value ?? string.Empty);
    }

    public string PurchaseContractReference
    {
        get => _purchaseContractReference;
        set => SetProperty(ref _purchaseContractReference, value ?? string.Empty);
    }

    public string Source
    {
        get => _source;
        set => SetProperty(ref _source, value ?? string.Empty);
    }

    public string Destination
    {
        get => _destination;
        set => SetProperty(ref _destination, value ?? string.Empty);
    }

    public bool FocFlag
    {
        get => _focFlag;
        set
        {
            if (!SetProperty(ref _focFlag, value))
                return;

            if (value)
                RateAmount = 0;

            OnPropertyChanged(nameof(IsRateAmountEditable));
        }
    }

    public bool IsRateAmountEditable => !FocFlag;

    public decimal RateAmount
    {
        get => _rateAmount;
        set => SetProperty(ref _rateAmount, value);
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

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class WeighmentTransferDetails : INotifyPropertyChanged
{
    private string _transferDirection = string.Empty;
    private string _fromLegalEntity = string.Empty;
    private string _toLegalEntity = string.Empty;
    private string _fromLocation = string.Empty;
    private string _toLocation = string.Empty;
    private string _transferReference = string.Empty;
    private string _sendingSlipReference = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int TransferDetailsId { get; set; }
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string DataAreaId { get; set; } = string.Empty;
    public string TransferDirection { get => _transferDirection; set => SetProperty(ref _transferDirection, value ?? string.Empty); }
    public string FromLegalEntity { get => _fromLegalEntity; set => SetProperty(ref _fromLegalEntity, value ?? string.Empty); }
    public string ToLegalEntity { get => _toLegalEntity; set => SetProperty(ref _toLegalEntity, value ?? string.Empty); }
    public string FromLocation { get => _fromLocation; set => SetProperty(ref _fromLocation, value ?? string.Empty); }
    public string ToLocation { get => _toLocation; set => SetProperty(ref _toLocation, value ?? string.Empty); }
    public string TransferReference { get => _transferReference; set => SetProperty(ref _transferReference, value ?? string.Empty); }
    public string SendingSlipReference { get => _sendingSlipReference; set => SetProperty(ref _sendingSlipReference, value ?? string.Empty); }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

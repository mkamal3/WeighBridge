using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class WeighmentDisposalDetails : INotifyPropertyChanged
{
    private string _disposalType = string.Empty;
    private string _source = string.Empty;
    private string _disposalDestination = string.Empty;
    private string _reason = string.Empty;
    private string _permitManifestNumber = string.Empty;
    private string _authorizedBy = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int DisposalDetailsId { get; set; }
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string DataAreaId { get; set; } = string.Empty;
    public string DisposalType { get => _disposalType; set => SetProperty(ref _disposalType, value ?? string.Empty); }
    public string Source { get => _source; set => SetProperty(ref _source, value ?? string.Empty); }
    public string DisposalDestination { get => _disposalDestination; set => SetProperty(ref _disposalDestination, value ?? string.Empty); }
    public string Reason { get => _reason; set => SetProperty(ref _reason, value ?? string.Empty); }
    public string PermitManifestNumber { get => _permitManifestNumber; set => SetProperty(ref _permitManifestNumber, value ?? string.Empty); }
    public string AuthorizedBy { get => _authorizedBy; set => SetProperty(ref _authorizedBy, value ?? string.Empty); }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

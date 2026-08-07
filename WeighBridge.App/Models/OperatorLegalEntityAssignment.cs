using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class OperatorLegalEntityAssignment : INotifyPropertyChanged
{
    private string _dataAreaId = string.Empty;
    private string _legalEntityName = string.Empty;
    private bool _isDefault;

    public int Id { get; set; }
    public int OperatorId { get; set; }

    public string DataAreaId
    {
        get => _dataAreaId;
        set
        {
            if (_dataAreaId == value)
                return;
            _dataAreaId = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public string LegalEntityName
    {
        get => _legalEntityName;
        set
        {
            if (_legalEntityName == value)
                return;
            _legalEntityName = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public bool IsDefault
    {
        get => _isDefault;
        set
        {
            if (_isDefault == value)
                return;
            _isDefault = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

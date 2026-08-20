using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeightBridgeApp.Models;

public class WeighmentProductionDetails : INotifyPropertyChanged
{
    private string _productionMovement = string.Empty;
    private string _productionOrderReference = string.Empty;
    private string _productionLine = string.Empty;
    private string _warehouseLocation = string.Empty;
    private string _batchNumber = string.Empty;
    private int _numberOfRollsUnits;
    private string _gradeGsmWidth = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int ProductionDetailsId { get; set; }
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string DataAreaId { get; set; } = string.Empty;
    public string ProductionMovement { get => _productionMovement; set => SetProperty(ref _productionMovement, value ?? string.Empty); }
    public string ProductionOrderReference { get => _productionOrderReference; set => SetProperty(ref _productionOrderReference, value ?? string.Empty); }
    // Intentionally manual. There is no Production Line Master.
    public string ProductionLine { get => _productionLine; set => SetProperty(ref _productionLine, value ?? string.Empty); }
    public string WarehouseLocation { get => _warehouseLocation; set => SetProperty(ref _warehouseLocation, value ?? string.Empty); }
    public string BatchNumber { get => _batchNumber; set => SetProperty(ref _batchNumber, value ?? string.Empty); }
    public int NumberOfRollsUnits { get => _numberOfRollsUnits; set => SetProperty(ref _numberOfRollsUnits, value); }
    public string GradeGsmWidth { get => _gradeGsmWidth; set => SetProperty(ref _gradeGsmWidth, value ?? string.Empty); }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

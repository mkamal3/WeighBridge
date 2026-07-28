namespace WeightBridgeApp.Models;

public class WeighbridgeMaster
{
    public int WeighbridgeId { get; set; }
    public string WeighbridgeCode { get; set; } = string.Empty;
    public string WeighbridgeName { get; set; } = string.Empty;
    public string WeighbridgeDisplay => string.IsNullOrWhiteSpace(WeighbridgeName) ? WeighbridgeCode : $"{WeighbridgeCode} - {WeighbridgeName}";
    public string Description { get; set; } = string.Empty;
    public string PlantSite { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string WarehouseAddress { get; set; } = string.Empty;
    public string WeighbridgeType { get; set; } = string.Empty;
    public string ScaleType { get; set; } = string.Empty;
    public decimal ScaleCapacity { get; set; }
    public string CapacityUnit { get; set; } = "kg";
    public decimal? MinimumWeight { get; set; }
    public decimal? WeightIncrement { get; set; }
    public int? WeightStabilityTime { get; set; }
    public string ScaleIpAddress { get; set; } = string.Empty;
    public int TcpPort { get; set; } = 4001;
    public string ScaleComPort { get; set; } = string.Empty;
    public int BaudRate { get; set; } = 9600;
    public string Parity { get; set; } = "None";
    public int DataBits { get; set; } = 8;
    public string StopBits { get; set; } = "One";
    public string CommunicationType { get; set; } = "Mock";
    public string ScaleManufacturer { get; set; } = string.Empty;
    public string ScaleModel { get; set; } = string.Empty;
    public string ScaleSerialNumber { get; set; } = string.Empty;
    public string CalibrationCertificateNo { get; set; } = string.Empty;
    public DateTime? LastCalibrationDate { get; set; }
    public DateTime? NextCalibrationDate { get; set; }
    public string Printer { get; set; } = string.Empty;
    public bool CameraAvailable { get; set; }
    public bool AnprAvailable { get; set; }
    public bool TrafficLightAvailable { get; set; }
    public bool BoomBarrierAvailable { get; set; }
    public bool CctvAvailable { get; set; }
    public string DefaultTicketTemplate { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = string.Empty;
    public string DefaultOperator { get; set; } = string.Empty;
    public string AllowedOperators { get; set; } = string.Empty;
    public string OperatingStatus { get; set; } = "Active";
    public DateTime? EffectiveFrom { get; set; } = DateTime.Today;
    // Availability is controlled by OperatingStatus. Kept only for backward compatibility with old databases.
    public bool IsActive { get => string.Equals(OperatingStatus, "Active", StringComparison.OrdinalIgnoreCase); set { } }
    public string Remarks { get; set; } = string.Empty;
}

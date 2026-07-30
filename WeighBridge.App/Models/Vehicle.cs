namespace WeightBridgeApp.Models;

public class Vehicle
{
    public int VehicleId { get; set; }

    // Backward-compatible alias used by existing weighment lookup code.
    public string VehicleNo
    {
        get => PlateNumber;
        set => PlateNumber = value;
    }

    public string PlateNumber { get; set; } = string.Empty;
    public string PlateEmirate { get; set; } = string.Empty;
    public string PlateCategory { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string OwnershipType { get; set; } = string.Empty;
    public string OwnerPartyAccount { get; set; } = string.Empty;
    public string Transporter { get; set; } = string.Empty;
    public decimal Capacity { get; set; }
    public string DefaultDriver { get; set; } = string.Empty;
    public DateTime? RegistrationExpiryDate { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public string LegalEntity { get => DataAreaId; set => DataAreaId = value; }
    public string Status { get; set; } = "Active";
    // Availability is controlled by Status. Kept only for backward compatibility with old databases.
    public bool IsActive { get => string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase); set { } }
}

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
    public string LegalEntity { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
}

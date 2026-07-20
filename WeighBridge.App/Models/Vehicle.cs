namespace WeightBridgeApp.Models;

public class Vehicle
{
    public int VehicleId { get; set; }
    public string VehicleNo { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string ContactNo { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

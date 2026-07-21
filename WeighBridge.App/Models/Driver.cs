namespace WeightBridgeApp.Models;

public class Driver
{
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string CNIC { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public string LicenseNo { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

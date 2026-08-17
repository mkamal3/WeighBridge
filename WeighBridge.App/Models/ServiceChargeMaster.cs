namespace WeightBridgeApp.Models;

public class ServiceChargeMaster
{
    public int ServiceChargeMasterId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public string ServiceMode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Validity { get; set; } = string.Empty;
}

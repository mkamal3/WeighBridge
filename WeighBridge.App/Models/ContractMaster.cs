namespace WeightBridgeApp.Models;

public class ContractMaster
{
    public int ContractMasterId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Parties { get; set; } = string.Empty;
    public string Locations { get; set; } = string.Empty;
    public string BillingBasis { get; set; } = string.Empty;
    public string Validity { get; set; } = string.Empty;
}

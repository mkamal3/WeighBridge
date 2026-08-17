namespace WeightBridgeApp.Models;

public class TransactionTypeMaster
{
    public int TransactionTypeMasterId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Form { get; set; } = string.Empty;
}

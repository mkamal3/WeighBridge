namespace WeightBridgeApp.Models;

public class Party
{
    public int PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string PartyType { get; set; } = "Customer";
}

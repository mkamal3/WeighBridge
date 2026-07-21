namespace WeightBridgeApp.Models;

public class Party
{
    public int PartyId { get; set; }
    public string PartyAccount { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string PartyType { get; set; } = "Customer";
    public string PartyDisplay => string.IsNullOrWhiteSpace(PartyAccount) ? PartyName : $"{PartyAccount} - {PartyName}";
}

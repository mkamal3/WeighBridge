namespace WeightBridgeApp.Models;

public class LegalEntityMaster
{
    public int LegalEntityId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public string LegalEntityName { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
}

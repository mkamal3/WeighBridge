namespace WeightBridgeApp.Models;

public class ReasonMaster
{
    public int ReasonMasterId { get; set; }
    public string QcRejection { get; set; } = string.Empty;
    public string Return { get; set; } = string.Empty;
    public string Disposal { get; set; } = string.Empty;
    public string Correction { get; set; } = string.Empty;
    public string Void { get; set; } = string.Empty;
    public string Conversion { get; set; } = string.Empty;
}

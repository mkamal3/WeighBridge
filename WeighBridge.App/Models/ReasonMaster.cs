namespace WeightBridgeApp.Models;

public class ReasonMaster
{
    public int ReasonMasterId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string DisplayText => string.IsNullOrWhiteSpace(Description)
        ? Code
        : $"{Code} - {Description}";
}

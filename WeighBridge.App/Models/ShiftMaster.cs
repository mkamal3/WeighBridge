namespace WeightBridgeApp.Models;

public class ShiftMaster
{
    public int ShiftMasterId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string CrossingMidnightRule { get; set; } = string.Empty;
}

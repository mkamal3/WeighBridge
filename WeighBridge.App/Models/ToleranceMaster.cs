namespace WeightBridgeApp.Models;

public class ToleranceMaster
{
    public int ToleranceMasterId { get; set; }
    public string AbsoluteTolerance { get; set; } = string.Empty;
    public string PercentageTolerance { get; set; } = string.Empty;
    public string AllocationTolerance { get; set; } = string.Empty;
    public string ApprovalThreshold { get; set; } = string.Empty;
}

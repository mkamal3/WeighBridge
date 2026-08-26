namespace WeightBridgeApp.Models;

public class WeighmentCorrection
{
    public int CorrectionId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string CorrectionNumber { get; set; } = string.Empty;
    public string CorrectionType { get; set; } = "General";
    public string Reason { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime? SubmittedDateTime { get; set; }
    public string ApprovedRejectedBy { get; set; } = string.Empty;
    public DateTime? ApprovalRejectedDateTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

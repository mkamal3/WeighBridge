namespace WeightBridgeApp.Models;

public class CancellationVoidRequest
{
    public int CancellationVoidId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string GatePassNumber { get; set; } = string.Empty;
    public string CancellationVoidNumber { get; set; } = string.Empty;
    public string Type { get; set; } = "Cancel";
    public string Reason { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime? SubmittedDateTime { get; set; }
    public string ApprovedRejectedBy { get; set; } = string.Empty;
    public DateTime? ApprovalRejectedDateTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

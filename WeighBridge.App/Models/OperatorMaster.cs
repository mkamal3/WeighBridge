namespace WeightBridgeApp.Models;

public class OperatorMaster
{
    public int OperatorId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string LegalEntity { get; set; } = string.Empty;
    public string DefaultLegalEntity { get; set; } = string.Empty;
    public string DefaultWeighbridge { get; set; } = string.Empty;
    public string AssignedWeighbridges { get; set; } = string.Empty;
    public string DefaultShift { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PermissionProfile { get; set; } = string.Empty;
    public bool CanCaptureFirstWeight { get; set; } = true;
    public bool CanCaptureSecondWeight { get; set; } = true;
    public bool CanPerformManualWeightEntry { get; set; }
    public bool CanCorrectTransactions { get; set; }
    public bool CanCancelTransactions { get; set; }
    public bool CanOverrideWeight { get; set; }
    public bool CanApproveQc { get; set; }
    public bool CanRetryIntegration { get; set; }
    public DateTime? LastLogin { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? EffectiveFrom { get; set; } = DateTime.Today;
    public bool IsActive { get; set; } = true;
    public string Remarks { get; set; } = string.Empty;
}

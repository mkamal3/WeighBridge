namespace WeightBridgeApp.Models;

public class OperatorMaster
{
    public int OperatorId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
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
    public bool CanAccessWeighment { get; set; } = true;
    public bool CanAccessMasters { get; set; }
    public bool CanAccessReports { get; set; } = true;
    public bool CanAccessSettings { get; set; }
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

    // Compatibility aliases for old bindings/code while User Management is removed.
    public int UserId { get => OperatorId; set => OperatorId = value; }
    public string FullName { get => OperatorName; set => OperatorName = value; }
    public string CompanyName { get => DefaultLegalEntity; set => DefaultLegalEntity = value; }
    public bool CanAccessUserManagement { get => CanAccessMasters; set => CanAccessMasters = value; }
    public bool CanEditCompletedTransaction { get => CanCorrectTransactions; set => CanCorrectTransactions = value; }
    public bool CanDeleteCompletedTransaction { get => CanCancelTransactions; set => CanCancelTransactions = value; }
}

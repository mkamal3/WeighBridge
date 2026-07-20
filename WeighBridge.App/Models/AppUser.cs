namespace WeightBridgeApp.Models;

public class AppUser
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool CanAccessWeighment { get; set; } = true;
    public bool CanAccessSettings { get; set; }
    public bool CanAccessMasters { get; set; }
    public bool CanAccessReports { get; set; } = true;
    public bool CanAccessUserManagement { get; set; }
    public bool CanEditCompletedTransaction { get; set; }
    public bool CanDeleteCompletedTransaction { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

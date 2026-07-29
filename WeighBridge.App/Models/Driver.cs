namespace WeightBridgeApp.Models;

public class Driver
{
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string SecondaryMobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string DriverType { get; set; } = string.Empty;
    public string EmployerPartyType { get; set; } = string.Empty;
    public string EmployerAccount { get; set; } = string.Empty;
    public string IdentificationType { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public DateTime? IdentificationExpiryDate { get; set; }
    public DateTime? EmiratesIdExpiryDate { get; set; }
    public string PassportNumber { get; set; } = string.Empty;
    public DateTime? PassportExpiryDate { get; set; }
    public string DrivingLicenceNumber { get; set; } = string.Empty;
    public string DrivingLicenceIssuedBy { get; set; } = string.Empty;
    public DateTime? DrivingLicenceExpiryDate { get; set; }
    public string LicenceCategories { get; set; } = string.Empty;
    public string DefaultVehicle { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DriverPhoto { get; set; } = string.Empty;
    public string EmiratesIdAttachment { get; set; } = string.Empty;
    public string PassportAttachment { get; set; } = string.Empty;
    public string DrivingLicenceAttachment { get; set; } = string.Empty;
    public string LegalEntity { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public bool Blacklisted { get; set; }
    public string BlacklistReason { get; set; } = string.Empty;
    public DateTime? EffectiveFrom { get; set; } = DateTime.Today;
    // Availability is controlled by Status. Kept only for backward compatibility with old databases.
    public bool IsActive { get => string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase); set { } }
    public string Remarks { get; set; } = string.Empty;

    // Backward-compatible aliases used by old UI/filter bindings.
    public string CNIC
    {
        get => IdentificationNumber;
        set => IdentificationNumber = value;
    }

    public string MobileNo
    {
        get => MobileNumber;
        set => MobileNumber = value;
    }

    public string LicenseNo
    {
        get => DrivingLicenceNumber;
        set => DrivingLicenceNumber = value;
    }
}

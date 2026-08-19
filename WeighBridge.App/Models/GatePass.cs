namespace WeightBridgeApp.Models;

public class GatePass
{
    public int GatePassId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public string GatePassNumber { get; set; } = string.Empty;
    public string Type { get; set; } = "Inbound";
    public DateTime? EntryDateTime { get; set; } = DateTime.Now;
    public string VehiclePlate { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string DriverMobile { get; set; } = string.Empty;
    public string PartyType { get; set; } = "Customer";
    public string PartyAccount { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string ExpectedTransactionType { get; set; } = string.Empty;
    public string ExpectedItemNumber { get; set; } = string.Empty;
    public string ExpectedItem { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string SecurityOfficer { get; set; } = string.Empty;
    public DateTime? ExitDateTime { get; set; }
    public string ClosedBy { get; set; } = string.Empty;
    public string LinkedTicketNo { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string Remarks { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

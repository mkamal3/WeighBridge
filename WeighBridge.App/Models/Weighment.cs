namespace WeightBridgeApp.Models;

public class Weighment
{
    public int WeighmentId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public string TicketNo { get; set; } = string.Empty;
    public string SlipNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public string GatePassNumber { get; set; } = string.Empty;
    public string WeighbridgeCode { get; set; } = string.Empty;
    public DateTime? TransactionDateTime { get; set; }
    public string ShiftCode { get; set; } = string.Empty;
    public string OperatorUsername { get; set; } = string.Empty;
    public string ExternalReference { get; set; } = string.Empty;
    public string OperatorRemarks { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string VehicleNo { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public int? MaterialId { get; set; }
    public string ItemNumber { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal FirstWeight { get; set; }
    public DateTime FirstWeightTime { get; set; }
    public string FirstWeightBy { get; set; } = string.Empty;
    public string FirstWeightByDisplay { get; set; } = string.Empty;
    public decimal? SecondWeight { get; set; }
    public DateTime? SecondWeightTime { get; set; }
    public string SecondWeightBy { get; set; } = string.Empty;
    public string SecondWeightByDisplay { get; set; } = string.Empty;
    public decimal? NetWeight { get; set; }
    public string Status { get; set; } = "Open";
    public string Remarks { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

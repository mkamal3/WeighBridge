namespace WeightBridgeApp.Models;

public class Weighment
{
    public int WeighmentId { get; set; }
    public string TicketNo { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string VehicleNo { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public int? PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string PartyType { get; set; } = string.Empty;
    public int? MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public decimal FirstWeight { get; set; }
    public DateTime FirstWeightTime { get; set; }
    public decimal? SecondWeight { get; set; }
    public DateTime? SecondWeightTime { get; set; }
    public decimal? NetWeight { get; set; }
    public string Status { get; set; } = "Open";
    public string Remarks { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

namespace WeightBridgeApp.Models;

public class LocationMaster
{
    public int LocationMasterId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

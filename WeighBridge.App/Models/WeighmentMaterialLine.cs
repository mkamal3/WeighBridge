namespace WeightBridgeApp.Models;

public class WeighmentMaterialLine
{
    public int MaterialLineId { get; set; }
    public int WeighmentId { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string DataAreaId { get; set; } = string.Empty;
    public int LineNo { get; set; }
    public int? ItemMasterId { get; set; }
    public string ItemNumber { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal ExpectedQty { get; set; }
    public string Uom { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

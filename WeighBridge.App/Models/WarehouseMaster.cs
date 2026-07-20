namespace WeightBridgeApp.Models;

public class WarehouseMaster
{
    public int WarehouseMasterId { get; set; }
    public string Warehouse { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string QuarantineWarehouse { get; set; } = string.Empty;
    public string TransitWarehouse { get; set; } = string.Empty;

    public string GoodsInTransitWarehouse { get; set; } = string.Empty;
    public string UnderDeliveryWarehouse { get; set; } = string.Empty;
    public string VendorAccount { get; set; } = string.Empty;

    public string DefaultReceiptLocation { get; set; } = string.Empty;
    public string DefaultIssueLocation { get; set; } = string.Empty;
    public string DefaultProductionFinishedGood { get; set; } = string.Empty;

    public string AddressNameDescription { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}

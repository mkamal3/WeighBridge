namespace WeightBridgeApp.Models;

public class WarehouseMaster
{
    public int WarehouseMasterId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
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
    public string Id { get; set; } = string.Empty;
    public string mserp_mk_wbwarehousemasterId { get; set; } = string.Empty;

    // Backend-only D365/Dataverse sync fields. These are not shown on UI.
    public string SinkCreatedOn { get; set; } = string.Empty;
    public string SinkModifiedOn { get; set; } = string.Empty;
    public string mserp_dataareaid_id { get; set; } = string.Empty;
    public string mserp_dataareaid_id_entitytype { get; set; } = string.Empty;
    public string mserp_dataareaid { get; set; } = string.Empty;
    public string versionnumber { get; set; } = string.Empty;
    public string IsDelete { get; set; } = string.Empty;
    public string CreatedOn { get; set; } = string.Empty;
    public string createdonpartition { get; set; } = string.Empty;

}

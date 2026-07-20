namespace WeightBridgeApp.Models;

public class ItemMaster
{
    public int ItemMasterId { get; set; }
    public string ItemNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string SearchName { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public string ProductSubtype { get; set; } = string.Empty;

    public string ProductNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StorageDimensionGroup { get; set; } = string.Empty;
    public string TrackingDimensionGroup { get; set; } = string.Empty;
    public string ItemModelGroup { get; set; } = string.Empty;
    public string ReservationHierarchy { get; set; } = string.Empty;

    public string PurchaseUnit { get; set; } = string.Empty;
    public string PurchaseOverDelivery { get; set; } = string.Empty;
    public string PurchaseUnderDelivery { get; set; } = string.Empty;
    public string BuyerGroup { get; set; } = string.Empty;
    public string ItemPriceToleranceGroup { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string PurchaseItemSalesTaxGroup { get; set; } = string.Empty;

    public string SellUnit { get; set; } = string.Empty;
    public string SellOverDelivery { get; set; } = string.Empty;
    public string SellUnderDelivery { get; set; } = string.Empty;
    public string SellItemSalesTaxGroup { get; set; } = string.Empty;

    public string BatchNumberGroup { get; set; } = string.Empty;
    public string SerialNumberGroup { get; set; } = string.Empty;
    public string InventoryOverDelivery { get; set; } = string.Empty;
    public string InventoryUnderDelivery { get; set; } = string.Empty;
    public string CatchWeightItem { get; set; } = string.Empty;
    public string CWUnit { get; set; } = string.Empty;
    public string NominalQuantity { get; set; } = string.Empty;
    public string MinimumQuantity { get; set; } = string.Empty;
    public string MaximumQuantity { get; set; } = string.Empty;

    public string BOMUnit { get; set; } = string.Empty;
    public string ConstantScrap { get; set; } = string.Empty;
    public string VariableScrap { get; set; } = string.Empty;
    public string CostingLevel { get; set; } = string.Empty;
    public string PlanningLevel { get; set; } = string.Empty;
    public string CostCalculationLevel { get; set; } = string.Empty;
    public string Phantom { get; set; } = string.Empty;
    public string CalculationGroup { get; set; } = string.Empty;
    public string ProductionType { get; set; } = string.Empty;

    public string ItemGroup { get; set; } = string.Empty;
    public string CostUnit { get; set; } = string.Empty;
    public string LastCostPrice { get; set; } = string.Empty;
    public string DateOfPrice { get; set; } = string.Empty;

    public string UnitSequenceGroupId { get; set; } = string.Empty;
}

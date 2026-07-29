namespace WeightBridgeApp.Models;

public class ItemMaster
{
    public int ItemMasterId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
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
    public decimal? PurchaseOverDelivery { get; set; }
    public decimal? PurchaseUnderDelivery { get; set; }
    public string BuyerGroup { get; set; } = string.Empty;
    public string ItemPriceToleranceGroup { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string PurchaseItemSalesTaxGroup { get; set; } = string.Empty;

    public string SellUnit { get; set; } = string.Empty;
    public decimal? SellOverDelivery { get; set; }
    public decimal? SellUnderDelivery { get; set; }
    public string SellItemSalesTaxGroup { get; set; } = string.Empty;

    public string BatchNumberGroup { get; set; } = string.Empty;
    public string SerialNumberGroup { get; set; } = string.Empty;
    public decimal? InventoryOverDelivery { get; set; }
    public decimal? InventoryUnderDelivery { get; set; }
    public bool CatchWeightItem { get; set; }
    public string CWUnit { get; set; } = string.Empty;
    public decimal? NominalQuantity { get; set; }
    public decimal? MinimumQuantity { get; set; }
    public decimal? MaximumQuantity { get; set; }

    public string BOMUnit { get; set; } = string.Empty;
    public decimal? ConstantScrap { get; set; }
    public decimal? VariableScrap { get; set; }
    public int? CostingLevel { get; set; }
    public int? PlanningLevel { get; set; }
    public int? CostCalculationLevel { get; set; }
    public bool Phantom { get; set; }
    public string CalculationGroup { get; set; } = string.Empty;
    public string ProductionType { get; set; } = string.Empty;

    public string ItemGroup { get; set; } = string.Empty;
    public string CostUnit { get; set; } = string.Empty;
    public decimal? LastCostPrice { get; set; }
    public DateTime? DateOfPrice { get; set; }

    public string UnitSequenceGroupId { get; set; } = string.Empty;
}

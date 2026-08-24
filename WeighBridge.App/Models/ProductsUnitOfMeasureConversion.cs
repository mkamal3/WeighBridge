namespace WeightBridgeApp.Models;

/// <summary>
/// Product-specific unit of measure conversion synchronized from the source system.
/// Records are linked to Item Master by mserp_productnumber = ItemMaster.ProductNumber.
/// </summary>
public class ProductsUnitOfMeasureConversion
{
    // Local SQLite identity. Not part of the source payload.
    public int ProductsUnitOfMeasureConversionId { get; set; }

    public string Id { get; set; } = string.Empty;
    public string SinkCreatedOn { get; set; } = string.Empty;
    public string SinkModifiedOn { get; set; } = string.Empty;
    public decimal? mserp_rounding { get; set; }
    public decimal? mserp_denominator { get; set; }
    public decimal? mserp_factor { get; set; }
    public string mserp_fromunitsymbol { get; set; } = string.Empty;
    public decimal? mserp_inneroffset { get; set; }
    public string mserp_mk_wb_ecoresproductspecificunitofmeasureconversionentityid { get; set; } = string.Empty;
    public decimal? mserp_numerator { get; set; }
    public decimal? mserp_outeroffset { get; set; }
    public string mserp_primaryfield { get; set; } = string.Empty;
    public string mserp_productnumber { get; set; } = string.Empty;
    public string mserp_tounitsymbol { get; set; } = string.Empty;
    public string versionnumber { get; set; } = string.Empty;
    public string IsDelete { get; set; } = string.Empty;
    public string CreatedOn { get; set; } = string.Empty;
    public string PartitionId { get; set; } = string.Empty;
}

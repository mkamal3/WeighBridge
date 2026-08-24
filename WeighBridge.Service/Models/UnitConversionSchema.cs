namespace DeltaToSqlitePoc.Models;

/// <summary>
/// Canonical Vendor columns from Synapse Link Delta metaData.schemaString
/// (partitioned by PartitionId). Used by demo generation and docs.
/// </summary>
public static class UnitConversionSchema
{

    public const string DefaultDeltaPath = "mserp_mk_wb_ecoresproductspecificunitofmeasureconversionentity_partitioned";
    //public const string DefaultDeltaPath = "mserp_mk_wb_ecoresproductspecificunitofmeasureconversionentity";

    public const string DefaultTableName = "UnitConversion";
    public const string PartitionColumn = "PartitionId";

    /// <summary>Core identity / sync columns first, then business fields.</summary>
    public static IReadOnlyList<string> Columns { get; } =
    [
        "Id",
        "SinkCreatedOn",
        "SinkModifiedOn",
        "mserp_rounding",
        "mserp_denominator",
        "mserp_factor",
        "mserp_fromunitsymbol",
        "mserp_inneroffset",
        "mserp_mk_wb_ecoresproductspecificunitofmeasureconversionentityid",
        "mserp_numerator",
        "mserp_outeroffset",
        "mserp_primaryfield",
        "mserp_productnumber",
        "mserp_tounitsymbol",
        "versionnumber",
        "IsDelete",
        "CreatedOn",
        "PartitionId"
    ];
}

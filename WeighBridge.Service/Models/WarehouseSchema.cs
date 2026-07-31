namespace DeltaToSqlitePoc.Models;

/// <summary>
/// Canonical Vendor columns from Synapse Link Delta metaData.schemaString
/// (partitioned by PartitionId). Used by demo generation and docs.
/// </summary>
public static class WarehouseSchema
{
    public const string DefaultDeltaPath = "deltalake/mserp_mk_wbwarehousemaster_partitioned";
    public const string DefaultTableName = "WarehouseMasters";
    public const string PartitionColumn = "PartitionId";

    /// <summary>Core identity / sync columns first, then business fields.</summary>
    public static IReadOnlyList<string> Columns { get; } =
    [
                "Id",
        "SinkCreatedOn",
        "SinkModifiedOn",
        "mserp_warehousetype",
        "mserp_dataareaid_id",
        "mserp_dataareaid_id_entitytype",
        "mserp_dataareaid",
        "mserp_dataareaid_idname",
        "mserp_defaultproductionfinishgoodslocation",
        "mserp_locationiddefaultissue",
        "mserp_locationiddefaultreceipt",
        "mserp_mk_wbwarehousemasterid",
        "mserp_quarantinewarehouseid",
        "mserp_site",
        "mserp_transitwarehouseid",
        "mserp_vendor",
        "mserp_warehouseid",
        "mserp_warehousename",
        "mserp_warehousetransit",//
        "mserp_warehouseunder",
        "versionnumber",
        "IsDelete",
        "CreatedOn",
        "PartitionId",
    ];
}

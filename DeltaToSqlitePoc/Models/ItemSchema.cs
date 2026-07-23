namespace DeltaToSqlitePoc.Models;

/// <summary>
/// Canonical Item columns from Synapse Link Delta metaData.schemaString
/// (partitioned by PartitionId). Used by demo generation and docs.
/// </summary>
public static class ItemSchema
{
    public const string DefaultDeltaPath = "deltalake/mserp_mk_wb_ecoresreleasedproductv2entity_partitioned";
    public const string DefaultTableName = "ReleasedProducts";
    public const string PartitionColumn = "PartitionId";

    /// <summary>Core identity / sync columns first, then business fields.</summary>
    public static IReadOnlyList<string> Columns { get; } =
    [
        "Id",
        "SinkCreatedOn",
        "SinkModifiedOn",
        "mserp_costmodel",
        "mserp_pdscwproduct",
        "mserp_phantom",
        "mserp_pmfproducttype",
        "mserp_productsubtype",
        "mserp_producttype",
        "mserp_dataareaid_id",
        "mserp_dataareaid_id_entitytype",
        "mserp_batchnumgroupid",
        "mserp_bomcalcgroupid",
        "mserp_bomlevel",
        "mserp_bomunitid",
        "mserp_costbomlevel",
        "mserp_dataareaid",
        "mserp_dataareaid_idname",
        "mserp_inventoverdeliverypct",
        "mserp_inventunderdeliverypct",
        "mserp_inventunitid",
        "mserp_itembuyergroupid",
        "mserp_itemgroupid",
        "mserp_itemid",
        "mserp_itempricetolerancegroupid",
        "mserp_mk_wb_ecoresreleasedproductv2entityid",
        "mserp_modelgroupid",
        "mserp_namealias",
        "mserp_pdscwmax",
        "mserp_pdscwmin",
        "mserp_pdscwunitid",
        "mserp_planninglevel",
        "mserp_pricedate",
        "mserp_primaryvendorid",
        "mserp_productsearchname",
        "mserp_purchoverdeliverypct",
        "mserp_purchtaxitemgroupid",
        "mserp_purchunderdeliverypct",
        "mserp_purchunitid",
        "mserp_reservationhierarchy",
        "mserp_reservationhierarchy_bigint",
        "mserp_salesoverdeliverypct",
        "mserp_salestaxitemgroupid",
        "mserp_salesunderdeliverypct",
        "mserp_salesunitid",
        "mserp_scrapconst",
        "mserp_scrapvar",
        "mserp_serialnumgroupid",
        "mserp_storagedimensiongroup",
        "mserp_storagedimensiongroup_bigint",
        "mserp_trackingdimensiongroup",
        "mserp_trackingdimensiongroup_bigint",
        "mserp_uomseqgroupid",
        "versionnumber",
        "IsDelete",
        "CreatedOn",
        "PartitionId",
    ];
}

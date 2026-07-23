namespace DeltaToSqlitePoc.Models;

/// <summary>
/// Canonical Vendor columns from Synapse Link Delta metaData.schemaString
/// (partitioned by PartitionId). Used by demo generation and docs.
/// </summary>
public static class VendorSchema
{
    //public const string DefaultDeltaPath = "d365/tables/mserp_mk_wbvendormaster_partitioned";
    public const string DefaultDeltaPath = "deltalake/mserp_mk_wbvendormaster_partitioned";
    public const string DefaultTableName = "VendorMaster";
    public const string PartitionColumn = "PartitionId";

    /// <summary>Core identity / sync columns first, then business fields.</summary>
    public static IReadOnlyList<string> Columns { get; } =
    [
        "Id",
        "SinkCreatedOn",
        "SinkModifiedOn",
        "mserp_dataareaid_id",
        "mserp_dataareaid_id_entitytype",
        "mserp_accountstatus",
        "mserp_classificationgroup",
        "mserp_currencycode",
        "mserp_dataareaid",
        "mserp_dataareaid_idname",
        "mserp_dlvterms",
        "mserp_employeeresp",
        "mserp_invoiceaccountnum",
        "mserp_mk_wbvendormasterid",
        "mserp_name",
        "mserp_partytype",
        "mserp_paymmethod",
        "mserp_paymterms",
        "mserp_primarycontactphone",
        "mserp_primarycontactphoneextension",
        "mserp_salestaxgroupcode",
        "mserp_vendoraccountnumber",
        "mserp_vendorgroupid",
        "mserp_vendorsearchname",
        "versionnumber",
        "IsDelete",
        "CreatedOn",
        "PartitionId",
    ];
}

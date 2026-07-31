namespace DeltaToSqlitePoc.Models;

/// <summary>
/// Canonical Customer columns from Synapse Link Delta metaData.schemaString
/// (partitioned by PartitionId). Used by demo generation and docs.
/// </summary>
public static class CustomerSchema
{
    public const string DefaultDeltaPath = "deltalake/mserp_mk_wbcustomermaster_partitioned";
    public const string DefaultTableName = "CustomerMaster";
    public const string PartitionColumn = "PartitionId";

    /// <summary>Core identity / sync columns first, then business fields.</summary>
    public static IReadOnlyList<string> Columns { get; } =
    [
"Id",
"SinkCreatedOn",
"SinkModifiedOn",
"mserp_onholdstatus",
"mserp_dataareaid_id",
"mserp_dataareaid_id_entitytype",
"mserp_currency",
"mserp_custclassificationid",
"mserp_customeraccount",
"mserp_customergroup",
"mserp_dataareaid",
"mserp_dataareaid_idname",
"mserp_deliveryterms",
"mserp_employeeresp",
"mserp_invoiceaccount",
"mserp_mk_wbcustomermasterid",
"mserp_name",
"mserp_namealias",
"mserp_partytype",
"mserp_paymentmethod",
"mserp_paymentterms",
"mserp_primarycontactphone",
"mserp_primarycontactphoneextension",
"mserp_salestaxgroup",
"versionnumber",
"IsDelete",
"CreatedOn",
"PartitionId",
    ];
}

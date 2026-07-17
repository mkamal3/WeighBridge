namespace DeltaToSqlitePoc.Models;

/// <summary>
/// Canonical Vendor columns from Synapse Link Delta metaData.schemaString
/// (partitioned by PartitionId). Used by demo generation and docs.
/// </summary>
public static class VendorSchema
{
    public const string DefaultDeltaPath = "d365/tables/mserp_vendvendoraientity";
    public const string DefaultTableName = "Vendor";
    public const string PartitionColumn = "PartitionId";

    /// <summary>Core identity / sync columns first, then business fields.</summary>
    public static IReadOnlyList<string> Columns { get; } =
    [
        "Id",
        "SinkCreatedOn",
        "SinkModifiedOn",
        "mserp_arepricesincludingsalestax",
        "mserp_isminorityowned",
        "mserp_isownerdisabled",
        "mserp_isserviceveteranowned",
        "mserp_issmallbusiness",
        "mserp_isvendorlocallyowned",
        "mserp_iswomanowner",
        "mserp_onholdstatus",
        "mserp_dataareaid_id",
        "mserp_dataareaid_id_entitytype",
        "mserp_businesssegmentcode",
        "mserp_businesssubsegmentcode",
        "mserp_buyergroupid",
        "mserp_cashdiscountcode",
        "mserp_chargevendorgroupid",
        "mserp_clearingperiodpaymenttermsid",
        "mserp_companychainname",
        "mserp_creditlimit",
        "mserp_creditrating",
        "mserp_currencycode",
        "mserp_dataareaid",
        "mserp_dataareaid_idname",
        "mserp_defaultdeliverymodeid",
        "mserp_defaultdeliverytermscode",
        "mserp_defaultpaymentdayname",
        "mserp_defaultpaymentschedulename",
        "mserp_defaultpaymenttermsname",
        "mserp_defaultpurchaseorderpoolid",
        "mserp_defaulttotaldiscountvendorgroupcode",
        "mserp_defaultvendorpaymentmethodname",
        "mserp_ethnicoriginid",
        "mserp_invoicevendoraccountnumber",
        "mserp_languageid",
        "mserp_linediscountvendorgroupcode",
        "mserp_lineofbusinessid",
        "mserp_multilinediscountvendorgroupcode",
        "mserp_nationality",
        "mserp_notes",
        "mserp_paymentspecificationid",
        "mserp_pricevendorgroupid",
        "mserp_primaryemailaddress",
        "mserp_primaryfaxnumber",
        "mserp_primaryphonenumber",
        "mserp_primaryurl",
        "mserp_purchaseworkcalendarid",
        "mserp_vendoraccountnumber",
        "mserp_vendorgroupid",
        "mserp_vendorholdreleasedate",
        "mserp_vendorknownasname",
        "mserp_vendororganizationname",
        "mserp_vendorsearchname",
        "mserp_vendvendoraientityid",
        "versionnumber",
        "IsDelete",
        "CreatedOn",
        "createdonpartition",
        "PartitionId"
    ];
}

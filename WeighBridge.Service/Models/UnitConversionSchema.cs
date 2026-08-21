namespace DeltaToSqlitePoc.Models;

/// <summary>
/// Canonical Vendor columns from Synapse Link Delta metaData.schemaString
/// (partitioned by PartitionId). Used by demo generation and docs.
/// </summary>
public static class UnitConversionSchema
{

    public const string DefaultDeltaPath = "msdyn_productspecificunitofmeasureconversion_partitioned";

    public const string DefaultTableName = "UnitConversion";
    public const string PartitionColumn = "PartitionId";

    /// <summary>Core identity / sync columns first, then business fields.</summary>
    public static IReadOnlyList<string> Columns { get; } =
    [
        "Id",
        "SinkCreatedOn",
        "SinkModifiedOn",
        "statecode",
        "statuscode",
        "msdyn_rounding",
        "createdby",
        "createdby_entitytype",
        "createdonbehalfby",
        "createdonbehalfby_entitytype",
        "modifiedby",
        "modifiedby_entitytype",
        "modifiedonbehalfby",
        "modifiedonbehalfby_entitytype",
        "msdyn_fromunit",
        "msdyn_fromunit_entitytype",
        "msdyn_globalproduct",
        "msdyn_globalproduct_entitytype",
        "msdyn_tounit",
        "msdyn_tounit_entitytype",
        "owningbusinessunit",
        "owningbusinessunit_entitytype",
        "owningteam",
        "owningteam_entitytype",
        "owninguser",
        "owninguser_entitytype",
        "ownerid",
        "ownerid_entitytype",
        "createdbyname",
        "createdbyyominame",
        "createdon",
        "createdonbehalfbyname",
        "createdonbehalfbyyominame",
        "importsequencenumber",
        "modifiedbyname",
        "modifiedbyyominame",
        "modifiedon",
        "modifiedonbehalfbyname",
        "modifiedonbehalfbyyominame",
        "msdyn_denominator",
        "msdyn_factor",
        "msdyn_fromunitname",
        "msdyn_globalproductname",
        "msdyn_inneroffset",
        "msdyn_name",
        "msdyn_numerator",
        "msdyn_outeroffset",
        "msdyn_productspecificunitofmeasureconversionid",
        "msdyn_tounitname",
        "overriddencreatedon",
        "owneridname",
        "owneridtype",
        "owneridyominame",
        "owningbusinessunitname",
        "timezoneruleversionnumber",
        "utcconversiontimezonecode",
        "versionnumber",
        "IsDelete",
        "createdonpartition"
    ];
}

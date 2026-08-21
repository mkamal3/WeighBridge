namespace DeltaToSqlitePoc.Models;

/// <summary>
/// Canonical Vendor columns from Synapse Link Delta metaData.schemaString
/// (partitioned by PartitionId). Used by demo generation and docs.
/// </summary>
public static class LegalEntitySchema
{
    public const string DefaultDeltaPath = "deltalake/companyinfo_partitioned";
    public const string DefaultTableName = "LegalEntities";
    public const string PartitionColumn = "PartitionId";

    /// <summary>Core identity / sync columns first, then business fields.</summary>
    public static IReadOnlyList<string> Columns { get; } =
    [
        "Id",
        "SinkCreatedOn",
        "SinkModifiedOn",
        "dataarea",
        "mk_golivedate",
        "versionnumber",
        "IsDelete",
        "modifiedon",
        "createdon",
        "createdonpartition",
        "PartitionId"
    ];
}

namespace DeltaToSqlitePoc.Models;
/// <summary>
/// Canonical Vendor columns from Synapse Link Delta metaData.schemaString
/// (partitioned by PartitionId). Used by demo generation and docs.
/// </summary>
public static class UnitOfMeasureSchema
{

    public const string DefaultDeltaPath = "unitofmeasure_partitioned";

    public const string DefaultTableName = "UnitOfMeasureMasters";
    public const string PartitionColumn = "PartitionId";

    /// <summary>Core identity / sync columns first, then business fields.</summary>
    public static IReadOnlyList<string> Columns { get; } =
    [
        "Id",
        "SinkCreatedOn",
        "SinkModifiedOn",
        "isbaseunit",
        "issystemunit",
        "systemofunits",
        "unitofmeasureclass",
        "sysdatastatecode",
        "decimalprecision",
        "symbol",
        "modifieddatetime",
        "modifiedby",
        "modifiedtransactionid",
        "createddatetime",
        "createdby",
        "createdtransactionid",
        "dataareaid",
        "recversion",
        "partition",
        "sysrowversion",
        "recid",
        "tableid",
        "versionnumber",
        "createdon",
        "modifiedon",
        "IsDelete",
        "PartitionId"
    ];
}

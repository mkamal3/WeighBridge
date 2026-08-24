namespace WeightBridgeApp.Models;

/// <summary>
/// Read-only synchronized Unit of Measure master.
/// Frontend fields are shown in BridgeOne; backend fields are retained only for integration/synchronization.
/// </summary>
public class UnitOfMeasureMaster
{
    public int UnitOfMeasureMasterId { get; set; }

    // Frontend columns
    public string symbol { get; set; } = string.Empty;
    public bool isbaseunit { get; set; }
    public bool issystemunit { get; set; }
    public string systemofunits { get; set; } = string.Empty;
    public string unitofmeasureclass { get; set; } = string.Empty;
    public string sysdatastatecode { get; set; } = string.Empty;
    public int? decimalprecision { get; set; }

    // Backend-only columns. These are stored for D365/Fabric/Dataverse synchronization and are not shown on the master UI.
    public string Id { get; set; } = string.Empty;
    public string SinkCreatedOn { get; set; } = string.Empty;
    public string SinkModifiedOn { get; set; } = string.Empty;
    public string modifieddatetime { get; set; } = string.Empty;
    public string modifiedby { get; set; } = string.Empty;
    public string modifiedtransactionid { get; set; } = string.Empty;
    public string createddatetime { get; set; } = string.Empty;
    public string createdby { get; set; } = string.Empty;
    public string createdtransactionid { get; set; } = string.Empty;
    public string dataareaid { get; set; } = string.Empty;
    public string recversion { get; set; } = string.Empty;
    public string partition { get; set; } = string.Empty;
    public string sysrowversion { get; set; } = string.Empty;
    public string recid { get; set; } = string.Empty;
    public string tableid { get; set; } = string.Empty;
    public string versionnumber { get; set; } = string.Empty;
    public string createdon { get; set; } = string.Empty;
    public string modifiedon { get; set; } = string.Empty;
    public string IsDelete { get; set; } = string.Empty;
    public string PartitionId { get; set; } = string.Empty;
}

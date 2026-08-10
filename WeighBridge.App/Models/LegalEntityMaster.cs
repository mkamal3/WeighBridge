namespace WeightBridgeApp.Models;

public class LegalEntityMaster
{
    public int LegalEntityId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public string LegalEntityName { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;

    // Backend-only D365/Dataverse sync fields. These are not shown on the UI.
    public string ID { get; set; } = string.Empty;
    public string SinkCreatedOn { get; set; } = string.Empty;
    public string SinkModifiedOn { get; set; } = string.Empty;
    public string mserp_dataareaid_id { get; set; } = string.Empty;
    public string mserp_dataareaid_id_entitytype { get; set; } = string.Empty;
    public string mserp_dataareaid { get; set; } = string.Empty;
    public string versionnumber { get; set; } = string.Empty;
    public string IsDelete { get; set; } = string.Empty;
    public string CreatedOn { get; set; } = string.Empty;
    public string createdonpartition { get; set; } = string.Empty;
}

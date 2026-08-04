namespace WeightBridgeApp.Models;

public class Vendor
{
    public int VendorId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public string VendorAccount { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MethodOfPayment { get; set; } = string.Empty;
    public string TermsOfPayment { get; set; } = string.Empty;
    public string DeliveryTerms { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty;
    public string AccountStatusReason { get; set; } = string.Empty;
    public string VendorGroup { get; set; } = string.Empty;
    public string EmployeeResponsible { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
    public string VendorClassificationGroup { get; set; } = string.Empty;
    public string SearchName { get; set; } = string.Empty;

    public string AddressNameDescription { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AddressPurpose { get; set; } = string.Empty;

    public string ContactDescription { get; set; } = string.Empty;
    public string ContactType { get; set; } = string.Empty;
    public string ContactNumberAddress { get; set; } = string.Empty;
    public string ContactExtension { get; set; } = string.Empty;

    public string InvoiceAccount { get; set; } = string.Empty;
    public string ModeOfDelivery { get; set; } = string.Empty;
    public string SalesTaxGroup { get; set; } = string.Empty;
    public string mserp_mk_wbvendormasterId { get; set; } = string.Empty;

    // Backend-only D365/Dataverse sync fields. These are not shown on UI.
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

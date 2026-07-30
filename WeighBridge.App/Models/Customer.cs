namespace WeightBridgeApp.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public string CustomerAccount { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MethodOfPayment { get; set; } = string.Empty;
    public string TermsOfPayment { get; set; } = string.Empty;
    public string DeliveryTerms { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty;
    public string AccountStatusReason { get; set; } = string.Empty;
    public string CustomerGroup { get; set; } = string.Empty;
    public string EmployeeResponsible { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;

    public string OrganizationPerson { get; set; } = string.Empty;
    public string SearchName { get; set; } = string.Empty;
    public string ClassificationGroup { get; set; } = string.Empty;

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
}

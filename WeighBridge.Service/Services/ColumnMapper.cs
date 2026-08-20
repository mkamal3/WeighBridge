using DeltaToSqlitePoc.Models;
using System.Text.RegularExpressions;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace DeltaToSqlitePoc.Services;

/// <summary>
/// Maps source Delta column names (e.g. mserp_warehousename) to application model property
/// names (e.g. Name) using simple normalization and token matching heuristics. If no good
/// match is found the source name is preserved.
/// </summary>
public static class ColumnMapper
{
    /*
    private static readonly Regex NonAlphaNum = new(@"[^a-z0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static (Dictionary<string, string> SourceToTarget, List<string> Targets) MapColumns(string tableName, IEnumerable<string> sourceColumns)
    {
        var modelProps = GetModelPropertiesForTable(tableName);
        var sourceToTarget = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var targets = new List<string>();

        foreach (var src in sourceColumns)
        {
            if (string.IsNullOrWhiteSpace(src))
            {
                continue;
            }

            var normalized = NormalizeName(src);

            // If source is plain 'Id' prefer a model property that ends with 'Id'
            if (string.Equals(normalized, "id", StringComparison.OrdinalIgnoreCase))
            {
                var idProp = modelProps.FirstOrDefault(p => p.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
                if (idProp is not null)
                {
                    sourceToTarget[src] = idProp;
                    if (!targets.Contains(idProp)) targets.Add(idProp);
                    continue;
                }
            }

            // try to find best match by token containment
            string? best = null;
            foreach (var prop in modelProps)
            {
                var pnorm = NormalizeName(prop);
                if (pnorm.Contains(normalized, StringComparison.OrdinalIgnoreCase) || normalized.Contains(pnorm, StringComparison.OrdinalIgnoreCase))
                {
                    best = prop;
                    break;
                }
            }

            if (best is null)
            {
                // fallback: try splitting tokens and matching any token
                var sTokens = normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var prop in modelProps)
                {
                    var pnorm = NormalizeName(prop);
                    foreach (var t in sTokens)
                    {
                        if (pnorm.Contains(t, StringComparison.OrdinalIgnoreCase))
                        {
                            best = prop;
                            break;
                        }
                    }
                    if (best is not null) break;
                }
            }

            var target = best ?? src;
            sourceToTarget[src] = target;
            if (!targets.Contains(target)) targets.Add(target);
        }

        return (sourceToTarget, targets);
    }

    public static List<string> GetModelProperties(string tableName) => GetModelPropertiesForTable(tableName);

    private static List<string> GetModelPropertiesForTable(string tableName)
    {
        if (string.Equals(tableName, VendorSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new List<string>
            {
                "VendorId",
                "VendorAccount",
                "Name",
                "MethodOfPayment",
                "TermsOfPayment",
                "DeliveryTerms",
                "AccountStatus",
                "AccountStatusReason",
                "VendorGroup",
                "EmployeeResponsible",
                "Currency",
                "Telephone",
                "Type",
                "VendorClassificationGroup",
                "SearchName",
                "AddressNameDescription",
                "Address",
                "AddressPurpose",
                "ContactDescription",
                "ContactType",
                "ContactNumberAddress",
                "ContactExtension",
                "InvoiceAccount",
                "ModeOfDelivery",
                "SalesTaxGroup"
            };
        }

        if (string.Equals(tableName, CustomerSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new List<string>
            {
                "CustomerId",
                "CustomerAccount",
                "Name",
                "MethodOfPayment",
                "TermsOfPayment",
                "DeliveryTerms",
                "AccountStatus",
                "AccountStatusReason",
                "CustomerGroup",
                "EmployeeResponsible",
                "Currency",
                "Telephone",
                "OrganizationPerson",
                "SearchName",
                "ClassificationGroup",
                "AddressNameDescription",
                "Address",
                "AddressPurpose",
                "ContactDescription",
                "ContactType",
                "ContactNumberAddress",
                "ContactExtension",
                "InvoiceAccount",
                "ModeOfDelivery",
                "SalesTaxGroup"
            };
        }

        if (string.Equals(tableName, WarehouseSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new List<string>
            {
                "WarehouseMasterId",
                "Warehouse",
                "Name",
                "Site",
                "Type",
                "QuarantineWarehouse",
                "TransitWarehouse",
                "GoodsInTransitWarehouse",
                "UnderDeliveryWarehouse",
                "VendorAccount",
                "DefaultReceiptLocation",
                "DefaultIssueLocation",
                "DefaultProductionFinishedGood",
                "AddressNameDescription",
                "Address",
                "Purpose"
            };
        }

        if (string.Equals(tableName, ItemSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new List<string>
            {
                "ItemMasterId",
                "ItemNumber",
                "ProductName",
                "SearchName",
                "ProductType",
                "ProductSubtype",
                "ProductNumber",
                "Description",
                "StorageDimensionGroup",
                "TrackingDimensionGroup",
                "ItemModelGroup",
                "ReservationHierarchy",
                "PurchaseUnit",
                "PurchaseOverDelivery",
                "PurchaseUnderDelivery",
                "BuyerGroup",
                "ItemPriceToleranceGroup",
                "Vendor",
                "PurchaseItemSalesTaxGroup",
                "SellUnit",
                "SellOverDelivery",
                "SellUnderDelivery",
                "SellItemSalesTaxGroup",
                "BatchNumberGroup",
                "SerialNumberGroup",
                "InventoryOverDelivery",
                "InventoryUnderDelivery",
                "CatchWeightItem",
                "CWUnit",
                "NominalQuantity",
                "MinimumQuantity",
                "MaximumQuantity",
                "BOMUnit",
                "ConstantScrap",
                "VariableScrap",
                "CostingLevel",
                "PlanningLevel",
                "CostCalculationLevel",
                "Phantom",
                "CalculationGroup",
                "ProductionType",
                "ItemGroup",
                "CostUnit",
                "LastCostPrice",
                "DateOfPrice",
                "UnitSequenceGroupId"
            };
        }

        // default: no model props known => preserve source columns
        return new List<string>();
    }

    private static string NormalizeName(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var lower = s.ToLowerInvariant();
        lower = lower.Replace("mserp_", "");
        lower = lower.Replace("mk_wb_", "");
        lower = lower.Replace("mserp_", "");
        lower = NonAlphaNum.Replace(lower, " ").Trim();
        return lower;
    }*/


    public static (Dictionary<string, string> SourceToTarget, List<string> Targets)
    MapColumns(string tableName, IEnumerable<string> sourceColumns)
    {
        var mappings = GetColumnMappings(tableName);

        var sourceToTarget = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var targets = new List<string>();

        foreach (var sourceColumn in sourceColumns)
        {
            if (string.IsNullOrWhiteSpace(sourceColumn))
            {
                continue;
            }

            if (mappings.TryGetValue(sourceColumn, out var targetColumn))
            {
                sourceToTarget[sourceColumn] = targetColumn;

                if (!targets.Contains(targetColumn))
                {
                    targets.Add(targetColumn);
                }
            }
            else
            {
                // Keep the source column if no mapping exists
                sourceToTarget[sourceColumn] = sourceColumn;

                if (!targets.Contains(sourceColumn))
                {
                    targets.Add(sourceColumn);
                }
            }
        }

        return (sourceToTarget, targets);
    }

    public static Dictionary<string, string> GetColumnMappings(string tableName)
    {
        if (string.Equals(tableName, VendorSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Vendor
                 { "MasterId",                                           "mserp_mk_wbvendormasterid" },
                 { "mserp_dataareaid_idname",                            "DataAreaId" },
                 { "mserp_vendoraccountnumber",                          "VendorAccount" },
                 { "mserp_name",                                         "Name" },
                 { "mserp_paymmethod",                                   "MethodOfPayment" },
                 { "mserp_paymterms",                                    "TermsOfPayment" },
                 { "mserp_dlvterms",                                     "DeliveryTerms" },
                 { "mserp_accountstatus",                                "AccountStatus" },
                 //{ "mserp_accountstatus",                                "AccountStatusReason" },
                 { "mserp_vendorgroupid",                                "VendorGroup" },
                 { "mserp_employeeresp",                                 "EmployeeResponsible" },
                 { "mserp_currencycode",                                 "Currency" },
                 { "mserp_primarycontactphone",                          "Telephone" },
                 { "mserp_partytype",                                    "Type" },
                 { "mserp_classificationgroup",                          "VendorClassificationGroup" },
                 { "mserp_vendorsearchname",                             "SearchName" },
                 //{ "mserp_dataareaid",                                   "DataAreaId" },
                 //{ "",                                                   "AddressNameDescription" },
                 //{ "",                                                   "Address" },
                 //{ "",                                                   "AddressPurpose" },
                 //{ "",                                                   "ContactDescription" },
                 //{ "",                                                   "ContactType" },
                 //{ "",                                                   "ContactNumberAddress" },
                 { "mserp_primarycontactphoneextension",                 "ContactExtension" },
                 { "mserp_invoiceaccountnum",                            "InvoiceAccount" },
                 { "",                                                   "ModeOfDelivery" },
                 { "mserp_salestaxgroupcode",                            "SalesTaxGroup" },
                 {"mserp_mk_wbvendormasterid" , "mserp_mk_wbvendormasterid" },
                 {"SinkCreatedOn" , "SinkCreatedOn" },
                 {"SinkModifiedOn" , "SinkModifiedOn" },
                 {"mserp_dataareaid_id" , "mserp_dataareaid_id" },
                 {"mserp_dataareaid_id_entitytype" , "mserp_dataareaid_id_entitytype" },
                 {"mserp_dataareaid" , "mserp_dataareaid" },
                 {"versionnumber" , "versionnumber" },
                 {"IsDelete" , "IsDelete" },
                 {"CreatedOn" , "CreatedOn" },
                 //{"createdonpartition" , 
                 //{ "CreatedAt" 
             };
        }

        if (string.Equals(tableName, CustomerSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Customer
                { "MasterId",                           "mserp_mk_wbcustomermasterId" },
                //{ "Id",                                        "CustomerId" },
                { "mserp_dataareaid_idname",            "DataAreaId" },
                //{ "mserp_customeraccount",                     "CustomerId" },
                { "mserp_customeraccount",               "CustomerAccount" },
                { "mserp_name",                         "Name" },
                { "mserp_paymentmethod",                "MethodOfPayment" },
                { "mserp_paymentterms",                 "TermsOfPayment" },
                { "mserp_deliveryterms",                "DeliveryTerms" },
                //{ "", "AccountStatus" },
                //{ "", "AccountStatusReason" },
                { "mserp_customergroup",                "CustomerGroup" },
                { "mserp_employeeresp",                 "EmployeeResponsible" },
                { "mserp_currency",                     "Currency" },
                { "mserp_primarycontactphone",          "Telephone" },
                //{ "", "OrganizationPerson" },
                { "mserp_namealias",                    "SearchName" },
                { "mserp_custclassificationid",         "ClassificationGroup" },
                //{ "", "AddressNameDescription" },
                //{ "", "Address" },
                //{ "", "AddressPurpose" },
                //{ "", "ContactDescription" },
                //{ "", "ContactType" },
                //{ "", "ContactNumberAddress" },
                { "mserp_primarycontactphoneextension", "ContactExtension" },
                { "mserp_invoiceaccount",               "InvoiceAccount" },
                //{ "",                                   "ModeOfDelivery" },
                { "mserp_salestaxgroup",                "SalesTaxGroup" },
                { "mserp_mk_wbcustomermasterid",        "mserp_mk_wbcustomermasterId"},
                 { "SinkCreatedOn",                     "SinkCreatedOn" },
                 { "SinkModifiedOn",                    "SinkModifiedOn" },
                 { "mserp_dataareaid_id",               "mserp_dataareaid_id" },
                 { "mserp_dataareaid_id_entitytype",    "mserp_dataareaid_id_entitytype" },
                 { "mserp_dataareaid",                  "mserp_dataareaid"},
                 { "versionnumber",                     "versionnumber" },
                 { "IsDelete",                          "IsDelete" },
                 { "CreatedOn",                         "CreatedOn" }
                 //{ "", "createdonpartition" }
                 //{ "", "CreatedAt" }
            };
        }

        if (string.Equals(tableName, WarehouseSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Warehouse
                 //{ "Id",                                            "Id" },
                 { "MasterId",                                           "mserp_mk_wbwarehousemasterId" },
                 //{ "Id",                                            "WarehouseMasterId" },
                 { "mserp_dataareaid_idname",                       "DataAreaId" },
                 { "mserp_warehouseid",                             "Warehouse" },
                 { "mserp_warehousename",                           "Name" },
                 { "mserp_site",                                    "Site" },
                 { "mserp_warehousetype",                           "Type" },
                 { "mserp_quarantinewarehouseid",                   "QuarantineWarehouse" },
                 { "mserp_transitwarehouseid",                      "TransitWarehouse" },
                 { "mserp_warehousetransit",                        "GoodsInTransitWarehouse" },
                 { "mserp_warehouseunder",                          "UnderDeliveryWarehouse" },
                 { "mserp_vendor",                                  "VendorAccount" },
                 { "mserp_locationiddefaultreceipt",                "DefaultReceiptLocation" },
                 { "mserp_locationiddefaultissue",                  "DefaultIssueLocation" },
                 { "mserp_defaultproductionfinishgoodslocation",    "DefaultProductionFinishedGood" },
                 //{ "mserp_dataareaid_idname",                       "DataAreaId" },
                 //{ "<SourceColumn>",                              "AddressNameDescription" },
                 //{ "<SourceColumn>",                              "Address" },
                 //{ "<SourceColumn>",                              "Purpose" }
                 {"mserp_mk_wbwarehousemasterid",                   "mserp_mk_wbwarehousemasterId"},
                 {"SinkCreatedOn", "SinkCreatedOn"},
                 {"SinkModifiedOn", "SinkModifiedOn"},
                 {"mserp_dataareaid_id", "mserp_dataareaid_id"},
                 {"mserp_dataareaid_id_entitytype", "mserp_dataareaid_id_entitytype"},
                 {"mserp_dataareaid", "mserp_dataareaid"},
                 {"versionnumber", "versionnumber"},
                 {"IsDelete", "IsDelete"},
                 {"CreatedOn", "CreatedOn"},
                //{"", "createdonpartition "},
                //{"", "CreatedAt "}
            };
        }

        if (string.Equals(tableName, ItemSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Item
                 //{ "Id",                                "ItemMasterId" },
                 { "MasterId",                            "mserp_mk_wb_ecoresreleasedproductv2entityid" },
                 { "mserp_dataareaid_idname",                 "DataAreaId" },
                 { "mserp_itemid",                 "ItemNumber" },
                 { "mserp_namealias",                 "ProductName" },
                 { "mserp_productsearchname",                 "SearchName" },
                 { "mserp_producttype",                 "ProductType" },
                 //{ "mserp_pmfproducttype",                 "ProductType" },
                 { "mserp_productsubtype",                 "ProductSubtype" },
                 //{ "",                 "ProductNumber" },
                 //{ "",                 "Description" },
                 { "mserp_storagedimensiongroup",                 "StorageDimensionGroup" },
                 { "mserp_trackingdimensiongroup",                 "TrackingDimensionGroup" },
                 { "mserp_modelgroupid",                 "ItemModelGroup" },
                 { "mserp_reservationhierarchy",                 "ReservationHierarchy" },
                 { "mserp_purchunitid",                 "PurchaseUnit" },
                 { "mserp_purchoverdeliverypct",                 "PurchaseOverDelivery" },
                 { "mserp_purchunderdeliverypct",                 "PurchaseUnderDelivery" },
                 { "mserp_itembuyergroupid",                 "BuyerGroup" },
                 { "mserp_itempricetolerancegroupid",                 "ItemPriceToleranceGroup" },
                 { "mserp_primaryvendorid",                 "Vendor" },
                 { "mserp_purchtaxitemgroupid",                 "PurchaseItemSalesTaxGroup" },
                 { "mserp_salesunitid",                 "SellUnit" },
                 { "mserp_salesoverdeliverypct",                 "SellOverDelivery" },
                 { "mserp_salesunderdeliverypct",                 "SellUnderDelivery" },
                 { "mserp_salestaxitemgroupid",                 "SellItemSalesTaxGroup" },
                 { "mserp_batchnumgroupid",                 "BatchNumberGroup" },
                 { "mserp_serialnumgroupid",                 "SerialNumberGroup" },
                 { "mserp_inventoverdeliverypct",                 "InventoryOverDelivery" },
                 { "mserp_inventunderdeliverypct",                 "InventoryUnderDelivery" },
                 { "mserp_pdscwproduct",                 "CatchWeightItem" },
                 { "mserp_pdscwunitid",                 "CWUnit" },
                 //{ "",                 "NominalQuantity" },
                 { "mserp_pdscwmin",                 "MinimumQuantity" },
                 { "mserp_pdscwmax",                 "MaximumQuantity" },
                 { "mserp_bomunitid",                 "BOMUnit" },
                 { "mserp_scrapconst",                 "ConstantScrap" },
                 { "mserp_scrapvar",                 "VariableScrap" },
                 { "mserp_costbomlevel",                 "CostingLevel" },
                 { "mserp_planninglevel",                 "PlanningLevel" },
                 //{ "mserp_costbomlevel",                 "CostCalculationLevel" },
                 { "mserp_phantom",                 "Phantom" },
                 { "mserp_bomcalcgroupid",                 "CalculationGroup" },
                 //{ "",                 "ProductionType" },
                 { "mserp_itemgroupid",                 "ItemGroup" },
                 //{ "",                 "CostUnit" },
                 //{ "",                 "LastCostPrice" },
                 { "mserp_pricedate",                 "DateOfPrice" },
                 { "mserp_uomseqgroupid",                 "UnitSequenceGroupId" },
                {"mserp_mk_wb_ecoresreleasedproductv2entityId", "mserp_mk_wb_ecoresreleasedproductv2entityid" },
                {"SinkCreatedOn", "SinkCreatedOn" },        
                {"SinkModifiedOn", "SinkModifiedOn" },
                {"mserp_dataareaid_id", "mserp_dataareaid_id" },
                {"mserp_dataareaid_id_entitytype", "mserp_dataareaid_id_entitytype" },
                {"mserp_dataareaid", "mserp_dataareaid" },
                {"versionnumber", "versionnumber" },
                {"IsDelete", "IsDelete" },
                {"CreatedOn", "CreatedOn" }
                //{"createdonpartition", "createdonpartition" },
                //{"CreatedAt", "CreatedAt" },
            };
        }
        if (string.Equals(tableName, LegalEntitySchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Warehouse
                {"MasterId",                          "Id"},
                {"Id",                          "Id"},
                {"dataarea",                    "DataAreaId"},
                //{"mserp_dataareaid_idname",              "LegalEntityName"},
                //{"mserp_remarks",              "Remarks"},
                //{"createdon",                   "CreatedAt" },
                {"SinkCreatedOn",               "SinkCreatedOn"},
                {"SinkModifiedOn",              "SinkModifiedOn"},
                {"versionnumber",               "versionnumber"},
                {"IsDelete",                    "IsDelete"},
                {"createdon",                   "CreatedOn"},
                {"createdonpartition",          "createdonpartition"}
                //{"", "CreatedAt "}
            };
        }
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}


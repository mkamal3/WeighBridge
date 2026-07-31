using System.Text.RegularExpressions;
using DeltaToSqlitePoc.Models;

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
                 { "mserp_mk_wbvendormasterid",                          "VendorId" },
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
                 //{ "",                                                   "AddressNameDescription" },
                 //{ "",                                                   "Address" },
                 //{ "",                                                   "AddressPurpose" },
                 //{ "",                                                   "ContactDescription" },
                 //{ "",                                                   "ContactType" },
                 //{ "",                                                   "ContactNumberAddress" },
                 { "mserp_primarycontactphoneextension",                 "ContactExtension" },
                 { "mserp_invoiceaccountnum",                            "InvoiceAccount" },
                 { "",                                                   "ModeOfDelivery" },
                 { "mserp_salestaxgroupcode",                            "SalesTaxGroup" }
            };
        }

        if (string.Equals(tableName, CustomerSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Customer
                // { "<SourceColumn>", "CustomerId" },
                // { "<SourceColumn>", "CustomerAccount" },
                // { "<SourceColumn>", "Name" },
                // { "<SourceColumn>", "MethodOfPayment" },
                // { "<SourceColumn>", "TermsOfPayment" },
                // { "<SourceColumn>", "DeliveryTerms" },
                // { "<SourceColumn>", "AccountStatus" },
                // { "<SourceColumn>", "AccountStatusReason" },
                // { "<SourceColumn>", "CustomerGroup" },
                // { "<SourceColumn>", "EmployeeResponsible" },
                // { "<SourceColumn>", "Currency" },
                // { "<SourceColumn>", "Telephone" },
                // { "<SourceColumn>", "OrganizationPerson" },
                // { "<SourceColumn>", "SearchName" },
                // { "<SourceColumn>", "ClassificationGroup" },
                // { "<SourceColumn>", "AddressNameDescription" },
                // { "<SourceColumn>", "Address" },
                // { "<SourceColumn>", "AddressPurpose" },
                // { "<SourceColumn>", "ContactDescription" },
                // { "<SourceColumn>", "ContactType" },
                // { "<SourceColumn>", "ContactNumberAddress" },
                // { "<SourceColumn>", "ContactExtension" },
                // { "<SourceColumn>", "InvoiceAccount" },
                // { "<SourceColumn>", "ModeOfDelivery" },
                // { "<SourceColumn>", "SalesTaxGroup" }
            };
        }

        if (string.Equals(tableName, WarehouseSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Warehouse
                 { "mserp_mk_wbwarehousemasterid",                                      "WarehouseMasterId" },
                 { "mserp_warehouseid",                                      "Warehouse" },
                 { "mserp_warehousename",                                      "Name" },
                 { "mserp_site",                                      "Site" },
                 //{ "<SourceColumn>",                                      "Type" },
                 { "mserp_quarantinewarehouseid",                                      "QuarantineWarehouse" },
                 { "mserp_transitwarehouseid",                                      "TransitWarehouse" },
                 { "mserp_warehousetransit",                                      "GoodsInTransitWarehouse" },
                 { "mserp_warehouseunder",                                      "UnderDeliveryWarehouse" },
                 { "mserp_vendor",                                      "VendorAccount" },
                 { "mserp_locationiddefaultreceipt",                                      "DefaultReceiptLocation" },
                 { "mserp_locationiddefaultissue",                                      "DefaultIssueLocation" },
                 { "mserp_defaultproductionfinishgoodslocation",                                      "DefaultProductionFinishedGood" },
                 //{ "<SourceColumn>",                                      "AddressNameDescription" },
                 //{ "<SourceColumn>",                                      "Address" },
                 //{ "<SourceColumn>",                                      "Purpose" }
            };
        }

        if (string.Equals(tableName, ItemSchema.DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Item
                // { "<SourceColumn>", "ItemMasterId" },
                // { "<SourceColumn>", "ItemNumber" },
                // { "<SourceColumn>", "ProductName" },
                // { "<SourceColumn>", "SearchName" },
                // { "<SourceColumn>", "ProductType" },
                // { "<SourceColumn>", "ProductSubtype" },
                // { "<SourceColumn>", "ProductNumber" },
                // { "<SourceColumn>", "Description" },
                // { "<SourceColumn>", "StorageDimensionGroup" },
                // { "<SourceColumn>", "TrackingDimensionGroup" },
                // { "<SourceColumn>", "ItemModelGroup" },
                // { "<SourceColumn>", "ReservationHierarchy" },
                // { "<SourceColumn>", "PurchaseUnit" },
                // { "<SourceColumn>", "PurchaseOverDelivery" },
                // { "<SourceColumn>", "PurchaseUnderDelivery" },
                // { "<SourceColumn>", "BuyerGroup" },
                // { "<SourceColumn>", "ItemPriceToleranceGroup" },
                // { "<SourceColumn>", "Vendor" },
                // { "<SourceColumn>", "PurchaseItemSalesTaxGroup" },
                // { "<SourceColumn>", "SellUnit" },
                // { "<SourceColumn>", "SellOverDelivery" },
                // { "<SourceColumn>", "SellUnderDelivery" },
                // { "<SourceColumn>", "SellItemSalesTaxGroup" },
                // { "<SourceColumn>", "BatchNumberGroup" },
                // { "<SourceColumn>", "SerialNumberGroup" },
                // { "<SourceColumn>", "InventoryOverDelivery" },
                // { "<SourceColumn>", "InventoryUnderDelivery" },
                // { "<SourceColumn>", "CatchWeightItem" },
                // { "<SourceColumn>", "CWUnit" },
                // { "<SourceColumn>", "NominalQuantity" },
                // { "<SourceColumn>", "MinimumQuantity" },
                // { "<SourceColumn>", "MaximumQuantity" },
                // { "<SourceColumn>", "BOMUnit" },
                // { "<SourceColumn>", "ConstantScrap" },
                // { "<SourceColumn>", "VariableScrap" },
                // { "<SourceColumn>", "CostingLevel" },
                // { "<SourceColumn>", "PlanningLevel" },
                // { "<SourceColumn>", "CostCalculationLevel" },
                // { "<SourceColumn>", "Phantom" },
                // { "<SourceColumn>", "CalculationGroup" },
                // { "<SourceColumn>", "ProductionType" },
                // { "<SourceColumn>", "ItemGroup" },
                // { "<SourceColumn>", "CostUnit" },
                // { "<SourceColumn>", "LastCostPrice" },
                // { "<SourceColumn>", "DateOfPrice" },
                // { "<SourceColumn>", "UnitSequenceGroupId" }
            };
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}


namespace WeightBridgeApp.Models;

public class WeighmentCorrectionDetail
{
    public int CorrectionDetailId { get; set; }
    public int CorrectionId { get; set; }
    public string DetailType { get; set; } = "Header"; // Header or MaterialLine
    public int LineNo { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string OriginalValue { get; set; } = string.Empty;
    public string CorrectedValue { get; set; } = string.Empty;

    public int? OriginalMaterialLineId { get; set; }
    public string ActionType { get; set; } = string.Empty; // Add, Modify, Replace, Remove
    public int? OriginalItemMasterId { get; set; }
    public int? CorrectedItemMasterId { get; set; }
    public string OriginalItemNumber { get; set; } = string.Empty;
    public string CorrectedItemNumber { get; set; } = string.Empty;
    public string OriginalItemName { get; set; } = string.Empty;
    public string CorrectedItemName { get; set; } = string.Empty;
    public string OriginalUom { get; set; } = string.Empty;
    public string CorrectedUom { get; set; } = string.Empty;
    public decimal? OriginalExpectedQty { get; set; }
    public decimal? CorrectedExpectedQty { get; set; }
    public string OriginalRemarks { get; set; } = string.Empty;
    public string CorrectedRemarks { get; set; } = string.Empty;
}

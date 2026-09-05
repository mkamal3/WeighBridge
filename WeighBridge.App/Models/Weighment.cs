namespace WeightBridgeApp.Models;

public class Weighment
{
    public int WeighmentId { get; set; }
    public string DataAreaId { get; set; } = string.Empty;
    public string TicketNo { get; set; } = string.Empty;
    public string SlipNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public string GatePassNumber { get; set; } = string.Empty;
    public string WeighbridgeCode { get; set; } = string.Empty;
    public DateTime? TransactionDateTime { get; set; }
    public string ShiftCode { get; set; } = string.Empty;
    public string OperatorUsername { get; set; } = string.Empty;
    public string ExternalReference { get; set; } = string.Empty;
    public string OperatorRemarks { get; set; } = string.Empty;
    public string VehicleNo { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public int? MaterialId { get; set; }
    public string ItemNumber { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal FirstWeight { get; set; }
    public DateTime FirstWeightTime { get; set; }
    public string FirstWeightBy { get; set; } = string.Empty;
    public string FirstWeightByDisplay { get; set; } = string.Empty;
    public decimal? SecondWeight { get; set; }
    public DateTime? SecondWeightTime { get; set; }
    public string SecondWeightBy { get; set; } = string.Empty;
    public string SecondWeightByDisplay { get; set; } = string.Empty;
    public decimal? NetWeight { get; set; }
    public string Status { get; set; } = "Open";
    public string CancellationVoidNumber { get; set; } = string.Empty;
    public string CancellationVoidStatus { get; set; } = string.Empty;
    public bool IsCorrected { get; set; }
    public int CorrectionVersion { get; set; }
    public string LastCorrectionNumber { get; set; } = string.Empty;
    public DateTime? LastCorrectedDateTime { get; set; }
    public string LastCorrectedBy { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string ResumeLockedBy { get; set; } = string.Empty;
    public DateTime? ResumeLockedAt { get; set; }
    public string LastUpdatedBy { get; set; } = string.Empty;
    public DateTime? LastUpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string CurrentStage
    {
        get
        {
            if (string.Equals(Status, "Cancelled", StringComparison.OrdinalIgnoreCase)) return "Cancelled";
            if (string.Equals(Status, "Completed", StringComparison.OrdinalIgnoreCase)) return IsCorrected ? "Completed / Corrected" : "Completed";
            if (!SecondWeight.HasValue && FirstWeight > 0) return "W1 Captured / W2 Pending";
            if (FirstWeight <= 0) return "Waiting for First Weight";
            return Status;
        }
    }

    public string OpenAgeText
    {
        get
        {
            var start = TransactionDateTime ?? (FirstWeightTime == default ? CreatedAt : FirstWeightTime);
            var age = DateTime.Now - start;
            if (age.TotalDays >= 1) return $"{(int)age.TotalDays}d {age.Hours}h";
            if (age.TotalHours >= 1) return $"{(int)age.TotalHours}h {age.Minutes}m";
            return $"{Math.Max(0, age.Minutes)}m";
        }
    }

    public bool IsStaleOpenTransaction
    {
        get
        {
            if (!string.Equals(Status, "Open", StringComparison.OrdinalIgnoreCase)) return false;
            var start = TransactionDateTime ?? (FirstWeightTime == default ? CreatedAt : FirstWeightTime);
            return DateTime.Now - start >= TimeSpan.FromHours(24);
        }
    }

    public string AttentionStatus => IsStaleOpenTransaction ? "Stale" : string.Empty;
    public string ResumeLockDisplay => string.IsNullOrWhiteSpace(ResumeLockedBy)
        ? string.Empty
        : $"{ResumeLockedBy}{(ResumeLockedAt.HasValue ? $" @ {ResumeLockedAt:yyyy-MM-dd HH:mm}" : string.Empty)}";
}

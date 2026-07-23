using System.IO;
using System.Globalization;
using System.Text;
using WeightBridgeApp.Models;

namespace WeightBridgeApp.Services;

public static class CsvExportService
{
    public static string ExportWeighments(IEnumerable<Weighment> rows)
    {
        var folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BridgeOneExports");
        System.IO.Directory.CreateDirectory(folder);

        var filePath = System.IO.Path.Combine(folder, $"BridgeOneReport-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        var sb = new StringBuilder();
        sb.AppendLine("TicketNo,CompanyName,VehicleNo,DriverName,PartyAccount,PartyName,PartyType,ItemNumber,ItemName,FirstWeight,FirstWeightTime,FirstWeightBy,SecondWeight,SecondWeightTime,SecondWeightBy,NetWeight,Status,Remarks");

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",",
                Escape(row.TicketNo),
                Escape(row.CompanyName),
                Escape(row.VehicleNo),
                Escape(row.DriverName),
                Escape(row.PartyAccount),
                Escape(row.PartyName),
                Escape(row.PartyType),
                Escape(row.ItemNumber),
                Escape(string.IsNullOrWhiteSpace(row.ItemName) ? row.MaterialName : row.ItemName),
                row.FirstWeight.ToString(CultureInfo.InvariantCulture),
                Escape(row.FirstWeightTime.ToString("yyyy-MM-dd HH:mm:ss")),
                Escape(row.FirstWeightBy),
                row.SecondWeight?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                Escape(row.SecondWeightTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty),
                Escape(row.SecondWeightBy),
                row.NetWeight?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                Escape(row.Status),
                Escape(row.Remarks)
            ));
        }

        System.IO.File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        return filePath;
    }

    private static string Escape(string value)
    {
        value ??= string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}

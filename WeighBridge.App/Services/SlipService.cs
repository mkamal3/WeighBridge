using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using WeightBridgeApp.Models;

namespace WeightBridgeApp.Services;

public static class SlipService
{
    public static string ExportSlip(Weighment weighment)
    {
        var folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BridgeOneSlips");
        Directory.CreateDirectory(folder);

        var filePath = System.IO.Path.Combine(folder, $"Slip-{SafeFileName(weighment.TicketNo)}.pdf");
        File.WriteAllBytes(filePath, BuildSlipPdf(weighment));
        return filePath;
    }

    public static bool PrintSlip(Weighment weighment)
    {
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true)
            return false;

        var pageWidth = printDialog.PrintableAreaWidth > 0 ? printDialog.PrintableAreaWidth : 793;
        var pageHeight = printDialog.PrintableAreaHeight > 0 ? printDialog.PrintableAreaHeight : 1122;
        var page = BuildSlipPage(weighment, pageWidth, pageHeight);

        page.Measure(new Size(pageWidth, pageHeight));
        page.Arrange(new Rect(new Size(pageWidth, pageHeight)));
        page.UpdateLayout();

        printDialog.PrintVisual(page, $"BridgeOne Slip {weighment.TicketNo}");
        return true;
    }

    public static string BuildSlipText(Weighment w)
    {
        var sb = new StringBuilder();
        sb.AppendLine("========================================");
        sb.AppendLine("           BRIDGEONE SLIP             ");
        sb.AppendLine("========================================");
        sb.AppendLine($"Ticket No     : {w.TicketNo}");
        sb.AppendLine($"Company       : {w.CompanyName}");
        sb.AppendLine($"Vehicle No    : {w.VehicleNo}");
        sb.AppendLine($"Driver Name   : {w.DriverName}");
        sb.AppendLine($"Party         : {FormatParty(w)}");
        sb.AppendLine($"Item          : {w.MaterialName}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"First Weight  : {FormatWeight(w.FirstWeight)}");
        sb.AppendLine($"First Time    : {FormatDate(w.FirstWeightTime)}");
        sb.AppendLine($"Second Weight : {(w.SecondWeight.HasValue ? FormatWeight(w.SecondWeight.Value) : string.Empty)}");
        sb.AppendLine($"Second Time   : {(w.SecondWeightTime.HasValue ? FormatDate(w.SecondWeightTime.Value) : string.Empty)}");
        sb.AppendLine($"Net Weight    : {(w.NetWeight.HasValue ? FormatWeight(w.NetWeight.Value) : string.Empty)}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"Remarks       : {w.Remarks}");
        sb.AppendLine($"Printed At    : {FormatDate(DateTime.Now)}");
        sb.AppendLine("========================================");
        sb.AppendLine("This is a system generated slip.");
        return sb.ToString();
    }

    private static FixedPage BuildSlipPage(Weighment w, double pageWidth, double pageHeight)
    {
        var page = new FixedPage
        {
            Width = pageWidth,
            Height = pageHeight,
            Background = Brushes.White
        };

        var margin = Math.Max(35, Math.Min(55, pageWidth * 0.06));
        var contentWidth = pageWidth - (margin * 2);
        var y = 42d;

        AddText(page, "BRIDGEONE SLIP", margin, y, contentWidth, 22, FontWeights.Bold, TextAlignment.Center);
        y += 30;
        AddText(page, "System generated weighment slip", margin, y, contentWidth, 11, FontWeights.Normal, TextAlignment.Center);
        y += 35;

        DrawLine(page, margin, y, margin + contentWidth, y, 1.2);
        y += 15;

        var rows = GetSlipRows(w);
        foreach (var row in rows)
        {
            AddText(page, row.Label, margin + 8, y, 155, 12, FontWeights.SemiBold, TextAlignment.Left);
            AddText(page, row.Value, margin + 175, y, contentWidth - 185, 12, FontWeights.Normal, TextAlignment.Left);
            y += 25;

            if (row.Label == "Item" || row.Label == "Net Weight")
            {
                DrawLine(page, margin, y - 4, margin + contentWidth, y - 4, 0.8);
                y += 10;
            }
        }

        y += 22;
        DrawLine(page, margin, y, margin + contentWidth, y, 1.0);
        y += 35;
        AddText(page, "Authorized Signature ____________________", margin + 8, y, contentWidth / 2 - 20, 11, FontWeights.Normal, TextAlignment.Left);
        AddText(page, "Operator Signature ____________________", margin + contentWidth / 2 + 10, y, contentWidth / 2 - 18, 11, FontWeights.Normal, TextAlignment.Left);

        y += 42;
        AddText(page, "This is a system generated slip.", margin, y, contentWidth, 10, FontWeights.Normal, TextAlignment.Center);

        return page;
    }

    private static void AddText(FixedPage page, string text, double x, double y, double width, double fontSize, FontWeight fontWeight, TextAlignment alignment)
    {
        var block = new TextBlock
        {
            Text = text ?? string.Empty,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = fontSize,
            FontWeight = fontWeight,
            TextAlignment = alignment,
            Width = width,
            TextWrapping = TextWrapping.Wrap
        };

        FixedPage.SetLeft(block, x);
        FixedPage.SetTop(block, y);
        page.Children.Add(block);
    }

    private static void DrawLine(FixedPage page, double x1, double y1, double x2, double y2, double thickness)
    {
        var line = new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = Brushes.LightGray,
            StrokeThickness = thickness
        };
        page.Children.Add(line);
    }

    private static List<(string Label, string Value)> GetSlipRows(Weighment w) => new()
    {
        ("Ticket No", w.TicketNo),
        ("Company", w.CompanyName),
        ("Vehicle No", w.VehicleNo),
        ("Driver Name", w.DriverName),
        ("Party", FormatParty(w)),
        ("Item", w.MaterialName),
        ("First Weight", FormatWeight(w.FirstWeight)),
        ("First Time", FormatDate(w.FirstWeightTime)),
        ("Second Weight", w.SecondWeight.HasValue ? FormatWeight(w.SecondWeight.Value) : string.Empty),
        ("Second Time", w.SecondWeightTime.HasValue ? FormatDate(w.SecondWeightTime.Value) : string.Empty),
        ("Net Weight", w.NetWeight.HasValue ? FormatWeight(w.NetWeight.Value) : string.Empty),
        ("Status", w.Status),
        ("Remarks", w.Remarks),
        ("Printed At", FormatDate(DateTime.Now))
    };

    private static byte[] BuildSlipPdf(Weighment w)
    {
        var content = new StringBuilder();

        AddPdfText(content, 0, 790, "BRIDGEONE SLIP", 20, bold: true, centered: true);
        AddPdfText(content, 0, 770, "System generated weighment slip", 10, bold: false, centered: true);
        DrawPdfLine(content, 50, 748, 545, 748);

        var y = 720;
        foreach (var row in GetSlipRows(w))
        {
            AddPdfText(content, 65, y, row.Label, 11, bold: true);
            AddPdfText(content, 190, y, row.Value, 11, bold: false);
            y -= 24;

            if (row.Label == "Item" || row.Label == "Net Weight")
            {
                DrawPdfLine(content, 50, y + 8, 545, y + 8);
                y -= 10;
            }
        }

        DrawPdfLine(content, 50, 92, 545, 92);
        AddPdfText(content, 65, 68, "Authorized Signature ____________________", 10, bold: false);
        AddPdfText(content, 335, 68, "Operator Signature ____________________", 10, bold: false);
        AddPdfText(content, 0, 38, "This is a system generated slip.", 9, bold: false, centered: true);

        return CreatePdf(content.ToString());
    }

    private static string FormatParty(Weighment w)
    {
        if (string.IsNullOrWhiteSpace(w.PartyType))
            return w.PartyName ?? string.Empty;

        return string.IsNullOrWhiteSpace(w.PartyName)
            ? w.PartyType
            : $"{w.PartyName} ({w.PartyType})";
    }

    private static string FormatWeight(decimal value) => value.ToString("N2", CultureInfo.CurrentCulture) + " kg";

    private static string FormatDate(DateTime value) => value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);

    private static void AddPdfText(StringBuilder content, double x, double y, string? text, int size, bool bold, bool centered = false)
    {
        var safe = PdfEscape(text);
        var font = bold ? "F2" : "F1";

        if (centered)
        {
            var estimatedWidth = safe.Length * size * 0.27;
            x = Math.Max(50, 297 - estimatedWidth);
        }

        content.AppendLine("BT");
        content.AppendLine($"/{font} {size} Tf");
        content.AppendLine($"{x.ToString("0.##", CultureInfo.InvariantCulture)} {y.ToString("0.##", CultureInfo.InvariantCulture)} Td");
        content.AppendLine($"({safe}) Tj");
        content.AppendLine("ET");
    }

    private static void DrawPdfLine(StringBuilder content, double x1, double y1, double x2, double y2)
    {
        content.AppendLine("0.75 w");
        content.AppendLine($"{x1.ToString("0.##", CultureInfo.InvariantCulture)} {y1.ToString("0.##", CultureInfo.InvariantCulture)} m {x2.ToString("0.##", CultureInfo.InvariantCulture)} {y2.ToString("0.##", CultureInfo.InvariantCulture)} l S");
    }

    private static byte[] CreatePdf(string pageContent)
    {
        var streamData = pageContent + "\n";
        var contentBytes = Encoding.ASCII.GetBytes(streamData);
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>",
            $"<< /Length {contentBytes.Length} >>\nstream\n{streamData}endstream"
        };

        using var ms = new MemoryStream();
        WriteAscii(ms, "%PDF-1.4\n%\u00E2\u00E3\u00CF\u00D3\n");
        var offsets = new List<long> { 0 };

        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(ms.Position);
            WriteAscii(ms, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }

        var xrefOffset = ms.Position;
        WriteAscii(ms, $"xref\n0 {objects.Count + 1}\n");
        WriteAscii(ms, "0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
            WriteAscii(ms, offset.ToString("0000000000", CultureInfo.InvariantCulture) + " 00000 n \n");

        WriteAscii(ms, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
        return ms.ToArray();
    }

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string PdfEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var normalized = value.Replace("\r", " ").Replace("\n", " ");
        var safe = normalized.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        var sb = new StringBuilder(safe.Length);
        foreach (var ch in safe)
            sb.Append(ch <= 127 ? ch : '?');

        return sb.ToString();
    }

    private static string SafeFileName(string? value)
    {
        var text = string.IsNullOrWhiteSpace(value) ? DateTime.Now.ToString("yyyyMMddHHmmss") : value;
        foreach (var invalid in System.IO.Path.GetInvalidFileNameChars())
            text = text.Replace(invalid, '-');
        return text;
    }
}

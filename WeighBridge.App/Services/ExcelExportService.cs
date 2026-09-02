using System.IO;
using System.IO.Compression;
using System.Text;

namespace WeightBridgeApp.Services;

public sealed class ExcelWorksheetData
{
    public ExcelWorksheetData(
        string name,
        IReadOnlyList<string> headers,
        IReadOnlyList<IDictionary<string, string>> rows)
    {
        Name = name;
        Headers = headers;
        Rows = rows;
    }

    public string Name { get; }
    public IReadOnlyList<string> Headers { get; }
    public IReadOnlyList<IDictionary<string, string>> Rows { get; }
}

public static class ExcelExportService
{
    public static void ExportWorkbook(string filePath, IReadOnlyList<ExcelWorksheetData> worksheets)
    {
        if (worksheets == null || worksheets.Count == 0)
            throw new ArgumentException("At least one worksheet is required.", nameof(worksheets));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        if (File.Exists(filePath)) File.Delete(filePath);

        var normalizedSheets = NormalizeSheetNames(worksheets);

        using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);
        WriteEntry(archive, "[Content_Types].xml", ContentTypesXml(normalizedSheets.Count));
        WriteEntry(archive, "_rels/.rels", RootRelsXml());
        WriteEntry(archive, "xl/workbook.xml", WorkbookXml(normalizedSheets));
        WriteEntry(archive, "xl/_rels/workbook.xml.rels", WorkbookRelsXml(normalizedSheets.Count));
        WriteEntry(archive, "xl/styles.xml", StylesXml());

        for (var i = 0; i < normalizedSheets.Count; i++)
        {
            var sheet = normalizedSheets[i];
            WriteEntry(
                archive,
                $"xl/worksheets/sheet{i + 1}.xml",
                WorksheetXml(sheet.Headers, sheet.Rows));
        }
    }

    private static List<ExcelWorksheetData> NormalizeSheetNames(IReadOnlyList<ExcelWorksheetData> worksheets)
    {
        var result = new List<ExcelWorksheetData>();
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var worksheet in worksheets)
        {
            var baseName = SanitizeSheetName(worksheet.Name);
            var name = baseName;
            var suffix = 2;

            while (!usedNames.Add(name))
            {
                var suffixText = $" ({suffix++})";
                var maxBaseLength = Math.Max(1, 31 - suffixText.Length);
                name = baseName[..Math.Min(baseName.Length, maxBaseLength)] + suffixText;
            }

            result.Add(new ExcelWorksheetData(name, worksheet.Headers, worksheet.Rows));
        }

        return result;
    }

    private static string SanitizeSheetName(string? value)
    {
        var name = string.IsNullOrWhiteSpace(value) ? "Sheet" : value.Trim();
        foreach (var invalid in new[] { '\\', '/', '?', '*', '[', ']', ':' })
            name = name.Replace(invalid, '-');

        name = name.Trim('\'');
        if (string.IsNullOrWhiteSpace(name)) name = "Sheet";
        if (name.Length > 31) name = name[..31];
        return name;
    }

    private static void WriteEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string WorksheetXml(
        IReadOnlyList<string> requestedHeaders,
        IReadOnlyList<IDictionary<string, string>> rows)
    {
        var headers = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in requestedHeaders)
        {
            if (!string.IsNullOrWhiteSpace(header) && seen.Add(header))
                headers.Add(header);
        }

        foreach (var row in rows)
        {
            foreach (var key in row.Keys)
            {
                if (seen.Add(key)) headers.Add(key);
            }
        }

        if (headers.Count == 0) headers.Add("No Data");

        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
        AppendRow(sb, 1, headers.Select(h => (h, 1)).ToList());
        for (var i = 0; i < rows.Count; i++)
        {
            var values = headers.Select(h => (rows[i].TryGetValue(h, out var v) ? v ?? string.Empty : string.Empty, 0)).ToList();
            AppendRow(sb, i + 2, values);
        }
        sb.Append("</sheetData><autoFilter ref=\"A1:")
          .Append(ColumnName(headers.Count))
          .Append(Math.Max(1, rows.Count + 1))
          .Append("\"/></worksheet>");
        return sb.ToString();
    }

    private static void AppendRow(StringBuilder sb, int rowIndex, IReadOnlyList<(string Value, int Style)> values)
    {
        sb.Append("<row r=\"").Append(rowIndex).Append("\">");
        for (var i = 0; i < values.Count; i++)
        {
            var cellRef = ColumnName(i + 1) + rowIndex;
            sb.Append("<c r=\"").Append(cellRef).Append("\" t=\"inlineStr\"");
            if (values[i].Style > 0) sb.Append(" s=\"").Append(values[i].Style).Append("\"");
            sb.Append("><is><t xml:space=\"preserve\">")
              .Append(Escape(values[i].Value))
              .Append("</t></is></c>");
        }
        sb.Append("</row>");
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&apos;");
    }

    private static string ColumnName(int number)
    {
        var result = string.Empty;
        while (number > 0)
        {
            number--;
            result = (char)('A' + number % 26) + result;
            number /= 26;
        }
        return result;
    }

    private static string ContentTypesXml(int worksheetCount)
    {
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        sb.Append("<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");
        sb.Append("<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>");
        sb.Append("<Default Extension=\"xml\" ContentType=\"application/xml\"/>");
        sb.Append("<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>");
        for (var i = 1; i <= worksheetCount; i++)
            sb.Append($"<Override PartName=\"/xl/worksheets/sheet{i}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
        sb.Append("<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>");
        sb.Append("</Types>");
        return sb.ToString();
    }

    private static string RootRelsXml() => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>
""";

    private static string WorkbookXml(IReadOnlyList<ExcelWorksheetData> worksheets)
    {
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        sb.Append("<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets>");
        for (var i = 0; i < worksheets.Count; i++)
        {
            sb.Append("<sheet name=\"")
              .Append(Escape(worksheets[i].Name))
              .Append("\" sheetId=\"")
              .Append(i + 1)
              .Append("\" r:id=\"rId")
              .Append(i + 1)
              .Append("\"/>");
        }
        sb.Append("</sheets></workbook>");
        return sb.ToString();
    }

    private static string WorkbookRelsXml(int worksheetCount)
    {
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        sb.Append("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
        for (var i = 1; i <= worksheetCount; i++)
            sb.Append($"<Relationship Id=\"rId{i}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{i}.xml\"/>");
        sb.Append($"<Relationship Id=\"rId{worksheetCount + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>");
        sb.Append("</Relationships>");
        return sb.ToString();
    }

    private static string StylesXml() => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><name val="Calibri"/></font></fonts>
  <fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills>
  <borders count="1"><border/></borders>
  <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
  <cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/><xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/></cellXfs>
</styleSheet>
""";
}

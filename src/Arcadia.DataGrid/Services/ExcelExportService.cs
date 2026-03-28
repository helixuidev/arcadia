using System.IO;
using Arcadia.DataGrid.Components;

namespace Arcadia.DataGrid.Services;

/// <summary>
/// Generates Excel (XLSX) files from DataGrid data using Open XML format.
/// Pure C# implementation — no COM, no Office dependency.
/// </summary>
public static class ExcelExportService
{
    /// <summary>
    /// Export grid data to an XLSX byte array.
    /// Uses the SpreadsheetML format (Office Open XML) without requiring DocumentFormat.OpenXml.
    /// </summary>
    public static byte[] ToXlsx<TItem>(
        IReadOnlyList<ArcadiaColumn<TItem>> columns,
        IEnumerable<TItem> data,
        string sheetName = "Sheet1")
    {
        using var ms = new MemoryStream();
        using (var writer = new XlsxWriter(ms, sheetName))
        {
            // Header row
            var visibleCols = columns.Where(c => c.IsVisible && c.ResolvedField is not null).ToList();
            writer.WriteRow(visibleCols.Select(c => c.Title).ToArray());

            // Data rows
            foreach (var item in data)
            {
                var values = visibleCols.Select(c => c.FormatValue(c.ResolvedField!(item))).ToArray();
                writer.WriteRow(values);
            }
        }

        return ms.ToArray();
    }
}

/// <summary>
/// Minimal XLSX writer using raw Open XML (ZIP + XML).
/// No external dependencies. Generates a valid .xlsx file that Excel, Google Sheets, and LibreOffice can open.
/// </summary>
internal sealed class XlsxWriter : IDisposable
{
    private readonly System.IO.Compression.ZipArchive _zip;
    private readonly StreamWriter _sheetWriter;
    private readonly string _sheetName;
    private int _rowIndex;

    public XlsxWriter(Stream output, string sheetName)
    {
        _sheetName = sheetName;
        _zip = new System.IO.Compression.ZipArchive(output, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true);

        // [Content_Types].xml
        WriteEntry("[Content_Types].xml", ContentTypesXml());

        // _rels/.rels
        WriteEntry("_rels/.rels", RelsXml());

        // xl/_rels/workbook.xml.rels
        WriteEntry("xl/_rels/workbook.xml.rels", WorkbookRelsXml());

        // xl/workbook.xml
        WriteEntry("xl/workbook.xml", WorkbookXml(sheetName));

        // xl/styles.xml (minimal — header bold)
        WriteEntry("xl/styles.xml", StylesXml());

        // Start sheet — we'll write rows incrementally
        var entry = _zip.CreateEntry("xl/worksheets/sheet1.xml", System.IO.Compression.CompressionLevel.Fastest);
        var stream = entry.Open();
        _sheetWriter = new StreamWriter(stream, System.Text.Encoding.UTF8);
        _sheetWriter.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        _sheetWriter.Write("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
        _sheetWriter.Write("<sheetData>");
    }

    public void WriteRow(string[] values)
    {
        _rowIndex++;
        var isHeader = _rowIndex == 1;
        _sheetWriter.Write($"<row r=\"{_rowIndex}\">");
        for (var c = 0; c < values.Length; c++)
        {
            var colRef = GetColumnRef(c);
            var cellRef = $"{colRef}{_rowIndex}";
            var escaped = System.Security.SecurityElement.Escape(values[c] ?? "");

            // Try to write as number if possible
            if (!isHeader && double.TryParse(values[c], System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var num))
            {
                _sheetWriter.Write($"<c r=\"{cellRef}\"{(isHeader ? " s=\"1\"" : "")}><v>{num}</v></c>");
            }
            else
            {
                _sheetWriter.Write($"<c r=\"{cellRef}\" t=\"inlineStr\"{(isHeader ? " s=\"1\"" : "")}><is><t>{escaped}</t></is></c>");
            }
        }
        _sheetWriter.Write("</row>");
    }

    public void Dispose()
    {
        _sheetWriter.Write("</sheetData></worksheet>");
        _sheetWriter.Flush();
        _sheetWriter.Dispose();
        _zip.Dispose();
    }

    private static string GetColumnRef(int index)
    {
        var result = "";
        while (index >= 0)
        {
            result = (char)('A' + index % 26) + result;
            index = index / 26 - 1;
        }
        return result;
    }

    private void WriteEntry(string path, string content)
    {
        var entry = _zip.CreateEntry(path, System.IO.Compression.CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), System.Text.Encoding.UTF8);
        writer.Write(content);
    }

    private static string ContentTypesXml() => """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
          <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
          <Default Extension="xml" ContentType="application/xml"/>
          <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
          <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
          <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
        </Types>
        """;

    private static string RelsXml() => """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
        </Relationships>
        """;

    private static string WorkbookRelsXml() => """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
          <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
        </Relationships>
        """;

    private static string WorkbookXml(string sheetName) => $"""
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
          <sheets><sheet name="{System.Security.SecurityElement.Escape(sheetName)}" sheetId="1" r:id="rId1"/></sheets>
        </workbook>
        """;

    private static string StylesXml() => """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
          <fonts count="2">
            <font><sz val="11"/><name val="Calibri"/></font>
            <font><b/><sz val="11"/><name val="Calibri"/></font>
          </fonts>
          <fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills>
          <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
          <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
          <cellXfs count="2">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
            <xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/>
          </cellXfs>
        </styleSheet>
        """;
}

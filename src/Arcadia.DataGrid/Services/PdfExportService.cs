using Arcadia.DataGrid.Components;

namespace Arcadia.DataGrid.Services;

/// <summary>
/// Generates PDF files from DataGrid data using raw PDF 1.4 syntax.
/// Pure C# implementation — no external dependencies, no interop.
/// Produces a valid PDF with a styled table that Acrobat, Preview, Chrome, and Edge can open.
/// </summary>
public static class PdfExportService
{
    /// <summary>
    /// Export grid data to a PDF byte array.
    /// </summary>
    /// <param name="columns">Visible columns with resolved fields.</param>
    /// <param name="data">Enumerable of data items.</param>
    /// <param name="title">Optional title rendered above the table.</param>
    /// <param name="pageOrientation">Page orientation: "portrait" or "landscape". Default is "landscape".</param>
    public static byte[] ToPdf<TItem>(
        IReadOnlyList<ArcadiaColumn<TItem>> columns,
        IEnumerable<TItem> data,
        string? title = null,
        string pageOrientation = "landscape")
    {
        var visibleCols = columns.Where(c => c.IsVisible && c.ResolvedField is not null).ToList();
        var rows = data.ToList();

        var builder = new PdfTableBuilder(
            visibleCols.Count,
            string.Equals(pageOrientation, "portrait", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(title))
            builder.AddTitle(title);

        builder.AddHeaderRow(visibleCols.Select(c => c.Title).ToArray());

        foreach (var item in rows)
        {
            var values = visibleCols.Select(c => c.FormatValue(c.ResolvedField!(item))).ToArray();
            builder.AddDataRow(values);
        }

        return builder.Build();
    }
}

/// <summary>
/// Builds a multi-page PDF with table content. Collects all page content streams
/// first, then assembles the complete PDF in one pass.
/// </summary>
internal sealed class PdfTableBuilder
{
    // Page dimensions in points (1 pt = 1/72 inch)
    private readonly float _pageWidth;
    private readonly float _pageHeight;
    private const float Margin = 40f;
    private const float CellPadding = 4f;
    private const float FontSize = 8f;
    private const float HeaderFontSize = 8f;
    private const float TitleFontSize = 14f;
    private const float RowHeight = 16f;
    private const float HeaderRowHeight = 20f;

    private readonly float _tableWidth;
    private readonly float _colWidth;
    private float _currentY;

    private readonly List<string> _pageContents = new();
    private readonly List<string> _currentPageOps = new();

    // Stored header for reprinting on each new page
    private string[]? _headerValues;

    public PdfTableBuilder(int columnCount, bool portrait)
    {
        if (portrait)
        {
            _pageWidth = 595.28f;
            _pageHeight = 841.89f;
        }
        else
        {
            _pageWidth = 841.89f;
            _pageHeight = 595.28f;
        }

        _tableWidth = _pageWidth - 2 * Margin;
        _colWidth = _tableWidth / Math.Max(columnCount, 1);
        _currentY = _pageHeight - Margin;
    }

    public void AddTitle(string title)
    {
        _currentY -= TitleFontSize + 8;
        _currentPageOps.Add($"BT /F2 {TitleFontSize} Tf 0.1 0.05 0.2 rg {Margin} {F(_currentY)} Td ({Esc(title)}) Tj ET");
        _currentY -= 8;
    }

    public void AddHeaderRow(string[] headers)
    {
        _headerValues = headers;
        RenderHeaderRow(headers);
    }

    private void RenderHeaderRow(string[] headers)
    {
        EnsureSpace(HeaderRowHeight);
        var y = _currentY;

        // Background
        _currentPageOps.Add($"0.92 0.91 0.96 rg {F(Margin)} {F(y - HeaderRowHeight)} {F(_tableWidth)} {F(HeaderRowHeight)} re f");
        // Border
        _currentPageOps.Add($"0.7 0.7 0.8 RG 0.5 w {F(Margin)} {F(y - HeaderRowHeight)} {F(_tableWidth)} {F(HeaderRowHeight)} re S");

        for (var i = 0; i < headers.Length; i++)
        {
            var x = Margin + i * _colWidth;
            if (i > 0)
                _currentPageOps.Add($"0.8 0.8 0.85 RG 0.3 w {F(x)} {F(y)} m {F(x)} {F(y - HeaderRowHeight)} l S");

            var text = Truncate(headers[i], _colWidth - 2 * CellPadding, HeaderFontSize);
            _currentPageOps.Add($"BT /F2 {HeaderFontSize} Tf 0.2 0.2 0.3 rg {F(x + CellPadding)} {F(y - HeaderRowHeight + CellPadding + 2)} Td ({Esc(text)}) Tj ET");
        }

        _currentY -= HeaderRowHeight;
    }

    public void AddDataRow(string[] values)
    {
        EnsureSpace(RowHeight);
        var y = _currentY;

        // Row border
        _currentPageOps.Add($"0.85 0.85 0.88 RG 0.3 w {F(Margin)} {F(y - RowHeight)} {F(_tableWidth)} {F(RowHeight)} re S");

        for (var i = 0; i < values.Length; i++)
        {
            var x = Margin + i * _colWidth;
            if (i > 0)
                _currentPageOps.Add($"0.9 0.9 0.92 RG 0.2 w {F(x)} {F(y)} m {F(x)} {F(y - RowHeight)} l S");

            var text = Truncate(values[i], _colWidth - 2 * CellPadding, FontSize);
            _currentPageOps.Add($"BT /F1 {FontSize} Tf 0.15 0.15 0.2 rg {F(x + CellPadding)} {F(y - RowHeight + CellPadding + 1)} Td ({Esc(text)}) Tj ET");
        }

        _currentY -= RowHeight;
    }

    private void EnsureSpace(float needed)
    {
        if (_currentY - needed < Margin)
        {
            // Flush current page
            _pageContents.Add(string.Join("\n", _currentPageOps));
            _currentPageOps.Clear();
            _currentY = _pageHeight - Margin;

            // Reprint header on new page
            if (_headerValues is not null)
                RenderHeaderRow(_headerValues);
        }
    }

    public byte[] Build()
    {
        // Flush last page
        if (_currentPageOps.Count > 0)
        {
            _pageContents.Add(string.Join("\n", _currentPageOps));
            _currentPageOps.Clear();
        }

        if (_pageContents.Count == 0)
            _pageContents.Add(""); // at least one blank page

        using var ms = new System.IO.MemoryStream();
        using var w = new System.IO.BinaryWriter(ms, System.Text.Encoding.ASCII, leaveOpen: true);

        var offsets = new List<long>();
        var objCount = 0;

        // Header
        WriteRaw(w, "%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");

        // Fixed objects: 1=Catalog, 2=Pages, 3=Font1, 4=Font2
        // Then for each page: stream obj + page obj (2 per page)
        // Page obj IDs: 5, 7, 9, ... for page 0, 1, 2, ...
        var pageObjIds = new List<int>();
        for (var i = 0; i < _pageContents.Count; i++)
            pageObjIds.Add(5 + i * 2);

        // 1: Catalog
        objCount++;
        offsets.Add(ms.Position);
        WriteRaw(w, $"{objCount} 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        // 2: Pages
        objCount++;
        offsets.Add(ms.Position);
        var kids = string.Join(" ", pageObjIds.Select(id => $"{id} 0 R"));
        WriteRaw(w, $"{objCount} 0 obj\n<< /Type /Pages /Kids [{kids}] /Count {_pageContents.Count} >>\nendobj\n");

        // 3: Helvetica
        objCount++;
        offsets.Add(ms.Position);
        WriteRaw(w, $"{objCount} 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n");

        // 4: Helvetica-Bold
        objCount++;
        offsets.Add(ms.Position);
        WriteRaw(w, $"{objCount} 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n");

        // Pages (stream + page object pairs)
        for (var i = 0; i < _pageContents.Count; i++)
        {
            var content = _pageContents[i];
            var contentBytes = System.Text.Encoding.ASCII.GetBytes(content);

            // Stream object
            objCount++;
            offsets.Add(ms.Position);
            var streamObjId = objCount;
            WriteRaw(w, $"{objCount} 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
            w.Write(contentBytes);
            WriteRaw(w, "\nendstream\nendobj\n");

            // Page object
            objCount++;
            offsets.Add(ms.Position);
            WriteRaw(w, $"{objCount} 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {F(_pageWidth)} {F(_pageHeight)}] /Contents {streamObjId} 0 R /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> >>\nendobj\n");
        }

        // Cross-reference table
        w.Flush();
        var xrefOffset = ms.Position;
        WriteRaw(w, $"xref\n0 {objCount + 1}\n");
        WriteRaw(w, "0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            WriteRaw(w, $"{offset:D10} 00000 n \n");
        }

        // Trailer
        WriteRaw(w, $"trailer\n<< /Size {objCount + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");

        w.Flush();
        return ms.ToArray();
    }

    private static void WriteRaw(System.IO.BinaryWriter w, string text)
    {
        w.Write(System.Text.Encoding.ASCII.GetBytes(text));
    }

    private static string F(float v) => v.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

    private static string Esc(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("\r", "")
            .Replace("\n", " ");
    }

    private static string Truncate(string text, float availableWidth, float fontSize)
    {
        var avgCharWidth = fontSize * 0.5f;
        var maxChars = (int)(availableWidth / avgCharWidth);
        if (text.Length <= maxChars) return text;
        return maxChars > 3 ? text[..(maxChars - 3)] + "..." : text[..Math.Max(maxChars, 1)];
    }
}

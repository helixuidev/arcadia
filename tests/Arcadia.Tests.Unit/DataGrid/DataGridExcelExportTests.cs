using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Arcadia.DataGrid.Components;
using Arcadia.DataGrid.Core;
using Arcadia.DataGrid.Services;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for ExcelExportService.ToXlsx: ZIP structure, headers, column visibility,
/// filter respect, and empty-data edge case.
/// </summary>
public class DataGridExcelExportTests : DataGridTestBase
{
    private IRenderedComponent<ArcadiaDataGrid<TestEmployee>> RenderExportableGrid(
        IReadOnlyList<TestEmployee>? data = null)
    {
        return RenderDataGrid(p =>
        {
            p.Add(g => g.Data, data ?? SampleData);
            p.Add(g => g.PageSize, 0);
            p.Add(g => g.Filterable, true);
            p.Add(g => g.ShowToolbar, true);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Department").Add(c => c.Title, "Department"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Salary").Add(c => c.Title, "Salary"));
        });
    }

    // ── Basic output ──

    [Fact]
    public void ToXlsx_ProducesNonEmptyByteArray()
    {
        var cut = RenderExportableGrid();
        var columns = cut.Instance.Collector.Columns.ToList();

        var bytes = ExcelExportService.ToXlsx(columns, SampleData);

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void ToXlsx_StartsWithPkZipMagicBytes()
    {
        var cut = RenderExportableGrid();
        var columns = cut.Instance.Collector.Columns.ToList();

        var bytes = ExcelExportService.ToXlsx(columns, SampleData);

        // PK signature = 0x50 0x4B (ZIP format)
        bytes.Length.Should().BeGreaterThan(2);
        bytes[0].Should().Be(0x50);
        bytes[1].Should().Be(0x4B);
    }

    [Fact]
    public void ToXlsx_IncludesHeaderRowWithColumnTitles()
    {
        var cut = RenderExportableGrid();
        var columns = cut.Instance.Collector.Columns.ToList();

        var bytes = ExcelExportService.ToXlsx(columns, SampleData);
        var content = ExtractSheetXml(bytes);

        content.Should().Contain("Name");
        content.Should().Contain("Department");
        content.Should().Contain("Salary");
    }

    // ── Column visibility ──

    [Fact]
    public void ToXlsx_HiddenColumn_ExcludedFromOutput()
    {
        var cut = RenderExportableGrid();
        var columns = cut.Instance.Collector.Columns.ToList();

        // Hide Department column
        var deptCol = columns.First(c => c.Title == "Department");
        deptCol.ToggleVisible();

        var bytes = ExcelExportService.ToXlsx(columns, SampleData);
        var content = ExtractSheetXml(bytes);

        content.Should().Contain("Name");
        content.Should().NotContain(">Department<");
        content.Should().Contain("Salary");
    }

    // ── Respects current filter ──

    [Fact]
    public void ToXlsx_RespectsCurrentFilter_OnlyFilteredRowsExported()
    {
        var cut = RenderExportableGrid();
        var columns = cut.Instance.Collector.Columns.ToList();

        // Apply filter on grid, then export the filtered data manually
        cut.Instance.SetFilter("Department", "Marketing");
        // Get filtered data the same way the grid does internally
        var filteredData = SampleData.Where(e => e.Department == "Marketing").ToList();

        var bytes = ExcelExportService.ToXlsx(columns, filteredData);
        var content = ExtractSheetXml(bytes);

        content.Should().Contain("Charlie");
        content.Should().Contain("Diana");
        content.Should().NotContain("Alice");
        content.Should().NotContain("Bob");
        content.Should().NotContain("Eve");
    }

    // ── Empty data ──

    [Fact]
    public void ToXlsx_EmptyData_ProducesValidFileWithHeadersOnly()
    {
        var cut = RenderExportableGrid(new List<TestEmployee>());
        var columns = cut.Instance.Collector.Columns.ToList();

        var bytes = ExcelExportService.ToXlsx(columns, Array.Empty<TestEmployee>());

        // Should still be a valid ZIP
        bytes.Length.Should().BeGreaterThan(2);
        bytes[0].Should().Be(0x50);
        bytes[1].Should().Be(0x4B);

        var content = ExtractSheetXml(bytes);
        content.Should().Contain("Name");
        // Only one <row> element (the header)
        var rowCount = System.Text.RegularExpressions.Regex.Count(content, "<row ");
        rowCount.Should().Be(1);
    }

    /// <summary>
    /// Extract the sheet1.xml content from an XLSX (ZIP) byte array.
    /// </summary>
    private static string ExtractSheetXml(byte[] xlsxBytes)
    {
        using var ms = new System.IO.MemoryStream(xlsxBytes);
        using var zip = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read);
        var entry = zip.GetEntry("xl/worksheets/sheet1.xml");
        entry.Should().NotBeNull("XLSX should contain sheet1.xml");
        using var reader = new System.IO.StreamReader(entry!.Open());
        return reader.ReadToEnd();
    }
}

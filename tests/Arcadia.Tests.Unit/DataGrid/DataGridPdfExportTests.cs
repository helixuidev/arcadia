using Bunit;
using FluentAssertions;
using Arcadia.DataGrid.Components;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for PDF export toolbar button localization.
/// </summary>
public class DataGridPdfExportTests : DataGridTestBase
{
    [Fact]
    public void TextPdf_CustomValue_RendersInToolbar()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.Add(g => g.ShowToolbar, true);
            p.Add(g => g.TextPdf, "Export PDF");
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        cut.Markup.Should().Contain("Export PDF");
        cut.Markup.Should().NotContain(">PDF<");
    }
}

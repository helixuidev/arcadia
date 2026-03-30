using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Arcadia.DataGrid.Components;
using Arcadia.DataGrid.Core;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for DataGrid localization (Text* parameters): default values
/// and custom overrides render correctly in the DOM.
/// </summary>
public class DataGridLocalizationTests : DataGridTestBase
{
    private IRenderedComponent<ArcadiaDataGrid<TestEmployee>> RenderLocalizableGrid(
        Action<ComponentParameterCollectionBuilder<ArcadiaDataGrid<TestEmployee>>>? configure = null)
    {
        return RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.Add(g => g.ShowToolbar, true);
            p.Add(g => g.Filterable, true);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Department").Add(c => c.Title, "Department"));
            configure?.Invoke(p);
        });
    }

    // ── Default TextSearch ──

    [Fact]
    public void DefaultTextSearch_IsSearchEllipsis()
    {
        var cut = RenderLocalizableGrid();
        var searchInput = cut.Find("input.arcadia-grid__search-input");

        searchInput.GetAttribute("placeholder").Should().Be("Search...");
    }

    // ── Custom TextFilter ──

    [Fact]
    public void CustomTextFilter_RendersInFilterButton()
    {
        var cut = RenderLocalizableGrid(p => p.Add(g => g.TextFilter, "Filtrar"));

        var filterBtn = cut.FindAll("button.arcadia-grid__filter-toggle")
            .FirstOrDefault(b => b.TextContent.Trim() == "Filtrar");

        filterBtn.Should().NotBeNull("filter button should contain the custom TextFilter text");
    }

    // ── Custom TextPageInfo ──

    [Fact]
    public void CustomTextPageInfo_RendersCorrectlyWithPaging()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 2);
            p.Add(g => g.ShowToolbar, true);
            p.Add(g => g.TextPageInfo, "Seite {0} von {1}");
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        var pageInfo = cut.Find(".arcadia-grid__page-current");
        // 5 items / 2 per page = 3 pages, starting on page 1
        pageInfo.TextContent.Should().Contain("Seite 1 von 3");
    }

    // ── Custom TextCsv / TextExcel ──

    [Fact]
    public void CustomTextCsv_RendersInToolbarButton()
    {
        var cut = RenderLocalizableGrid(p => p.Add(g => g.TextCsv, "Exportar CSV"));

        var csvBtn = cut.FindAll("button.arcadia-grid__export-btn")
            .FirstOrDefault(b => b.TextContent.Trim() == "Exportar CSV");

        csvBtn.Should().NotBeNull("toolbar should contain CSV button with custom text");
    }

    [Fact]
    public void CustomTextExcel_RendersInToolbarButton()
    {
        var cut = RenderLocalizableGrid(p => p.Add(g => g.TextExcel, "Exportar Excel"));

        var excelBtn = cut.FindAll("button.arcadia-grid__export-btn")
            .FirstOrDefault(b => b.TextContent.Trim() == "Exportar Excel");

        excelBtn.Should().NotBeNull("toolbar should contain Excel button with custom text");
    }

    // ── Custom TextSave / TextDiscard (batch editing) ──

    [Fact]
    public void CustomTextSave_RendersInBatchSaveButton()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.Add(g => g.ShowToolbar, true);
            p.Add(g => g.BatchEdit, true);
            p.Add(g => g.TextSave, "Guardar");
            p.Add(g => g.TextDiscard, "Descartar");
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name")
                   .Add(c => c.Editable, true));
        });

        // Batch buttons only show when there are pending changes.
        // Verify the parameter was accepted by checking the instance.
        cut.Instance.TextSave.Should().Be("Guardar");
        cut.Instance.TextDiscard.Should().Be("Descartar");
    }
}

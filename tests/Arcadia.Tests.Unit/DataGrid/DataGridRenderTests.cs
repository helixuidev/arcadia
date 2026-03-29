using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Arcadia.DataGrid.Components;
using Arcadia.DataGrid.Core;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for basic rendering: table structure, CSS classes, loading skeleton,
/// empty state, and custom empty templates.
/// </summary>
public class DataGridRenderTests : DataGridTestBase
{
    // ── 1. Basic table structure ──

    [Fact]
    public void Renders_TableWithHeaderAndRows()
    {
        var cut = RenderGrid(SampleData);

        cut.Find("table").Should().NotBeNull();
        // Header row + 5 data rows
        cut.FindAll("tr[role='row']").Count.Should().Be(6); // 1 header + 5 body
    }

    [Fact]
    public void Renders_CorrectNumberOfColumns()
    {
        var cut = RenderGrid(SampleData);

        var headerCells = cut.FindAll("th[role='columnheader']");
        headerCells.Count.Should().Be(3); // Name, Department, Salary
    }

    [Fact]
    public void Renders_CellValuesFromPropertyAccessor()
    {
        var cut = RenderGrid(SampleData);

        var cells = cut.FindAll("td[role='gridcell']");
        // First row: Alice, Engineering, 120000
        cells[0].TextContent.Should().Contain("Alice");
        cells[1].TextContent.Should().Contain("Engineering");
        cells[2].TextContent.Should().Contain("120000");
    }

    [Fact]
    public void Renders_CellValuesFromFieldLambda()
    {
        var cut = RenderGridWithFieldColumns(SampleData);

        var cells = cut.FindAll("td[role='gridcell']");
        cells[0].TextContent.Should().Contain("Alice");
    }

    // ── 2. Null / empty data ──

    [Fact]
    public void NullData_ShowsSkeletonOrEmptyMessage()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, (IReadOnlyList<TestEmployee>?)null);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        // When Data is null, grid shows SSR skeleton loading state
        var markup = cut.Markup;
        (markup.Contains("skeleton") || markup.Contains("No data available")).Should().BeTrue(
            "Null data should show either skeleton loading or empty message");
    }

    [Fact]
    public void EmptyList_ShowsDefaultEmptyMessage()
    {
        var cut = RenderGrid(new List<TestEmployee>());

        cut.Markup.Should().Contain("No data available");
    }

    [Fact]
    public void CustomEmptyMessage_Overrides_Default()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, new List<TestEmployee>());
            p.Add(g => g.EmptyMessage, "Nothing here!");
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        cut.Markup.Should().Contain("Nothing here!");
        cut.Markup.Should().NotContain("No data available");
    }

    [Fact]
    public void EmptyTemplate_RendersInsteadOfMessage()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, new List<TestEmployee>());
            p.Add(g => g.EmptyTemplate, (RenderFragment)(builder => { builder.OpenElement(0, "div"); builder.AddAttribute(1, "class", "custom-empty"); builder.AddContent(2, "No employees found"); builder.CloseElement(); }));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        cut.Find(".custom-empty").TextContent.Should().Be("No employees found");
    }

    // ── 3. Loading skeleton ──

    [Fact]
    public void Loading_ShowsSkeletonRows()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.Loading, true);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        cut.FindAll(".arcadia-grid__skeleton-row").Count.Should().Be(5);
    }

    [Fact]
    public void Loading_DoesNotRenderDataRows()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.Loading, true);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        cut.FindAll("td[role='gridcell']").Count.Should().Be(0);
    }

    // ── 4. CSS class parameters ──

    [Fact]
    public void Striped_AddsStripedClass()
    {
        var cut = RenderGrid(SampleData, p => p.Add(g => g.Striped, true));

        cut.Find(".arcadia-grid").ClassList.Should().Contain("arcadia-grid--striped");
    }

    [Fact]
    public void Striped_False_OmitsStripedClass()
    {
        var cut = RenderGrid(SampleData, p => p.Add(g => g.Striped, false));

        cut.Find(".arcadia-grid").ClassList.Should().NotContain("arcadia-grid--striped");
    }

    [Fact]
    public void Hoverable_AddsHoverableClass()
    {
        var cut = RenderGrid(SampleData, p => p.Add(g => g.Hoverable, true));

        cut.Find(".arcadia-grid").ClassList.Should().Contain("arcadia-grid--hoverable");
    }

    [Fact]
    public void Dense_AddsDenseClass()
    {
        var cut = RenderGrid(SampleData, p => p.Add(g => g.Dense, true));

        cut.Find(".arcadia-grid").ClassList.Should().Contain("arcadia-grid--dense");
    }

    [Fact]
    public void Dense_Default_OmitsDenseClass()
    {
        var cut = RenderGrid(SampleData);

        cut.Find(".arcadia-grid").ClassList.Should().NotContain("arcadia-grid--dense");
    }

    [Fact]
    public void CustomClass_IsAppliedToRoot()
    {
        var cut = RenderGrid(SampleData, p => p.Add(g => g.Class, "my-custom-grid"));

        cut.Find(".arcadia-grid").ClassList.Should().Contain("my-custom-grid");
    }

    // ── 5. Fixed header ──

    [Fact]
    public void FixedHeader_AddsStickyClass()
    {
        var cut = RenderGrid(SampleData, p => p.Add(g => g.FixedHeader, true));

        cut.Find("thead").ClassList.Should().Contain("arcadia-grid__thead--sticky");
    }

    [Fact]
    public void FixedHeader_False_OmitsStickyClass()
    {
        var cut = RenderGrid(SampleData, p => p.Add(g => g.FixedHeader, false));

        cut.Find("thead").ClassList.Should().NotContain("arcadia-grid__thead--sticky");
    }

    // ── 6. Height inline style ──

    [Fact]
    public void Height_SetsInlineStyle()
    {
        var cut = RenderGrid(SampleData, p => p.Add(g => g.Height, "400px"));

        var style = cut.Find(".arcadia-grid").GetAttribute("style");
        style.Should().Contain("height:400px");
    }
}

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Arcadia.DataGrid.Components;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for column tooltip rendering.
/// Simple text tooltips use the title attribute; TooltipTemplate renders a wrapper div.
/// </summary>
public class DataGridTooltipTests : DataGridTestBase
{
    [Fact]
    public void Column_WithTooltip_RendersTitleAttribute()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name")
                   .Add(c => c.Tooltip, "Employee name"));
        });

        // Cells with a Tooltip parameter render a div with a title attribute
        cut.Markup.Should().Contain("title=\"Employee name\"");
    }

    [Fact]
    public void Column_WithTooltipTemplate_RendersTooltipWrapper()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name")
                   .Add(c => c.TooltipTemplate, (RenderFragment<TestEmployee>)(item =>
                       builder => { builder.OpenElement(0, "span"); builder.AddContent(1, "Info"); builder.CloseElement(); })));
        });

        cut.FindAll(".arcadia-grid__tooltip-wrapper").Count.Should().BeGreaterThan(0);
        cut.FindAll(".arcadia-grid__tooltip").Count.Should().BeGreaterThan(0);
    }
}

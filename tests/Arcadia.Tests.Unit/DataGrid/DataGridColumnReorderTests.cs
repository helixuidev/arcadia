using Bunit;
using FluentAssertions;
using Arcadia.DataGrid.Components;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for column reorder (drag-and-drop) feature.
/// Verifies that the AllowColumnReorder parameter controls draggable attributes
/// and CSS classes on column headers.
/// </summary>
public class DataGridColumnReorderTests : DataGridTestBase
{
    [Fact]
    public void AllowColumnReorder_False_ThNotDraggable()
    {
        var cut = RenderGrid(SampleData, p => p.Add(g => g.AllowColumnReorder, false));

        var headers = cut.FindAll("th[role='columnheader']");
        headers.Should().NotBeEmpty();
        foreach (var th in headers)
        {
            th.GetAttribute("draggable").Should().Be("false");
        }
    }

    [Fact]
    public void AllowColumnReorder_True_ThHasDraggable()
    {
        var cut = RenderGrid(SampleData, p => p.Add(g => g.AllowColumnReorder, true));

        var headers = cut.FindAll("th[role='columnheader']");
        headers.Should().NotBeEmpty();
        foreach (var th in headers)
        {
            th.GetAttribute("draggable").Should().Be("true");
        }
    }

    [Fact]
    public void AllowColumnReorder_True_ThHasReorderableClass()
    {
        var cut = RenderGrid(SampleData, p => p.Add(g => g.AllowColumnReorder, true));

        var headers = cut.FindAll("th[role='columnheader']");
        headers.Should().NotBeEmpty();
        foreach (var th in headers)
        {
            th.ClassList.Should().Contain("arcadia-grid__th--reorderable");
        }
    }
}

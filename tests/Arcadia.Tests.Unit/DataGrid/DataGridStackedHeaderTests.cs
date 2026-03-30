using Bunit;
using FluentAssertions;
using Arcadia.DataGrid.Components;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for stacked (grouped) column headers.
/// Columns with the same Group value share a spanning header row above them.
/// </summary>
public class DataGridStackedHeaderTests : DataGridTestBase
{
    [Fact]
    public void GroupProperty_Null_NoGroupHeaderRow()
    {
        var cut = RenderGrid(SampleData);

        // No group header row should be rendered when no columns have Group set
        cut.FindAll("tr.arcadia-grid__header-group").Count.Should().Be(0);
    }

    [Fact]
    public void GroupProperty_Set_RendersGroupHeaderRow()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name")
                   .Add(c => c.Group, "Personal"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Department").Add(c => c.Title, "Department")
                   .Add(c => c.Group, "Work"));
        });

        var groupRow = cut.FindAll("tr.arcadia-grid__header-group");
        groupRow.Count.Should().Be(1);
    }

    [Fact]
    public void GroupProperty_SameGroup_SpansColumns()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name")
                   .Add(c => c.Group, "Personal"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Department").Add(c => c.Title, "Department")
                   .Add(c => c.Group, "Personal"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Salary").Add(c => c.Title, "Salary")
                   .Add(c => c.Group, "Finance"));
        });

        var groupHeaders = cut.FindAll("th.arcadia-grid__th--group");
        // Two group headers: "Personal" spanning 2 and "Finance" spanning 1
        groupHeaders.Count.Should().Be(2);
        groupHeaders[0].GetAttribute("colspan").Should().Be("2");
        groupHeaders[0].TextContent.Should().Contain("Personal");
        groupHeaders[1].GetAttribute("colspan").Should().Be("1");
        groupHeaders[1].TextContent.Should().Contain("Finance");
    }

    [Fact]
    public void GroupProperty_MixedGroups_CorrectSpans()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name")
                   .Add(c => c.Group, "A"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Department").Add(c => c.Title, "Department")
                   .Add(c => c.Group, "B"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Salary").Add(c => c.Title, "Salary")
                   .Add(c => c.Group, "A"));
        });

        var groupHeaders = cut.FindAll("th.arcadia-grid__th--group");
        // A(1), B(1), A(1) — non-adjacent same groups are NOT merged
        groupHeaders.Count.Should().Be(3);
        foreach (var gh in groupHeaders)
        {
            gh.GetAttribute("colspan").Should().Be("1");
        }
    }
}

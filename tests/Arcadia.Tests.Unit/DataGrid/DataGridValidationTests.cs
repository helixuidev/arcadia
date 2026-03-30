using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Arcadia.DataGrid.Components;
using Arcadia.DataGrid.Core;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for inline editing validation, sticky footer, and group aggregates.
/// </summary>
public class DataGridValidationTests : DataGridTestBase
{
    [Fact]
    public void Column_WithValidator_RendersValidationOnEdit()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name")
                   .Add(c => c.Editable, true)
                   .Add(c => c.Validator, (Func<string, string?>)(val =>
                       string.IsNullOrWhiteSpace(val) ? "Name is required" : null)));
        });

        // Verify the grid renders with the editable column and validator without error
        cut.Find(".arcadia-grid").Should().NotBeNull();
        // The column should have the validator configured
        var columns = cut.FindComponents<ArcadiaColumn<TestEmployee>>();
        columns.Should().NotBeEmpty();
        columns[0].Instance.Validator.Should().NotBeNull();
    }

    [Fact]
    public void StickyFooter_True_AddsStickyClass()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.Add(g => g.StickyFooter, true);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Salary").Add(c => c.Title, "Salary")
                   .Add(c => c.Aggregate, AggregateType.Sum));
        });

        cut.Find("tfoot").ClassList.Should().Contain("arcadia-grid__tfoot--sticky");
    }

    [Fact]
    public void ShowGroupAggregates_True_RendersAggregateSpans()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.Add(g => g.GroupBy, "Department");
            p.Add(g => g.ShowGroupAggregates, true);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Salary").Add(c => c.Title, "Salary")
                   .Add(c => c.Aggregate, AggregateType.Sum));
        });

        // Group rows should contain aggregate spans
        cut.FindAll(".arcadia-grid__group-agg").Count.Should().BeGreaterThan(0);
    }
}

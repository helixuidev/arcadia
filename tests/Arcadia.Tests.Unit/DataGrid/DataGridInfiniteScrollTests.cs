using Bunit;
using FluentAssertions;
using Arcadia.DataGrid.Components;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for infinite scroll feature.
/// When InfiniteScroll is true, pagination is hidden and a scroll sentinel is rendered.
/// </summary>
public class DataGridInfiniteScrollTests : DataGridTestBase
{
    [Fact]
    public void InfiniteScroll_True_HidesPagination()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, LargeData);
            p.Add(g => g.PageSize, 10);
            p.Add(g => g.InfiniteScroll, true);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        // Pagination controls should not be present
        cut.FindAll(".arcadia-grid__pagination").Count.Should().Be(0);
    }

    [Fact]
    public void InfiniteScroll_True_RendersSentinel()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.PageSize, 0);
            p.Add(g => g.InfiniteScroll, true);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        cut.FindAll(".arcadia-grid__infinite-sentinel").Count.Should().Be(1);
    }

    [Fact]
    public void InfiniteScroll_False_ShowsPagination()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, LargeData);
            p.Add(g => g.PageSize, 10);
            p.Add(g => g.InfiniteScroll, false);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        cut.FindAll(".arcadia-grid__pagination").Count.Should().BeGreaterThan(0);
    }
}

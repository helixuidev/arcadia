using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Arcadia.DataGrid.Components;
using Arcadia.DataGrid.Core;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for WAI-ARIA grid pattern compliance: role attributes, aria-sort,
/// aria-label, aria-busy, live region, keyboard navigation, and focus management.
/// </summary>
public class DataGridAccessibilityTests : DataGridTestBase
{
    /// <summary>
    /// Helper: render a grid and focus it so _gridHasFocus is true for keyboard tests.
    /// </summary>
    private IRenderedComponent<ArcadiaDataGrid<TestEmployee>> RenderAndFocusGrid()
    {
        var cut = RenderGrid(SampleData);
        // Trigger the @onfocus handler on the grid root element
        cut.Find("[role='grid']").Focus();
        return cut;
    }

    // ── role="grid" ──

    [Fact]
    public void RootElement_HasRoleGrid()
    {
        var cut = RenderGrid(SampleData);

        cut.Find("[role='grid']").Should().NotBeNull();
    }

    [Fact]
    public void RootElement_HasTabindex()
    {
        var cut = RenderGrid(SampleData);

        cut.Find("[role='grid']").GetAttribute("tabindex").Should().Be("0");
    }

    // ── role="row" and role="gridcell" ──

    [Fact]
    public void HeaderRow_HasRoleRow()
    {
        var cut = RenderGrid(SampleData);

        var headerRow = cut.Find("thead tr");
        headerRow.GetAttribute("role").Should().Be("row");
    }

    [Fact]
    public void DataRows_HaveRoleRow()
    {
        var cut = RenderGrid(SampleData);

        // Data rows are in tbody
        var bodyRows = cut.FindAll("tbody tr[role='row']");
        bodyRows.Count.Should().Be(5);
    }

    [Fact]
    public void DataCells_HaveRoleGridcell()
    {
        var cut = RenderGrid(SampleData);

        var cells = cut.FindAll("td[role='gridcell']");
        cells.Count.Should().Be(15); // 5 rows * 3 columns
    }

    // ── role="columnheader" with aria-sort ──

    [Fact]
    public void ColumnHeaders_HaveRoleColumnheader()
    {
        var cut = RenderGrid(SampleData);

        var headers = cut.FindAll("th[role='columnheader']");
        headers.Count.Should().Be(3);
    }

    [Fact]
    public void AriaSortNone_WhenNoSortApplied()
    {
        var cut = RenderGrid(SampleData);

        var headers = cut.FindAll("th[role='columnheader']");
        foreach (var h in headers)
        {
            h.GetAttribute("aria-sort").Should().Be("none");
        }
    }

    [Fact]
    public void AriaSortAscending_WhenSortedAsc()
    {
        var cut = RenderGrid(SampleData);

        cut.FindAll("th[role='columnheader']")[0].Click();

        cut.FindAll("th[role='columnheader']")[0]
            .GetAttribute("aria-sort").Should().Be("ascending");
    }

    [Fact]
    public void AriaSortDescending_WhenSortedDesc()
    {
        var cut = RenderGrid(SampleData);

        cut.FindAll("th[role='columnheader']")[0].Click(); // ascending
        cut.FindAll("th[role='columnheader']")[0].Click(); // descending

        cut.FindAll("th[role='columnheader']")[0]
            .GetAttribute("aria-sort").Should().Be("descending");
    }

    // ── aria-label ──

    [Fact]
    public void SortableHeader_HasAriaLabel()
    {
        var cut = RenderGrid(SampleData);

        var header = cut.FindAll("th[role='columnheader']")[0];
        header.GetAttribute("aria-label").Should().Contain("Name");
    }

    // ── aria-busy ──

    [Fact]
    public void Loading_AriaBusyIsTrue()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, SampleData);
            p.Add(g => g.Loading, true);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        cut.Find("[role='grid']").GetAttribute("aria-busy").Should().Be("true");
    }

    [Fact]
    public void NotLoading_AriaBusyIsFalse()
    {
        var cut = RenderGrid(SampleData);

        cut.Find("[role='grid']").GetAttribute("aria-busy").Should().Be("false");
    }

    // ── ARIA live region ──

    [Fact]
    public void LiveRegion_Exists()
    {
        var cut = RenderGrid(SampleData);

        var liveRegion = cut.Find("[role='status'][aria-live='polite']");
        liveRegion.Should().NotBeNull();
    }

    [Fact]
    public void LiveRegion_HasAriaAtomic()
    {
        var cut = RenderGrid(SampleData);

        var liveRegion = cut.Find("[role='status']");
        liveRegion.GetAttribute("aria-atomic").Should().Be("true");
    }

    [Fact]
    public void LiveRegion_HasScreenReaderOnlyClass()
    {
        var cut = RenderGrid(SampleData);

        var liveRegion = cut.Find("[role='status']");
        liveRegion.ClassList.Should().Contain("arcadia-sr-only");
    }

    // ── Keyboard navigation ──
    // After focusing the grid, keyboard events move the internal focus row/col.
    // IsFocusedCell requires _gridHasFocus=true, which is set by the @onfocus handler.

    [Fact]
    public void ArrowDown_MovesFocusRow()
    {
        var cut = RenderAndFocusGrid();
        var grid = cut.Instance;

        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        grid.IsFocusedCell(1, 0).Should().BeTrue();
    }

    [Fact]
    public void ArrowUp_MovesFocusRowUp()
    {
        var cut = RenderAndFocusGrid();
        var grid = cut.Instance;

        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowUp" });

        grid.IsFocusedCell(1, 0).Should().BeTrue();
    }

    [Fact]
    public void ArrowUp_AtTop_StaysAtZero()
    {
        var cut = RenderAndFocusGrid();
        var grid = cut.Instance;

        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowUp" });

        grid.IsFocusedCell(0, 0).Should().BeTrue();
    }

    [Fact]
    public void ArrowRight_MovesFocusColumn()
    {
        var cut = RenderAndFocusGrid();
        var grid = cut.Instance;

        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        grid.IsFocusedCell(0, 1).Should().BeTrue();
    }

    [Fact]
    public void ArrowLeft_MovesFocusColumnLeft()
    {
        var cut = RenderAndFocusGrid();
        var grid = cut.Instance;

        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        grid.IsFocusedCell(0, 1).Should().BeTrue();
    }

    [Fact]
    public void Home_MovesFocusToFirstColumn()
    {
        var cut = RenderAndFocusGrid();
        var grid = cut.Instance;

        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "Home" });

        grid.IsFocusedCell(0, 0).Should().BeTrue();
    }

    [Fact]
    public void End_MovesFocusToLastColumn()
    {
        var cut = RenderAndFocusGrid();
        var grid = cut.Instance;

        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "End" });

        grid.IsFocusedCell(0, 2).Should().BeTrue(); // 3 cols, last index = 2
    }

    [Fact]
    public void CtrlHome_MovesFocusToTopLeft()
    {
        var cut = RenderAndFocusGrid();
        var grid = cut.Instance;

        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "Home", CtrlKey = true });

        grid.IsFocusedCell(0, 0).Should().BeTrue();
    }

    [Fact]
    public void CtrlEnd_MovesFocusToBottomRight()
    {
        var cut = RenderAndFocusGrid();
        var grid = cut.Instance;

        grid.HandleGridKeyDown(new KeyboardEventArgs { Key = "End", CtrlKey = true });

        grid.IsFocusedCell(4, 2).Should().BeTrue(); // 5 rows, 3 cols
    }

    // ── Cell ID pattern ──

    [Fact]
    public void GetCellId_FollowsPattern()
    {
        var cut = RenderGrid(SampleData);

        cut.Instance.GetCellId(2, 1).Should().Be("cell-2-1");
    }

    // ── Active descendant ──

    [Fact]
    public void GetActiveDescendant_NullWhenNotFocused()
    {
        var cut = RenderGrid(SampleData);

        cut.Instance.GetActiveDescendant().Should().BeNull();
    }

    [Fact]
    public void GetActiveDescendant_ReturnsCellIdWhenFocused()
    {
        var cut = RenderAndFocusGrid();

        cut.Instance.GetActiveDescendant().Should().Be("cell-0-0");
    }

    // ── Pagination aria ──

    [Fact]
    public void PaginationNav_HasAriaLabel()
    {
        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, LargeData);
            p.Add(g => g.PageSize, 10);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        cut.Find(".arcadia-grid__pagination").GetAttribute("aria-label")
            .Should().Be("Grid pagination");
    }

    // ── Focus Cell Tests ──

    [Fact]
    public void SetFocusCell_UpdatesFocusPosition()
    {
        var cut = RenderGrid(SampleData);
        var grid = cut.Instance;

        // Initially focus is at 0,0 but grid has no focus
        grid.IsFocusedCell(0, 0).Should().BeFalse("grid has no focus initially");

        // Set focus to row 2, col 1
        grid.SetFocusCell(2, 1);
        grid.IsFocusedCell(2, 1).Should().BeTrue("SetFocusCell should move focus");
        grid.IsFocusedCell(0, 0).Should().BeFalse("old cell should not be focused");
    }

    [Fact]
    public void ClickCell_MovesFocusToClickedCell()
    {
        var cut = RenderGrid(SampleData);

        var cells = cut.FindAll("td[role='gridcell']");
        cells.Count.Should().BeGreaterThan(3);

        // Click cell at index 3 (second row, second column in a 3-column grid)
        cells[3].Click();

        var grid = cut.Instance;
        // Row 1, Col 0 in a 3-col grid (index 3 = row 1 * 3 cols + col 0)
        grid.IsFocusedCell(1, 0).Should().BeTrue("clicking a cell should focus it");
        grid.IsFocusedCell(0, 0).Should().BeFalse("first cell should not be focused after clicking another");
    }
}

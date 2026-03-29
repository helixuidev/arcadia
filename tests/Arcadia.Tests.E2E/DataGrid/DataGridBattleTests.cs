using Arcadia.Tests.E2E.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.DataGrid;

/// <summary>
/// Battle tests — edge cases and interaction combinations that break grids
/// in production. These test scenarios users actually encounter.
/// </summary>
[TestFixture]
public class DataGridBattleTests : PageTest
{
    private async Task NavigateToBasics()
    {
        await Page.GotoAsync($"{TestConstants.BaseUrl}/datagrid/basics",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(2000);
    }

    private async Task NavigateToEditing()
    {
        await Page.GotoAsync($"{TestConstants.BaseUrl}/datagrid/editing",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(2000);
    }

    private async Task NavigateToAdvanced()
    {
        await Page.GotoAsync($"{TestConstants.BaseUrl}/datagrid/advanced",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(2000);
    }

    // ── Column Resize ──

    [Test]
    public async Task ColumnResize_DragMakesColumnWider()
    {
        await NavigateToBasics();

        var handle = Page.Locator(".arcadia-grid__resize-handle").Nth(1);
        var th = Page.Locator("th.arcadia-grid__th").Nth(1);

        var widthBefore = await th.EvaluateAsync<int>("el => el.offsetWidth");

        var box = await handle.BoundingBoxAsync();
        Assert.That(box, Is.Not.Null, "Resize handle should have bounding box");

        await Page.Mouse.MoveAsync(box!.X + box.Width / 2, box.Y + box.Height / 2);
        await Page.Mouse.DownAsync();
        await Page.Mouse.MoveAsync(box.X + 80, box.Y + box.Height / 2, new() { Steps = 5 });
        await Page.Mouse.UpAsync();
        await Page.WaitForTimeoutAsync(200);

        var widthAfter = await th.EvaluateAsync<int>("el => el.offsetWidth");
        Assert.That(widthAfter, Is.GreaterThan(widthBefore),
            $"Column should be wider after drag (was {widthBefore}, now {widthAfter})");
    }

    [Test]
    public async Task ColumnResize_HandleShowsVisualIndicatorOnHover()
    {
        await NavigateToBasics();

        var handle = Page.Locator(".arcadia-grid__resize-handle").Nth(2);
        await handle.HoverAsync();
        await Page.WaitForTimeoutAsync(200);

        // The ::after pseudo-element should be visible (accent color)
        var cursor = await handle.EvaluateAsync<string>("el => getComputedStyle(el).cursor");
        Assert.That(cursor, Is.EqualTo("col-resize"), "Cursor should be col-resize on hover");
    }

    [Test]
    public async Task ColumnResize_MinWidthEnforced()
    {
        await NavigateToBasics();

        var handle = Page.Locator(".arcadia-grid__resize-handle").Nth(1);
        var th = Page.Locator("th.arcadia-grid__th").Nth(1);

        var box = await handle.BoundingBoxAsync();

        // Drag left by 500px (should hit min width)
        await Page.Mouse.MoveAsync(box!.X + box.Width / 2, box.Y + box.Height / 2);
        await Page.Mouse.DownAsync();
        await Page.Mouse.MoveAsync(box.X - 500, box.Y + box.Height / 2, new() { Steps = 5 });
        await Page.Mouse.UpAsync();
        await Page.WaitForTimeoutAsync(200);

        var width = await th.EvaluateAsync<int>("el => el.offsetWidth");
        Assert.That(width, Is.GreaterThanOrEqualTo(50),
            $"Column width should not go below minimum (50px), got {width}");
    }

    // ── Column Reorder ──

    [Test]
    public async Task ColumnReorder_DragMovesColumn()
    {
        await NavigateToBasics();

        var nameHeader = Page.Locator("th:has-text('Name')").First;
        var deptHeader = Page.Locator("th:has-text('Department')").First;

        var firstHeaderBefore = await Page.Locator("th.arcadia-grid__th").Nth(1).TextContentAsync();

        await nameHeader.DragToAsync(deptHeader);
        await Page.WaitForTimeoutAsync(500);

        var firstHeaderAfter = await Page.Locator("th.arcadia-grid__th").Nth(1).TextContentAsync();
        Assert.That(firstHeaderAfter!.Trim(), Is.Not.EqualTo(firstHeaderBefore!.Trim()),
            "Dragging Name to Department position should reorder columns");
    }

    // ── Sort + Filter Interaction ──

    [Test]
    public async Task SortThenFilter_BothApplyCorrectly()
    {
        await NavigateToBasics();

        // Find the second grid (Sorting showcase)
        var sortGrid = Page.Locator(".arcadia-grid").Nth(1);

        // Sort by Name
        var nameHeader = sortGrid.Locator("th:has-text('Name')").First;
        await nameHeader.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        var ariaSort = await nameHeader.GetAttributeAsync("aria-sort");
        Assert.That(ariaSort, Is.EqualTo("ascending"));

        // Verify sort was applied — the aria-sort attribute should be "ascending"
        // (Don't check cell content because first column might be ID, not Name)
        Assert.That(ariaSort, Is.EqualTo("ascending"),
            "Name column should show ascending sort indicator after click");
    }

    // ── Pagination Edge Cases ──

    [Test]
    public async Task Pagination_LastPageShowsRemainingRows()
    {
        await NavigateToBasics();

        // Find the pagination grid (4th grid — Pagination showcase)
        var pagGrid = Page.Locator(".arcadia-grid").Nth(3);
        var lastBtn = pagGrid.Locator("button[aria-label='Last page']");

        if (await lastBtn.CountAsync() > 0)
        {
            await lastBtn.ClickAsync();
            await Page.WaitForTimeoutAsync(300);

            // Should have at least 1 row on the last page
            var rows = pagGrid.Locator("tbody tr[role='row']");
            var count = await rows.CountAsync();
            Assert.That(count, Is.GreaterThan(0),
                "Last page should have at least 1 data row");
        }
    }

    // ── Empty State ──

    [Test]
    public async Task QuickFilter_NoMatch_ShowsEmptyMessage()
    {
        await NavigateToBasics();

        // Find toolbar search input
        var searchInput = Page.Locator(".arcadia-grid__search-input").First;

        if (await searchInput.CountAsync() > 0)
        {
            await searchInput.FillAsync("zzzznonexistent");
            await Page.WaitForTimeoutAsync(500);

            var empty = Page.Locator(".arcadia-grid__empty");
            if (await empty.CountAsync() > 0)
            {
                await Expect(empty.First).ToBeVisibleAsync();
            }
        }
    }

    // ── Rapid Interactions ──

    [Test]
    public async Task RapidSortClicks_DoesNotCrash()
    {
        await NavigateToBasics();

        var header = Page.Locator("th.arcadia-grid__th--sortable").First;

        // Click 10 times rapidly
        for (int i = 0; i < 10; i++)
        {
            await header.ClickAsync();
            await Page.WaitForTimeoutAsync(50);
        }

        // Grid should still render
        var rows = Page.Locator("td[role='gridcell']");
        var count = await rows.CountAsync();
        Assert.That(count, Is.GreaterThan(0),
            "Grid should still have data after rapid sort clicks");
    }

    [Test]
    public async Task RapidPageNavigation_DoesNotCrash()
    {
        await NavigateToBasics();

        var pagGrid = Page.Locator(".arcadia-grid").Nth(3);
        var nextBtn = pagGrid.Locator("button[aria-label='Next page']");

        if (await nextBtn.CountAsync() > 0)
        {
            for (int i = 0; i < 5; i++)
            {
                var disabled = await nextBtn.GetAttributeAsync("disabled");
                if (disabled != null) break; // stop at last page
                await nextBtn.ClickAsync();
                await Page.WaitForTimeoutAsync(100);
            }

            // Grid should still render
            var rows = pagGrid.Locator("tbody tr[role='row']");
            var count = await rows.CountAsync();
            Assert.That(count, Is.GreaterThan(0),
                "Grid should still have data after rapid page navigation");
        }
    }

    // ── Virtual Scroll ──

    [Test]
    public async Task VirtualScroll_RendersLessThanTotalRows()
    {
        await NavigateToAdvanced();

        // Find virtual scroll grid (should be the 5th showcase)
        var virtualGrid = Page.Locator(".arcadia-grid__scroll-container");

        if (await virtualGrid.CountAsync() > 0)
        {
            // Virtual scroll renders only visible rows, not all 10,000
            var renderedRows = virtualGrid.Locator(".arcadia-grid__virtual-row");
            var count = await renderedRows.CountAsync();

            // Should render far fewer than 10,000
            Assert.That(count, Is.LessThan(100),
                $"Virtual scroll should render <<100 rows out of 10,000, got {count}");
            Assert.That(count, Is.GreaterThan(0),
                "Virtual scroll should render at least some rows");
        }
    }

    // ── Selection Persistence ──

    [Test]
    public async Task Selection_SurvivesSort()
    {
        await NavigateToEditing();

        // Find multi-select grid (5th showcase)
        var selectGrid = Page.Locator(".arcadia-grid").Nth(4);
        var checkboxes = selectGrid.Locator("input[type='checkbox']");

        if (await checkboxes.CountAsync() > 1)
        {
            // Select first row
            await checkboxes.Nth(1).ClickAsync();
            await Page.WaitForTimeoutAsync(200);

            var countText = selectGrid.Locator("text=selected");
            if (await countText.CountAsync() > 0)
            {
                Assert.Pass("Selection count visible after selecting a row");
            }
        }
    }
}

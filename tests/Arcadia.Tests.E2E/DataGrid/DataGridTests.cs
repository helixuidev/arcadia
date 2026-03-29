using Arcadia.Tests.E2E.Infrastructure;

namespace Arcadia.Tests.E2E.DataGrid;

[TestFixture]
public class DataGridTests : ChartTestBase
{
    private ILocator _mainGrid = null!;

    private async Task NavigateToGrid()
    {
        await Page.GotoAsync($"{TestConstants.BaseUrl}/test/datagrid",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        // Wait for Blazor interactive render (columns collect on first render)
        await Page.WaitForSelectorAsync(".arcadia-grid__table",
            new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await Page.WaitForTimeoutAsync(1000);
        // Scope all queries to the first grid (the one with a table, not the virtual one)
        _mainGrid = Page.Locator(".arcadia-grid:has(.arcadia-grid__table)").First;
    }

    [Test]
    public async Task Grid_RendersTableWithRows()
    {
        await NavigateToGrid();
        var rows = Page.Locator("tbody tr[role='row']");
        var count = await rows.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1), "Grid should render at least 1 data row");
    }

    [Test]
    public async Task Grid_HasSortableHeaders()
    {
        await NavigateToGrid();
        var sortableHeaders = Page.Locator("th.arcadia-grid__th--sortable");
        var count = await sortableHeaders.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(5), "Grid should have sortable column headers");
    }

    [Test]
    public async Task Grid_SortChangesOnClick()
    {
        await NavigateToGrid();
        var nameHeader = Page.Locator("th.arcadia-grid__th--sortable:has-text('Name')");
        await nameHeader.ClickAsync();
        await Page.WaitForTimeoutAsync(300);
        var ariaSort = await nameHeader.GetAttributeAsync("aria-sort");
        Assert.That(ariaSort, Is.EqualTo("ascending"), "First click should sort ascending");

        await nameHeader.ClickAsync();
        await Page.WaitForTimeoutAsync(300);
        ariaSort = await nameHeader.GetAttributeAsync("aria-sort");
        Assert.That(ariaSort, Is.EqualTo("descending"), "Second click should sort descending");
    }

    [Test]
    public async Task Grid_PaginationWorks()
    {
        await NavigateToGrid();
        var pageInfo = _mainGrid.Locator(".arcadia-grid__page-info");
        var text = await pageInfo.InnerTextAsync();
        Assert.That(text, Does.Contain("of 75"), "Should show total count");

        // Click next page
        var nextBtn = _mainGrid.Locator(".arcadia-grid__page-btn[aria-label='Next page']");
        await nextBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(300);
        var newText = await pageInfo.InnerTextAsync();
        Assert.That(newText, Does.Contain("11"), "Should show page 2 starting at 11");
    }

    [Test]
    public async Task Grid_MasterDetail_ExpandsOnClick()
    {
        await NavigateToGrid();
        // Find expand arrows
        var expandBtns = Page.Locator(".arcadia-grid__td--expand");
        var count = await expandBtns.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1), "Should have expand buttons for detail rows");

        // Click the first expand arrow
        await expandBtns.First.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Detail row should appear
        var detailRow = Page.Locator(".arcadia-grid__detail-row");
        await Expect(detailRow.First).ToBeVisibleAsync();

        // Detail should contain employee email
        var detailText = await detailRow.First.InnerTextAsync();
        Assert.That(detailText, Does.Contain("@arcadia.com"), "Detail row should show employee email");
    }

    [Test]
    public async Task Grid_MasterDetail_CollapsesOnSecondClick()
    {
        await NavigateToGrid();
        var expandBtn = Page.Locator(".arcadia-grid__td--expand").First;

        // Expand
        await expandBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(300);
        var detailRows = Page.Locator(".arcadia-grid__detail-row");
        Assert.That(await detailRows.CountAsync(), Is.EqualTo(1));

        // Collapse
        await expandBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(300);
        Assert.That(await detailRows.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task Grid_FilterShowsResults()
    {
        await NavigateToGrid();
        // Click Filter button
        var filterBtn = Page.Locator("button:has-text('Filter')");
        await filterBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Type in the Name filter
        var filterInputs = Page.Locator(".arcadia-grid__filter-input");
        var nameFilter = filterInputs.Nth(1); // second input (after ID)
        await nameFilter.FillAsync("Alice");
        await Page.WaitForTimeoutAsync(500);

        // Should have fewer rows
        var rows = Page.Locator("tbody tr[role='row']");
        var count = await rows.CountAsync();
        Assert.That(count, Is.LessThan(10), "Filtering by 'Alice' should reduce row count");
    }

    [Test]
    public async Task Grid_MultiSelect_CheckboxWorks()
    {
        await NavigateToGrid();
        var checkboxes = Page.Locator(".arcadia-grid__td--checkbox input[type='checkbox']");
        var count = await checkboxes.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1), "Should have row checkboxes");

        // Click first checkbox
        await checkboxes.First.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Selection count should appear
        var selCount = Page.Locator("text=selected");
        await Expect(selCount.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Grid_ExportCsvButton_Exists()
    {
        await NavigateToGrid();
        var exportBtn = _mainGrid.Locator("button:has-text('CSV')");
        await Expect(exportBtn).ToBeVisibleAsync();
    }

    [Test]
    public async Task Grid_FooterAggregate_Shows()
    {
        await NavigateToGrid();
        var footer = Page.Locator("tfoot .arcadia-grid__td--footer");
        var count = await footer.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1), "Should have footer aggregate cells");
    }
}

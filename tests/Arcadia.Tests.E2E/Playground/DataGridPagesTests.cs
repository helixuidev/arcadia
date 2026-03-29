using Arcadia.Tests.E2E.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.Playground;

[TestFixture]
public class DataGridPagesTests : PageTest
{
    private async Task NavigateTo(string route)
    {
        await Page.GotoAsync(TestConstants.BaseUrl + route,
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(1500);
    }

    // ── Basics page ──

    [Test]
    public async Task Basics_RendersGridWithDataRows()
    {
        await NavigateTo("/datagrid/basics");

        var grids = Page.Locator(".arcadia-grid");
        var gridCount = await grids.CountAsync();
        Assert.That(gridCount, Is.GreaterThanOrEqualTo(1), "Basics page should have at least 1 grid");

        var rows = Page.Locator("tbody tr[role='row']");
        var rowCount = await rows.CountAsync();
        Assert.That(rowCount, Is.GreaterThanOrEqualTo(1), "Grid should have at least 1 data row visible");
    }

    [Test]
    public async Task Basics_ShowsSortingGrid()
    {
        await NavigateTo("/datagrid/basics");

        var sortHeader = Page.Locator(".gallery__showcase-title:has-text('Sorting')");
        await Expect(sortHeader.First).ToBeVisibleAsync();

        // The sorting grid should have sortable headers
        var sortableHeaders = Page.Locator("th.arcadia-grid__th--sortable");
        var count = await sortableHeaders.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1), "Sorting section should have sortable column headers");
    }

    // ── Editing page ──

    [Test]
    public async Task Editing_GridWithEditableCellsRenders()
    {
        await NavigateTo("/datagrid/editing");

        var header = Page.Locator("h1:has-text('DataGrid')");
        await Expect(header.First).ToBeVisibleAsync();

        // Check for inline edit hint text
        var editHint = Page.Locator("text=Double-click a cell to edit");
        await Expect(editHint.First).ToBeVisibleAsync();

        // Should have grid rows
        var rows = Page.Locator("tbody tr[role='row']");
        var rowCount = await rows.CountAsync();
        Assert.That(rowCount, Is.GreaterThanOrEqualTo(1), "Editing page grid should have data rows");
    }

    // ── Advanced page ──

    [Test]
    public async Task Advanced_GroupingGridWithGroupHeaders()
    {
        await NavigateTo("/datagrid/advanced");

        var header = Page.Locator("h1:has-text('DataGrid')");
        await Expect(header.First).ToBeVisibleAsync();

        var groupingTitle = Page.Locator(".gallery__showcase-title:has-text('Grouping')");
        await Expect(groupingTitle.First).ToBeVisibleAsync();

        // Group header rows should be present
        var groupRows = Page.Locator(".arcadia-grid__group-row");
        var count = await groupRows.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1),
            "Grouping grid should render at least 1 group header row");
    }

    // ── Enterprise page ──

    [Test]
    public async Task Enterprise_ToolbarWithExportButtons()
    {
        await NavigateTo("/datagrid/enterprise");

        var header = Page.Locator("h1:has-text('DataGrid')");
        await Expect(header.First).ToBeVisibleAsync();

        var csvButton = Page.Locator("button:has-text('CSV')");
        await Expect(csvButton.First).ToBeVisibleAsync();

        var excelButton = Page.Locator("button:has-text('Excel')");
        await Expect(excelButton.First).ToBeVisibleAsync();
    }

    // ── Shared sidebar presence ──

    [TestCase("/datagrid/basics")]
    [TestCase("/datagrid/editing")]
    [TestCase("/datagrid/advanced")]
    [TestCase("/datagrid/enterprise")]
    public async Task AllDataGridPages_HaveSidebarNavigation(string route)
    {
        await NavigateTo(route);

        var sidebar = Page.Locator(".gallery__sidebar");
        await Expect(sidebar.First).ToBeVisibleAsync();

        // Sidebar should contain the gallery title
        var galleryTitle = Page.Locator(".gallery__title");
        await Expect(galleryTitle.First).ToBeVisibleAsync();
    }

    // ── Active page highlighting ──

    [Test]
    public async Task BasicsPage_IsHighlightedInSidebar()
    {
        await NavigateTo("/datagrid/basics");

        var activeLink = Page.Locator(".gallery__nav-btn--active:has-text('Basics')");
        await Expect(activeLink.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task EditingPage_IsHighlightedInSidebar()
    {
        await NavigateTo("/datagrid/editing");

        var activeLink = Page.Locator(".gallery__nav-btn--active:has-text('Editing')");
        await Expect(activeLink.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdvancedPage_IsHighlightedInSidebar()
    {
        await NavigateTo("/datagrid/advanced");

        var activeLink = Page.Locator(".gallery__nav-btn--active:has-text('Advanced')");
        await Expect(activeLink.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task EnterprisePage_IsHighlightedInSidebar()
    {
        await NavigateTo("/datagrid/enterprise");

        var activeLink = Page.Locator(".gallery__nav-btn--active:has-text('Enterprise')");
        await Expect(activeLink.First).ToBeVisibleAsync();
    }
}

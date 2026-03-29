using Arcadia.Tests.E2E.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.Playground;

[TestFixture]
public class PlaygroundNavigationTests : PageTest
{
    private const string HomeUrl = TestConstants.BaseUrl + "/";

    private async Task NavigateHome()
    {
        await Page.GotoAsync(HomeUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(1500);
    }

    // ── Home page basics ──

    [Test]
    public async Task HomePage_Returns200AndTitleVisible()
    {
        var response = await Page.GotoAsync(HomeUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        Assert.That(response!.Status, Is.EqualTo(200), "Home page should return HTTP 200");
        await Page.WaitForTimeoutAsync(1500);

        var title = Page.Locator("text=Arcadia Controls").First;
        await Expect(title).ToBeVisibleAsync();
    }

    // ── Sidebar section headers ──

    [Test]
    public async Task Sidebar_AllSectionHeadersRender()
    {
        await NavigateHome();

        var sectionNames = new[] { "Charts", "DataGrid", "Forms", "UI", "Tools" };
        foreach (var name in sectionNames)
        {
            var header = Page.Locator($".gallery__nav-group >> text='{name}'");
            await Expect(header.First).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task Sidebar_SectionHeaders_HaveNoGrayBackground()
    {
        await NavigateHome();

        var groupButtons = Page.Locator(".gallery__nav-group");
        var count = await groupButtons.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(5), "Should have at least 5 nav group buttons");

        for (var i = 0; i < count; i++)
        {
            var btn = groupButtons.Nth(i);
            var bg = await btn.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // Should be transparent (rgba(0,0,0,0)) — not gray
            Assert.That(bg, Does.Not.Contain("rgb(128")
                .And.Not.Contain("rgb(192")
                .And.Not.Contain("rgb(160")
                .And.Not.Contain("rgb(224"),
                $"Nav group button {i} should not have a gray background, got: {bg}");
        }
    }

    // ── DataGrid route links ──

    [TestCase("/datagrid/basics")]
    [TestCase("/datagrid/editing")]
    [TestCase("/datagrid/advanced")]
    [TestCase("/datagrid/enterprise")]
    public async Task DataGridRoutes_Return200(string route)
    {
        var response = await Page.GotoAsync(TestConstants.BaseUrl + route,
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        Assert.That(response!.Status, Is.EqualTo(200),
            $"Route {route} should return HTTP 200");
    }

    // ── Sidebar expand/collapse ──

    [Test]
    public async Task ClickChartsHeader_ExpandsToShowChartLinks()
    {
        await NavigateHome();

        // Charts section might already be expanded on home; ensure we test the toggle
        var chartsHeader = Page.Locator(".gallery__nav-group >> text='Charts'");
        await chartsHeader.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Look for a chart sub-link like "Dashboard Widgets" or "Line & Area"
        var chartLink = Page.Locator(".gallery__nav-btn:has-text('Dashboard Widgets')");
        // If collapsed, click again to expand
        if (await chartLink.CountAsync() == 0)
        {
            await chartsHeader.First.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }

        await Expect(chartLink.First).ToBeVisibleAsync();
    }

    // ── Home page product cards ──

    [Test]
    public async Task HomePage_ProductCards_AreClickable()
    {
        await NavigateHome();

        // The DataGrid card is an <a> link to /datagrid/basics
        var datagridCard = Page.Locator("a[href='/datagrid/basics']").First;
        await Expect(datagridCard).ToBeVisibleAsync();

        // The Charts card is a button that sets _tab = "dashboard"
        var chartsCard = Page.Locator("button:has-text('Charts & Dashboard')");
        await Expect(chartsCard.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomePage_DataGridCard_NavigatesToBasics()
    {
        await NavigateHome();

        var datagridLink = Page.Locator("a[href='/datagrid/basics']").First;
        await datagridLink.ClickAsync();
        await Page.WaitForTimeoutAsync(1500);

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("datagrid/basics"));
        var header = Page.Locator("h1:has-text('DataGrid')");
        await Expect(header.First).ToBeVisibleAsync();
    }

    // ── Console errors ──

    [Test]
    public async Task HomePage_NoConsoleErrors()
    {
        var errors = new List<string>();
        Page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                errors.Add(msg.Text);
        };

        await Page.GotoAsync(HomeUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(2000);

        // Filter out known benign errors (e.g., favicon, dev mode warnings)
        var realErrors = errors.Where(e =>
            !e.Contains("favicon") &&
            !e.Contains("DevTools") &&
            !e.Contains("manifest.json")).ToList();

        Assert.That(realErrors, Is.Empty,
            $"Console errors on home page: {string.Join("; ", realErrors)}");
    }
}

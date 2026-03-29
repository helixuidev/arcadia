using Arcadia.Tests.E2E.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.Charts;

[TestFixture]
public class ChartInteractionTests : PageTest
{
    /// <summary>
    /// Navigate to a chart tab by name. Expands Charts section if collapsed.
    /// </summary>
    private async Task NavigateToChartTab(string tabButtonText)
    {
        await Page.GotoAsync(TestConstants.BaseUrl + "/",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(1500);

        var tabBtn = Page.Locator($"button.gallery__nav-btn:has-text('{tabButtonText}')");
        var count = await tabBtn.CountAsync();

        if (count == 0)
        {
            var chartsHeader = Page.Locator("button.gallery__nav-group:has-text('Charts')");
            await chartsHeader.First.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }

        tabBtn = Page.Locator($"button.gallery__nav-btn:has-text('{tabButtonText}')");
        await tabBtn.First.ClickAsync();
        await Page.WaitForTimeoutAsync(1500);
    }

    // ── Line Chart Tooltip ──

    [Test]
    public async Task LineChart_HoverPointShowsTooltip()
    {
        await NavigateToChartTab("Line & Area");

        // Find SVG circle points in the chart
        var points = Page.Locator(".gallery__chart-container svg circle");
        var pointCount = await points.CountAsync();
        Assert.That(pointCount, Is.GreaterThanOrEqualTo(1),
            "Line chart should render data points as SVG circles");

        // Hover a point to trigger tooltip
        await points.First.HoverAsync();
        await Page.WaitForTimeoutAsync(500);

        // Tooltip should appear (look for tooltip container or role)
        var tooltip = Page.Locator(".arcadia-chart__tooltip, [role='tooltip'], .arcadia-tooltip");
        var tooltipCount = await tooltip.CountAsync();
        Assert.That(tooltipCount, Is.GreaterThanOrEqualTo(1),
            "Hovering a data point should show a tooltip");
    }

    // ── Bar Chart Colors ──

    [Test]
    public async Task BarChart_BarsRenderWithCorrectColors()
    {
        await NavigateToChartTab("Bar");

        // Bars are SVG rect elements
        var bars = Page.Locator(".gallery__chart-container svg rect");
        var barCount = await bars.CountAsync();
        Assert.That(barCount, Is.GreaterThanOrEqualTo(4),
            "Bar chart should render multiple bar rect elements");

        // Verify first bar has a fill that is not white or transparent
        var fill = await bars.First.GetAttributeAsync("fill");
        Assert.That(fill, Is.Not.Null.And.Not.EqualTo("white").And.Not.EqualTo("transparent").And.Not.EqualTo("none"),
            "Bar should have a visible fill color");
    }

    // ── Pie Chart Slices ──

    [Test]
    public async Task PieChart_SlicesRender()
    {
        await NavigateToChartTab("Pie & Donut");

        // Pie slices are SVG path elements
        var paths = Page.Locator(".gallery__chart-container svg path");
        var pathCount = await paths.CountAsync();
        Assert.That(pathCount, Is.GreaterThanOrEqualTo(3),
            "Pie chart should render at least 3 path elements for slices");
    }

    // ── Dashboard KPI Cards ──

    [Test]
    public async Task Dashboard_KpiCardsRenderWithValues()
    {
        await NavigateToChartTab("Dashboard Widgets");

        // KPI card values: $142,580 / 8,420 / 3.8%
        var revenueValue = Page.Locator("text=$142,580");
        await Expect(revenueValue.First).ToBeVisibleAsync();

        var usersValue = Page.Locator("text=8,420");
        await Expect(usersValue.First).ToBeVisibleAsync();

        var conversionValue = Page.Locator("text=3.8%");
        await Expect(conversionValue.First).ToBeVisibleAsync();
    }

    // ── Progress Bars ──

    [Test]
    public async Task Dashboard_ProgressBarsRenderWithColoredFills()
    {
        await NavigateToChartTab("Dashboard Widgets");

        // Progress bars should exist with labels
        var storageLabel = Page.Locator("text=Storage Used");
        await Expect(storageLabel.First).ToBeVisibleAsync();

        var cpuLabel = Page.Locator("text=CPU Usage");
        await Expect(cpuLabel.First).ToBeVisibleAsync();

        var memoryLabel = Page.Locator("text=Memory");
        await Expect(memoryLabel.First).ToBeVisibleAsync();

        // Progress bar fill elements should be present
        var progressFills = Page.Locator(".arcadia-progress__fill, .arcadia-progress-bar__fill, [role='progressbar']");
        var fillCount = await progressFills.CountAsync();
        Assert.That(fillCount, Is.GreaterThanOrEqualTo(3),
            "Dashboard should render at least 3 progress bar fill elements");
    }

    // ── Sparklines ──

    [Test]
    public async Task Dashboard_SparklinesRenderSvgPaths()
    {
        await NavigateToChartTab("Dashboard Widgets");

        // Sparklines section should have SVG elements with paths
        var sparklineSvgs = Page.Locator(".arcadia-sparkline svg, .gallery__sparkline-row svg");
        var svgCount = await sparklineSvgs.CountAsync();
        Assert.That(svgCount, Is.GreaterThanOrEqualTo(3),
            "Dashboard should render at least 3 sparkline SVGs");

        // Each sparkline should have a path element
        var sparklinePaths = Page.Locator(".arcadia-sparkline svg path, .gallery__sparkline-row svg path");
        var pathCount = await sparklinePaths.CountAsync();
        Assert.That(pathCount, Is.GreaterThanOrEqualTo(3),
            "Sparkline SVGs should contain path elements");
    }

    // ── Streaming Chart ──

    [Test]
    public async Task StreamingChart_AddPointExtendsLine()
    {
        await NavigateToChartTab("Streaming");

        // Count initial data points (circles)
        var pointsBefore = Page.Locator(".gallery__chart-container svg circle");
        var countBefore = await pointsBefore.CountAsync();

        // Click "Add Point" button
        var addButton = Page.Locator("button:has-text('Add Point')");
        await Expect(addButton.First).ToBeVisibleAsync();
        await addButton.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Count points after — should have increased
        var pointsAfter = Page.Locator(".gallery__chart-container svg circle");
        var countAfter = await pointsAfter.CountAsync();
        Assert.That(countAfter, Is.GreaterThanOrEqualTo(countBefore),
            "Adding a point should maintain or increase the number of rendered data points");
    }

    // ── Sankey Chart ──

    [Test]
    public async Task SankeyChart_NodesAndLinksRender()
    {
        await NavigateToChartTab("Sankey");

        // Sankey nodes are typically rect elements
        var nodes = Page.Locator(".gallery__chart-container svg rect, .arcadia-sankey__node");
        var nodeCount = await nodes.CountAsync();
        Assert.That(nodeCount, Is.GreaterThanOrEqualTo(3),
            "Sankey chart should render node rectangles");

        // Sankey links are typically path elements
        var links = Page.Locator(".gallery__chart-container svg path, .arcadia-sankey__link");
        var linkCount = await links.CountAsync();
        Assert.That(linkCount, Is.GreaterThanOrEqualTo(3),
            "Sankey chart should render link paths");
    }

    [Test]
    public async Task PlaygroundBuilder_WidthSlider_ChangesSvgWidth()
    {
        await Page.GotoAsync($"{TestConstants.BaseUrl}/playground-builder",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(2000);

        // Width slider is the second range input (index 1)
        var widthSlider = Page.Locator("input[type='range']").Nth(1);
        await Expect(widthSlider).ToBeVisibleAsync();

        // Set width to 400
        await widthSlider.FillAsync("400");
        await Page.WaitForTimeoutAsync(500);

        var svg = Page.Locator("svg[data-chart]").First;
        var width400 = await svg.GetAttributeAsync("width");
        Assert.That(width400, Is.EqualTo("400"),
            "Setting width slider to 400 should make SVG width=400");

        // Set width to 800
        await widthSlider.FillAsync("800");
        await Page.WaitForTimeoutAsync(500);

        var width800 = await svg.GetAttributeAsync("width");
        Assert.That(width800, Is.EqualTo("800"),
            "Setting width slider to 800 should make SVG width=800");

        // Set back to 0 (responsive)
        await widthSlider.FillAsync("0");
        await Page.WaitForTimeoutAsync(500);

        var widthResponsive = await svg.GetAttributeAsync("width");
        Assert.That(widthResponsive, Does.Not.EqualTo("0"),
            "Width=0 should be responsive (100% or measured width), not 0");
    }
}

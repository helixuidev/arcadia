using Arcadia.Tests.E2E.Infrastructure;

namespace Arcadia.Tests.E2E.CrossCutting;

/// <summary>
/// Tests that clicking legend buttons toggles series visibility.
/// Only applies to multi-series charts with clickable legends (line, bar).
/// </summary>
[TestFixture]
public class LegendInteractionTests : ChartTestBase
{
    [Test]
    public async Task LineChart_ToggleLegend_HidesSeries()
    {
        await EnableReducedMotion();
        await NavigateToChart("line");

        var firstBtn = Legend.Locator(".arcadia-chart__legend-btn").First;
        await firstBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Button should now have the hidden class
        await Expect(firstBtn).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("legend-btn--hidden"));

        // Remove focus + hover state before snapshotting — both render with subpixel
        // antialiasing that varies run-to-run, causing nondeterministic pixel diffs.
        await Page.EvaluateAsync("() => (document.activeElement instanceof HTMLElement) && document.activeElement.blur()");
        await Page.Mouse.MoveAsync(0, 0);
        await Page.WaitForTimeoutAsync(100);

        await AssertChartScreenshot("line-legend-toggled.png");
    }

    [Test]
    public async Task BarChart_ToggleLegend_HidesSeries()
    {
        await EnableReducedMotion();
        await NavigateToChart("bar");

        var firstBtn = Legend.Locator(".arcadia-chart__legend-btn").First;
        await firstBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        await Expect(firstBtn).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("legend-btn--hidden"));
        await AssertChartScreenshot("bar-legend-toggled.png");
    }

    [Test]
    public async Task LineChart_ToggleLegend_RestoresSeries()
    {
        await EnableReducedMotion();
        await NavigateToChart("line");

        var firstBtn = Legend.Locator(".arcadia-chart__legend-btn").First;
        // Toggle off
        await firstBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(200);
        // Toggle back on
        await firstBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(200);

        // Should no longer have hidden class
        await Expect(firstBtn).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("legend-btn--hidden"));
    }
}

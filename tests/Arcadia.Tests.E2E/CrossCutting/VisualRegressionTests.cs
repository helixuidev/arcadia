using Arcadia.Tests.E2E.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.CrossCutting;

[TestFixture]
public class VisualRegressionTests : PageTest
{
    private static readonly string ScreenshotDir = Path.Combine(
        FindRepoRoot(), "tests", "Arcadia.Tests.E2E", "Screenshots");

    private async Task NavigateAndWait(string route)
    {
        await Page.GotoAsync(TestConstants.BaseUrl + route,
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(2000); // Extra time for Blazor render + animations
    }

    /// <summary>
    /// Takes a full-page or element screenshot and compares against a stored baseline.
    /// On first run (no baseline), saves the screenshot as the new baseline.
    /// Set env var UPDATE_SNAPSHOTS=1 to overwrite existing baselines.
    /// </summary>
    private async Task AssertPageScreenshot(string name, ILocator? element = null, double maxDiffPercent = 2.0)
    {
        var baselinePath = Path.Combine(ScreenshotDir, "baselines", name);
        var actualPath = Path.Combine(ScreenshotDir, "actual", name);

        Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(actualPath)!);

        byte[] screenshot;
        if (element is not null)
        {
            screenshot = await element.ScreenshotAsync(new LocatorScreenshotOptions
            {
                Type = ScreenshotType.Png
            });
        }
        else
        {
            screenshot = await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Type = ScreenshotType.Png,
                FullPage = false // viewport only to keep consistent
            });
        }

        var updateSnapshots = Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS") == "1";

        if (!File.Exists(baselinePath) || updateSnapshots)
        {
            await File.WriteAllBytesAsync(baselinePath, screenshot);
            Console.WriteLine($"[Baseline saved] {name}");
            return;
        }

        await File.WriteAllBytesAsync(actualPath, screenshot);

        var baseline = await File.ReadAllBytesAsync(baselinePath);
        var diffBytes = CountDiffBytes(baseline, screenshot);
        var totalBytes = Math.Max(baseline.Length, screenshot.Length);
        var diffPercent = totalBytes > 0 ? (diffBytes * 100.0 / totalBytes) : 0;

        if (diffPercent > maxDiffPercent)
        {
            Assert.Fail(
                $"Screenshot '{name}' differs from baseline by {diffPercent:F1}% ({diffBytes} of {totalBytes} bytes). " +
                $"Actual: {actualPath}. Baseline: {baselinePath}. " +
                $"Set UPDATE_SNAPSHOTS=1 to update baselines.");
        }

        if (File.Exists(actualPath)) File.Delete(actualPath);
    }

    // ── Home page ──

    [Test]
    public async Task HomePage_VisualRegression()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await NavigateAndWait("/");

        await AssertPageScreenshot("home-page.png");
    }

    // ── DataGrid Basics ──

    [Test]
    public async Task DataGridBasics_VisualRegression()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await NavigateAndWait("/datagrid/basics");

        // Screenshot the main content area (not sidebar) to reduce noise
        var content = Page.Locator(".gallery__content").First;
        await Expect(content).ToBeVisibleAsync();

        await AssertPageScreenshot("datagrid-basics.png", content);
    }

    // ── Charts Dashboard tab ──

    [Test]
    public async Task ChartsDashboard_VisualRegression()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await NavigateAndWait("/");

        // Click "Dashboard Widgets" in the Charts section
        var dashboardBtn = Page.Locator("button:has-text('Dashboard Widgets')");
        var count = await dashboardBtn.CountAsync();
        if (count == 0)
        {
            // Charts section may be collapsed; expand it
            var chartsHeader = Page.Locator(".gallery__nav-group >> text='Charts'");
            await chartsHeader.First.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
            dashboardBtn = Page.Locator("button:has-text('Dashboard Widgets')");
        }

        await dashboardBtn.First.ClickAsync();
        await Page.WaitForTimeoutAsync(TestConstants.ChartRenderWaitMs);

        var content = Page.Locator(".gallery__content").First;
        await Expect(content).ToBeVisibleAsync();

        await AssertPageScreenshot("charts-dashboard.png", content);
    }

    // ── UI Components tab ──

    [Test]
    public async Task UIComponents_VisualRegression()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await NavigateAndWait("/");

        // Navigate to UI tab
        var uiButton = Page.Locator("button:has-text('Dialog / Tabs / Tooltip')");
        var uiCount = await uiButton.CountAsync();
        if (uiCount == 0)
        {
            var uiHeader = Page.Locator(".gallery__nav-group >> text='UI'");
            await uiHeader.First.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
            uiButton = Page.Locator("button:has-text('Dialog / Tabs / Tooltip')");
        }

        await uiButton.First.ClickAsync();
        await Page.WaitForTimeoutAsync(1500);

        var content = Page.Locator(".gallery__content").First;
        await Expect(content).ToBeVisibleAsync();

        await AssertPageScreenshot("ui-components.png", content);
    }

    // ── DataGrid Enterprise ──

    [Test]
    public async Task DataGridEnterprise_VisualRegression()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await NavigateAndWait("/datagrid/enterprise");

        var content = Page.Locator(".gallery__content").First;
        await Expect(content).ToBeVisibleAsync();

        await AssertPageScreenshot("datagrid-enterprise.png", content);
    }

    // ── Sidebar visual consistency ──

    [Test]
    public async Task Sidebar_VisualRegression()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await NavigateAndWait("/");

        var sidebar = Page.Locator(".gallery__sidebar").First;
        await Expect(sidebar).ToBeVisibleAsync();

        await AssertPageScreenshot("sidebar.png", sidebar);
    }

    // ── Helpers ──

    private static int CountDiffBytes(byte[] a, byte[] b)
    {
        var maxLen = Math.Max(a.Length, b.Length);
        var minLen = Math.Min(a.Length, b.Length);
        var diff = maxLen - minLen;
        for (var i = 0; i < minLen; i++)
        {
            if (a[i] != b[i]) diff++;
        }
        return diff;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "HelixUI.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..");
    }
}

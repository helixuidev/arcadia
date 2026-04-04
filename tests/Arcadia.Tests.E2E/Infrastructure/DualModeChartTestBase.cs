using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.Infrastructure;

/// <summary>
/// Chart-specific test base that supports running against both Server and WASM render modes.
/// <para>
/// This mirrors <see cref="ChartTestBase"/> but adds a <see cref="RenderMode"/> parameter
/// to navigation and screenshot helpers so the same chart tests can target either demo app.
/// </para>
/// <example>
/// <code>
/// [TestFixture]
/// public class MyChartTests : DualModeChartTestBase
/// {
///     [Test]
///     public async Task Chart_Renders([ValueSource(nameof(AllRenderModes))] RenderMode mode)
///     {
///         await NavigateToChart(mode, "line");
///         await Expect(ChartSvg).ToBeVisibleAsync();
///     }
/// }
/// </code>
/// </example>
/// </summary>
public class DualModeChartTestBase : RenderModeTestBase
{
    private static readonly string ScreenshotDir = Path.Combine(
        FindRepoRoot(), "tests", "Arcadia.Tests.E2E", "Screenshots");

    /// <summary>
    /// Navigates to a chart test page on the specified render mode's demo app.
    /// </summary>
    protected async Task NavigateToChart(RenderMode mode, string chartType)
    {
        var baseUrl = BaseUrlFor(mode);
        await Page.GotoAsync($"{baseUrl}{TestConstants.TestPagePrefix}/{chartType}",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForSelectorAsync(TestConstants.Selectors.ChartContainer,
            new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        await Page.WaitForTimeoutAsync(TestConstants.ChartRenderWaitMs);
    }

    protected async Task EnableReducedMotion()
    {
        await Page.EmulateMediaAsync(new PageEmulateMediaOptions
        {
            ReducedMotion = ReducedMotion.Reduce
        });
    }

    protected async Task SetViewport(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await Page.WaitForTimeoutAsync(500);
    }

    protected ILocator ChartContainer => Page.Locator(TestConstants.Selectors.ChartContainer).First;
    protected ILocator ChartSvg => Page.Locator(TestConstants.Selectors.ChartSvg).First;
    protected ILocator Legend => Page.Locator(TestConstants.Selectors.Legend).First;
    protected ILocator SrTable => Page.Locator(TestConstants.Selectors.SrTable).First;

    /// <summary>
    /// Takes a screenshot and compares against a baseline, using render-mode-specific directories.
    /// </summary>
    protected async Task AssertChartScreenshot(RenderMode mode, string name, double maxDiffPercent = 1.0)
    {
        var modeDir = mode.ToString().ToLowerInvariant();
        var baselinePath = Path.Combine(ScreenshotDir, "baselines", modeDir, name);
        var actualPath = Path.Combine(ScreenshotDir, "actual", modeDir, name);

        Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(actualPath)!);

        var screenshot = await ChartContainer.ScreenshotAsync(new LocatorScreenshotOptions
        {
            Type = ScreenshotType.Png
        });

        var updateSnapshots = Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS") == "1";

        if (!File.Exists(baselinePath) || updateSnapshots)
        {
            await File.WriteAllBytesAsync(baselinePath, screenshot);
            Console.WriteLine($"[Baseline saved] {modeDir}/{name}");
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
                $"Screenshot '{modeDir}/{name}' differs from baseline by {diffPercent:F1}% ({diffBytes} of {totalBytes} bytes). " +
                $"Actual: {actualPath}. Baseline: {baselinePath}. " +
                $"Set UPDATE_SNAPSHOTS=1 to update baselines.");
        }

        if (File.Exists(actualPath)) File.Delete(actualPath);
    }

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

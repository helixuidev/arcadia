using System.Text.Json;
using System.Text.Json.Serialization;
using Arcadia.Tests.E2E.Infrastructure;

namespace Arcadia.Tests.E2E.CrossCutting;

/// <summary>
/// Runs axe-core accessibility audits against every chart type in the demo gallery.
/// Injects axe-core from CDN, runs axe.run() on the chart container, and asserts
/// zero critical or serious violations.
/// </summary>
[TestFixture]
public class AccessibilityAuditTests : ChartTestBase
{
    private static readonly string ReportDir = Path.Combine(
        FindRepoRoot(), "docs");

    private static readonly string ReportPath = Path.Combine(
        ReportDir, "accessibility-audit.md");

    /// <summary>All gallery tabs that contain chart content.</summary>
    private static readonly string[] GalleryTabs =
    {
        "dashboard", "line", "bar", "pie", "scatter", "candle",
        "gauge", "radar", "heatmap", "rose", "boxplot", "rangearea",
        "sankey", "chord"
    };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Accumulated results across all tabs, written to report at the end.</summary>
    private static readonly List<TabAuditResult> AllResults = new();
    private static readonly object Lock = new();

    // ──────────────────────────────────────────────
    //  Per-tab test
    // ──────────────────────────────────────────────

    private static IEnumerable<TestCaseData> AllTabs() =>
        GalleryTabs.Select(t => new TestCaseData(t).SetName($"A11y_{t}"));

    [TestCaseSource(nameof(AllTabs))]
    public async Task Chart_Tab_HasNoSeriousAccessibilityViolations(string tab)
    {
        // 1. Navigate to the gallery page
        await Page.GotoAsync($"{TestConstants.BaseUrl}/charts",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // 2. Click the tab button
        var tabButton = Page.Locator($"button.gallery__nav-btn").Filter(
            new LocatorFilterOptions { HasText = GetTabLabel(tab) });
        await tabButton.ClickAsync();

        // 3. Wait for chart content to render
        await Page.WaitForTimeoutAsync(TestConstants.ChartRenderWaitMs);

        // 4. Inject axe-core from CDN
        await InjectAxeCore();

        // 5. Run axe on the visible gallery content area
        var axeResultJson = await Page.EvaluateAsync<JsonElement>(@"
            async () => {
                const contentArea = document.querySelector('.gallery__content');
                if (!contentArea) {
                    return { violations: [], passes: [], incomplete: [], inapplicable: [] };
                }
                const results = await axe.run(contentArea, {
                    runOnly: {
                        type: 'tag',
                        values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'best-practice']
                    }
                });
                // Exclude watermark from contrast violations (intentionally low-contrast decorative text)
                results.violations = results.violations.map(v => {
                    if (v.id === 'color-contrast') {
                        v.nodes = v.nodes.filter(n => !n.html.includes('arcadia-watermark'));
                        if (v.nodes.length === 0) return null;
                    }
                    return v;
                }).filter(Boolean);
                // Serialize only what we need (full nodes are huge)
                return {
                    violations: results.violations.map(v => ({
                        id: v.id,
                        impact: v.impact,
                        description: v.description,
                        helpUrl: v.helpUrl,
                        nodes: v.nodes.length,
                        tags: v.tags
                    })),
                    passes: results.passes.map(p => ({
                        id: p.id,
                        description: p.description,
                        nodes: p.nodes.length
                    })),
                    incomplete: results.incomplete.map(i => ({
                        id: i.id,
                        impact: i.impact,
                        description: i.description,
                        nodes: i.nodes.length
                    })),
                    inapplicable: results.inapplicable.map(i => ({
                        id: i.id
                    }))
                };
            }
        ");

        // 6. Parse results
        var violations = ParseViolations(axeResultJson);
        var passes = ParsePasses(axeResultJson);
        var incomplete = ParseIncomplete(axeResultJson);

        // 7. Store for the summary report
        var result = new TabAuditResult
        {
            Tab = tab,
            Violations = violations,
            Passes = passes,
            Incomplete = incomplete,
            TotalViolationNodes = violations.Sum(v => v.Nodes),
            CriticalCount = violations.Count(v => v.Impact == "critical"),
            SeriousCount = violations.Count(v => v.Impact == "serious"),
            ModerateCount = violations.Count(v => v.Impact == "moderate"),
            MinorCount = violations.Count(v => v.Impact == "minor")
        };

        lock (Lock)
        {
            AllResults.Add(result);
        }

        // 8. Log summary for this tab
        TestContext.Out.WriteLine($"=== Accessibility Audit: {tab} ===");
        TestContext.Out.WriteLine($"  Passes:     {passes.Count}");
        TestContext.Out.WriteLine($"  Violations: {violations.Count} ({result.CriticalCount} critical, {result.SeriousCount} serious, {result.ModerateCount} moderate, {result.MinorCount} minor)");
        TestContext.Out.WriteLine($"  Incomplete: {incomplete.Count}");

        foreach (var v in violations)
        {
            TestContext.Out.WriteLine($"  [{v.Impact?.ToUpper()}] {v.Id}: {v.Description} ({v.Nodes} nodes)");
        }

        // 9. Assert: zero critical or serious violations
        var blocking = violations.Where(v => v.Impact is "critical" or "serious").ToList();
        if (blocking.Count > 0)
        {
            var details = string.Join("\n", blocking.Select(v =>
                $"  [{v.Impact?.ToUpper()}] {v.Id}: {v.Description} ({v.Nodes} affected nodes)\n    {v.HelpUrl}"));
            Assert.Fail(
                $"Tab '{tab}' has {blocking.Count} critical/serious accessibility violation(s):\n{details}");
        }
    }

    // ──────────────────────────────────────────────
    //  Report generation (runs after all tests)
    // ──────────────────────────────────────────────

    [OneTimeTearDown]
    public void GenerateReport()
    {
        if (AllResults.Count == 0) return;

        Directory.CreateDirectory(ReportDir);
        var report = BuildMarkdownReport(AllResults);
        File.WriteAllText(ReportPath, report);
        TestContext.Out.WriteLine($"\nAccessibility audit report written to: {ReportPath}");
    }

    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    private async Task InjectAxeCore()
    {
        // Check if axe is already loaded (avoid double-injection)
        var axeLoaded = await Page.EvaluateAsync<bool>("() => typeof axe !== 'undefined'");
        if (axeLoaded) return;

        // Use Playwright's built-in script tag injection (handles loading/waiting properly)
        await Page.AddScriptTagAsync(new PageAddScriptTagOptions
        {
            Url = "https://cdnjs.cloudflare.com/ajax/libs/axe-core/4.10.2/axe.min.js"
        });

        // Verify it loaded
        var loaded = await Page.EvaluateAsync<bool>("() => typeof axe !== 'undefined'");
        Assert.That(loaded, Is.True, "axe-core failed to load from CDN");
    }

    private static string GetTabLabel(string tab) => tab switch
    {
        "dashboard" => "Dashboard Widgets",
        "line" => "Line & Area",
        "bar" => "Bar",
        "pie" => "Pie & Donut",
        "scatter" => "Scatter",
        "candle" => "Candlestick",
        "gauge" => "Gauges",
        "radar" => "Radar",
        "heatmap" => "Heatmap",
        "rose" => "Rose / Polar",
        "boxplot" => "Box Plot",
        "rangearea" => "Range Area",
        "sankey" => "Sankey",
        "chord" => "Chord",
        _ => tab
    };

    private static List<AxeViolation> ParseViolations(JsonElement root)
    {
        var list = new List<AxeViolation>();
        if (root.TryGetProperty("violations", out var arr))
        {
            foreach (var item in arr.EnumerateArray())
            {
                list.Add(new AxeViolation
                {
                    Id = item.GetProperty("id").GetString() ?? "",
                    Impact = item.GetProperty("impact").GetString() ?? "",
                    Description = item.GetProperty("description").GetString() ?? "",
                    HelpUrl = item.TryGetProperty("helpUrl", out var url) ? url.GetString() ?? "" : "",
                    Nodes = item.GetProperty("nodes").GetInt32(),
                    Tags = item.TryGetProperty("tags", out var tags)
                        ? tags.EnumerateArray().Select(t => t.GetString() ?? "").ToList()
                        : new List<string>()
                });
            }
        }
        return list;
    }

    private static List<AxePass> ParsePasses(JsonElement root)
    {
        var list = new List<AxePass>();
        if (root.TryGetProperty("passes", out var arr))
        {
            foreach (var item in arr.EnumerateArray())
            {
                list.Add(new AxePass
                {
                    Id = item.GetProperty("id").GetString() ?? "",
                    Description = item.GetProperty("description").GetString() ?? "",
                    Nodes = item.GetProperty("nodes").GetInt32()
                });
            }
        }
        return list;
    }

    private static List<AxeIncomplete> ParseIncomplete(JsonElement root)
    {
        var list = new List<AxeIncomplete>();
        if (root.TryGetProperty("incomplete", out var arr))
        {
            foreach (var item in arr.EnumerateArray())
            {
                list.Add(new AxeIncomplete
                {
                    Id = item.GetProperty("id").GetString() ?? "",
                    Impact = item.GetProperty("impact").GetString() ?? "",
                    Description = item.GetProperty("description").GetString() ?? "",
                    Nodes = item.GetProperty("nodes").GetInt32()
                });
            }
        }
        return list;
    }

    private static string BuildMarkdownReport(List<TabAuditResult> results)
    {
        var sorted = results.OrderBy(r => Array.IndexOf(GalleryTabs, r.Tab)).ToList();
        var totalViolations = sorted.Sum(r => r.Violations.Count);
        var totalPasses = sorted.Sum(r => r.Passes.Count);
        var totalIncomplete = sorted.Sum(r => r.Incomplete.Count);
        var totalCritical = sorted.Sum(r => r.CriticalCount);
        var totalSerious = sorted.Sum(r => r.SeriousCount);
        var totalModerate = sorted.Sum(r => r.ModerateCount);
        var totalMinor = sorted.Sum(r => r.MinorCount);

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("# Arcadia Charts Accessibility Audit Report");
        sb.AppendLine();
        sb.AppendLine($"**Date:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"**Tool:** axe-core 4.10.2");
        sb.AppendLine($"**Standards:** WCAG 2.0 A/AA, WCAG 2.1 A/AA, Best Practices");
        sb.AppendLine($"**Target:** {TestConstants.BaseUrl}/charts (demo gallery)");
        sb.AppendLine();

        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine($"| Metric | Count |");
        sb.AppendLine($"|--------|-------|");
        sb.AppendLine($"| Tabs audited | {sorted.Count} |");
        sb.AppendLine($"| Total rules passed | {totalPasses} |");
        sb.AppendLine($"| Total violations | {totalViolations} |");
        sb.AppendLine($"| Critical | {totalCritical} |");
        sb.AppendLine($"| Serious | {totalSerious} |");
        sb.AppendLine($"| Moderate | {totalModerate} |");
        sb.AppendLine($"| Minor | {totalMinor} |");
        sb.AppendLine($"| Incomplete (needs review) | {totalIncomplete} |");
        sb.AppendLine();

        if (totalCritical == 0 && totalSerious == 0)
        {
            sb.AppendLine("> **PASS** -- No critical or serious accessibility violations detected.");
        }
        else
        {
            sb.AppendLine($"> **FAIL** -- {totalCritical + totalSerious} critical/serious violation(s) require remediation.");
        }
        sb.AppendLine();

        // Per-tab breakdown
        sb.AppendLine("## Per-Tab Results");
        sb.AppendLine();
        sb.AppendLine("| Tab | Passes | Violations | Critical | Serious | Moderate | Minor | Incomplete |");
        sb.AppendLine("|-----|--------|------------|----------|---------|----------|-------|------------|");
        foreach (var r in sorted)
        {
            sb.AppendLine($"| {r.Tab} | {r.Passes.Count} | {r.Violations.Count} | {r.CriticalCount} | {r.SeriousCount} | {r.ModerateCount} | {r.MinorCount} | {r.Incomplete.Count} |");
        }
        sb.AppendLine();

        // Violation details
        var allViolations = sorted
            .SelectMany(r => r.Violations.Select(v => (Tab: r.Tab, Violation: v)))
            .ToList();

        if (allViolations.Count > 0)
        {
            sb.AppendLine("## Violation Details");
            sb.AppendLine();

            // Group by rule ID
            var grouped = allViolations.GroupBy(x => x.Violation.Id).OrderBy(g =>
            {
                var impact = g.First().Violation.Impact;
                return impact switch
                {
                    "critical" => 0,
                    "serious" => 1,
                    "moderate" => 2,
                    "minor" => 3,
                    _ => 4
                };
            });

            foreach (var group in grouped)
            {
                var first = group.First().Violation;
                var tabs = string.Join(", ", group.Select(x => x.Tab).Distinct());
                sb.AppendLine($"### `{first.Id}` ({first.Impact})");
                sb.AppendLine();
                sb.AppendLine($"**Description:** {first.Description}");
                sb.AppendLine();
                sb.AppendLine($"**Affected tabs:** {tabs}");
                sb.AppendLine();
                sb.AppendLine($"**Total affected nodes:** {group.Sum(x => x.Violation.Nodes)}");
                sb.AppendLine();
                if (!string.IsNullOrEmpty(first.HelpUrl))
                {
                    sb.AppendLine($"**Reference:** {first.HelpUrl}");
                    sb.AppendLine();
                }
            }
        }

        // Incomplete checks
        var allIncomplete = sorted
            .SelectMany(r => r.Incomplete.Select(i => (Tab: r.Tab, Incomplete: i)))
            .ToList();

        if (allIncomplete.Count > 0)
        {
            sb.AppendLine("## Incomplete Checks (Manual Review Needed)");
            sb.AppendLine();

            var grouped = allIncomplete.GroupBy(x => x.Incomplete.Id);
            foreach (var group in grouped)
            {
                var first = group.First().Incomplete;
                var tabs = string.Join(", ", group.Select(x => x.Tab).Distinct());
                sb.AppendLine($"- **`{first.Id}`** ({first.Impact}): {first.Description} -- affects: {tabs}");
            }
            sb.AppendLine();
        }

        // Passing rules
        sb.AppendLine("## Passing Rules (Sample)");
        sb.AppendLine();
        sb.AppendLine("Rules that passed across all audited tabs (showing first 20):");
        sb.AppendLine();

        var passingRules = sorted
            .SelectMany(r => r.Passes)
            .GroupBy(p => p.Id)
            .OrderByDescending(g => g.Count())
            .Take(20);

        foreach (var group in passingRules)
        {
            var first = group.First();
            sb.AppendLine($"- `{first.Id}`: {first.Description} ({group.Sum(p => p.Nodes)} nodes across {group.Count()} tabs)");
        }
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("*Generated by `Arcadia.Tests.E2E.CrossCutting.AccessibilityAuditTests` using axe-core via Playwright.*");

        return sb.ToString();
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

    // ──────────────────────────────────────────────
    //  DTOs
    // ──────────────────────────────────────────────

    private class TabAuditResult
    {
        public string Tab { get; init; } = "";
        public List<AxeViolation> Violations { get; init; } = new();
        public List<AxePass> Passes { get; init; } = new();
        public List<AxeIncomplete> Incomplete { get; init; } = new();
        public int TotalViolationNodes { get; init; }
        public int CriticalCount { get; init; }
        public int SeriousCount { get; init; }
        public int ModerateCount { get; init; }
        public int MinorCount { get; init; }
    }

    private class AxeViolation
    {
        public string Id { get; init; } = "";
        public string? Impact { get; init; }
        public string Description { get; init; } = "";
        public string HelpUrl { get; init; } = "";
        public int Nodes { get; init; }
        public List<string> Tags { get; init; } = new();
    }

    private class AxePass
    {
        public string Id { get; init; } = "";
        public string Description { get; init; } = "";
        public int Nodes { get; init; }
    }

    private class AxeIncomplete
    {
        public string Id { get; init; } = "";
        public string? Impact { get; init; }
        public string Description { get; init; } = "";
        public int Nodes { get; init; }
    }
}

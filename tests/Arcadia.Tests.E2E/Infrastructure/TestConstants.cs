namespace Arcadia.Tests.E2E.Infrastructure;

public static class TestConstants
{
    public const string BaseUrl = "http://localhost:5050";
    public const string WasmBaseUrl = "http://localhost:5051";
    public const string TestPagePrefix = "/test";
    public const int ChartRenderWaitMs = 1500;

    public static readonly string[] AllChartTypes =
    {
        "line", "bar", "pie", "scatter", "radar", "gauge",
        "candlestick", "funnel", "heatmap", "treemap", "waterfall", "sankey", "chord"
    };

    public static class Viewports
    {
        public static readonly (int Width, int Height) Mobile = (375, 812);
        public static readonly (int Width, int Height) Tablet = (768, 1024);
        public static readonly (int Width, int Height) Desktop = (1280, 800);
    }

    public static class Selectors
    {
        public const string ChartContainer = ".arcadia-chart";
        public const string ChartSvg = "svg[data-chart]";
        public const string Legend = ".arcadia-chart__legend";
        public const string LegendButton = ".arcadia-chart__legend-btn";
        public const string LegendButtonHidden = ".arcadia-chart__legend-btn--hidden";
        public const string TickLabel = ".arcadia-chart__tick-label";
        public const string SrTable = "table.arcadia-sr-only";
    }
}

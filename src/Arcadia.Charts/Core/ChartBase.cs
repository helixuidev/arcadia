using Microsoft.AspNetCore.Components;
using Arcadia.Charts.Core.Layout;

namespace Arcadia.Charts.Core;

/// <summary>
/// Base class for all chart components. Provides common parameters,
/// layout engine integration, and SVG container rendering.
/// </summary>
public abstract class ChartBase<T> : Arcadia.Core.Base.HelixComponentBase
{
    // ── Data ──────────────────────────────────────────────
    /// <summary>The data to visualize.</summary>
    [Parameter] public IReadOnlyList<T>? Data { get; set; }

    // ── Dimensions ───────────────────────────────────────
    /// <summary>Chart height in pixels.</summary>
    [Parameter] public double Height { get; set; } = 300;

    /// <summary>Chart width in pixels.</summary>
    [Parameter] public double Width { get; set; } = 600;

    // ── Titles ───────────────────────────────────────────
    /// <summary>Chart title.</summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>Chart subtitle (smaller, below title).</summary>
    [Parameter] public string? Subtitle { get; set; }

    // ── Axes ─────────────────────────────────────────────
    /// <summary>X-axis title label.</summary>
    [Parameter] public string? XAxisTitle { get; set; }

    /// <summary>Y-axis title label.</summary>
    [Parameter] public string? YAxisTitle { get; set; }

    /// <summary>Manual Y-axis minimum. Null = auto from data.</summary>
    [Parameter] public double? YAxisMin { get; set; }

    /// <summary>Manual Y-axis maximum. Null = auto from data.</summary>
    [Parameter] public double? YAxisMax { get; set; }

    /// <summary>Maximum number of Y-axis ticks.</summary>
    [Parameter] public int YAxisMaxTicks { get; set; } = 8;

    /// <summary>Maximum number of X-axis ticks.</summary>
    [Parameter] public int XAxisMaxTicks { get; set; } = 12;

    /// <summary>Format string for Y-axis tick labels (e.g., "C0", "P1", "N2").</summary>
    [Parameter] public string? YAxisFormatString { get; set; }

    /// <summary>Format string for X-axis tick labels.</summary>
    [Parameter] public string? XAxisFormatString { get; set; }

    // ── Grid ─────────────────────────────────────────────
    /// <summary>Whether to show grid lines.</summary>
    [Parameter] public bool ShowGrid { get; set; } = true;

    /// <summary>Grid line color. Null = theme border color.</summary>
    [Parameter] public string? GridColor { get; set; }

    /// <summary>Grid line dash pattern (e.g., "4,4"). Empty = solid.</summary>
    [Parameter] public string GridDash { get; set; } = "4,4";

    /// <summary>Show horizontal grid lines.</summary>
    [Parameter] public bool ShowHorizontalGrid { get; set; } = true;

    /// <summary>Show vertical grid lines.</summary>
    [Parameter] public bool ShowVerticalGrid { get; set; } = false;

    // ── Legend ────────────────────────────────────────────
    /// <summary>Whether to show the legend.</summary>
    [Parameter] public bool ShowLegend { get; set; } = true;

    /// <summary>Legend position: "top", "bottom", "left", "right".</summary>
    [Parameter] public string LegendPosition { get; set; } = "bottom";

    // ── Appearance ───────────────────────────────────────
    /// <summary>Color palette for series.</summary>
    [Parameter] public ChartPalette? Palette { get; set; }

    /// <summary>Whether to animate on load.</summary>
    [Parameter] public bool AnimateOnLoad { get; set; } = true;

    /// <summary>Animation duration in milliseconds.</summary>
    [Parameter] public int AnimationDuration { get; set; } = 800;

    // ── Margins ──────────────────────────────────────────
    /// <summary>Manual top margin override. Null = auto.</summary>
    [Parameter] public double? MarginTop { get; set; }

    /// <summary>Manual right margin override. Null = auto.</summary>
    [Parameter] public double? MarginRight { get; set; }

    /// <summary>Manual bottom margin override. Null = auto.</summary>
    [Parameter] public double? MarginBottom { get; set; }

    /// <summary>Manual left margin override. Null = auto.</summary>
    [Parameter] public double? MarginLeft { get; set; }

    // ── Data labels ──────────────────────────────────────
    /// <summary>Whether to show value labels on data points/bars.</summary>
    [Parameter] public bool ShowDataLabels { get; set; }

    /// <summary>Format string for data labels.</summary>
    [Parameter] public string? DataLabelFormatString { get; set; }

    // ── Events ───────────────────────────────────────────
    /// <summary>Fired when a data point is clicked. Receives the data item.</summary>
    [Parameter] public EventCallback<T> OnPointClick { get; set; }

    /// <summary>Fired when a series is clicked. Receives the series index.</summary>
    [Parameter] public EventCallback<int> OnSeriesClick { get; set; }

    // ── States ───────────────────────────────────────────
    /// <summary>Whether the chart is in a loading state (shows skeleton).</summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>Message to show when Data is null or empty.</summary>
    [Parameter] public string? NoDataMessage { get; set; } = "No data available";

    // ── Accessibility ────────────────────────────────────
    /// <summary>Accessible description for screen readers.</summary>
    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>Format provider for number/date formatting.</summary>
    [Parameter] public IFormatProvider? FormatProvider { get; set; }

    // ── Tooltip ──────────────────────────────────────────
    /// <summary>Custom tooltip template rendered for each data point.</summary>
    [Parameter] public RenderFragment<T>? TooltipTemplate { get; set; }

    // ── Internals ────────────────────────────────────────
    [Inject] protected Microsoft.JSInterop.IJSRuntime JSRuntime { get; set; } = default!;

    protected ChartPalette EffectivePalette => Palette ?? ChartPalette.Default;
    protected ChartLayoutEngine LayoutEngine { get; } = new();
    protected ChartInteropService? Interop { get; private set; }
    protected ElementReference ContainerRef;

    protected string EffectiveGridColor => GridColor ?? "var(--arcadia-color-border, #e2e8f0)";

    protected bool HasData => Data is not null && Data.Count > 0;

    protected override void OnInitialized()
    {
        Interop = new ChartInteropService(JSRuntime);
    }

    /// <summary>Shows a default tooltip for a data point.</summary>
    protected async Task ShowTooltipForPoint(string seriesName, double value, double mouseX, double mouseY)
    {
        if (Interop is null) return;
        var formatted = FormatValue(value, YAxisFormatString ?? DataLabelFormatString);
        var html = $"<div style='font-weight:600;margin-bottom:2px'>{seriesName}</div>" +
                   $"<div>{formatted}</div>";
        await Interop.ShowTooltipAsync(html, mouseX, mouseY);
    }

    /// <summary>Hides the tooltip.</summary>
    protected async Task HideTooltipAction()
    {
        if (Interop is not null)
            await Interop.HideTooltipAsync();
    }

    /// <summary>Exports chart as PNG.</summary>
    public async Task ExportPngAsync(string filename = "chart.png")
    {
        if (Interop is not null)
            await Interop.ExportPngAsync(ContainerRef, filename);
    }

    /// <summary>Exports chart as SVG.</summary>
    public async Task ExportSvgFileAsync(string filename = "chart.svg")
    {
        if (Interop is not null)
            await Interop.ExportSvgAsync(ContainerRef, filename);
    }

    protected string ResolveColor(string? color, int seriesIndex)
    {
        if (!string.IsNullOrEmpty(color))
        {
            return color switch
            {
                "primary" => "var(--arcadia-color-primary)",
                "secondary" => "var(--arcadia-color-secondary)",
                "success" => "var(--arcadia-color-success)",
                "danger" => "var(--arcadia-color-danger)",
                "warning" => "var(--arcadia-color-warning)",
                "info" => "var(--arcadia-color-info)",
                _ => color
            };
        }
        return EffectivePalette.GetColor(seriesIndex);
    }

    protected string FormatValue(double value, string? formatString)
    {
        if (formatString is not null)
            return value.ToString(formatString, FormatProvider);
        return value.ToString("G4");
    }
}

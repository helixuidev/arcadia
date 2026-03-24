using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Arcadia.Charts.Core.Layout;

namespace Arcadia.Charts.Core;

/// <summary>
/// Base class for all chart components. Provides common parameters,
/// layout engine integration, and SVG container rendering.
/// </summary>
public abstract class ChartBase<T> : Arcadia.Core.Base.ArcadiaComponentBase, IAsyncDisposable
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

    /// <summary>Whether to show the export toolbar (PNG/SVG) on hover. Default true.</summary>
    [Parameter] public bool ShowToolbar { get; set; } = true;

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

    // ── Pan/Zoom ─────────────────────────────────────
    /// <summary>Enable mouse wheel zoom on the chart.</summary>
    [Parameter] public bool EnableZoom { get; set; }

    /// <summary>Enable click-drag pan on the chart.</summary>
    [Parameter] public bool EnablePan { get; set; }

    /// <summary>Zoom mode: "x" (horizontal only), "y" (vertical only), "xy" (both).</summary>
    [Parameter] public string ZoomMode { get; set; } = "x";

    // ── Crosshair ──────────────────────────────────
    /// <summary>Whether to show a vertical crosshair line following the cursor.</summary>
    [Parameter] public bool ShowCrosshair { get; set; }

    // ── Annotations ─────────────────────────────────
    /// <summary>Annotations to display on the chart (markers, labels at specific data points).</summary>
    [Parameter] public List<ChartAnnotation>? Annotations { get; set; }

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
    private protected ChartLayoutEngine LayoutEngine { get; } = new();
    private protected ChartInteropService? Interop { get; set; }
    protected ElementReference ContainerRef;
    private bool _disposed;

    protected string EffectiveGridColor => GridColor ?? "var(--arcadia-color-border, #e2e8f0)";

    protected bool HasData => Data is not null && Data.Count > 0;

    protected override void OnInitialized()
    {
        Interop = new ChartInteropService(JSRuntime);
    }

    private DotNetObjectReference<ResizeCallbackHandler>? _resizeRef;

    /// <summary>Whether the chart is in responsive mode (Width=0).</summary>
    protected bool IsResponsive => Width <= 0;

    /// <summary>The actual rendered width (either fixed or measured from container).</summary>
    protected double EffectiveWidth => _measuredWidth > 0 ? _measuredWidth : (Width > 0 ? Width : 600);

    private double _measuredWidth;

    private DotNetObjectReference<PanZoomCallbackHandler>? _panZoomRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_disposed) return;
        if (firstRender && Interop is not null)
        {
            if (IsResponsive)
            {
                _resizeRef = DotNetObjectReference.Create(new ResizeCallbackHandler(this));
                await Interop.ObserveResizeAsync(ContainerRef, _resizeRef);
            }

            if (EnableZoom || EnablePan)
            {
                _panZoomRef = DotNetObjectReference.Create(new PanZoomCallbackHandler(this));
                await Interop.EnablePanZoomAsync(ContainerRef, _panZoomRef, ZoomMode);
            }
        }
    }

    private void OnResized(double width, double height)
    {
        if (Math.Abs(width - _measuredWidth) > 1)
        {
            _measuredWidth = width;
            InvokeAsync(() =>
            {
                OnParametersSet(); // Recalculate layout with new EffectiveWidth
                StateHasChanged();
            });
        }
    }

    private class ResizeCallbackHandler : IResizeHandler
    {
        private readonly ChartBase<T> _chart;
        public ResizeCallbackHandler(ChartBase<T> chart) => _chart = chart;

        [Microsoft.JSInterop.JSInvokable]
        public Task OnContainerResized(double width, double height)
        {
            _chart.OnResized(width, height);
            return Task.CompletedTask;
        }
    }

    /// <summary>Current zoom level (1.0 = 100%).</summary>
    protected double ZoomLevel { get; private set; } = 1.0;

    /// <summary>Current pan offset X in pixels.</summary>
    protected double PanOffsetX { get; private set; }

    /// <summary>Current pan offset Y in pixels.</summary>
    protected double PanOffsetY { get; private set; }

    /// <summary>SVG transform string for pan/zoom. Apply to a group wrapping chart content.</summary>
    protected string PanZoomTransform =>
        (EnableZoom || EnablePan) && (ZoomLevel != 1.0 || PanOffsetX != 0 || PanOffsetY != 0)
            ? $"translate({PanOffsetX.ToString("F1")},{PanOffsetY.ToString("F1")}) scale({ZoomLevel.ToString("F3")})"
            : "";

    private class PanZoomCallbackHandler : IPanZoomHandler
    {
        private readonly ChartBase<T> _chart;
        public PanZoomCallbackHandler(ChartBase<T> chart) => _chart = chart;

        [Microsoft.JSInterop.JSInvokable]
        public Task OnZoomChanged(double zoom, double centerX, double centerY)
        {
            _chart.ZoomLevel = zoom;
            _chart.InvokeAsync(_chart.StateHasChanged);
            return Task.CompletedTask;
        }

        [Microsoft.JSInterop.JSInvokable]
        public Task OnPanChanged(double offsetX, double offsetY)
        {
            _chart.PanOffsetX = offsetX;
            _chart.PanOffsetY = offsetY;
            _chart.InvokeAsync(_chart.StateHasChanged);
            return Task.CompletedTask;
        }
    }

    /// <summary>Shows a default tooltip for a data point.</summary>
    protected async Task ShowTooltipForPoint(string seriesName, double value, double mouseX, double mouseY)
    {
        if (_disposed || Interop is null) return;
        try
        {
            var formatted = FormatValue(value, YAxisFormatString ?? DataLabelFormatString);
            var html = $"<div style='font-weight:600;margin-bottom:2px'>{seriesName}</div>" +
                       $"<div>{formatted}</div>";
            await Interop.ShowTooltipAsync(html, mouseX, mouseY);
        }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { }
#endif
        catch (ObjectDisposedException) { }
    }

    /// <summary>Hides the tooltip.</summary>
    protected async Task HideTooltipAction()
    {
        if (_disposed || Interop is null) return;
        try
        {
            await Interop.HideTooltipAsync();
        }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { }
#endif
        catch (ObjectDisposedException) { }
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
                "primary" => "var(--arcadia-color-primary, #2563eb)",
                "secondary" => "var(--arcadia-color-secondary, #7c3aed)",
                "success" => "var(--arcadia-color-success, #16a34a)",
                "danger" => "var(--arcadia-color-danger, #dc2626)",
                "warning" => "var(--arcadia-color-warning, #d97706)",
                "info" => "var(--arcadia-color-info, #0284c7)",
                _ => color
            };
        }
        return EffectivePalette.GetColor(seriesIndex);
    }

    /// <summary>Formats a value for screen reader tables, replacing NaN with "—".</summary>
    protected static string FormatSrValue(double value) =>
        double.IsNaN(value) || double.IsInfinity(value) ? "—" : value.ToString("G4");

    protected string FormatValue(double value, string? formatString)
    {
        if (formatString is not null)
            return value.ToString(formatString, FormatProvider);
        return value.ToString("G4");
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        try
        {
            if (IsResponsive && Interop is not null)
                await Interop.UnobserveResizeAsync(ContainerRef);
        }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { }
#endif
        catch (ObjectDisposedException) { }

        if (Interop is not null)
            await Interop.DisposeAsync();

        _resizeRef?.Dispose();
        _panZoomRef?.Dispose();

        GC.SuppressFinalize(this);
    }
}

using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core.Layout;
using Arcadia.Charts.Core.Scales;

namespace Arcadia.Charts.Core;

/// <summary>
/// Base class for all chart components. Provides common parameters,
/// layout engine integration, and SVG container rendering.
/// </summary>
public abstract class ChartBase<T> : Arcadia.Core.Base.ArcadiaComponentBase, IAsyncDisposable
{
    // ── Data ──────────────────────────────────────────────
    /// <summary>Collection of data items to plot. Each item maps to one or more chart points via SeriesConfig field accessors. When null or empty, the chart renders the NoDataMessage placeholder.</summary>
    /// <example>
    /// <code>
    /// &lt;ArcadiaLineChart Data="@sales"
    ///     XField="@(s =&gt; s.Month)"
    ///     YField="@(s =&gt; s.Revenue)" /&gt;
    /// </code>
    /// </example>
    [Parameter] public IReadOnlyList<T>? Data { get; set; }

    // ── Dimensions ───────────────────────────────────────
    /// <summary>Chart height in pixels. The SVG viewBox scales to this height. Default is 300.</summary>
    [Parameter] public double Height { get; set; } = 300;

    /// <summary>Chart width in pixels. 0 = responsive (fills container via width="100%" and ResizeObserver). Default is 0 (responsive).</summary>
    /// <remarks>0 = responsive (fills container). Non-zero = fixed pixel width. Responsive mode uses ResizeObserver via JS interop.</remarks>
    [Parameter] public double Width { get; set; } = 0;

    // ── Titles ───────────────────────────────────────────
    /// <summary>Primary title displayed above the chart area. When set, the layout engine reserves top margin space automatically.</summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>Secondary text rendered below the Title in a smaller font. Only visible when Title is also set. Useful for date ranges or data source attribution.</summary>
    [Parameter] public string? Subtitle { get; set; }

    // ── Axes ─────────────────────────────────────────────
    /// <summary>Label below the X-axis describing the horizontal dimension (e.g., "Month", "Date"). Reserves additional bottom margin when set.</summary>
    [Parameter] public string? XAxisTitle { get; set; }

    /// <summary>Label alongside the primary Y-axis describing the vertical dimension (e.g., "Revenue ($)"). Reserves additional left margin when set.</summary>
    [Parameter] public string? YAxisTitle { get; set; }

    /// <summary>Manual Y-axis minimum. When null, auto-calculated from data with zero baseline included. Set to override (e.g., negative ranges).</summary>
    [Parameter] public double? YAxisMin { get; set; }

    /// <summary>Manual Y-axis maximum. When null, auto-calculated from data with 5% padding. Set to fix a consistent upper bound across chart updates.</summary>
    [Parameter] public double? YAxisMax { get; set; }

    /// <summary>Y-axis scale type: "linear" (default) or "log" (logarithmic). Log scale requires positive values.</summary>
    [Parameter] public string YAxisType { get; set; } = "linear";

    /// <summary>Whether the Y-axis is logarithmic.</summary>
    protected bool IsLogScale => string.Equals(YAxisType, "log", StringComparison.OrdinalIgnoreCase);

    /// <summary>Upper limit on the number of Y-axis tick marks. The tick generator may produce fewer for narrow ranges. Default is 8.</summary>
    [Parameter] public int YAxisMaxTicks { get; set; } = 8;

    /// <summary>Upper limit on the number of X-axis tick marks. The tick generator may produce fewer when data has few points. Default is 12.</summary>
    [Parameter] public int XAxisMaxTicks { get; set; } = 12;

    /// <summary>Format string for Y-axis tick labels (e.g., "C0" for currency, "P1" for percentage, "N2" for two decimal places). Uses FormatProvider for culture-specific formatting.</summary>
    [Parameter] public string? YAxisFormatString { get; set; }

    /// <summary>Format string for X-axis tick labels (e.g., "MMM yyyy" for dates). Uses FormatProvider for culture-specific formatting.</summary>
    [Parameter] public string? XAxisFormatString { get; set; }

    // ── Secondary Y-Axis ──────────────────────────────────
    /// <summary>Label alongside the secondary (right-side) Y-axis. Only rendered when at least one series targets YAxisIndex = 1.</summary>
    [Parameter] public string? YAxis2Title { get; set; }

    /// <summary>Manual secondary Y-axis minimum. When null, auto-calculated from data of series with YAxisIndex = 1.</summary>
    [Parameter] public double? YAxis2Min { get; set; }

    /// <summary>Manual secondary Y-axis maximum. When null, auto-calculated from data of series with YAxisIndex = 1.</summary>
    [Parameter] public double? YAxis2Max { get; set; }

    /// <summary>Format string for secondary Y-axis tick labels (e.g., "P1" for percentage when paired with a rate series). Uses FormatProvider for culture-specific formatting.</summary>
    [Parameter] public string? YAxis2FormatString { get; set; }

    /// <summary>Upper limit on secondary Y-axis tick marks. Only rendered when at least one series targets YAxisIndex = 1. Default is 8.</summary>
    [Parameter] public int YAxis2MaxTicks { get; set; } = 8;

    // ── Grid ─────────────────────────────────────────────
    /// <summary>Controls visibility of background grid lines. When true, lines are drawn per ShowHorizontalGrid and ShowVerticalGrid. Default is true.</summary>
    [Parameter] public bool ShowGrid { get; set; } = true;

    /// <summary>Grid line color. Null = theme border color.</summary>
    [Parameter] public string? GridColor { get; set; }

    /// <summary>Grid line dash pattern (e.g., "4,4"). Empty = solid.</summary>
    [Parameter] public string GridDash { get; set; } = "4,4";

    /// <summary>Grid line opacity. Default 0.12.</summary>
    [Parameter] public double GridOpacity { get; set; } = 0.12;

    /// <summary>Axis line opacity. Default 0.2.</summary>
    [Parameter] public double AxisLineOpacity { get; set; } = 0.2;

    /// <summary>Draw horizontal grid lines across the plot area. Only takes effect when ShowGrid is true. Default is true.</summary>
    [Parameter] public bool ShowHorizontalGrid { get; set; } = true;

    /// <summary>Draw vertical grid lines across the plot area. Only takes effect when ShowGrid is true. Default is false.</summary>
    [Parameter] public bool ShowVerticalGrid { get; set; } = false;

    // ── Legend ────────────────────────────────────────────
    /// <summary>Show the series legend. Clicking a legend entry toggles that series' visibility. Position is set via LegendPosition. Default is true.</summary>
    [Parameter] public bool ShowLegend { get; set; } = true;

    /// <summary>Legend position: "top", "bottom", "left", "right".</summary>
    [Parameter] public string LegendPosition { get; set; } = "bottom";

    // ── Appearance ───────────────────────────────────────
    /// <summary>Color palette for auto-assigning series colors. When null, ChartPalette.Default is used. Provide a custom palette to match your brand (e.g., ChartPalette.Accessible for color-blind safety).</summary>
    /// <remarks>Only applies when series don't have explicit Color values. 7 built-in palettes: Default, Cool, Warm, Pastel, Vibrant, Monochrome, Accessible.</remarks>
    [Parameter] public ChartPalette? Palette { get; set; }

    /// <summary>Show the PNG/SVG export toolbar on hover. Default is false to avoid toolbar spam on dashboards with many charts.</summary>
    [Parameter] public bool ShowToolbar { get; set; } = false;

    /// <summary>Play an entrance animation when the chart first renders. Set to false for instant rendering on prerendered pages or dashboards with many charts. Default is true.</summary>
    [Parameter] public bool AnimateOnLoad { get; set; } = true;

    /// <summary>Duration of the entrance animation in milliseconds. Only applies when AnimateOnLoad is true. Default is 800.</summary>
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

    // ── Points ─────────────────────────────────────────
    /// <summary>Radius of data point marker circles in pixels. Can be overridden per-series via SeriesConfig.PointRadius. Default is 3.</summary>
    [Parameter] public double PointRadius { get; set; } = 3;

    // ── Data labels ──────────────────────────────────────
    /// <summary>Render numeric value labels on each data point or bar segment. Formatted using DataLabelFormatString when set. Can cause clutter on dense charts. Default is false.</summary>
    /// <remarks>Uses DataLabelFormatString when set, otherwise N0 for whole numbers and N2 for decimals.</remarks>
    [Parameter] public bool ShowDataLabels { get; set; }

    /// <summary>Format string for data label values (e.g., "C0" for currency, "P1" for percentage). When null, values render with G4 general format.</summary>
    [Parameter] public string? DataLabelFormatString { get; set; }

    // ── Pan/Zoom ─────────────────────────────────────
    /// <summary>Enable mouse-wheel zooming on the chart canvas. Zoom direction is controlled by ZoomMode. Requires JS interop. Default is false.</summary>
    /// <remarks>ZoomMode controls direction: "x", "y", "xy", or "selection" for rubber band zoom.</remarks>
    [Parameter] public bool EnableZoom { get; set; }

    /// <summary>Enable click-and-drag panning on the chart canvas. Works alongside zoom. Requires JS interop. Default is false.</summary>
    [Parameter] public bool EnablePan { get; set; }

    /// <summary>Zoom mode: "x" (horizontal only), "y" (vertical only), "xy" (both), "selection" (rubber band selection zoom).</summary>
    [Parameter] public string ZoomMode { get; set; } = "x";

    /// <summary>Print the chart at high resolution. Opens browser print dialog with print-optimized CSS. Requires JS interop.</summary>
    [Parameter] public EventCallback OnPrint { get; set; }

    /// <summary>Synchronization group name. Charts in the same group share crosshair position and zoom level. Null = no sync.</summary>
    /// <example>
    /// <code>
    /// &lt;ArcadiaLineChart Data="@data" SyncGroup="dashboard"
    ///     XField="@(d =&gt; d.Date)" YField="@(d =&gt; d.Revenue)" /&gt;
    /// &lt;ArcadiaBarChart Data="@data" SyncGroup="dashboard"
    ///     XField="@(d =&gt; d.Date)" YField="@(d =&gt; d.Orders)" /&gt;
    /// </code>
    /// </example>
    [Parameter] public string? SyncGroup { get; set; }

    // ── Crosshair ──────────────────────────────────
    /// <summary>Draw a vertical crosshair line that tracks the cursor, making it easier to align data points across series. Default is false.</summary>
    [Parameter] public bool ShowCrosshair { get; set; }

    // ── Annotations ─────────────────────────────────
    /// <summary>Annotations to display on the chart (markers, labels at specific data points).</summary>
    [Parameter] public List<ChartAnnotation>? Annotations { get; set; }

    // ── Events ───────────────────────────────────────────
    /// <summary>Fired when a data point is clicked. Receives item, index, and series context.</summary>
    [Parameter] public EventCallback<PointClickEventArgs<T>> OnPointClick { get; set; }

    /// <summary>Fired when a series is clicked. Receives the series index.</summary>
    [Parameter] public EventCallback<int> OnSeriesClick { get; set; }

    // ── States ───────────────────────────────────────────
    /// <summary>When true, renders a skeleton shimmer placeholder instead of the chart SVG. Set while awaiting async data, then set back to false once Data is populated.</summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>Message to show when Data is null or empty.</summary>
    [Parameter] public string? NoDataMessage { get; set; } = "No data available";

    // ── Accessibility ────────────────────────────────────
    /// <summary>ARIA label for the chart SVG element. Provide a meaningful summary (e.g., "Monthly revenue trend 2025") for WCAG compliance. When null, a generic label is generated from Title.</summary>
    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>IFormatProvider (e.g., CultureInfo) for locale-specific number and date formatting in axis labels, tooltips, and data labels. When null, invariant formatting is used.</summary>
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
    private CollectionObserver<T>? _collectionObserver;

    protected string EffectiveGridColor => GridColor ?? "var(--arcadia-color-border, #e2e8f0)";

    protected bool HasData => Data is not null && Data.Count > 0;

    /// <summary>Whether any series is assigned to the secondary Y-axis.</summary>
    protected bool HasSecondaryYAxis { get; set; }

    protected override void OnInitialized()
    {
        Interop = new ChartInteropService(JSRuntime);
    }

    protected override void OnParametersSet()
    {
        _collectionObserver ??= new CollectionObserver<T>(
            () => { OnParametersSet(); StateHasChanged(); return Task.CompletedTask; },
            InvokeAsync
        );
        _collectionObserver.Attach(Data);
    }

    private DotNetObjectReference<ResizeCallbackHandler>? _resizeRef;

    /// <summary>Whether the chart is in responsive mode (Width=0).</summary>
    protected bool IsResponsive => Width <= 0;

    /// <summary>The actual rendered width used for layout calculations.</summary>
    protected double EffectiveWidth => Width > 0 ? Width : (_measuredWidth > 0 ? _measuredWidth : 600);

    /// <summary>The SVG width attribute — "100%" when responsive and unmeasured, pixel value otherwise.</summary>
    protected string SvgWidth => IsResponsive && _measuredWidth <= 0 ? "100%" : EffectiveWidth.ToString("F0");

    /// <summary>Always false — skeleton approach removed in favor of width="100%" scaling.
    /// Kept for API compatibility with chart razor files.</summary>
    protected bool AwaitingMeasurement => false;

    /// <summary>Gets the container div style — adds width:100% and min-height when awaiting measurement.</summary>
    protected string GetContainerStyle() =>
        AwaitingMeasurement ? $"width:100%;min-height:{Height.ToString("F0")}px;{Style}" : Style ?? "";

    private double _measuredWidth;

    private DotNetObjectReference<PanZoomCallbackHandler>? _panZoomRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_disposed) return;
        if (firstRender && Interop is not null)
        {
            try
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
            catch (Microsoft.JSInterop.JSException) { }
#if NET6_0_OR_GREATER
            catch (JSDisconnectedException) { }
#endif
        }
    }

    private void OnResized(double width, double height)
    {
        if (_disposed) return; // guard against callback after disposal
        if (Math.Abs(width - _measuredWidth) > 1)
        {
            _measuredWidth = width;
            InvokeAsync(() =>
            {
                if (_disposed) return;
                OnParametersSet();
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

    // ── Custom tooltip state ────────────────────────────
    private T? _hoveredItem;
    private double _tooltipX;
    private double _tooltipY;
    private bool _tooltipVisible;

    /// <summary>Shows the custom tooltip template for a data item, or falls back to JS tooltip.</summary>
    protected async Task ShowTooltipOrTemplate(T item, string fallbackHtml, double mouseX, double mouseY)
    {
        if (_disposed) return;
        if (TooltipTemplate is not null)
        {
            _hoveredItem = item;
            _tooltipX = mouseX;
            _tooltipY = mouseY;
            _tooltipVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        else if (Interop is not null)
        {
            try { await Interop.ShowTooltipAsync(fallbackHtml, mouseX, mouseY); }
#if NET6_0_OR_GREATER
            catch (JSDisconnectedException) { }
#endif
            catch (ObjectDisposedException) { }
        }
    }

    /// <summary>Hides the custom tooltip or JS tooltip.</summary>
    protected async Task HideTooltipOrTemplate()
    {
        if (_disposed) return;
        if (TooltipTemplate is not null)
        {
            _tooltipVisible = false;
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            await HideTooltipAction();
        }
    }

    /// <summary>Renders the custom tooltip portal div if TooltipTemplate is set and an item is hovered.</summary>
    protected RenderFragment? RenderTooltipTemplate => _tooltipVisible && _hoveredItem is not null && TooltipTemplate is not null
        ? builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "arcadia-tooltip arcadia-tooltip--custom");
            builder.AddAttribute(2, "style", $"position:fixed;left:{_tooltipX + 12}px;top:{_tooltipY - 8}px;z-index:9999;pointer-events:none;");
            builder.AddContent(3, TooltipTemplate(_hoveredItem));
            builder.CloseElement();
        }
        : null;

    /// <summary>Creates a Y-axis scale appropriate for the current YAxisType.</summary>
    private protected LinearScale CreateYScale(double domainMin, double domainMax, double rangeMin, double rangeMax)
    {
        if (IsLogScale)
        {
            var logScale = new Scales.LogarithmicScale(domainMin, domainMax, rangeMin, rangeMax);
            // Wrap in a LinearScale-compatible adapter using log transform
            return new LogScaleAdapter(logScale, rangeMin, rangeMax);
        }
        return new LinearScale(domainMin, domainMax, rangeMin, rangeMax);
    }

    /// <summary>Generates Y-axis ticks appropriate for the current YAxisType.</summary>
    private protected List<double> CreateYTicks(double domainMin, double domainMax)
    {
        if (IsLogScale)
            return Scales.LogarithmicScale.GenerateTicks(domainMin, domainMax, YAxisMaxTicks);
        return TickGenerator.GenerateNumericTicks(domainMin, domainMax, YAxisMaxTicks).ToList();
    }

    /// <summary>Adapter that makes LogarithmicScale usable where LinearScale is expected.</summary>
    private protected class LogScaleAdapter : LinearScale
    {
        private readonly LogarithmicScale _logScale;

        public LogScaleAdapter(LogarithmicScale logScale, double rangeMin, double rangeMax)
            : base(logScale.DomainMin, logScale.DomainMax, rangeMin, rangeMax)
        {
            _logScale = logScale;
        }

        public override double Scale(double value) => _logScale.Scale(value);
        public override double Invert(double pixel) => _logScale.Invert(pixel);
    }

    /// <summary>Maximum total stagger duration in milliseconds. Per-element delay is reduced when element count is high so total animation never exceeds this cap. Default: 2000ms.</summary>
    private const int MaxStaggerMs = 2000;

    /// <summary>Above this element count, per-element stagger animations are skipped entirely (only the line-draw / area-fade plays).</summary>
    private const int StaggerDisableThreshold = 200;

    /// <summary>Calculates a capped animation delay for a specific element index. Returns 0 if element count exceeds the stagger threshold.</summary>
    /// <param name="index">Zero-based index of the element.</param>
    /// <param name="totalCount">Total number of elements to animate.</param>
    /// <param name="baseDelayMs">Desired per-element delay in ms for small datasets.</param>
    protected static int GetAnimationDelay(int index, int totalCount, int baseDelayMs)
    {
        if (totalCount >= StaggerDisableThreshold)
            return 0;
        // Cap so total stagger never exceeds MaxStaggerMs
        var effectiveDelay = Math.Min(baseDelayMs, MaxStaggerMs / Math.Max(totalCount, 1));
        return index * effectiveDelay;
    }

    /// <summary>Returns true if per-element stagger animations should be applied. False when data count is too large.</summary>
    protected static bool ShouldStagger(int totalCount) => totalCount < StaggerDisableThreshold;

    /// <summary>Formats a value for screen reader tables, replacing NaN with "—".</summary>
    protected static string FormatSrValue(double value) =>
        double.IsNaN(value) || double.IsInfinity(value) ? "—" : value.ToString("G4");

    /// <summary>Builds a user-friendly message listing which required fields are missing.</summary>
    protected static string GetMissingFieldsMessage(string chartName, string requiredList, (string Name, bool IsMissing)[] fields)
    {
        var missing = fields.Where(f => f.IsMissing).Select(f => f.Name).ToList();
        var verb = missing.Count == 1 ? "is" : "are";
        return $"{chartName} requires {requiredList}. {string.Join(", ", missing)} {verb} not set.";
    }

    protected string FormatValue(double value, string? formatString)
    {
        if (formatString is not null)
            return value.ToString(formatString, FormatProvider);

        // Avoid scientific notation (G4 produces "3.5E+2" for 350)
        // Use compact formatting: no decimals for whole numbers, up to 2 for fractional
        if (value == Math.Floor(value) && Math.Abs(value) < 1_000_000_000)
            return value.ToString("N0", FormatProvider);
        return value.ToString("N2", FormatProvider);
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        try
        {
            if (IsResponsive && Interop is not null)
                await Interop.UnobserveResizeAsync(ContainerRef);
            if ((EnableZoom || EnablePan) && Interop is not null)
                await Interop.DisablePanZoomAsync(ContainerRef);
        }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { }
#endif
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { } // JS interop not available during SSR prerendering

        try
        {
            if (Interop is not null)
                await Interop.DisposeAsync();
        }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { }
#endif
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }

        _collectionObserver?.Dispose();
        _resizeRef?.Dispose();
        _panZoomRef?.Dispose();

        GC.SuppressFinalize(this);
    }
}

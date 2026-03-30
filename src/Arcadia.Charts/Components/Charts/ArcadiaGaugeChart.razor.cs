using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;

namespace Arcadia.Charts.Components.Charts;

/// <summary>
/// Renders a radial gauge chart for KPI dashboards.
/// Displays a value as an arc with optional color thresholds, needle pointer,
/// tick marks, gradient fills, colored range bands, and interactive editing.
/// </summary>
public partial class ArcadiaGaugeChart : Arcadia.Core.Base.ArcadiaComponentBase
{
    // ── Core Parameters ──

    /// <summary>The current gauge value.</summary>
    [Parameter] public double Value { get; set; }

    /// <summary>Minimum value of the gauge range.</summary>
    [Parameter] public double Min { get; set; } = 0;

    /// <summary>Maximum value of the gauge range.</summary>
    [Parameter] public double Max { get; set; } = 100;

    /// <summary>Chart height in pixels.</summary>
    [Parameter] public double Height { get; set; } = 200;

    /// <summary>Chart width in pixels.</summary>
    [Parameter] public double Width { get; set; } = 300;

    /// <summary>Chart title.</summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>Label text displayed below the value.</summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>Threshold boundaries that change the gauge color.</summary>
    [Parameter] public List<GaugeThreshold>? Thresholds { get; set; }

    /// <summary>Default color when no threshold is matched.</summary>
    [Parameter] public string Color { get; set; } = "var(--arcadia-color-primary, #2563eb)";

    /// <summary>Background track color.</summary>
    [Parameter] public string TrackColor { get; set; } = "var(--arcadia-color-border, #e2e8f0)";

    /// <summary>Stroke width of the gauge arc.</summary>
    [Parameter] public double StrokeWidth { get; set; } = 20;

    /// <summary>Whether to render as a full circle (360) or semi-circle (180).</summary>
    [Parameter] public bool FullCircle { get; set; } = false;

    /// <summary>Format string for the center value display.</summary>
    [Parameter] public string? ValueFormatString { get; set; }

    /// <summary>Accessible description for screen readers.</summary>
    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>Opacity of the background track arc (0.0 fully transparent, 1.0 fully opaque).</summary>
    [Parameter] public double TrackOpacity { get; set; } = 0.3;

    /// <summary>Whether to animate on load.</summary>
    [Parameter] public bool AnimateOnLoad { get; set; } = true;

    /// <summary>Duration of the gauge fill animation in milliseconds. Only applies when AnimateOnLoad is true. Default is 800.</summary>
    [Parameter] public int AnimationDuration { get; set; } = 800;

    // ── Angle Configuration ──

    /// <summary>Start angle in degrees. Overrides the default derived from FullCircle. For semi-circle default is 180°, for full circle default is -90°.</summary>
    [Parameter] public double? StartAngle { get; set; }

    /// <summary>End angle in degrees. Overrides the default derived from FullCircle. For semi-circle default is 0° (360°), for full circle default is 270°.</summary>
    [Parameter] public double? EndAngle { get; set; }

    // ── Arc Appearance ──

    /// <summary>Use rounded stroke-linecap on arc ends. Default: true.</summary>
    [Parameter] public bool RoundedCaps { get; set; } = true;

    /// <summary>Gradient colors for the value arc. When set (2+ colors), overrides Color/Thresholds with a linear gradient.</summary>
    [Parameter] public List<string>? GradientColors { get; set; }

    // ── Ranges (colored bands) ──

    /// <summary>Colored range bands on the arc. Each range has Start, End, Color, Width, Opacity.</summary>
    [Parameter] public List<GaugeRange>? Ranges { get; set; }

    // ── Needle ──

    /// <summary>Show a needle pointer at the current value. Default: false.</summary>
    [Parameter] public bool ShowNeedle { get; set; }

    /// <summary>Needle color. Default: var(--arcadia-color-text, #1e293b).</summary>
    [Parameter] public string NeedleColor { get; set; } = "var(--arcadia-color-text, #1e293b)";

    /// <summary>Needle base width in pixels. Default: 8.</summary>
    [Parameter] public double NeedleBaseWidth { get; set; } = 8;

    /// <summary>Needle cap circle radius. Default: 6.</summary>
    [Parameter] public double NeedleCapRadius { get; set; } = 6;

    // ── Tick Marks ──

    /// <summary>Show tick marks on the scale. Default: false.</summary>
    [Parameter] public bool ShowTicks { get; set; }

    /// <summary>Number of major tick intervals. Default: 5.</summary>
    [Parameter] public int MajorTickCount { get; set; } = 5;

    /// <summary>Number of minor ticks between each major tick. Default: 4.</summary>
    [Parameter] public int MinorTickCount { get; set; } = 4;

    /// <summary>Tick mark color. Default: var(--arcadia-color-border, #cbd5e1).</summary>
    [Parameter] public string TickColor { get; set; } = "var(--arcadia-color-border, #cbd5e1)";

    /// <summary>Show numeric labels at major ticks. Default: false.</summary>
    [Parameter] public bool ShowTickLabels { get; set; }

    /// <summary>Tick label color. Default: var(--arcadia-color-text-secondary, #64748b).</summary>
    [Parameter] public string TickLabelColor { get; set; } = "var(--arcadia-color-text-secondary, #64748b)";

    /// <summary>Tick label font size. Default: 10.</summary>
    [Parameter] public double TickLabelFontSize { get; set; } = 10;

    // ── Value Display ──

    /// <summary>Show the value number in the center. Default: true.</summary>
    [Parameter] public bool ShowValue { get; set; } = true;

    /// <summary>Value text font size. Default: 0 (uses CSS class sizing).</summary>
    [Parameter] public double ValueFontSize { get; set; }

    /// <summary>Value text color. Overrides the CSS class color when set.</summary>
    [Parameter] public string? ValueColor { get; set; }

    /// <summary>Label text font size. Default: 0 (uses CSS class sizing).</summary>
    [Parameter] public double LabelFontSize { get; set; }

    /// <summary>Label text color. Overrides the CSS class color when set.</summary>
    [Parameter] public string? LabelColor { get; set; }

    /// <summary>Custom center content template. Overrides default value/label display when set.</summary>
    [Parameter] public RenderFragment? CenterTemplate { get; set; }

    // ── Interactivity ──

    /// <summary>Allow the user to click on the gauge to change the value. Default: false.</summary>
    [Parameter] public bool Editable { get; set; }

    /// <summary>Callback fired when the value changes (via click in editable mode).</summary>
    [Parameter] public EventCallback<double> ValueChanged { get; set; }

    // ── Internal State ──

    private double _cx;
    private double _pathLength;
    private double _cy;
    private double _radius;
    private double _startAngleRad;
    private double _endAngleRad;
    private double _valueAngleRad;
    private string _trackPath = "";
    private string _valuePath = "";
    private string _valueColor = "";
    private string _displayValue = "";
    private string _gradientId = $"gauge-grad-{Guid.NewGuid():N}";
    private List<TickInfo> _ticks = new();
    private List<TickLabelInfo> _tickLabels = new();

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        var clampedValue = Math.Max(Min, Math.Min(Max, Value));
        var ratio = (Max - Min) > 0 ? (clampedValue - Min) / (Max - Min) : 0;

        var padding = StrokeWidth / 2 + (ShowTicks ? 20 : 0) + (ShowTickLabels ? 15 : 0) + 10;
        _cx = Width / 2;

        if (FullCircle)
        {
            _cy = Height / 2;
            _radius = Math.Min(Width, Height) / 2 - padding;
        }
        else
        {
            // Semi-circle: position center at bottom of area
            _cy = Height - StrokeWidth - 20;
            _radius = Math.Min(Width / 2, Height - StrokeWidth - 20) - 10 - (ShowTicks ? 15 : 0) - (ShowTickLabels ? 10 : 0);
        }

        if (_radius < 10) _radius = 10;

        // Build arc angles — support custom StartAngle/EndAngle (in degrees) or defaults
        double startAngle, endAngle;
        if (StartAngle.HasValue || EndAngle.HasValue)
        {
            // User-specified angles in degrees → convert to radians
            var startDeg = StartAngle ?? (FullCircle ? -90.0 : 180.0);
            var endDeg = EndAngle ?? (FullCircle ? 270.0 : 360.0);
            startAngle = startDeg * Math.PI / 180;
            endAngle = endDeg * Math.PI / 180;
        }
        else
        {
            // Legacy defaults matching original behavior
            startAngle = FullCircle ? -Math.PI / 2 : Math.PI;
            endAngle = FullCircle ? -Math.PI / 2 + Math.PI * 2 : Math.PI + Math.PI;
        }

        _startAngleRad = startAngle;
        _endAngleRad = endAngle;

        var totalSweep = endAngle - startAngle;
        var endAngleValue = startAngle + totalSweep * ratio;
        _valueAngleRad = endAngleValue;

        var isFullCircleArc = Math.Abs(totalSweep) >= Math.PI * 2 - 0.001;
        _trackPath = BuildArc(_cx, _cy, _radius, startAngle, endAngle, isFullCircleArc);
        _valuePath = ratio > 0.001 ? BuildArc(_cx, _cy, _radius, startAngle, endAngleValue, ratio > 0.999 && isFullCircleArc) : "";
        _pathLength = Math.Abs(endAngleValue - startAngle) * _radius;

        // Determine color from thresholds
        _valueColor = ResolveThresholdColor(clampedValue);

        // Format display value
        _displayValue = ValueFormatString is not null
            ? clampedValue.ToString(ValueFormatString)
            : clampedValue.ToString("G");

        // Build ticks
        BuildTicks();
    }

    private string ResolveThresholdColor(double value)
    {
        if (Thresholds is null || Thresholds.Count == 0)
            return ResolveColorName(Color);

        // Sort thresholds descending and find the first one the value meets or exceeds
        var sorted = Thresholds.OrderByDescending(t => t.Value).ToList();
        foreach (var threshold in sorted)
        {
            if (value >= threshold.Value)
                return ResolveColorName(threshold.Color);
        }

        return ResolveColorName(Color);
    }

    private static string ResolveColorName(string color)
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

    private static string BuildArc(double cx, double cy, double r, double startAngle, double endAngle, bool forceFullCircle)
    {
        if (forceFullCircle)
        {
            // SVG cannot draw a full circle with a single arc; use two half-arcs
            var midAngle = startAngle + Math.PI;
            var x1 = cx + Math.Cos(startAngle) * r;
            var y1 = cy + Math.Sin(startAngle) * r;
            var xMid = cx + Math.Cos(midAngle) * r;
            var yMid = cy + Math.Sin(midAngle) * r;
            return $"M{F(x1)},{F(y1)} A{F(r)},{F(r)} 0 1,1 {F(xMid)},{F(yMid)} A{F(r)},{F(r)} 0 1,1 {F(x1)},{F(y1)}";
        }

        var sx = cx + Math.Cos(startAngle) * r;
        var sy = cy + Math.Sin(startAngle) * r;
        var ex = cx + Math.Cos(endAngle) * r;
        var ey = cy + Math.Sin(endAngle) * r;
        var largeArc = (endAngle - startAngle) > Math.PI ? 1 : 0;
        return $"M{F(sx)},{F(sy)} A{F(r)},{F(r)} 0 {largeArc},1 {F(ex)},{F(ey)}";
    }

    private string BuildRangeArc(double fromValue, double toValue)
    {
        var totalSweep = _endAngleRad - _startAngleRad;
        var frac1 = Math.Clamp((fromValue - Min) / (Max - Min), 0, 1);
        var frac2 = Math.Clamp((toValue - Min) / (Max - Min), 0, 1);
        var a1 = _startAngleRad + frac1 * totalSweep;
        var a2 = _startAngleRad + frac2 * totalSweep;
        return BuildArc(_cx, _cy, _radius, a1, a2, false);
    }

    private (double X, double Y) PointOnArc(double angle, double radius)
    {
        return (_cx + radius * Math.Cos(angle), _cy + radius * Math.Sin(angle));
    }

    private void BuildTicks()
    {
        _ticks.Clear();
        _tickLabels.Clear();
        if (!ShowTicks && !ShowTickLabels) return;

        var totalSweep = _endAngleRad - _startAngleRad;
        var totalSegments = MajorTickCount * (MinorTickCount + 1);

        for (var i = 0; i <= totalSegments; i++)
        {
            var frac = (double)i / totalSegments;
            var angle = _startAngleRad + frac * totalSweep;
            var isMajor = i % (MinorTickCount + 1) == 0;
            var outerR = _radius + StrokeWidth / 2 + 2;
            var innerR = outerR + (isMajor ? 8 : 4);

            if (ShowTicks)
            {
                var outer = PointOnArc(angle, outerR);
                var inner = PointOnArc(angle, innerR);
                _ticks.Add(new TickInfo(outer.X, outer.Y, inner.X, inner.Y, isMajor));
            }

            if (ShowTickLabels && isMajor)
            {
                var labelR = innerR + 10;
                var pos = PointOnArc(angle, labelR);
                var val = Min + frac * (Max - Min);
                _tickLabels.Add(new TickLabelInfo(pos.X, pos.Y, FormatTickValue(val)));
            }
        }
    }

    private string FormatTickValue(double val)
    {
        if (ValueFormatString is not null) return val.ToString(ValueFormatString);
        if (val == Math.Floor(val)) return val.ToString("N0");
        return val.ToString("N1");
    }

    private async Task HandleClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
        if (!Editable) return;

        var dx = e.OffsetX - _cx;
        var dy = e.OffsetY - _cy;
        var angle = Math.Atan2(dy, dx);

        var totalSweep = _endAngleRad - _startAngleRad;

        // Normalize angle relative to start
        var relAngle = angle - _startAngleRad;
        while (relAngle < -Math.PI) relAngle += 2 * Math.PI;
        while (relAngle > Math.PI) relAngle -= 2 * Math.PI;
        if (relAngle < 0 && totalSweep > 0) relAngle += 2 * Math.PI;

        var fraction = Math.Clamp(relAngle / totalSweep, 0, 1);
        var newValue = Math.Round(Min + fraction * (Max - Min), 1);

        Value = newValue;
        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(newValue);
    }

    private static string F(double v) => v.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);

    private record struct TickInfo(double X1, double Y1, double X2, double Y2, bool IsMajor);
    private record struct TickLabelInfo(double X, double Y, string Text);
}

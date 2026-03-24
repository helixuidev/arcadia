using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;

namespace Arcadia.Charts.Components.Charts;

/// <summary>
/// Renders a radial gauge chart for KPI dashboards.
/// Displays a value as an arc with optional color thresholds.
/// </summary>
public partial class ArcadiaGaugeChart : Arcadia.Core.Base.ArcadiaComponentBase
{
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

    [Parameter] public int AnimationDuration { get; set; } = 800;

    private double _cx;
    private double _pathLength;
    private double _cy;
    private double _radius;
    private string _trackPath = "";
    private string _valuePath = "";
    private string _valueColor = "";
    private string _displayValue = "";

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        var clampedValue = Math.Max(Min, Math.Min(Max, Value));
        var ratio = (Max - Min) > 0 ? (clampedValue - Min) / (Max - Min) : 0;

        _cx = Width / 2;
        _radius = Math.Min(Width, Height) / 2 - StrokeWidth - 10;

        if (FullCircle)
        {
            _cy = Height / 2;
        }
        else
        {
            // Semi-circle: position center at bottom of area
            _cy = Height - StrokeWidth - 20;
            _radius = Math.Min(Width / 2, Height - StrokeWidth - 20) - 10;
        }

        if (_radius < 10) _radius = 10;

        // Build arc paths
        var startAngle = FullCircle ? -Math.PI / 2 : Math.PI;
        var totalSweep = FullCircle ? Math.PI * 2 : Math.PI;
        var endAngleFull = startAngle + totalSweep;
        var endAngleValue = startAngle + totalSweep * ratio;

        _trackPath = BuildArc(_cx, _cy, _radius, startAngle, endAngleFull, FullCircle);
        _valuePath = ratio > 0.001 ? BuildArc(_cx, _cy, _radius, startAngle, endAngleValue, ratio > 0.999 && FullCircle) : "";
        _pathLength = Math.Abs(endAngleValue - startAngle) * _radius;

        // Determine color from thresholds
        _valueColor = ResolveThresholdColor(clampedValue);

        // Format display value
        _displayValue = ValueFormatString is not null
            ? clampedValue.ToString(ValueFormatString)
            : clampedValue.ToString("G");
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

    private static string F(double v) => v.ToString("F1");
}

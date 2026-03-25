using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;
using Arcadia.Charts.Core.Layout;
using Arcadia.Charts.Core.Scales;

namespace Arcadia.Charts.Components.Charts;

/// <summary>
/// Range/Band Area chart that displays a filled region between upper and lower bounds.
/// Used for confidence intervals, min/max ranges, temperature bands, etc.
/// </summary>
public partial class ArcadiaRangeAreaChart<T> : ChartBase<T>
{
    /// <summary>Function to extract the X-axis label from each data item.</summary>
    [Parameter] public Func<T, object>? XField { get; set; }

    /// <summary>Function to extract the upper bound value from each data item.</summary>
    [Parameter] public Func<T, double>? UpperField { get; set; }

    /// <summary>Function to extract the lower bound value from each data item.</summary>
    [Parameter] public Func<T, double>? LowerField { get; set; }

    /// <summary>Optional function to extract a middle line value (e.g., average) from each data item.</summary>
    [Parameter] public Func<T, double>? MiddleField { get; set; }

    /// <summary>Fill color for the range band area.</summary>
    [Parameter] public string FillColor { get; set; } = "var(--arcadia-color-primary, #2563eb)";

    /// <summary>Opacity of the range band fill (0.0 to 1.0).</summary>
    [Parameter] public double FillOpacity { get; set; } = 0.2;

    /// <summary>Stroke color for the upper and lower bound lines.</summary>
    [Parameter] public string StrokeColor { get; set; } = "var(--arcadia-color-primary, #2563eb)";

    /// <summary>Optional stroke color for the middle line. Defaults to StrokeColor if null.</summary>
    [Parameter] public string? MiddleColor { get; set; }

    /// <summary>Stroke width for the upper, lower, and middle boundary lines.</summary>
    [Parameter] public double LineStrokeWidth { get; set; } = 2;

    /// <summary>Curve interpolation type: "linear" or "smooth".</summary>
    [Parameter] public string CurveType { get; set; } = "linear";

    private ChartLayoutResult _layout = new();
    private LinearScale? _yScale;
    private List<string> _xLabels = new();
    private string _upperPath = "";
    private string _lowerPath = "";
    private string _fillPath = "";
    private string _middlePath = "";
    private List<PointInfo> _upperPoints = new();
    private List<PointInfo> _lowerPoints = new();
    private List<PointInfo> _middlePoints = new();

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count < 2 || XField is null || UpperField is null || LowerField is null)
            return;

        // Build X labels
        _xLabels = Data.Select(d => FormatLabel(XField(d))).ToList();

        // Calculate Y range across upper and lower bounds
        var allYValues = new List<double>();
        foreach (var item in Data)
        {
            var upper = UpperField(item);
            var lower = LowerField(item);
            if (!double.IsNaN(upper)) allYValues.Add(upper);
            if (!double.IsNaN(lower)) allYValues.Add(lower);
            if (MiddleField is not null)
            {
                var mid = MiddleField(item);
                if (!double.IsNaN(mid)) allYValues.Add(mid);
            }
        }

        if (allYValues.Count == 0) return;

        var yMin = YAxisMin ?? allYValues.Min();
        var yMax = YAxisMax ?? allYValues.Max();
        if (YAxisMin is null && yMin > 0) yMin = 0;

        var seriesNames = new List<string> { "Upper", "Lower" };
        if (MiddleField is not null) seriesNames.Add("Middle");

        _layout = LayoutEngine.Calculate(new ChartLayoutInput
        {
            Width = EffectiveWidth,
            Height = Height,
            Title = Title,
            XAxisTitle = XAxisTitle,
            YAxisTitle = YAxisTitle,
            XTickLabels = _xLabels,
            YMin = yMin,
            YMax = yMax,
            SeriesNames = seriesNames
        });

        var yTicks = TickGenerator.GenerateNumericTicks(yMin, yMax, YAxisMaxTicks);
        _yScale = new LinearScale(
            yTicks.Min(), yTicks.Max(),
            _layout.PlotArea.Y + _layout.PlotArea.Height,
            _layout.PlotArea.Y);

        // Build upper and lower point lists
        _upperPoints.Clear();
        _lowerPoints.Clear();
        _middlePoints.Clear();

        for (var i = 0; i < Data.Count; i++)
        {
            var x = _layout.PlotArea.X + (i + 0.5) * (_layout.PlotArea.Width / Data.Count);
            var upperVal = UpperField(Data[i]);
            var lowerVal = LowerField(Data[i]);

            if (!double.IsNaN(upperVal) && !double.IsNaN(lowerVal))
            {
                _upperPoints.Add(new PointInfo { X = x, Y = _yScale.Scale(upperVal), Value = upperVal, Index = i });
                _lowerPoints.Add(new PointInfo { X = x, Y = _yScale.Scale(lowerVal), Value = lowerVal, Index = i });
            }

            if (MiddleField is not null)
            {
                var midVal = MiddleField(Data[i]);
                if (!double.IsNaN(midVal))
                {
                    _middlePoints.Add(new PointInfo { X = x, Y = _yScale.Scale(midVal), Value = midVal, Index = i });
                }
            }
        }

        // Build SVG paths
        _upperPath = BuildLinePath(_upperPoints);
        _lowerPath = BuildLinePath(_lowerPoints);
        _fillPath = BuildFillPath(_upperPoints, _lowerPoints);

        if (MiddleField is not null && _middlePoints.Count >= 2)
        {
            _middlePath = BuildLinePath(_middlePoints);
        }
        else
        {
            _middlePath = "";
        }
    }

    private string BuildLinePath(List<PointInfo> points)
    {
        if (points.Count < 2) return "";

        if (CurveType == "smooth")
        {
            var pts = points.Select(p => (p.X, p.Y)).ToList();
            return PathSmoother.SmoothPath(pts);
        }

        var segments = points.Select(p => $"{F(p.X)},{F(p.Y)}").ToList();
        return "M" + string.Join(" L", segments);
    }

    private string BuildFillPath(List<PointInfo> upper, List<PointInfo> lower)
    {
        if (upper.Count < 2 || lower.Count < 2) return "";

        if (CurveType == "smooth")
        {
            var upperPts = upper.Select(p => (p.X, p.Y)).ToList();
            var lowerPts = lower.AsEnumerable().Reverse().Select(p => (p.X, p.Y)).ToList();

            var upperSmooth = PathSmoother.SmoothPath(upperPts);
            var lowerSmooth = PathSmoother.SmoothPath(lowerPts);

            // Replace the leading M of the lower path with L to connect
            if (lowerSmooth.StartsWith("M"))
            {
                lowerSmooth = "L" + lowerSmooth.Substring(1);
            }

            return upperSmooth + " " + lowerSmooth + " Z";
        }

        // Linear: upper line forward, lower line reversed, closed
        var upperSegments = upper.Select(p => $"{F(p.X)},{F(p.Y)}").ToList();
        var lowerSegments = lower.AsEnumerable().Reverse().Select(p => $"{F(p.X)},{F(p.Y)}").ToList();

        return "M" + string.Join(" L", upperSegments) + " L" + string.Join(" L", lowerSegments) + " Z";
    }

    private string FormatLabel(object? value)
    {
        if (value is null) return "";
        if (XAxisFormatString is not null && value is IFormattable formattable)
            return formattable.ToString(XAxisFormatString, FormatProvider);
        return value.ToString() ?? "";
    }

    internal string FormatDataLabel(double value) => FormatValue(value, DataLabelFormatString);

    private string EffectiveMiddleColor => MiddleColor ?? StrokeColor;

    private static string F(double v) => v.ToString("F1");

    private class PointInfo
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Value { get; set; }
        public int Index { get; set; }
    }
}

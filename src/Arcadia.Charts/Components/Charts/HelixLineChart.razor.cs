using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;
using Arcadia.Charts.Core.Data;
using Arcadia.Charts.Core.Layout;
using Arcadia.Charts.Core.Scales;

namespace Arcadia.Charts.Components.Charts;

public partial class HelixLineChart<T> : ChartBase<T>
{
    /// <summary>Function to extract the X-axis label from each data item.</summary>
    [Parameter] public Func<T, object>? XField { get; set; }

    /// <summary>Series configurations.</summary>
    [Parameter] public List<SeriesConfig<T>>? Series { get; set; }

    /// <summary>Whether to show data points on the line (overridden per-series by SeriesConfig.ShowPoints).</summary>
    [Parameter] public bool ShowPoints { get; set; } = true;

    /// <summary>How to handle null/missing data values (NaN in series Field).</summary>
    [Parameter] public NullHandling NullHandling { get; set; } = NullHandling.Gap;

    private ChartLayoutResult _layout = new();
    private LinearScale? _yScale;
    private List<string> _xLabels = new();
    private Dictionary<int, string> _paths = new();
    private Dictionary<int, string> _areaPaths = new();
    private Dictionary<int, string> _trendlinePaths = new();
    private Dictionary<int, List<DataPointInfo>> _dataPoints = new();

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count < 2 || XField is null || Series is null)
            return;

        // Build X labels
        _xLabels = Data.Select(d => FormatLabel(XField(d))).ToList();

        // Calculate Y range across all series (filter out NaN for null handling)
        var allYValues = Series.SelectMany(s => Data.Select(d => s.Field(d)))
            .Where(v => !double.IsNaN(v)).ToList();
        if (allYValues.Count == 0) return;
        var yMin = YAxisMin ?? allYValues.Min();
        var yMax = YAxisMax ?? allYValues.Max();
        if (YAxisMin is null && yMin > 0) yMin = 0; // Include zero baseline unless manually set

        _layout = LayoutEngine.Calculate(new ChartLayoutInput
        {
            Width = Width,
            Height = Height,
            Title = Title,
            XAxisTitle = XAxisTitle,
            YAxisTitle = YAxisTitle,
            XTickLabels = _xLabels,
            YMin = yMin,
            YMax = yMax,
            SeriesNames = Series.Select(s => s.Name).ToList()
        });

        var yTicks = TickGenerator.GenerateNumericTicks(yMin, yMax, YAxisMaxTicks);
        _yScale = new LinearScale(
            yTicks.Min(), yTicks.Max(),
            _layout.PlotArea.Y + _layout.PlotArea.Height,
            _layout.PlotArea.Y);

        // Build SVG paths for each series
        _paths.Clear();
        _areaPaths.Clear();
        _trendlinePaths.Clear();
        _dataPoints.Clear();

        for (var si = 0; si < Series.Count; si++)
        {
            var series = Series[si];
            var segments = new List<List<(double X, double Y, int Index)>>();
            var currentSegment = new List<(double X, double Y, int Index)>();
            var rawValues = new List<double>();
            var seriesPoints = new List<DataPointInfo>();

            for (var i = 0; i < Data.Count; i++)
            {
                var x = _layout.PlotArea.X + (i + 0.5) * (_layout.PlotArea.Width / Data.Count);
                var value = series.Field(Data[i]);
                var isNull = double.IsNaN(value);

                if (isNull && NullHandling == NullHandling.Gap)
                {
                    if (currentSegment.Count > 0)
                    {
                        segments.Add(currentSegment);
                        currentSegment = new List<(double X, double Y, int Index)>();
                    }
                    rawValues.Add(0);
                    continue;
                }

                if (isNull && NullHandling == NullHandling.Zero)
                    value = 0;
                else if (isNull && NullHandling == NullHandling.Connect)
                {
                    rawValues.Add(0);
                    continue; // Skip but don't break the segment
                }

                var y = _yScale.Scale(value);
                currentSegment.Add((x, y, i));
                rawValues.Add(value);
                seriesPoints.Add(new DataPointInfo { X = x, Y = y, Value = value, Index = i });
            }

            if (currentSegment.Count > 0)
                segments.Add(currentSegment);

            // Build path from segments
            var pathParts = new List<string>();
            foreach (var segment in segments)
            {
                if (segment.Count < 2) continue;
                var points = segment.Select(p => $"{F(p.X)},{F(p.Y)}").ToList();
                pathParts.Add("M" + string.Join(" L", points));
            }

            if (pathParts.Count > 0)
            {
                _paths[si] = string.Join(" ", pathParts);

                if (series.ShowArea)
                {
                    var areaPathParts = new List<string>();
                    foreach (var segment in segments)
                    {
                        if (segment.Count < 2) continue;
                        var points = segment.Select(p => $"{F(p.X)},{F(p.Y)}").ToList();
                        var baseY = _layout.PlotArea.Y + _layout.PlotArea.Height;
                        var part = "M" + string.Join(" L", points);
                        part += $" L{F(segment[^1].X)},{F(baseY)} L{F(segment[0].X)},{F(baseY)} Z";
                        areaPathParts.Add(part);
                    }
                    _areaPaths[si] = string.Join(" ", areaPathParts);
                }
            }

            _dataPoints[si] = seriesPoints;

            // Trendlines
            if (series.Trendline is not null && series.Trendline.Type != TrendlineType.None)
            {
                var validValues = seriesPoints.Select(p => p.Value).ToList();
                if (validValues.Count >= 2)
                {
                    double[] trendValues;
                    if (series.Trendline.Type == TrendlineType.MovingAverage)
                    {
                        trendValues = TrendlineCalculator.MovingAverage(validValues, series.Trendline.MovingAveragePeriod);
                    }
                    else
                    {
                        trendValues = TrendlineCalculator.LinearRegression(validValues);
                    }

                    var trendPoints = new List<string>();
                    for (var i = 0; i < trendValues.Length && i < seriesPoints.Count; i++)
                    {
                        var px = seriesPoints[i].X;
                        var py = _yScale.Scale(trendValues[i]);
                        trendPoints.Add($"{F(px)},{F(py)}");
                    }

                    if (trendPoints.Count >= 2)
                    {
                        _trendlinePaths[si] = "M" + string.Join(" L", trendPoints);
                    }
                }
            }
        }
    }

    private string FormatLabel(object? value)
    {
        if (value is null) return "";
        if (XAxisFormatString is not null && value is IFormattable formattable)
            return formattable.ToString(XAxisFormatString, FormatProvider);
        return value.ToString() ?? "";
    }

    internal string FormatYTick(double value)
    {
        if (YAxisFormatString is not null)
            return value.ToString(YAxisFormatString, FormatProvider);
        return _layout.YTicks.FirstOrDefault(t => Math.Abs(t.Value - value) < double.Epsilon)?.Label ?? value.ToString("G4");
    }

    internal string FormatDataLabel(double value) => FormatValue(value, DataLabelFormatString);

    private static string F(double v) => v.ToString("F1");

    private class DataPointInfo
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Value { get; set; }
        public int Index { get; set; }
    }
}

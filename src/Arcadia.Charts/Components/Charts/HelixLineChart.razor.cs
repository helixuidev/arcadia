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

    /// <summary>When true, area fills stack on top of each other instead of starting from zero.</summary>
    [Parameter] public bool Stacked { get; set; }

    private ChartLayoutResult _layout = new();
    private LinearScale? _yScale;
    private List<string> _xLabels = new();
    private Dictionary<int, string> _paths = new();
    private Dictionary<int, string> _areaPaths = new();
    private Dictionary<int, string> _trendlinePaths = new();
    private Dictionary<int, List<DataPointInfo>> _dataPoints = new();
    private Dictionary<int, Dictionary<int, double>> _stackBaselines = new();

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
        List<double> allYValues;
        if (Stacked)
        {
            // For stacked, the Y range needs to account for cumulative sums
            var cumulativeMax = new double[Data.Count];
            foreach (var s in Series.Where(s => s.Visible))
            {
                for (var j = 0; j < Data.Count; j++)
                {
                    var v = s.Field(Data[j]);
                    if (!double.IsNaN(v)) cumulativeMax[j] += v;
                }
            }
            allYValues = cumulativeMax.Where(v => !double.IsNaN(v)).ToList();
            allYValues.Add(0);
        }
        else
        {
            allYValues = Series.SelectMany(s => Data.Select(d => s.Field(d)))
                .Where(v => !double.IsNaN(v)).ToList();
        }
        if (allYValues.Count == 0) return;
        var yMin = YAxisMin ?? allYValues.Min();
        var yMax = YAxisMax ?? allYValues.Max();
        if (YAxisMin is null && yMin > 0) yMin = 0; // Include zero baseline unless manually set

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
        _stackBaselines.Clear();

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

                // In stacked mode, add previous series values
                var stackedValue = value;
                if (Stacked && si > 0)
                {
                    for (var prevSi = 0; prevSi < si; prevSi++)
                    {
                        if (Series[prevSi].Visible)
                        {
                            var prevVal = Series[prevSi].Field(Data[i]);
                            if (!double.IsNaN(prevVal)) stackedValue += prevVal;
                        }
                    }
                }
                var y = _yScale.Scale(Stacked ? stackedValue : value);
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

                        if (Stacked && si > 0 && _stackBaselines.ContainsKey(si - 1))
                        {
                            // Stack: bottom edge follows previous series
                            var prevBaseline = _stackBaselines[si - 1];
                            var bottomPoints = segment.Select(p => {
                                var baseY = prevBaseline.ContainsKey(p.Index) ? prevBaseline[p.Index] : _layout.PlotArea.Y + _layout.PlotArea.Height;
                                return $"{F(p.X)},{F(baseY)}";
                            }).Reverse().ToList();
                            var part = "M" + string.Join(" L", points) + " L" + string.Join(" L", bottomPoints) + " Z";
                            areaPathParts.Add(part);
                        }
                        else
                        {
                            // Normal: bottom edge is the x-axis
                            var baseY = _layout.PlotArea.Y + _layout.PlotArea.Height;
                            var part = "M" + string.Join(" L", points);
                            part += $" L{F(segment[^1].X)},{F(baseY)} L{F(segment[0].X)},{F(baseY)} Z";
                            areaPathParts.Add(part);
                        }
                    }
                    _areaPaths[si] = string.Join(" ", areaPathParts);
                }

                // Store baseline for stacking
                if (Stacked)
                {
                    var baseline = new Dictionary<int, double>();
                    foreach (var pt in seriesPoints)
                    {
                        var prevY = (si > 0 && _stackBaselines.ContainsKey(si - 1) && _stackBaselines[si - 1].ContainsKey(pt.Index))
                            ? _stackBaselines[si - 1][pt.Index] : _layout.PlotArea.Y + _layout.PlotArea.Height;
                        baseline[pt.Index] = pt.Y;
                    }
                    _stackBaselines[si] = baseline;
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

    private void ToggleSeries(int index)
    {
        if (Series is not null && index >= 0 && index < Series.Count)
        {
            Series[index].Visible = !Series[index].Visible;
            OnParametersSet(); // Recalculate paths
            StateHasChanged();
        }
    }

    private static string F(double v) => v.ToString("F1");

    private class DataPointInfo
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Value { get; set; }
        public int Index { get; set; }
    }
}

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;
using Arcadia.Charts.Core.Data;
using Arcadia.Charts.Core.Layout;
using Arcadia.Charts.Core.Scales;

namespace Arcadia.Charts.Components.Charts;

public partial class ArcadiaLineChart<T> : ChartBase<T>
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

    /// <summary>X-axis type: "category" (default, equal spacing) or "time" (continuous DateTime positioning).</summary>
    [Parameter] public string XAxisType { get; set; } = "category";

    private bool IsTimeAxis => string.Equals(XAxisType, "time", StringComparison.OrdinalIgnoreCase);

    private ChartLayoutResult _layout = new();
    private LinearScale? _yScale;
    private LinearScale? _y2Scale;
    private TimeScale? _xTimeScale;
    private List<DateTime> _xDateTimes = new();
    private List<string> _xLabels = new();
    private Dictionary<int, string> _paths = new();
    private Dictionary<int, string> _areaPaths = new();
    private Dictionary<int, string> _trendlinePaths = new();
    private Dictionary<int, List<DataPointInfo>> _dataPoints = new();
    private Dictionary<int, Dictionary<int, double>> _stackBaselines = new();

    // Change detection to skip unnecessary path recalculation
    private int _lastDataCount;
    private int _lastSeriesCount;
    private double _lastWidth;
    private double _lastHeight;

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count < 2 || XField is null || Series is null)
            return;

        // Quick change detection — skip full recalc if only cosmetic params changed
        var dataCount = Data.Count;
        var seriesCount = Series.Count;
        var width = EffectiveWidth;
        var height = Height;
        var dataChanged = dataCount != _lastDataCount || seriesCount != _lastSeriesCount
                       || Math.Abs(width - _lastWidth) > 0.5 || Math.Abs(height - _lastHeight) > 0.5;
        // Always recalc on first render or if data/dimensions changed
        // For non-data param changes (Title, ShowGrid, etc.), paths are still valid
        _lastDataCount = dataCount;
        _lastSeriesCount = seriesCount;
        _lastWidth = width;
        _lastHeight = height;

        // Build X labels
        _xDateTimes.Clear();
        _xTimeScale = null;

        if (IsTimeAxis)
        {
            _xDateTimes = Data.Select(d =>
            {
                var raw = XField(d);
                return raw switch
                {
                    DateTime dt => dt,
                    DateTimeOffset dto => dto.DateTime,
#if NET6_0_OR_GREATER
                    DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
#endif
                    _ => DateTime.MinValue
                };
            }).ToList();

            var timeRange = _xDateTimes.Count > 1 ? _xDateTimes.Max() - _xDateTimes.Min() : TimeSpan.FromDays(1);
            var timeTicks = TickGenerator.GenerateTimeTicks(_xDateTimes.Min(), _xDateTimes.Max(), XAxisMaxTicks);
            var tickFormat = XAxisFormatString ?? TimeTickLabelFormatter.GetFormat(timeRange);
            _xLabels = timeTicks.Select(t => t.ToString(tickFormat, FormatProvider)).ToList();
        }
        else
        {
            _xLabels = Data.Select(d => FormatLabel(XField(d))).ToList();
        }

        // Partition series by axis index
        var primarySeries = Series.Where(s => s.YAxisIndex == 0).ToList();
        var secondarySeries = Series.Where(s => s.YAxisIndex == 1).ToList();
        HasSecondaryYAxis = secondarySeries.Count > 0;

        // Calculate primary Y range (axis 0)
        List<double> allYValues;
        if (Stacked)
        {
            var cumulativeMax = new double[Data.Count];
            foreach (var s in primarySeries.Where(s => s.Visible))
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
            allYValues = primarySeries.SelectMany(s => Data.Select(d => s.Field(d)))
                .Where(v => !double.IsNaN(v)).ToList();
        }
        if (allYValues.Count == 0) allYValues.Add(0);
        var yMin = YAxisMin ?? allYValues.Min();
        var yMax = YAxisMax ?? allYValues.Max();
        if (YAxisMin is null && yMin > 0) yMin = 0;

        // Calculate secondary Y range (axis 1)
        double y2Min = 0, y2Max = 1;
        if (HasSecondaryYAxis)
        {
            var y2Values = secondarySeries.SelectMany(s => Data.Select(d => s.Field(d)))
                .Where(v => !double.IsNaN(v)).ToList();
            if (y2Values.Count > 0)
            {
                y2Min = YAxis2Min ?? y2Values.Min();
                y2Max = YAxis2Max ?? y2Values.Max();
                if (YAxis2Min is null && y2Min > 0) y2Min = 0;
            }
        }

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
            SeriesNames = Series.Select(s => s.Name).ToList(),
            HasSecondaryYAxis = HasSecondaryYAxis,
            Y2Min = HasSecondaryYAxis ? y2Min : null,
            Y2Max = HasSecondaryYAxis ? y2Max : null,
            Y2AxisTitle = YAxis2Title
        });

        var yTicks = CreateYTicks(yMin, yMax);
        _yScale = CreateYScale(
            yTicks.Min(), yTicks.Max(),
            _layout.PlotArea.Y + _layout.PlotArea.Height,
            _layout.PlotArea.Y);

        // Build secondary Y scale
        if (HasSecondaryYAxis)
        {
            var y2Ticks = CreateYTicks(y2Min, y2Max);
            _y2Scale = CreateYScale(
                y2Ticks.Min(), y2Ticks.Max(),
                _layout.PlotArea.Y + _layout.PlotArea.Height,
                _layout.PlotArea.Y);
        }
        else { _y2Scale = null; }

        // Build TimeScale for continuous X positioning
        if (IsTimeAxis && _xDateTimes.Count > 1)
        {
            _xTimeScale = TimeScale.FromData(_xDateTimes, _layout.PlotArea.X, _layout.PlotArea.X + _layout.PlotArea.Width);
        }

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
                var x = IsTimeAxis && _xTimeScale is not null && i < _xDateTimes.Count
                    ? _xTimeScale.Scale(_xDateTimes[i])
                    : _layout.PlotArea.X + (i + 0.5) * (_layout.PlotArea.Width / Data.Count);
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
                var yScale = series.YAxisIndex == 1 && _y2Scale is not null ? _y2Scale : _yScale!;
                var y = yScale.Scale(Stacked ? stackedValue : value);
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

                if (series.CurveType == "smooth")
                {
                    var pts = segment.Select(p => (p.X, p.Y)).ToList();
                    pathParts.Add(PathSmoother.SmoothPath(pts));
                }
                else
                {
                    var points = segment.Select(p => $"{F(p.X)},{F(p.Y)}").ToList();
                    pathParts.Add("M" + string.Join(" L", points));
                }
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
                        var trendScale = series.YAxisIndex == 1 && _y2Scale is not null ? _y2Scale : _yScale!;
                        var py = trendScale.Scale(trendValues[i]);
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

    internal string FormatDataLabel(double value) => FormatValue(value, DataLabelFormatString);

    /// <summary>Appends a data point for live/streaming data. Triggers re-render.</summary>
    public void AppendPoint(T item)
    {
        if (Data is IList<T> list)
        {
            list.Add(item);
            OnParametersSet();
            InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>Removes the first data point (for sliding window). Triggers re-render.</summary>
    public void RemoveFirst()
    {
        if (Data is IList<T> list && list.Count > 0)
        {
            list.RemoveAt(0);
            OnParametersSet();
            InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>Appends a point and removes the first (sliding window in one call). Animates the transition.</summary>
    public void AppendAndSlide(T item)
    {
        if (Data is IList<T> list)
        {
            var wasAtCapacity = SlidingWindow > 0 && list.Count >= SlidingWindow;
            list.Add(item);
            if (list.Count > SlidingWindow && SlidingWindow > 0)
                list.RemoveAt(0);
            OnParametersSet();
            InvokeAsync(async () =>
            {
                StateHasChanged();
                // Trigger slide animation if we removed a point
                if (wasAtCapacity && Interop is not null && Data is not null && Data.Count > 1)
                {
                    var stepWidth = _layout.PlotArea.Width / Data.Count;
                    try { await Interop.SlideChartContentAsync(ContainerRef, stepWidth, 400); }
                    catch (JSException) { } // JS interop unavailable during SSR
#if NET6_0_OR_GREATER
                    catch (JSDisconnectedException) { } // Circuit disconnected during Blazor Server navigation
#endif
                    catch (ObjectDisposedException) { } // Component already disposed
                }
            });
        }
    }

    /// <summary>Maximum number of points to keep in sliding window mode. 0 = no limit.</summary>
    [Parameter] public int SlidingWindow { get; set; }

    private double _crosshairX;
    private bool _crosshairVisible;

    private Task HandleMouseMove(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
        if (ShowCrosshair)
        {
            _crosshairX = e.OffsetX;
            _crosshairVisible = _crosshairX >= _layout.PlotArea.X && _crosshairX <= _layout.PlotArea.X + _layout.PlotArea.Width;
            StateHasChanged();
        }
        return Task.CompletedTask;
    }

    private async Task HandleMouseLeave()
    {
        _crosshairVisible = false;
        await HideTooltipAction();
    }

    private void ToggleSeries(int index)
    {
        if (Series is not null && index >= 0 && index < Series.Count)
        {
            Series[index].Visible = !Series[index].Visible;
            OnParametersSet(); // Recalculate paths
            StateHasChanged();
        }
    }

    private string GetPointStyle(int index)
    {
        var style = "";
        if (AnimateOnLoad)
            style += $"animation-delay: {index * 40}ms;";
        if (OnPointClick.HasDelegate)
            style += "cursor:pointer;";
        return style;
    }

    private async Task HandlePointHover(string seriesName, DataPointInfo pt, Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
        if (Data is not null && pt.Index >= 0 && pt.Index < Data.Count)
        {
            var item = Data[pt.Index];
            var fallbackHtml = $"<div style='font-weight:600;margin-bottom:2px'>{seriesName}</div><div>{FormatValue(pt.Value, YAxisFormatString ?? DataLabelFormatString)}</div>";
            await ShowTooltipOrTemplate(item, fallbackHtml, e.ClientX, e.ClientY);
        }
    }

    private async Task HandlePointClick(int index)
    {
        if (OnPointClick.HasDelegate && Data is not null && index >= 0 && index < Data.Count)
            await OnPointClick.InvokeAsync(new PointClickEventArgs<T>
            {
                Item = Data[index],
                DataIndex = index,
                SeriesIndex = -1
            });
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

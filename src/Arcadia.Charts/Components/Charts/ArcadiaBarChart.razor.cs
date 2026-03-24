using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;
using Arcadia.Charts.Core.Layout;
using Arcadia.Charts.Core.Scales;

namespace Arcadia.Charts.Components.Charts;

public partial class ArcadiaBarChart<T> : ChartBase<T>
{
    /// <summary>Function to extract the X-axis category from each data item.</summary>
    [Parameter] public Func<T, object>? XField { get; set; }

    /// <summary>Series configurations.</summary>
    [Parameter] public List<SeriesConfig<T>>? Series { get; set; }

    /// <summary>Whether to use rounded bar corners.</summary>
    [Parameter] public bool Rounded { get; set; } = true;

    /// <summary>Whether to stack bars on top of each other instead of side by side.</summary>
    [Parameter] public bool Stacked { get; set; }

    /// <summary>Bar padding ratio (0-1). Controls gap between bars.</summary>
    [Parameter] public double BarPadding { get; set; } = 0.15;

    /// <summary>Bar corner radius. Only applies when Rounded=true.</summary>
    [Parameter] public double CornerRadius { get; set; } = 3;

    private ChartLayoutResult _layout = new();
    private LinearScale? _yScale;
    private BandScale? _bandScale;
    private List<BarRect> _bars = new();

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count == 0 || XField is null || Series is null)
            return;

        var categories = Data.Select(d => XField(d)?.ToString() ?? "").ToList();

        double yMax;
        double yMin;

        if (Stacked)
        {
            // For stacked bars, max is the sum of all positive series at each data point
            yMax = 0;
            yMin = 0;
            for (var di = 0; di < Data.Count; di++)
            {
                double posSum = 0;
                double negSum = 0;
                for (var si = 0; si < Series.Count; si++)
                {
                    var v = Series[si].Field(Data[di]);
                    if (v >= 0) posSum += v;
                    else negSum += v;
                }
                yMax = Math.Max(yMax, posSum);
                yMin = Math.Min(yMin, negSum);
            }
        }
        else
        {
            var allYValues = Series.SelectMany(s => Data.Select(d => s.Field(d))).ToList();
            yMax = allYValues.Max();
            yMin = Math.Min(0, allYValues.Min());
        }

        _layout = LayoutEngine.Calculate(new ChartLayoutInput
        {
            Width = EffectiveWidth,
            Height = Height,
            Title = Title,
            XTickLabels = categories,
            YMin = yMin,
            YMax = yMax,
            SeriesNames = Series.Select(s => s.Name).ToList()
        });

        _bandScale = new BandScale(categories, _layout.PlotArea.X, _layout.PlotArea.X + _layout.PlotArea.Width, BarPadding);

        var yTicks = TickGenerator.GenerateNumericTicks(yMin, yMax, 8);
        _yScale = new LinearScale(
            yTicks.Min(), yTicks.Max(),
            _layout.PlotArea.Y + _layout.PlotArea.Height,
            _layout.PlotArea.Y);

        // Build bar rectangles
        _bars.Clear();
        for (var di = 0; di < Data.Count; di++)
        {
            var category = categories[di];
            var bandX = _bandScale.Scale(category);
            var groupWidth = _bandScale.BandWidth;

            if (Stacked)
            {
                double positiveStackY = 0;
                double negativeStackY = 0;

                for (var si = 0; si < Series.Count; si++)
                {
                    var series = Series[si];
                    var value = series.Field(Data[di]);
                    var color = ResolveColor(series.Color, si);

                    double barBottom, barTop;
                    if (value >= 0)
                    {
                        barBottom = positiveStackY;
                        barTop = positiveStackY + value;
                        positiveStackY = barTop;
                    }
                    else
                    {
                        barTop = negativeStackY + value;
                        barBottom = negativeStackY;
                        negativeStackY = barTop;
                    }

                    var y1 = _yScale.Scale(barTop);
                    var y2 = _yScale.Scale(barBottom);
                    var barHeight = Math.Abs(y2 - y1);

                    _bars.Add(new BarRect
                    {
                        X = bandX + 1,
                        Y = Math.Min(y1, y2),
                        Width = Math.Max(1, groupWidth - 2),
                        Height = barHeight,
                        Color = color,
                        Value = value,
                        DataIndex = di,
                        SeriesIndex = si,
                        SeriesName = series.Name
                    });
                }
            }
            else
            {
                var barWidth = groupWidth / Series.Count;
                for (var si = 0; si < Series.Count; si++)
                {
                    var series = Series[si];
                    var value = series.Field(Data[di]);
                    var color = ResolveColor(series.Color, si);
                    var barX = bandX + si * barWidth;
                    var barY = _yScale.Scale(value);
                    var baseY = _yScale.Scale(0);
                    var barHeight = Math.Abs(baseY - barY);
                    var barTop = Math.Min(barY, baseY);

                    _bars.Add(new BarRect
                    {
                        X = barX + 1,
                        Y = barTop,
                        Width = Math.Max(1, barWidth - 2),
                        Height = barHeight,
                        Color = color,
                        Value = value,
                        DataIndex = di,
                        SeriesIndex = si,
                        SeriesName = series.Name
                    });
                }
            }
        }
    }

    internal string FormatDataLabel(double value)
    {
        if (DataLabelFormatString is not null)
            return value.ToString(DataLabelFormatString, FormatProvider);
        return value.ToString("G4");
    }

    private static string F(double v) => v.ToString("F1");

    private void ToggleSeries(int index)
    {
        if (Series is not null && index >= 0 && index < Series.Count)
        {
            Series[index].Visible = !Series[index].Visible;
            OnParametersSet();
            StateHasChanged();
        }
    }

    private async Task HandleBarClick(BarRect bar)
    {
        if (OnPointClick.HasDelegate && Data is not null && bar.DataIndex >= 0 && bar.DataIndex < Data.Count)
            await OnPointClick.InvokeAsync(Data[bar.DataIndex]);
        if (OnSeriesClick.HasDelegate)
            await OnSeriesClick.InvokeAsync(bar.SeriesIndex);
    }

    private class BarRect
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Color { get; set; } = "";
        public double Value { get; set; }
        public int DataIndex { get; set; }
        public int SeriesIndex { get; set; }
        public string SeriesName { get; set; } = "";
    }
}

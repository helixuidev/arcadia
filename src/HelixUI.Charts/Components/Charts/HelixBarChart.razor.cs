using Microsoft.AspNetCore.Components;
using HelixUI.Core.Utilities;
using HelixUI.Charts.Core;
using HelixUI.Charts.Core.Layout;
using HelixUI.Charts.Core.Scales;

namespace HelixUI.Charts.Components.Charts;

public partial class HelixBarChart<T> : ChartBase<T>
{
    /// <summary>Function to extract the X-axis category from each data item.</summary>
    [Parameter] public Func<T, object>? XField { get; set; }

    /// <summary>Series configurations.</summary>
    [Parameter] public List<SeriesConfig<T>>? Series { get; set; }

    /// <summary>Whether to use rounded bar corners.</summary>
    [Parameter] public bool Rounded { get; set; } = true;

    /// <summary>Whether to stack bars on top of each other instead of side by side.</summary>
    [Parameter] public bool Stacked { get; set; }

    /// <summary>Custom tooltip template rendered for each data point.</summary>
    [Parameter] public RenderFragment<T>? TooltipTemplate { get; set; }

    /// <summary>Whether to show value labels on bars.</summary>
    [Parameter] public bool ShowDataLabels { get; set; } = false;

    /// <summary>Format string for data labels.</summary>
    [Parameter] public string? DataLabelFormatString { get; set; }

    /// <summary>Format string for Y-axis labels (e.g. "C0" for currency, "P0" for percent).</summary>
    [Parameter] public string? YAxisFormatString { get; set; }

    private ChartLayoutResult _layout = new();
    private LinearScale? _yScale;
    private BandScale? _bandScale;
    private List<BarRect> _bars = new();

    private string? CssClass => CssBuilder.Default("helix-chart__svg")
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
            Width = Width,
            Height = Height,
            Title = Title,
            XTickLabels = categories,
            YMin = yMin,
            YMax = yMax,
            SeriesNames = Series.Select(s => s.Name).ToList()
        });

        _bandScale = new BandScale(categories, _layout.PlotArea.X, _layout.PlotArea.X + _layout.PlotArea.Width, 0.15);

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
                        SeriesIndex = si
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
                        SeriesIndex = si
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
    }
}

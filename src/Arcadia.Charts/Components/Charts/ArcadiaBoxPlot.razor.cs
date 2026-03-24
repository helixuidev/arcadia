using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;
using Arcadia.Charts.Core.Layout;
using Arcadia.Charts.Core.Scales;

namespace Arcadia.Charts.Components.Charts;

/// <summary>
/// Box and whisker chart showing statistical distribution: minimum, Q1, median, Q3, and maximum
/// for each data category.
/// </summary>
public partial class ArcadiaBoxPlot<T> : ChartBase<T>
{
    /// <summary>Function to extract the category name from each data item.</summary>
    [Parameter] public Func<T, string>? CategoryField { get; set; }

    /// <summary>Function to extract the minimum value from each data item.</summary>
    [Parameter] public Func<T, double>? MinField { get; set; }

    /// <summary>Function to extract the first quartile (25th percentile) from each data item.</summary>
    [Parameter] public Func<T, double>? Q1Field { get; set; }

    /// <summary>Function to extract the median (50th percentile) from each data item.</summary>
    [Parameter] public Func<T, double>? MedianField { get; set; }

    /// <summary>Function to extract the third quartile (75th percentile) from each data item.</summary>
    [Parameter] public Func<T, double>? Q3Field { get; set; }

    /// <summary>Function to extract the maximum value from each data item.</summary>
    [Parameter] public Func<T, double>? MaxField { get; set; }

    /// <summary>Color for the box fill.</summary>
    [Parameter] public string BoxColor { get; set; } = "var(--arcadia-color-primary, #2563eb)";

    /// <summary>Box width as a fraction of available band space (0-1).</summary>
    [Parameter] public double BoxWidth { get; set; } = 0.6;

    /// <summary>Fill opacity of the box rectangle (0.0 to 1.0).</summary>
    [Parameter] public double BoxFillOpacity { get; set; } = 0.7;

    /// <summary>Opacity of the whisker and cap lines (0.0 to 1.0).</summary>
    [Parameter] public double WhiskerOpacity { get; set; } = 0.6;

    /// <summary>Color of the median line drawn across each box.</summary>
    [Parameter] public string MedianLineColor { get; set; } = "white";

    /// <summary>Stroke width of the median line drawn across each box.</summary>
    [Parameter] public double MedianLineWidth { get; set; } = 2.5;

    private ChartLayoutResult _layout = new();
    private LinearScale? _yScale;
    private BandScale? _bandScale;
    private List<BoxData> _boxes = new();

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (!HasData || CategoryField is null || MinField is null || Q1Field is null ||
            MedianField is null || Q3Field is null || MaxField is null)
            return;

        var categories = Data!.Select(d => CategoryField(d)).ToList();
        var allMin = Data!.Select(d => MinField(d)).Min();
        var allMax = Data!.Select(d => MaxField(d)).Max();
        var padding = (allMax - allMin) * 0.05;
        var yMin = allMin - padding;
        var yMax = allMax + padding;

        _layout = LayoutEngine.Calculate(new ChartLayoutInput
        {
            Width = EffectiveWidth,
            Height = Height,
            Title = Title,
            XTickLabels = categories,
            YMin = yMin,
            YMax = yMax
        });

        var yTicks = TickGenerator.GenerateNumericTicks(yMin, yMax, YAxisMaxTicks);
        _yScale = new LinearScale(yTicks.Min(), yTicks.Max(),
            _layout.PlotArea.Y + _layout.PlotArea.Height, _layout.PlotArea.Y);

        _bandScale = new BandScale(categories, _layout.PlotArea.X,
            _layout.PlotArea.X + _layout.PlotArea.Width, 0.2);

        _boxes.Clear();
        for (var i = 0; i < Data!.Count; i++)
        {
            var item = Data[i];
            var category = categories[i];
            var bandX = _bandScale.Scale(category);
            var bandW = _bandScale.BandWidth;
            var boxW = bandW * BoxWidth;
            var boxX = bandX + (bandW - boxW) / 2;
            var centerX = bandX + bandW / 2;
            var capHalfW = boxW * 0.3;

            var minVal = MinField(item);
            var q1Val = Q1Field(item);
            var medianVal = MedianField(item);
            var q3Val = Q3Field(item);
            var maxVal = MaxField(item);

            _boxes.Add(new BoxData
            {
                CenterX = centerX,
                BoxX = boxX,
                BoxWidth = boxW,
                CapHalfWidth = capHalfW,
                MinY = _yScale.Scale(minVal),
                Q1Y = _yScale.Scale(q1Val),
                MedianY = _yScale.Scale(medianVal),
                Q3Y = _yScale.Scale(q3Val),
                MaxY = _yScale.Scale(maxVal),
                Category = category,
                Min = minVal,
                Q1 = q1Val,
                Median = medianVal,
                Q3 = q3Val,
                Max = maxVal
            });
        }
    }

    private async Task ShowBoxTooltip(int index, double mouseX, double mouseY)
    {
        if (Interop is null || Data is null || index >= _boxes.Count) return;
        var b = _boxes[index];
        var fmt = YAxisFormatString ?? "N0";
        var html = $"<div style='font-weight:600;margin-bottom:4px'>{b.Category}</div>" +
                   $"<div>Max: {b.Max.ToString(fmt, FormatProvider)}</div>" +
                   $"<div>Q3: {b.Q3.ToString(fmt, FormatProvider)}</div>" +
                   $"<div>Median: {b.Median.ToString(fmt, FormatProvider)}</div>" +
                   $"<div>Q1: {b.Q1.ToString(fmt, FormatProvider)}</div>" +
                   $"<div>Min: {b.Min.ToString(fmt, FormatProvider)}</div>";
        await Interop.ShowTooltipAsync(html, mouseX, mouseY);
    }

    private static string F(double v) => v.ToString("F1");

    private class BoxData
    {
        public double CenterX { get; set; }
        public double BoxX { get; set; }
        public double BoxWidth { get; set; }
        public double CapHalfWidth { get; set; }
        public double MinY { get; set; }
        public double Q1Y { get; set; }
        public double MedianY { get; set; }
        public double Q3Y { get; set; }
        public double MaxY { get; set; }
        public string Category { get; set; } = "";
        public double Min { get; set; }
        public double Q1 { get; set; }
        public double Median { get; set; }
        public double Q3 { get; set; }
        public double Max { get; set; }
    }
}

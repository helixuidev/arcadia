using Microsoft.AspNetCore.Components;
using HelixUI.Core.Utilities;
using HelixUI.Charts.Core;
using HelixUI.Charts.Core.Data;
using HelixUI.Charts.Core.Layout;
using HelixUI.Charts.Core.Scales;

namespace HelixUI.Charts.Components.Charts;

public partial class HelixScatterChart<T> : ChartBase<T>
{
    /// <summary>Function to extract the X value from each data item.</summary>
    [Parameter] public Func<T, double>? XField { get; set; }

    /// <summary>Function to extract the Y value from each data item.</summary>
    [Parameter] public Func<T, double>? YField { get; set; }

    /// <summary>Function to extract the bubble size from each data item (optional).</summary>
    [Parameter] public Func<T, double>? SizeField { get; set; }

    /// <summary>Point color.</summary>
    [Parameter] public string? Color { get; set; }

    /// <summary>Default point size in pixels.</summary>
    [Parameter] public double PointSize { get; set; } = 5;

    /// <summary>X-axis label text.</summary>
    [Parameter] public string? XAxisLabel { get; set; }

    /// <summary>Y-axis label text.</summary>
    [Parameter] public string? YAxisLabel { get; set; }

    /// <summary>Custom tooltip template rendered for each data point.</summary>
    [Parameter] public RenderFragment<T>? TooltipTemplate { get; set; }

    /// <summary>Trendline configuration for the scatter data.</summary>
    [Parameter] public TrendlineConfig? Trendline { get; set; }

    /// <summary>Whether to show data labels on points.</summary>
    [Parameter] public bool ShowDataLabels { get; set; } = false;

    /// <summary>Format string for data labels.</summary>
    [Parameter] public string? DataLabelFormatString { get; set; }

    /// <summary>Format string for Y-axis labels.</summary>
    [Parameter] public string? YAxisFormatString { get; set; }

    /// <summary>Format string for X-axis labels.</summary>
    [Parameter] public string? XAxisFormatString { get; set; }

    private ChartLayoutResult _layout = new();
    private LinearScale? _xScale;
    private LinearScale? _yScale;
    private string? _trendlinePath;

    private string? CssClass => CssBuilder.Default("helix-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count == 0 || XField is null || YField is null)
            return;

        var xValues = Data.Select(XField).ToList();
        var yValues = Data.Select(YField).ToList();

        _layout = LayoutEngine.Calculate(new ChartLayoutInput
        {
            Width = Width,
            Height = Height,
            Title = Title,
            XAxisTitle = XAxisLabel,
            YAxisTitle = YAxisLabel,
            YMin = yValues.Min(),
            YMax = yValues.Max()
        });

        var xTicks = TickGenerator.GenerateNumericTicks(xValues.Min(), xValues.Max(), 8);
        var yTicks = TickGenerator.GenerateNumericTicks(yValues.Min(), yValues.Max(), 8);

        _xScale = new LinearScale(xTicks.Min(), xTicks.Max(), _layout.PlotArea.X, _layout.PlotArea.X + _layout.PlotArea.Width);
        _yScale = new LinearScale(yTicks.Min(), yTicks.Max(), _layout.PlotArea.Y + _layout.PlotArea.Height, _layout.PlotArea.Y);

        // Trendline
        _trendlinePath = null;
        if (Trendline is not null && Trendline.Type != TrendlineType.None && Data.Count >= 2)
        {
            // Sort by X for trendline
            var sortedIndices = Enumerable.Range(0, Data.Count)
                .OrderBy(i => XField(Data[i]))
                .ToList();

            var sortedY = sortedIndices.Select(i => YField(Data[i])).ToList();
            double[] trendValues;

            if (Trendline.Type == TrendlineType.MovingAverage)
            {
                trendValues = TrendlineCalculator.MovingAverage(sortedY, Trendline.MovingAveragePeriod);
            }
            else
            {
                trendValues = TrendlineCalculator.LinearRegression(sortedY);
            }

            var trendPoints = new List<string>();
            for (var i = 0; i < trendValues.Length; i++)
            {
                var px = _xScale.Scale(XField(Data[sortedIndices[i]]));
                var py = _yScale.Scale(trendValues[i]);
                trendPoints.Add($"{F(px)},{F(py)}");
            }

            if (trendPoints.Count >= 2)
            {
                _trendlinePath = "M" + string.Join(" L", trendPoints);
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
}

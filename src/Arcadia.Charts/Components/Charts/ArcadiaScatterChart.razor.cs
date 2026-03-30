using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;
using Arcadia.Charts.Core.Data;
using Arcadia.Charts.Core.Layout;
using Arcadia.Charts.Core.Scales;

namespace Arcadia.Charts.Components.Charts;

public partial class ArcadiaScatterChart<T> : ChartBase<T>
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

    /// <summary>Trendline configuration for the scatter data.</summary>
    [Parameter] public TrendlineConfig? Trendline { get; set; }

    /// <summary>Opacity of data points (0.0 to 1.0). Default is 0.7.</summary>
    [Parameter] public double PointOpacity { get; set; } = 0.7;

    /// <summary>Opacity of the trendline (0.0 to 1.0). Default is 0.6.</summary>
    [Parameter] public double TrendlineOpacity { get; set; } = 0.6;

    /// <summary>Whether to show data labels on points.</summary>

    /// <summary>Format string for data labels.</summary>

    /// <summary>Format string for Y-axis labels.</summary>

    /// <summary>Format string for X-axis labels.</summary>

    private ChartLayoutResult _layout = new();
    private LinearScale? _xScale;
    private LinearScale? _yScale;
    private string? _trendlinePath;

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
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
            Width = EffectiveWidth,
            Height = Height,
            Title = Title,
            XAxisTitle = XAxisLabel,
            YAxisTitle = YAxisLabel,
            YMin = yValues.Min(),
            YMax = yValues.Max()
        });

        var xTicks = TickGenerator.GenerateNumericTicks(xValues.Min(), xValues.Max(), 8);
        var yTicks = CreateYTicks(yValues.Min(), yValues.Max());

        _xScale = new LinearScale(xTicks.Min(), xTicks.Max(), _layout.PlotArea.X, _layout.PlotArea.X + _layout.PlotArea.Width);
        _yScale = CreateYScale(yTicks.Min(), yTicks.Max(), _layout.PlotArea.Y + _layout.PlotArea.Height, _layout.PlotArea.Y);

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


    private async Task ShowScatterTooltip(double xVal, double yVal, double mouseX, double mouseY)
    {
        if (Interop is null) return;
        var xFmt = XAxisFormatString is not null ? xVal.ToString(XAxisFormatString, FormatProvider) : xVal.ToString("G4");
        var yFmt = YAxisFormatString is not null ? yVal.ToString(YAxisFormatString, FormatProvider) : yVal.ToString("G4");
        var html = $"<div>X: {xFmt}</div><div>Y: {yFmt}</div>";
        await Interop.ShowTooltipAsync(html, mouseX, mouseY);
    }
    private string GetPointStyle(int index)
    {
        var style = "";
        if (AnimateOnLoad && ShouldStagger(Data?.Count ?? 0))
        {
            var delay = GetAnimationDelay(index, Data?.Count ?? 0, 20);
            if (delay > 0) style += $"animation-delay: {delay}ms;";
        }
        if (OnPointClick.HasDelegate)
            style += "cursor:pointer;";
        return style;
    }

    private async Task HandlePointClick(T item, int index)
    {
        if (OnPointClick.HasDelegate)
            await OnPointClick.InvokeAsync(new PointClickEventArgs<T>
            {
                Item = item,
                DataIndex = index,
                SeriesIndex = -1
            });
    }

    private static string F(double v) => v.ToString("F1");
}

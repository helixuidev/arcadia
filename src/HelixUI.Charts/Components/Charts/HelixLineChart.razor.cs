using Microsoft.AspNetCore.Components;
using HelixUI.Core.Utilities;
using HelixUI.Charts.Core;
using HelixUI.Charts.Core.Layout;
using HelixUI.Charts.Core.Scales;

namespace HelixUI.Charts.Components.Charts;

public partial class HelixLineChart<T> : ChartBase<T>
{
    /// <summary>Function to extract the X-axis label from each data item.</summary>
    [Parameter] public Func<T, object>? XField { get; set; }

    /// <summary>X-axis label text.</summary>
    [Parameter] public string? XAxisLabel { get; set; }

    /// <summary>Y-axis label text.</summary>
    [Parameter] public string? YAxisLabel { get; set; }

    /// <summary>Series configurations.</summary>
    [Parameter] public List<SeriesConfig<T>>? Series { get; set; }

    /// <summary>Whether to show data points on the line.</summary>
    [Parameter] public bool ShowPoints { get; set; } = true;

    private ChartLayoutResult _layout = new();
    private LinearScale? _yScale;
    private List<string> _xLabels = new();
    private Dictionary<int, string> _paths = new();
    private Dictionary<int, string> _areaPaths = new();

    private string? CssClass => CssBuilder.Default("helix-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count < 2 || XField is null || Series is null)
            return;

        // Build X labels
        _xLabels = Data.Select(d => XField(d)?.ToString() ?? "").ToList();

        // Calculate Y range across all series
        var allYValues = Series.SelectMany(s => Data.Select(d => s.Field(d))).ToList();
        var yMin = allYValues.Min();
        var yMax = allYValues.Max();
        if (yMin > 0) yMin = 0; // Include zero baseline

        // Run layout engine
        _layout = LayoutEngine.Calculate(new ChartLayoutInput
        {
            Width = Width,
            Height = Height,
            Title = Title,
            XAxisTitle = XAxisLabel,
            YAxisTitle = YAxisLabel,
            XTickLabels = _xLabels,
            YMin = yMin,
            YMax = yMax,
            SeriesNames = Series.Select(s => s.Name).ToList()
        });

        // Create Y scale (inverted — SVG Y goes down)
        var yTicks = TickGenerator.GenerateNumericTicks(yMin, yMax, 8);
        _yScale = new LinearScale(
            yTicks.Min(), yTicks.Max(),
            _layout.PlotArea.Y + _layout.PlotArea.Height,
            _layout.PlotArea.Y);

        // Build SVG paths for each series
        _paths.Clear();
        _areaPaths.Clear();
        for (var si = 0; si < Series.Count; si++)
        {
            var series = Series[si];
            var points = new List<string>();
            for (var i = 0; i < Data.Count; i++)
            {
                var x = _layout.PlotArea.X + (i + 0.5) * (_layout.PlotArea.Width / Data.Count);
                var y = _yScale.Scale(series.Field(Data[i]));
                points.Add($"{F(x)},{F(y)}");
            }

            _paths[si] = "M" + string.Join(" L", points);

            if (series.ShowArea)
            {
                var firstX = _layout.PlotArea.X + 0.5 * (_layout.PlotArea.Width / Data.Count);
                var lastX = _layout.PlotArea.X + (Data.Count - 0.5) * (_layout.PlotArea.Width / Data.Count);
                var baseY = _layout.PlotArea.Y + _layout.PlotArea.Height;
                _areaPaths[si] = _paths[si] + $" L{F(lastX)},{F(baseY)} L{F(firstX)},{F(baseY)} Z";
            }
        }
    }

    private static string F(double v) => v.ToString("F1");
}

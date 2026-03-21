using Microsoft.AspNetCore.Components;
using HelixUI.Core.Utilities;
using HelixUI.Charts.Core;
using HelixUI.Charts.Core.Layout;
using HelixUI.Charts.Core.Scales;

namespace HelixUI.Charts.Components.Charts;

public partial class HelixScatterChart<T> : ChartBase<T>
{
    [Parameter] public Func<T, double>? XField { get; set; }
    [Parameter] public Func<T, double>? YField { get; set; }
    [Parameter] public Func<T, double>? SizeField { get; set; }
    [Parameter] public string? Color { get; set; }
    [Parameter] public double PointSize { get; set; } = 5;
    [Parameter] public string? XAxisLabel { get; set; }
    [Parameter] public string? YAxisLabel { get; set; }

    private ChartLayoutResult _layout = new();
    private LinearScale? _xScale;
    private LinearScale? _yScale;

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
    }

    private static string F(double v) => v.ToString("F1");
}

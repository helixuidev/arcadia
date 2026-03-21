using Microsoft.AspNetCore.Components;
using HelixUI.Core.Utilities;
using HelixUI.Charts.Core;
using HelixUI.Charts.Core.Layout;
using HelixUI.Charts.Core.Scales;

namespace HelixUI.Charts.Components.Charts;

public partial class HelixBarChart<T> : ChartBase<T>
{
    [Parameter] public Func<T, object>? XField { get; set; }
    [Parameter] public List<SeriesConfig<T>>? Series { get; set; }
    [Parameter] public bool Rounded { get; set; } = true;
    [Parameter] public bool Stacked { get; set; }

    private ChartLayoutResult _layout = new();
    private LinearScale? _yScale;
    private BandScale? _bandScale;

    private string? CssClass => CssBuilder.Default("helix-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count == 0 || XField is null || Series is null)
            return;

        var categories = Data.Select(d => XField(d)?.ToString() ?? "").ToList();
        var allYValues = Series.SelectMany(s => Data.Select(d => s.Field(d))).ToList();
        var yMax = allYValues.Max();
        var yMin = Math.Min(0, allYValues.Min());

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
    }

    private static string F(double v) => v.ToString("F1");
}

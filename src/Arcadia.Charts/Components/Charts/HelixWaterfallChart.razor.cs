using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;
using Arcadia.Charts.Core.Layout;
using Arcadia.Charts.Core.Scales;

namespace Arcadia.Charts.Components.Charts;

public partial class HelixWaterfallChart<T> : ChartBase<T>
{
    [Parameter] public Func<T, string>? CategoryField { get; set; }
    [Parameter] public Func<T, double>? ValueField { get; set; }
    [Parameter] public string PositiveColor { get; set; } = "var(--arcadia-color-success, #16a34a)";
    [Parameter] public string NegativeColor { get; set; } = "var(--arcadia-color-danger, #dc2626)";
    [Parameter] public string TotalColor { get; set; } = "var(--arcadia-color-primary, #2563eb)";

    private ElementReference _containerRef;
    private ChartLayoutResult _layout = new();
    private LinearScale? _yScale;
    private List<WaterfallBar> _bars = new();

    protected override void OnParametersSet()
    {
        if (!HasData || CategoryField is null || ValueField is null) return;

        var labels = Data!.Select(d => CategoryField(d)).ToList();
        var values = Data!.Select(d => ValueField(d)).ToList();

        double running = 0;
        var barData = new List<(string Label, double Value, double Start, double End)>();
        foreach (var item in Data!)
        {
            var val = ValueField(item);
            var start = running;
            running += val;
            barData.Add((CategoryField(item), val, start, running));
        }

        var allValues = barData.SelectMany(b => new[] { b.Start, b.End }).ToList();
        allValues.Add(0);
        var yMin = allValues.Min();
        var yMax = allValues.Max();

        _layout = LayoutEngine.Calculate(new ChartLayoutInput
        {
            Width = Width, Height = Height, Title = Title,
            XTickLabels = labels, YMin = yMin, YMax = yMax
        });

        var yTicks = TickGenerator.GenerateNumericTicks(yMin, yMax, YAxisMaxTicks);
        _yScale = new LinearScale(yTicks.Min(), yTicks.Max(),
            _layout.PlotArea.Y + _layout.PlotArea.Height, _layout.PlotArea.Y);

        var bandScale = new BandScale(labels, _layout.PlotArea.X, _layout.PlotArea.X + _layout.PlotArea.Width, 0.2);

        _bars.Clear();
        for (var i = 0; i < barData.Count; i++)
        {
            var (label, value, start, end) = barData[i];
            var x = bandScale.Scale(label);
            var topY = _yScale.Scale(Math.Max(start, end));
            var bottomY = _yScale.Scale(Math.Min(start, end));
            var color = value >= 0 ? PositiveColor : NegativeColor;

            _bars.Add(new WaterfallBar
            {
                X = x, Y = topY, W = bandScale.BandWidth, H = Math.Max(1, bottomY - topY),
                Color = color, Label = label, Value = value, RunningTotal = end,
                ConnectY = _yScale.Scale(end)
            });
        }
    }

    private static string F(double v) => v.ToString("F1");

    private class WaterfallBar
    {
        public double X, Y, W, H, ConnectY, Value, RunningTotal;
        public string Color = "", Label = "";
    }
}

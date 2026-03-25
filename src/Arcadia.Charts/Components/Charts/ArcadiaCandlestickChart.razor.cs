using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;
using Arcadia.Charts.Core.Layout;
using Arcadia.Charts.Core.Scales;

namespace Arcadia.Charts.Components.Charts;

/// <summary>
/// Candlestick (OHLC) chart for financial data. Supports overlay line series
/// for indicators like moving averages.
/// </summary>
public partial class ArcadiaCandlestickChart<T> : ChartBase<T>
{
    /// <summary>Accessor that extracts the X-axis label (e.g., date or time period) from each data item. When null, labels are omitted from the category axis.</summary>
    [Parameter] public Func<T, string>? LabelField { get; set; }

    /// <summary>Accessor that extracts the opening price from each data item. Required for rendering candlestick bodies.</summary>
    [Parameter] public Func<T, double>? OpenField { get; set; }

    /// <summary>Accessor that extracts the highest price from each data item. Determines the top of the wick line.</summary>
    [Parameter] public Func<T, double>? HighField { get; set; }

    /// <summary>Accessor that extracts the lowest price from each data item. Determines the bottom of the wick line.</summary>
    [Parameter] public Func<T, double>? LowField { get; set; }

    /// <summary>Accessor that extracts the closing price from each data item. Together with OpenField, determines candle body direction and color.</summary>
    [Parameter] public Func<T, double>? CloseField { get; set; }

    /// <summary>Color for up (close > open) candles.</summary>
    [Parameter] public string UpColor { get; set; } = "var(--arcadia-color-success, #16a34a)";

    /// <summary>Color for down (close &lt; open) candles.</summary>
    [Parameter] public string DownColor { get; set; } = "var(--arcadia-color-danger, #dc2626)";

    /// <summary>Optional overlay line series (e.g., moving average, Bollinger bands).</summary>
    [Parameter] public List<SeriesConfig<T>>? OverlaySeries { get; set; }

    /// <summary>Stroke width of the wick (high-low) line on each candlestick.</summary>
    [Parameter] public double WickWidth { get; set; } = 1.5;

    /// <summary>Candle body width as a fraction of the available band space (0-1). Controls how wide each candlestick body is relative to its slot.</summary>
    [Parameter] public double CandleWidthRatio { get; set; } = 0.35;

    /// <summary>Format string for Y-axis labels.</summary>

    private ChartLayoutResult _layout = new();
    private LinearScale? _yScale;
    private List<CandleData> _candles = new();
    private Dictionary<int, string> _overlayPaths = new();
    private bool _animate;

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count == 0 || OpenField is null || HighField is null || LowField is null || CloseField is null)
            return;

        _animate = AnimateOnLoad;

        var labels = Data.Select(d => LabelField?.Invoke(d) ?? "").ToList();
        var allValues = Data.SelectMany(d => new[] { HighField(d), LowField(d) }).ToList();
        var yMin = allValues.Min();
        var yMax = allValues.Max();
        var padding = (yMax - yMin) * 0.05;

        _layout = LayoutEngine.Calculate(new ChartLayoutInput
        {
            Width = EffectiveWidth,
            Height = Height,
            Title = Title,
            XTickLabels = labels,
            YMin = yMin - padding,
            YMax = yMax + padding,
            SeriesNames = OverlaySeries?.Select(s => s.Name).ToList()
        });

        var yTicks = TickGenerator.GenerateNumericTicks(yMin - padding, yMax + padding, 8);
        _yScale = new LinearScale(yTicks.Min(), yTicks.Max(),
            _layout.PlotArea.Y + _layout.PlotArea.Height, _layout.PlotArea.Y);

        // Build candles
        _candles.Clear();
        var candleWidth = _layout.PlotArea.Width / Data.Count;
        var halfWidth = Math.Max(2, candleWidth * CandleWidthRatio);

        for (var i = 0; i < Data.Count; i++)
        {
            var item = Data[i];
            var open = OpenField(item);
            var high = HighField(item);
            var low = LowField(item);
            var close = CloseField(item);
            var isUp = close >= open;

            var x = _layout.PlotArea.X + (i + 0.5) * candleWidth;
            var openY = _yScale.Scale(open);
            var closeY = _yScale.Scale(close);
            var highY = _yScale.Scale(high);
            var lowY = _yScale.Scale(low);

            _candles.Add(new CandleData
            {
                X = x,
                HalfWidth = halfWidth,
                HighY = highY,
                LowY = lowY,
                BodyTop = Math.Min(openY, closeY),
                BodyHeight = Math.Abs(openY - closeY),
                Color = isUp ? UpColor : DownColor
            });
        }

        // Build overlay line paths
        _overlayPaths.Clear();
        if (OverlaySeries is not null)
        {
            for (var si = 0; si < OverlaySeries.Count; si++)
            {
                var series = OverlaySeries[si];
                var points = new List<string>();
                for (var i = 0; i < Data.Count; i++)
                {
                    var value = series.Field(Data[i]);
                    if (double.IsNaN(value)) continue;
                    var x = _layout.PlotArea.X + (i + 0.5) * candleWidth;
                    var y = _yScale.Scale(value);
                    points.Add($"{F(x)},{F(y)}");
                }
                if (points.Count >= 2)
                    _overlayPaths[si] = "M" + string.Join(" L", points);
            }
        }
    }

    private string FormatYTick(double value)
    {
        if (YAxisFormatString is not null)
            return value.ToString(YAxisFormatString, FormatProvider);
        return _layout.YTicks.FirstOrDefault(t => Math.Abs(t.Value - value) < double.Epsilon)?.Label ?? value.ToString("G4");
    }


    private async Task ShowCandleTooltip(int index, double mouseX, double mouseY)
    {
        if (Interop is null || Data is null || index >= Data.Count) return;
        var item = Data[index];
        var label = LabelField?.Invoke(item) ?? "";
        var o = OpenField!(item); var h = HighField!(item); var l = LowField!(item); var c = CloseField!(item);
        var fmt = YAxisFormatString ?? "F2";
        var html = $"<div style='font-weight:600;margin-bottom:4px'>{label}</div>" +
                   $"<div>O: {o.ToString(fmt)} H: {h.ToString(fmt)}</div>" +
                   $"<div>L: {l.ToString(fmt)} C: {c.ToString(fmt)}</div>";
        await Interop.ShowTooltipAsync(html, mouseX, mouseY);
    }
    private static string F(double v) => v.ToString("F1");

    private class CandleData
    {
        public double X { get; set; }
        public double HalfWidth { get; set; }
        public double HighY { get; set; }
        public double LowY { get; set; }
        public double BodyTop { get; set; }
        public double BodyHeight { get; set; }
        public string Color { get; set; } = "";
    }
}

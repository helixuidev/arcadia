using Microsoft.AspNetCore.Components;
using Arcadia.Charts.Core;

namespace Arcadia.Charts.Components.Charts;

public partial class HelixFunnelChart<T> : ChartBase<T>
{
    [Parameter] public Func<T, string>? NameField { get; set; }
    [Parameter] public Func<T, double>? ValueField { get; set; }

    private List<FunnelStage> _stages = new();

    protected override void OnParametersSet()
    {
        if (!HasData || NameField is null || ValueField is null) return;

        _stages.Clear();
        var maxValue = Data!.Max(ValueField);
        if (maxValue <= 0) return;

        var topPad = string.IsNullOrEmpty(Title) ? 20 : 44;
        var availableHeight = Height - topPad - 20;
        var stageHeight = availableHeight / Data!.Count;
        var cx = Width / 2;
        var maxHalfWidth = (Width - 80) / 2;

        for (var i = 0; i < Data!.Count; i++)
        {
            var item = Data[i];
            var value = ValueField(item);
            var nextValue = i < Data.Count - 1 ? ValueField(Data[i + 1]) : value * 0.7;

            var topWidth = (value / maxValue) * maxHalfWidth;
            var bottomWidth = (nextValue / maxValue) * maxHalfWidth;
            if (i == Data.Count - 1) bottomWidth = topWidth * 0.6;

            var y1 = topPad + i * stageHeight;
            var y2 = y1 + stageHeight;

            var path = $"M{F(cx - topWidth)},{F(y1)} L{F(cx + topWidth)},{F(y1)} " +
                       $"L{F(cx + bottomWidth)},{F(y2)} L{F(cx - bottomWidth)},{F(y2)} Z";

            _stages.Add(new FunnelStage
            {
                Name = NameField(item),
                Value = value,
                Percent = value / maxValue * 100,
                Path = path,
                Color = EffectivePalette.GetColor(i),
                LabelY = (y1 + y2) / 2 - 4
            });
        }
    }

    private static string F(double v) => v.ToString("F1");

    private class FunnelStage
    {
        public string Name = "", Path = "", Color = "";
        public double Value, Percent, LabelY;
    }
}

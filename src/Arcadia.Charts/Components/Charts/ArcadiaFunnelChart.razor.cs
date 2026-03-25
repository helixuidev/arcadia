using Microsoft.AspNetCore.Components;
using Arcadia.Charts.Core;

namespace Arcadia.Charts.Components.Charts;

/// <summary>
/// Funnel chart that visualizes progressive narrowing across sequential stages,
/// such as sales pipelines or conversion funnels.
/// </summary>
public partial class ArcadiaFunnelChart<T> : ChartBase<T>
{
    /// <summary>Accessor that extracts the display name for each funnel stage from a data item. Rendered as the centered label inside the stage shape.</summary>
    [Parameter] public Func<T, string>? NameField { get; set; }

    /// <summary>Accessor that extracts the numeric value for each funnel stage. Values determine the relative width of each stage; the largest value produces the widest section.</summary>
    [Parameter] public Func<T, double>? ValueField { get; set; }

    /// <summary>Opacity for each funnel stage shape. Default is 0.85.</summary>
    [Parameter] public double StageOpacity { get; set; } = 0.85;

    /// <summary>Fill color for the stage name label text. Default is "white".</summary>
    [Parameter] public string StageLabelColor { get; set; } = "white";

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
        var cx = EffectiveWidth / 2;
        var maxHalfWidth = (EffectiveWidth - 80) / 2;

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

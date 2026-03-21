using Microsoft.AspNetCore.Components;
using HelixUI.Core.Utilities;
using HelixUI.Charts.Core;

namespace HelixUI.Charts.Components.Charts;

public partial class HelixPieChart<T> : ChartBase<T>
{
    [Parameter] public Func<T, string>? NameField { get; set; }
    [Parameter] public Func<T, double>? ValueField { get; set; }
    [Parameter] public double InnerRadius { get; set; } = 0;
    [Parameter] public bool ShowLabels { get; set; } = true;
    [Parameter] public bool ShowLegend { get; set; } = true;

    private List<SliceData> _slices = new();

    private string? CssClass => CssBuilder.Default("helix-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count == 0 || NameField is null || ValueField is null)
            return;

        _slices.Clear();
        var total = Data.Sum(ValueField);
        if (total <= 0) return;

        var cx = Width / 2;
        var cy = (Height + (string.IsNullOrEmpty(Title) ? 0 : 30)) / 2;
        var outerRadius = Math.Min(Width, Height) / 2 - 40;
        var innerR = InnerRadius > 0 ? InnerRadius : 0;

        var startAngle = -Math.PI / 2; // Start at top

        for (var i = 0; i < Data.Count; i++)
        {
            var value = ValueField(Data[i]);
            var percent = value / total * 100;
            var sweepAngle = value / total * 2 * Math.PI;
            var endAngle = startAngle + sweepAngle;

            var path = BuildArcPath(cx, cy, outerRadius, innerR, startAngle, endAngle);

            // Label position at midpoint of arc, 70% from center
            var midAngle = startAngle + sweepAngle / 2;
            var labelR = innerR > 0 ? (outerRadius + innerR) / 2 : outerRadius * 0.65;

            _slices.Add(new SliceData
            {
                Name = NameField(Data[i]),
                Value = value,
                Percent = percent,
                Color = ResolveColor(null, i),
                Path = path,
                LabelX = cx + Math.Cos(midAngle) * labelR,
                LabelY = cy + Math.Sin(midAngle) * labelR + 4
            });

            startAngle = endAngle;
        }
    }

    private static string BuildArcPath(double cx, double cy, double outerR, double innerR, double startAngle, double endAngle)
    {
        var largeArc = (endAngle - startAngle) > Math.PI ? 1 : 0;

        var x1 = cx + Math.Cos(startAngle) * outerR;
        var y1 = cy + Math.Sin(startAngle) * outerR;
        var x2 = cx + Math.Cos(endAngle) * outerR;
        var y2 = cy + Math.Sin(endAngle) * outerR;

        if (innerR > 0)
        {
            var x3 = cx + Math.Cos(endAngle) * innerR;
            var y3 = cy + Math.Sin(endAngle) * innerR;
            var x4 = cx + Math.Cos(startAngle) * innerR;
            var y4 = cy + Math.Sin(startAngle) * innerR;

            return $"M{F(x1)},{F(y1)} A{F(outerR)},{F(outerR)} 0 {largeArc},1 {F(x2)},{F(y2)} " +
                   $"L{F(x3)},{F(y3)} A{F(innerR)},{F(innerR)} 0 {largeArc},0 {F(x4)},{F(y4)} Z";
        }

        return $"M{F(cx)},{F(cy)} L{F(x1)},{F(y1)} A{F(outerR)},{F(outerR)} 0 {largeArc},1 {F(x2)},{F(y2)} Z";
    }

    private static string F(double v) => v.ToString("F2");

    private class SliceData
    {
        public string Name { get; set; } = "";
        public double Value { get; set; }
        public double Percent { get; set; }
        public string Color { get; set; } = "";
        public string Path { get; set; } = "";
        public double LabelX { get; set; }
        public double LabelY { get; set; }
    }
}

using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;

namespace Arcadia.Charts.Components.Charts;

/// <summary>
/// Rose (polar area) chart where each category gets an equal-angle sector
/// with radius proportional to its value.
/// </summary>
public partial class ArcadiaRoseChart<T> : ChartBase<T>
{
    /// <summary>Function to extract the category name from each data item.</summary>
    [Parameter] public Func<T, string>? NameField { get; set; }

    /// <summary>Function to extract the numeric value from each data item (determines radius).</summary>
    [Parameter] public Func<T, double>? ValueField { get; set; }

    /// <summary>Whether to show category labels on each sector.</summary>
    [Parameter] public bool ShowLabels { get; set; } = true;

    /// <summary>Stroke width for each rose slice border. Default is 2.</summary>
    [Parameter] public double SliceStrokeWidth { get; set; } = 2;

    /// <summary>Stroke color for each rose slice border. Default is "var(--arcadia-color-surface, #fff)".</summary>
    [Parameter] public string SliceStrokeColor { get; set; } = "var(--arcadia-color-surface, #fff)";

    private List<SectorData> _sectors = new();

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count == 0 || NameField is null || ValueField is null)
            return;

        _sectors.Clear();
        var maxValue = Data.Max(ValueField);
        if (maxValue <= 0) return;

        var cx = EffectiveWidth / 2;
        var cy = (Height + (string.IsNullOrEmpty(Title) ? 0 : 30)) / 2;
        var maxRadius = Math.Min(EffectiveWidth, Height) / 2 - 40;

        var n = Data.Count;
        var sweepAngle = 2 * Math.PI / n;
        var startAngle = -Math.PI / 2; // Start at top

        for (var i = 0; i < n; i++)
        {
            var item = Data[i];
            var value = ValueField(item);
            var radius = (value / maxValue) * maxRadius;
            var endAngle = startAngle + sweepAngle;

            var path = BuildArcPath(cx, cy, radius, startAngle, endAngle);

            // Label position at midpoint of arc, slightly beyond radius
            var midAngle = startAngle + sweepAngle / 2;
            var labelR = Math.Max(radius * 0.65, maxRadius * 0.3);

            _sectors.Add(new SectorData
            {
                Name = NameField(item),
                Value = value,
                Color = EffectivePalette.GetColor(i),
                Path = path,
                LabelX = cx + Math.Cos(midAngle) * labelR,
                LabelY = cy + Math.Sin(midAngle) * labelR + 4
            });

            startAngle = endAngle;
        }
    }

    private async Task ShowRoseTooltip(string name, double value, double x, double y)
    {
        if (Interop is null) return;
        var formatted = DataLabelFormatString is not null
            ? value.ToString(DataLabelFormatString, FormatProvider)
            : value.ToString("N0");
        var html = $"<div style='font-weight:600;margin-bottom:2px'>{name}</div><div>{formatted}</div>";
        await Interop.ShowTooltipAsync(html, x, y);
    }

    private static string BuildArcPath(double cx, double cy, double r, double startAngle, double endAngle)
    {
        var largeArc = (endAngle - startAngle) > Math.PI ? 1 : 0;

        var x1 = cx + Math.Cos(startAngle) * r;
        var y1 = cy + Math.Sin(startAngle) * r;
        var x2 = cx + Math.Cos(endAngle) * r;
        var y2 = cy + Math.Sin(endAngle) * r;

        return $"M{F(cx)},{F(cy)} L{F(x1)},{F(y1)} A{F(r)},{F(r)} 0 {largeArc},1 {F(x2)},{F(y2)} Z";
    }

    private static string F(double v) => v.ToString("F1");

    private class SectorData
    {
        public string Name { get; set; } = "";
        public double Value { get; set; }
        public string Color { get; set; } = "";
        public string Path { get; set; } = "";
        public double LabelX { get; set; }
        public double LabelY { get; set; }
    }
}

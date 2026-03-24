using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;

namespace Arcadia.Charts.Components.Charts;

public partial class ArcadiaPieChart<T> : ChartBase<T>
{
    /// <summary>Function to extract the category name from each data item.</summary>
    [Parameter] public Func<T, string>? NameField { get; set; }

    /// <summary>Function to extract the numeric value from each data item.</summary>
    [Parameter] public Func<T, double>? ValueField { get; set; }

    /// <summary>Inner radius for donut charts. 0 = pie chart.</summary>
    [Parameter] public double InnerRadius { get; set; } = 0;

    /// <summary>Whether to show labels on slices.</summary>
    [Parameter] public bool ShowLabels { get; set; } = true;

    /// <summary>Label format for pie slices.</summary>
    [Parameter] public PieLabelFormat LabelFormat { get; set; } = PieLabelFormat.Percent;

    /// <summary>Format string for value labels (e.g., "C0" for currency).</summary>
    [Parameter] public string? ValueFormatString { get; set; }

    /// <summary>Minimum percentage for a slice to show its label. Smaller slices are label-free.</summary>
    [Parameter] public double MinLabelPercent { get; set; } = 5;

    /// <summary>Stroke width for each pie slice border. Default is 2.</summary>
    [Parameter] public double SliceStrokeWidth { get; set; } = 2;

    /// <summary>Stroke color for each pie slice border. Default is "var(--arcadia-color-surface, #fff)".</summary>
    [Parameter] public string SliceStrokeColor { get; set; } = "var(--arcadia-color-surface, #fff)";

    /// <summary>Fired when a pie slice is clicked.</summary>
    [Parameter] public EventCallback<T> OnSliceClick { get; set; }

    /// <summary>Outer radius override. Null = auto from dimensions.</summary>
    [Parameter] public double? OuterRadius { get; set; }

    /// <summary>Padding angle between slices in degrees.</summary>
    [Parameter] public double PaddingAngle { get; set; } = 0;

    /// <summary>Start angle in degrees (0 = top, 90 = right). Default -90 starts at top.</summary>
    [Parameter] public double StartAngle { get; set; } = -90;

    private List<SliceData> _slices = new();

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count == 0 || NameField is null || ValueField is null)
            return;

        _slices.Clear();
        var total = Data.Sum(ValueField);
        if (total <= 0) return;

        var cx = EffectiveWidth / 2;
        var cy = (Height + (string.IsNullOrEmpty(Title) ? 0 : 30)) / 2;
        var outerRadius = OuterRadius ?? Math.Min(EffectiveWidth, Height) / 2 - 40;
        var innerR = InnerRadius > 0 ? InnerRadius : 0;

        var startAngle = StartAngle * Math.PI / 180; // Convert degrees to radians

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

            var name = NameField(Data[i]);
            _slices.Add(new SliceData
            {
                Name = name,
                Value = value,
                Percent = percent,
                Color = ResolveColor(null, i),
                Path = path,
                LabelX = cx + Math.Cos(midAngle) * labelR,
                LabelY = cy + Math.Sin(midAngle) * labelR + 4,
                LabelText = FormatPieLabel(name, value, percent),
                DataIndex = i
            });

            startAngle = endAngle;
        }
    }

    private string FormatPieLabel(string name, double value, double percent)
    {
        var formattedValue = ValueFormatString is not null
            ? value.ToString(ValueFormatString, FormatProvider)
            : value.ToString("N0");

        return LabelFormat switch
        {
            PieLabelFormat.Percent => $"{percent:F0}%",
            PieLabelFormat.Value => formattedValue,
            PieLabelFormat.Name => name,
            PieLabelFormat.NamePercent => $"{name} {percent:F0}%",
            PieLabelFormat.NameValue => $"{name} {formattedValue}",
            _ => ""
        };
    }


    private async Task HandleSliceClick(int index)
    {
        if (Data is null || index < 0 || index >= Data.Count) return;
        if (OnSliceClick.HasDelegate)
            await OnSliceClick.InvokeAsync(Data[index]);
        if (OnPointClick.HasDelegate)
            await OnPointClick.InvokeAsync(new PointClickEventArgs<T>
            {
                Item = Data[index],
                DataIndex = index,
                SeriesIndex = -1
            });
    }

    private async Task ShowPieTooltip(string name, double value, double percent, double x, double y)
    {
        if (Interop is null) return;
        var formatted = ValueFormatString is not null ? value.ToString(ValueFormatString, FormatProvider) : value.ToString("N0");
        var html = $"<div style='font-weight:600;margin-bottom:2px'>{name}</div><div>{formatted} ({percent:F1}%)</div>";
        await Interop.ShowTooltipAsync(html, x, y);
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

    private string GetSliceStyle(int index)
    {
        var style = "";
        if (AnimateOnLoad)
            style += "animation-delay: " + (index * 100) + "ms;";
        if (OnPointClick.HasDelegate || OnSliceClick.HasDelegate)
            style += "cursor:pointer;";
        return style;
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
        public string LabelText { get; set; } = "";
        public int DataIndex { get; set; }
    }
}

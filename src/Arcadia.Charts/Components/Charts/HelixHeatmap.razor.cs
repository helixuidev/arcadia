using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;
using Arcadia.Charts.Core.Layout;

namespace Arcadia.Charts.Components.Charts;

/// <summary>
/// Renders a 2D heatmap grid with a color scale legend.
/// Each cell is colored based on its value relative to the data range.
/// </summary>
public partial class HelixHeatmap<T> : ChartBase<T>
{
    /// <summary>Function to extract the X category (column) from each data item.</summary>
    [Parameter] public Func<T, string>? XField { get; set; }

    /// <summary>Function to extract the Y category (row) from each data item.</summary>
    [Parameter] public Func<T, string>? YField { get; set; }

    /// <summary>Function to extract the numeric value from each data item.</summary>
    [Parameter] public Func<T, double>? ValueField { get; set; }

    /// <summary>Low end color of the color scale (hex color).</summary>
    [Parameter] public string LowColor { get; set; } = "#f1f5f9";

    /// <summary>High end color of the color scale (hex color).</summary>
    [Parameter] public string HighColor { get; set; } = "#2563eb";

    /// <summary>Whether to show values in each cell.</summary>
    [Parameter] public bool ShowValues { get; set; } = false;

    /// <summary>Format string for cell values.</summary>
    [Parameter] public string? ValueFormatString { get; set; }

    /// <summary>Custom tooltip template for cells.</summary>

    private List<string> _xCategories = new();
    private List<string> _yCategories = new();
    private List<CellData> _cells = new();
    private double _minValue;
    private double _maxValue;
    private double _plotX;
    private double _plotY;
    private double _plotWidth;
    private double _plotHeight;
    private double _cellWidth;
    private double _cellHeight;

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count == 0 || XField is null || YField is null || ValueField is null)
            return;

        // Extract unique categories preserving order of appearance
        _xCategories = Data.Select(d => XField(d)).Distinct().ToList();
        _yCategories = Data.Select(d => YField(d)).Distinct().ToList();

        // Find data range
        var values = Data.Select(d => ValueField(d)).ToList();
        _minValue = values.Min();
        _maxValue = values.Max();

        // Calculate layout areas
        var titleOffset = string.IsNullOrEmpty(Title) ? 8.0 : 32.0;
        var maxYLabelWidth = _yCategories.Max(l => TextMeasure.EstimateWidth(l, 11));
        var legendWidth = 50.0;

        _plotX = maxYLabelWidth + 12;
        _plotY = titleOffset;
        _plotWidth = Width - _plotX - legendWidth - 16;
        _plotHeight = Height - _plotY - 30; // Room for X labels

        if (_plotWidth < 10) _plotWidth = 10;
        if (_plotHeight < 10) _plotHeight = 10;

        _cellWidth = _xCategories.Count > 0 ? _plotWidth / _xCategories.Count : 0;
        _cellHeight = _yCategories.Count > 0 ? _plotHeight / _yCategories.Count : 0;

        // Build cell data
        _cells.Clear();
        foreach (var item in Data)
        {
            var xCat = XField(item);
            var yCat = YField(item);
            var value = ValueField(item);
            var xi = _xCategories.IndexOf(xCat);
            var yi = _yCategories.IndexOf(yCat);
            if (xi < 0 || yi < 0) continue;

            var color = InterpolateColor(value);
            var displayValue = ValueFormatString is not null
                ? value.ToString(ValueFormatString)
                : value.ToString("G4");

            _cells.Add(new CellData
            {
                X = _plotX + xi * _cellWidth,
                Y = _plotY + yi * _cellHeight,
                Color = color,
                Value = value,
                DisplayValue = displayValue,
                XCategory = xCat,
                YCategory = yCat,
                Index = _cells.Count
            });
        }
    }

    private string InterpolateColor(double value)
    {
        if (Math.Abs(_maxValue - _minValue) < double.Epsilon)
            return HighColor;

        var ratio = (_maxValue - _minValue) > 0 ? (value - _minValue) / (_maxValue - _minValue) : 0;
        ratio = Math.Max(0, Math.Min(1, ratio));

        var (lr, lg, lb) = ParseHexColor(LowColor);
        var (hr, hg, hb) = ParseHexColor(HighColor);

        var r = (int)(lr + (hr - lr) * ratio);
        var g = (int)(lg + (hg - lg) * ratio);
        var b = (int)(lb + (hb - lb) * ratio);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static (int R, int G, int B) ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length < 6) return (128, 128, 128);
        return (
            Convert.ToInt32(hex.Substring(0, 2), 16),
            Convert.ToInt32(hex.Substring(2, 2), 16),
            Convert.ToInt32(hex.Substring(4, 2), 16)
        );
    }

    /// <summary>Gets a formatted value for the scale legend at a given ratio (0-1).</summary>
    private string GetScaleLabel(double ratio)
    {
        var value = _minValue + (_maxValue - _minValue) * ratio;
        return ValueFormatString is not null
            ? value.ToString(ValueFormatString)
            : value.ToString("G3");
    }

    private static string F(double v) => v.ToString("F1");

    private class CellData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Color { get; set; } = "";
        public double Value { get; set; }
        public string DisplayValue { get; set; } = "";
        public string XCategory { get; set; } = "";
        public string YCategory { get; set; } = "";
        public int Index { get; set; }
    }
}

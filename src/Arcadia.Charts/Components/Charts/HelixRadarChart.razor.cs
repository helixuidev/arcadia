using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core;

namespace Arcadia.Charts.Components.Charts;

/// <summary>
/// Renders a radar/spider chart with multi-series support.
/// Each series is drawn as a polygon on a circular axis grid.
/// </summary>
public partial class HelixRadarChart<T> : ChartBase<T>
{
    /// <summary>Function to extract the axis label from each data item.</summary>
    [Parameter] public Func<T, string>? LabelField { get; set; }

    /// <summary>Series configurations (each with a Field to extract the value for each axis).</summary>
    [Parameter] public List<SeriesConfig<T>>? Series { get; set; }

    /// <summary>Number of concentric grid rings to show.</summary>
    [Parameter] public int GridRings { get; set; } = 5;

    /// <summary>Whether to fill the area inside the polygon.</summary>
    [Parameter] public bool ShowFill { get; set; } = true;

    /// <summary>Whether to show data points on the polygon vertices.</summary>
    [Parameter] public bool ShowPoints { get; set; } = true;

    /// <summary>Whether to show the legend.</summary>

    /// <summary>Custom tooltip template for data points.</summary>

    private double _cx;
    private double _cy;
    private double _radius;
    private int _axisCount;
    private double _maxValue;
    private List<string> _labels = new();
    private List<SeriesPolygon> _polygons = new();

    private string? CssClass => CssBuilder.Default("arcadia-chart__svg")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        if (Data is null || Data.Count == 0 || LabelField is null || Series is null || Series.Count == 0)
            return;

        _axisCount = Data.Count;
        _labels = Data.Select(d => LabelField(d)).ToList();

        // Calculate center and radius
        var titleOffset = string.IsNullOrEmpty(Title) ? 0 : 30;
        _cx = Width / 2;
        _cy = (Height + titleOffset) / 2;
        _radius = Math.Min(Width, Height) / 2 - 50;

        // Find max value across all series
        _maxValue = Series.SelectMany(s => Data.Select(d => s.Field(d))).Max();
        if (_maxValue <= 0) _maxValue = 1;

        // Build polygon data for each series
        _polygons.Clear();
        for (var si = 0; si < Series.Count; si++)
        {
            var series = Series[si];
            var color = ResolveColor(series.Color, si);
            var points = new List<(double X, double Y)>();

            for (var i = 0; i < _axisCount; i++)
            {
                var value = series.Field(Data[i]);
                var ratio = value / _maxValue;
                var angle = 2 * Math.PI * i / _axisCount - Math.PI / 2;
                var x = _cx + Math.Cos(angle) * _radius * ratio;
                var y = _cy + Math.Sin(angle) * _radius * ratio;
                points.Add((x, y));
            }

            _polygons.Add(new SeriesPolygon
            {
                Color = color,
                Opacity = series.AreaOpacity,
                StrokeWidth = series.StrokeWidth,
                Points = points,
                PointsString = string.Join(" ", points.Select(p => $"{F(p.X)},{F(p.Y)}"))
            });
        }
    }

    private (double X, double Y) GetAxisEndpoint(int index)
    {
        var angle = 2 * Math.PI * index / _axisCount - Math.PI / 2;
        return (_cx + Math.Cos(angle) * _radius, _cy + Math.Sin(angle) * _radius);
    }

    private (double X, double Y) GetGridPoint(int axisIndex, int ringIndex)
    {
        var angle = 2 * Math.PI * axisIndex / _axisCount - Math.PI / 2;
        var r = _radius * (ringIndex + 1) / GridRings;
        return (_cx + Math.Cos(angle) * r, _cy + Math.Sin(angle) * r);
    }

    private (double X, double Y) GetLabelPosition(int index)
    {
        var angle = 2 * Math.PI * index / _axisCount - Math.PI / 2;
        var labelR = _radius + 18;
        return (_cx + Math.Cos(angle) * labelR, _cy + Math.Sin(angle) * labelR + 4);
    }

    private static string GetTextAnchor(int index, int total)
    {
        var angle = 2 * Math.PI * index / total;
        if (Math.Abs(angle) < 0.01 || Math.Abs(angle - 2 * Math.PI) < 0.01) return "middle";
        if (angle > 0 && angle < Math.PI) return "start";
        if (Math.Abs(angle - Math.PI) < 0.01) return "middle";
        return "end";
    }

    private static string F(double v) => v.ToString("F1");

    private class SeriesPolygon
    {
        public string Color { get; set; } = "";
        public double Opacity { get; set; }
        public double StrokeWidth { get; set; }
        public List<(double X, double Y)> Points { get; set; } = new();
        public string PointsString { get; set; } = "";
    }
}

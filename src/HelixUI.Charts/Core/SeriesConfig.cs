namespace HelixUI.Charts.Core;

/// <summary>
/// Configuration for a data series in a chart.
/// </summary>
public class SeriesConfig<T>
{
    /// <summary>Series display name (used in legend and tooltips).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Function to extract the Y value from a data item.</summary>
    public Func<T, double> Field { get; set; } = _ => 0;

    /// <summary>Color override. If null, uses palette.</summary>
    public string? Color { get; set; }

    /// <summary>Stroke width for lines.</summary>
    public double StrokeWidth { get; set; } = 2;

    /// <summary>Whether to show as dashed line.</summary>
    public bool Dashed { get; set; }

    /// <summary>Whether to show area fill below line.</summary>
    public bool ShowArea { get; set; }

    /// <summary>Area fill opacity (0-1).</summary>
    public double AreaOpacity { get; set; } = 0.15;

    /// <summary>Trendline configuration. If null, no trendline is shown.</summary>
    public TrendlineConfig? Trendline { get; set; }
}

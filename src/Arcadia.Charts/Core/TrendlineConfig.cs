namespace Arcadia.Charts.Core;

/// <summary>
/// Type of trendline to overlay on a chart series.
/// </summary>
public enum TrendlineType
{
    /// <summary>No trendline.</summary>
    None,
    /// <summary>Linear regression line.</summary>
    Linear,
    /// <summary>Moving average line.</summary>
    MovingAverage
}

/// <summary>
/// Configuration for a trendline overlay on a series.
/// </summary>
public class TrendlineConfig
{
    /// <summary>The type of trendline to render.</summary>
    public TrendlineType Type { get; set; } = TrendlineType.Linear;

    /// <summary>Window size for moving average (number of data points).</summary>
    public int MovingAveragePeriod { get; set; } = 5;

    /// <summary>Trendline color override. If null, uses series color at reduced opacity.</summary>
    public string? Color { get; set; }

    /// <summary>Stroke width for the trendline.</summary>
    public double StrokeWidth { get; set; } = 1.5;

    /// <summary>Whether to show the trendline as a dashed line.</summary>
    public bool Dashed { get; set; } = true;
}

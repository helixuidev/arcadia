namespace HelixUI.Charts.Core;

/// <summary>
/// Defines a threshold boundary for a gauge chart.
/// When the gauge value exceeds this threshold, the specified color is used.
/// </summary>
public class GaugeThreshold
{
    /// <summary>The threshold value.</summary>
    public double Value { get; set; }

    /// <summary>The color to use when the gauge value is at or above this threshold.</summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>Optional label for this threshold zone.</summary>
    public string? Label { get; set; }
}

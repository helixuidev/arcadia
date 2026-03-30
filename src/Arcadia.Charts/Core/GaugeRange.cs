namespace Arcadia.Charts.Core;

/// <summary>
/// A colored range band displayed on the gauge arc background.
/// Ranges are rendered behind the value arc and can be used to show
/// zones like "good", "warning", "danger" with distinct colors.
/// </summary>
public class GaugeRange
{
    /// <summary>Range start value (in gauge units, not degrees).</summary>
    public double Start { get; set; }

    /// <summary>Range end value (in gauge units, not degrees).</summary>
    public double End { get; set; }

    /// <summary>Range arc color. Default: rgba(0,0,0,0.15).</summary>
    public string Color { get; set; } = "rgba(0,0,0,0.15)";

    /// <summary>Range arc width in pixels. 0 = use the gauge's StrokeWidth.</summary>
    public double Width { get; set; }

    /// <summary>Range arc opacity. Default: 0.4.</summary>
    public double Opacity { get; set; } = 0.4;
}

namespace Arcadia.Gauge.Components;

/// <summary>Color threshold — changes the gauge arc color when value exceeds this threshold.</summary>
public class GaugeThreshold
{
    /// <summary>Value at which this color activates.</summary>
    public double Value { get; set; }

    /// <summary>CSS color string.</summary>
    public string Color { get; set; } = "#8b5cf6";
}

/// <summary>A colored range band displayed on the gauge arc background.</summary>
public class GaugeRange
{
    /// <summary>Range start value (in gauge units, not degrees).</summary>
    public double Start { get; set; }

    /// <summary>Range end value (in gauge units, not degrees).</summary>
    public double End { get; set; }

    /// <summary>Range arc color.</summary>
    public string Color { get; set; } = "rgba(255,255,255,0.15)";

    /// <summary>Range arc width. 0 = use gauge ArcWidth.</summary>
    public double Width { get; set; }

    /// <summary>Range arc opacity. Default: 0.4.</summary>
    public double Opacity { get; set; } = 0.4;
}

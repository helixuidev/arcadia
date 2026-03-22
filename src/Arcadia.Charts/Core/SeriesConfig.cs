namespace Arcadia.Charts.Core;

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

    /// <summary>Custom dash pattern (e.g., "8,4", "2,2"). Overrides Dashed.</summary>
    public string? DashPattern { get; set; }

    /// <summary>Whether to show area fill below line.</summary>
    public bool ShowArea { get; set; }

    /// <summary>Area fill opacity (0-1).</summary>
    public double AreaOpacity { get; set; } = 0.15;

    /// <summary>Whether to show data points on the line.</summary>
    public bool ShowPoints { get; set; } = true;

    /// <summary>Point radius in pixels.</summary>
    public double PointRadius { get; set; } = 3;

    /// <summary>Point shape: "circle", "square", "diamond", "triangle".</summary>
    public string PointShape { get; set; } = "circle";

    /// <summary>Curve type for line charts: "linear", "smooth", "step".</summary>
    public string CurveType { get; set; } = "linear";

    /// <summary>Whether this series is visible. Can be toggled via legend.</summary>
    public bool Visible { get; set; } = true;

    /// <summary>Trendline configuration. If null, no trendline is shown.</summary>
    public TrendlineConfig? Trendline { get; set; }

    /// <summary>Function to get per-point color. Overrides series Color for individual points.</summary>
    public Func<T, string?>? PointColorField { get; set; }

    /// <summary>Effective dash pattern for rendering.</summary>
    public string EffectiveDash => DashPattern ?? (Dashed ? "6,4" : "");
}

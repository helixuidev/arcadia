namespace Arcadia.Charts.Core;

/// <summary>
/// Configuration for a data series in a chart.
/// </summary>
public class SeriesConfig<T>
{
    /// <summary>Series display name (used in legend and tooltips).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Lambda that extracts the numeric Y value from each data item (e.g., d => d.Revenue). Every series must have this set to render data.</summary>
    public Func<T, double> Field { get; set; } = _ => 0;

    /// <summary>Color override. If null, uses palette.</summary>
    public string? Color { get; set; }

    /// <summary>Width of the line stroke in pixels for line and area series. Default is 2.</summary>
    public double StrokeWidth { get; set; } = 2;

    /// <summary>Renders the line with a dash pattern to visually distinguish it (e.g., projections vs actuals). Overridden by DashPattern if set.</summary>
    public bool Dashed { get; set; }

    /// <summary>Custom dash pattern (e.g., "8,4", "2,2"). Overrides Dashed.</summary>
    public string? DashPattern { get; set; }

    /// <summary>Fills the region between the line and the X-axis with a semi-transparent color. Opacity controlled by AreaOpacity.</summary>
    public bool ShowArea { get; set; }

    /// <summary>Opacity of the area fill beneath the line (0 = invisible, 1 = opaque). Only applies when ShowArea is true. Default is 0.15.</summary>
    public double AreaOpacity { get; set; } = 0.15;

    /// <summary>Renders circular markers at each data point. Disable for cleaner visuals on dense time-series. Default is true.</summary>
    public bool ShowPoints { get; set; } = true;

    /// <summary>Radius of data point markers in pixels. Overrides the chart-level PointRadius for this series. Default is 3.</summary>
    public double PointRadius { get; set; } = 3;

    /// <summary>Point shape: "circle", "square", "diamond", "triangle".</summary>
    public string PointShape { get; set; } = "circle";

    /// <summary>Curve type for line charts: "linear", "smooth", "step".</summary>
    public string CurveType { get; set; } = "linear";

    /// <summary>Series rendering type: "line" (default), "bar", or "area". Used in combo charts.</summary>
    public string SeriesType { get; set; } = "line";

    /// <summary>Whether this series is visible. Can be toggled via legend.</summary>
    public bool Visible { get; set; } = true;

    /// <summary>Trendline configuration. If null, no trendline is shown.</summary>
    public TrendlineConfig? Trendline { get; set; }

    /// <summary>Function to get per-point color. Overrides series Color for individual points.</summary>
    public Func<T, string?>? PointColorField { get; set; }

    /// <summary>Which Y-axis this series maps to: 0 = primary (left), 1 = secondary (right). Default is 0.</summary>
    public int YAxisIndex { get; set; } = 0;

    /// <summary>Effective dash pattern for rendering.</summary>
    public string EffectiveDash => DashPattern ?? (Dashed ? "6,4" : "");
}

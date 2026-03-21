using Microsoft.AspNetCore.Components;
using HelixUI.Charts.Core.Layout;

namespace HelixUI.Charts.Core;

/// <summary>
/// Base class for all chart components. Provides common parameters,
/// layout engine integration, and SVG container rendering.
/// </summary>
public abstract class ChartBase<T> : HelixUI.Core.Base.HelixComponentBase
{
    /// <summary>The data to visualize.</summary>
    [Parameter] public IReadOnlyList<T>? Data { get; set; }

    /// <summary>Chart height in pixels.</summary>
    [Parameter] public double Height { get; set; } = 300;

    /// <summary>Chart width in pixels. 0 = auto (fill container).</summary>
    [Parameter] public double Width { get; set; } = 600;

    /// <summary>Chart title.</summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>Color palette for series.</summary>
    [Parameter] public ChartPalette? Palette { get; set; }

    /// <summary>Whether to show grid lines.</summary>
    [Parameter] public bool ShowGrid { get; set; } = true;

    /// <summary>Whether to animate on load.</summary>
    [Parameter] public bool AnimateOnLoad { get; set; } = true;

    /// <summary>Accessible description for screen readers.</summary>
    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>Format provider for number/date formatting.</summary>
    [Parameter] public IFormatProvider? FormatProvider { get; set; }

    protected ChartPalette EffectivePalette => Palette ?? ChartPalette.Default;
    protected ChartLayoutEngine LayoutEngine { get; } = new();

    protected string ResolveColor(string? color, int seriesIndex)
    {
        if (!string.IsNullOrEmpty(color))
        {
            return color switch
            {
                "primary" => "var(--helix-color-primary)",
                "secondary" => "var(--helix-color-secondary)",
                "success" => "var(--helix-color-success)",
                "danger" => "var(--helix-color-danger)",
                "warning" => "var(--helix-color-warning)",
                "info" => "var(--helix-color-info)",
                _ => color
            };
        }
        return EffectivePalette.GetColor(seriesIndex);
    }
}

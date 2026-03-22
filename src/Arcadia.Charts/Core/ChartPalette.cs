namespace Arcadia.Charts.Core;

/// <summary>
/// Color palette for chart series. Each palette provides ordered colors
/// that cycle when more series than colors exist.
/// </summary>
public class ChartPalette
{
    private readonly string[] _colors;

    public ChartPalette(params string[] colors)
    {
        _colors = colors.Length > 0 ? colors : Default._colors;
    }

    /// <summary>
    /// Gets the color for a series index, cycling if needed.
    /// </summary>
    public string GetColor(int index) => _colors[index % _colors.Length];

    /// <summary>
    /// Gets all colors in the palette.
    /// </summary>
    public IReadOnlyList<string> Colors => _colors;

    /// <summary>
    /// Number of colors before cycling.
    /// </summary>
    public int Count => _colors.Length;

    // ---- Built-in palettes ----

    /// <summary>Default palette using HelixUI theme tokens.</summary>
    public static readonly ChartPalette Default = new(
        "var(--arcadia-color-primary)",
        "var(--arcadia-color-success)",
        "var(--arcadia-color-warning)",
        "var(--arcadia-color-danger)",
        "var(--arcadia-color-info)",
        "var(--arcadia-color-secondary)",
        "#8b5cf6", "#ec4899", "#14b8a6", "#f97316"
    );

    /// <summary>Cool blues and greens.</summary>
    public static readonly ChartPalette Cool = new(
        "#3b82f6", "#06b6d4", "#14b8a6", "#22c55e", "#84cc16",
        "#0ea5e9", "#2dd4bf", "#4ade80", "#a3e635", "#67e8f9"
    );

    /// <summary>Warm reds and oranges.</summary>
    public static readonly ChartPalette Warm = new(
        "#ef4444", "#f97316", "#f59e0b", "#eab308", "#ec4899",
        "#f87171", "#fb923c", "#fbbf24", "#facc15", "#f472b6"
    );

    /// <summary>Single-hue monochrome.</summary>
    public static readonly ChartPalette Monochrome = new(
        "#1e293b", "#334155", "#475569", "#64748b", "#94a3b8",
        "#cbd5e1", "#e2e8f0", "#f1f5f9"
    );

    /// <summary>Soft pastel colors.</summary>
    public static readonly ChartPalette Pastel = new(
        "#93c5fd", "#86efac", "#fde68a", "#fca5a5", "#c4b5fd",
        "#fbcfe8", "#a5f3fc", "#d9f99d", "#fed7aa", "#e9d5ff"
    );

    /// <summary>Vibrant saturated colors.</summary>
    public static readonly ChartPalette Vibrant = new(
        "#2563eb", "#16a34a", "#dc2626", "#9333ea", "#ea580c",
        "#0891b2", "#ca8a04", "#be185d", "#4f46e5", "#059669"
    );

    /// <summary>Accessible palette optimized for color-blind users (Okabe-Ito).</summary>
    public static readonly ChartPalette Accessible = new(
        "#0072B2", "#E69F00", "#009E73", "#CC79A7", "#56B4E9",
        "#D55E00", "#F0E442", "#000000"
    );
}

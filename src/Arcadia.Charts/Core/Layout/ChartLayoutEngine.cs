namespace Arcadia.Charts.Core.Layout;

/// <summary>
/// The anti-collision layout engine. Runs as a pure C# pass before SVG rendering.
/// Resolves tick positions, label rotations, legend layout, and margins — guaranteeing no overlaps.
/// </summary>
public class ChartLayoutEngine
{
    private const double DefaultFontSize = 12;
    private const double TitleFontSize = 16;
    private const double MinPlotRatio = 0.4; // Plot area must be at least 40% of total

    /// <summary>
    /// Calculates the complete chart layout with collision-free positions.
    /// </summary>
    public ChartLayoutResult Calculate(ChartLayoutInput input)
    {
        var result = new ChartLayoutResult
        {
            TotalWidth = input.Width,
            TotalHeight = input.Height
        };

        // 1. Determine responsive tier
        result.Tier = GetResponsiveTier(input.Width);

        // 2. Calculate tick positions and resolve collisions
        result.XTicks = ResolveXTicks(input, result.Tier, out var xRotation);
        result.TickLabelRotation = xRotation;
        result.YTicks = ResolveYTicks(input);

        // 3. Calculate legend layout
        result.Legend = ResolveLegend(input, result.Tier);

        // 4. Calculate margins from resolved elements
        result.Margins = CalculateMargins(input, result);

        // 5. Calculate plot area
        result.PlotArea = new PlotArea
        {
            X = result.Margins.Left,
            Y = result.Margins.Top,
            Width = Math.Max(10, input.Width - result.Margins.Left - result.Margins.Right),
            Height = Math.Max(10, input.Height - result.Margins.Top - result.Margins.Bottom)
        };

        // 6. Safety: if margins consumed too much space, drop elements
        if (result.PlotArea.Width < input.Width * MinPlotRatio ||
            result.PlotArea.Height < input.Height * MinPlotRatio)
        {
            result.ShowAxisTitles = false;
            result.Margins = CalculateMargins(input, result);
            result.PlotArea = new PlotArea
            {
                X = result.Margins.Left,
                Y = result.Margins.Top,
                Width = Math.Max(10, input.Width - result.Margins.Left - result.Margins.Right),
                Height = Math.Max(10, input.Height - result.Margins.Top - result.Margins.Bottom)
            };
        }

        return result;
    }

    private ResponsiveTier GetResponsiveTier(double width) => width switch
    {
        > 600 => ResponsiveTier.Wide,
        > 400 => ResponsiveTier.Medium,
        > 300 => ResponsiveTier.Narrow,
        _ => ResponsiveTier.Compact
    };

    private List<TickMark> ResolveXTicks(ChartLayoutInput input, ResponsiveTier tier, out double rotation)
    {
        rotation = 0;
        if (input.XTickLabels is null || input.XTickLabels.Count == 0)
            return new List<TickMark>();

        // Estimate max ticks that can fit horizontally
        var avgLabelWidth = input.XTickLabels.Average(l => TextMeasure.EstimateWidth(l, DefaultFontSize));
        var fitsHorizontally = (int)(input.Width / Math.Max(1, avgLabelWidth + 10));

        var maxTicks = tier switch
        {
            ResponsiveTier.Wide => Math.Min(input.XTickLabels.Count, Math.Max(fitsHorizontally, 6)),
            ResponsiveTier.Medium => Math.Min(input.XTickLabels.Count, Math.Min(fitsHorizontally, 12)),
            ResponsiveTier.Narrow => Math.Min(input.XTickLabels.Count, 6),
            _ => 3
        };

        // Start with all labels, progressively reduce
        var labels = SelectLabels(input.XTickLabels, maxTicks);
        var spacing = input.Width / Math.Max(1, labels.Count);

        // Check if horizontal labels fit
        var maxLabelWidth = labels.Max(l => TextMeasure.EstimateWidth(l, DefaultFontSize));
        if (maxLabelWidth < spacing * 0.9)
        {
            // Horizontal fits
            rotation = 0;
        }
        else if (tier >= ResponsiveTier.Medium)
        {
            // Try 45° rotation
            var rotatedSize = labels.Max(l => TextMeasure.EstimateRotated(l, DefaultFontSize, 45));
            if (rotatedSize.Width < spacing * 0.9)
            {
                rotation = 45;
            }
            else
            {
                rotation = 90;
                // At 90°, re-check if we need fewer labels
                var rotated90 = labels.Max(l => TextMeasure.EstimateRotated(l, DefaultFontSize, 90));
                if (rotated90.Width > spacing * 0.9)
                {
                    labels = SelectLabels(input.XTickLabels, maxTicks / 2);
                }
            }
        }
        else
        {
            rotation = 90;
            labels = SelectLabels(input.XTickLabels, 3);
        }

        // Build tick marks with bounding boxes
        var resolvedRotation = rotation;
        var ticks = BuildTickMarks(labels, spacing, resolvedRotation);

        // Post-build collision removal: iteratively remove overlapping ticks
        ticks = RemoveOverlappingTicks(ticks);

        return ticks;
    }

    private static List<TickMark> BuildTickMarks(List<string> labels, double spacing, double rotation)
    {
        return labels.Select((label, i) =>
        {
            var x = spacing * i + spacing / 2;
            var (w, h) = TextMeasure.EstimateRotated(label, DefaultFontSize, rotation);
            return new TickMark
            {
                Label = label,
                Position = x,
                BoundingBox = new LabelBox(x - w / 2, 0, w, h)
            };
        }).ToList();
    }

    private static List<TickMark> RemoveOverlappingTicks(List<TickMark> ticks)
    {
        if (ticks.Count <= 1) return ticks;

        // Keep first and last, remove interior ticks that overlap
        var result = new List<TickMark> { ticks[0] };
        for (var i = 1; i < ticks.Count; i++)
        {
            if (!CollisionDetector.Overlaps(result[^1].BoundingBox, ticks[i].BoundingBox))
            {
                result.Add(ticks[i]);
            }
        }

        return result;
    }

    private List<TickMark> ResolveYTicks(ChartLayoutInput input)
    {
        if (input.YMin is null || input.YMax is null)
            return new List<TickMark>();

        var ticks = TickGenerator.GenerateNumericTicks(input.YMin.Value, input.YMax.Value, 8);
        return ticks.Select(v => new TickMark
        {
            Label = FormatNumber(v),
            Value = v,
            Position = 0 // Resolved later by the chart component using scales
        }).ToList();
    }

    private LegendLayout ResolveLegend(ChartLayoutInput input, ResponsiveTier tier)
    {
        if (input.SeriesNames is null || input.SeriesNames.Count == 0)
            return new LegendLayout { Visible = false };

        if (tier <= ResponsiveTier.Narrow)
            return new LegendLayout { Visible = false };

        var itemWidths = input.SeriesNames.Select(n => TextMeasure.EstimateWidth(n, DefaultFontSize) + 24).ToList(); // 24 = swatch + gap
        var totalWidth = itemWidths.Sum() + (itemWidths.Count - 1) * 16; // 16 = gap between items

        if (input.SeriesNames.Count > 12)
        {
            return new LegendLayout { Visible = true, Mode = LegendMode.Truncated, MaxItems = 10 };
        }
        if (totalWidth > input.Width * 0.8)
        {
            return new LegendLayout { Visible = true, Mode = LegendMode.Wrapped };
        }
        return new LegendLayout { Visible = true, Mode = LegendMode.Horizontal };
    }

    private ChartMargins CalculateMargins(ChartLayoutInput input, ChartLayoutResult result)
    {
        var top = 8.0;
        var right = 8.0;
        var bottom = 8.0;
        var left = 8.0;

        // Title
        if (!string.IsNullOrEmpty(input.Title))
            top += TitleFontSize * 1.5;

        // Y-axis labels
        if (result.YTicks.Count > 0)
        {
            var maxYLabelWidth = result.YTicks.Max(t => TextMeasure.EstimateWidth(t.Label, DefaultFontSize));
            left += maxYLabelWidth + 8;
        }

        // Y-axis title
        if (result.ShowAxisTitles && !string.IsNullOrEmpty(input.YAxisTitle))
            left += DefaultFontSize * 1.5;

        // X-axis labels (accounting for rotation)
        if (result.XTicks.Count > 0)
        {
            var labelHeight = result.XTicks.Max(t => t.BoundingBox.Height);
            bottom += labelHeight + 4;
        }

        // X-axis title
        if (result.ShowAxisTitles && !string.IsNullOrEmpty(input.XAxisTitle))
            bottom += DefaultFontSize * 1.5;

        // Legend (below chart)
        if (result.Legend.Visible)
            bottom += DefaultFontSize * 2;

        return new ChartMargins { Top = top, Right = right, Bottom = bottom, Left = left };
    }

    private static List<string> SelectLabels(IReadOnlyList<string> all, int max)
    {
        if (all.Count <= max) return all.ToList();
        if (max <= 2) return new List<string> { all[0], all[^1] };
        if (max == 3) return new List<string> { all[0], all[all.Count / 2], all[^1] };

        var step = Math.Max(1, all.Count / max);
        var selected = new List<string>();
        for (var i = 0; i < all.Count; i += step)
            selected.Add(all[i]);
        if (selected[^1] != all[^1])
            selected[^1] = all[^1]; // Always include last
        return selected;
    }

    private static string FormatNumber(double value)
    {
        if (Math.Abs(value) >= 1_000_000) return $"{value / 1_000_000:G4}M";
        if (Math.Abs(value) >= 1_000) return $"{value / 1_000:G4}K";
        return value.ToString("G4");
    }
}

public class ChartLayoutInput
{
    public double Width { get; set; }
    public double Height { get; set; }
    public string? Title { get; set; }
    public string? XAxisTitle { get; set; }
    public string? YAxisTitle { get; set; }
    public IReadOnlyList<string>? XTickLabels { get; set; }
    public double? YMin { get; set; }
    public double? YMax { get; set; }
    public IReadOnlyList<string>? SeriesNames { get; set; }
}

public class ChartLayoutResult
{
    public double TotalWidth { get; set; }
    public double TotalHeight { get; set; }
    public ResponsiveTier Tier { get; set; }
    public ChartMargins Margins { get; set; } = new();
    public PlotArea PlotArea { get; set; } = new();
    public List<TickMark> XTicks { get; set; } = new();
    public List<TickMark> YTicks { get; set; } = new();
    public double TickLabelRotation { get; set; }
    public LegendLayout Legend { get; set; } = new();
    public bool ShowAxisTitles { get; set; } = true;
    public bool ShowGridLines { get; set; } = true;
    public bool ShowDataLabels { get; set; } = true;
}

public struct ChartMargins
{
    public double Top { get; set; }
    public double Right { get; set; }
    public double Bottom { get; set; }
    public double Left { get; set; }
}

public struct PlotArea
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}

public class TickMark
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Position { get; set; }
    public LabelBox BoundingBox { get; set; }
}

public class LegendLayout
{
    public bool Visible { get; set; }
    public LegendMode Mode { get; set; }
    public int MaxItems { get; set; } = int.MaxValue;
}

public enum LegendMode { Horizontal, Wrapped, Vertical, Truncated }

public enum ResponsiveTier { Compact, Narrow, Medium, Wide }

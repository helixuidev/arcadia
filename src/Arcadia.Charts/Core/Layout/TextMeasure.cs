namespace Arcadia.Charts.Core.Layout;

/// <summary>
/// Estimates text dimensions without DOM access.
/// Calibrated for common sans-serif fonts at standard sizes.
/// </summary>
public static class TextMeasure
{
    /// <summary>
    /// Average character width as a ratio to font size for sans-serif fonts.
    /// </summary>
    private const double AvgCharWidthRatio = 0.55;

    /// <summary>
    /// Estimates the width of a text string in pixels.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="fontSize">The font size in pixels.</param>
    public static double EstimateWidth(string? text, double fontSize = 12)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return text.Length * fontSize * AvgCharWidthRatio;
    }

    /// <summary>
    /// Estimates the height of a single line of text.
    /// </summary>
    /// <param name="fontSize">The font size in pixels.</param>
    /// <param name="lineHeight">The line height multiplier.</param>
    public static double EstimateHeight(double fontSize = 12, double lineHeight = 1.4)
    {
        return fontSize * lineHeight;
    }

    /// <summary>
    /// Estimates the width of a rotated text label.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="fontSize">The font size in pixels.</param>
    /// <param name="angleDegrees">The rotation angle in degrees.</param>
    public static (double Width, double Height) EstimateRotated(string? text, double fontSize, double angleDegrees)
    {
        var w = EstimateWidth(text, fontSize);
        var h = EstimateHeight(fontSize);
        var rad = angleDegrees * Math.PI / 180;
        var cos = Math.Abs(Math.Cos(rad));
        var sin = Math.Abs(Math.Sin(rad));
        return (w * cos + h * sin, w * sin + h * cos);
    }
}

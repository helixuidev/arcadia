namespace HelixUI.Charts.Core;

/// <summary>
/// Defines how null or missing data values are handled in line charts.
/// </summary>
public enum NullHandling
{
    /// <summary>Leave a gap in the line where data is missing.</summary>
    Gap,
    /// <summary>Connect across missing values, drawing a line between the adjacent valid points.</summary>
    Connect,
    /// <summary>Treat null values as zero.</summary>
    Zero
}

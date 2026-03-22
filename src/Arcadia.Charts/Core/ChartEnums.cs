namespace Arcadia.Charts.Core;

/// <summary>
/// Pie/donut chart label format options.
/// </summary>
public enum PieLabelFormat
{
    /// <summary>Show percentage (e.g., "25%").</summary>
    Percent,
    /// <summary>Show the raw value.</summary>
    Value,
    /// <summary>Show the category name.</summary>
    Name,
    /// <summary>Show name and percentage (e.g., "Sales 25%").</summary>
    NamePercent,
    /// <summary>Show name and value.</summary>
    NameValue,
    /// <summary>Hide labels.</summary>
    None
}

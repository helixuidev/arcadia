namespace Arcadia.DataGrid.Core;

/// <summary>
/// Type of aggregate to compute for a column footer.
/// </summary>
public enum AggregateType
{
    /// <summary>No aggregate.</summary>
    None,
    /// <summary>Sum of all values.</summary>
    Sum,
    /// <summary>Average of all values.</summary>
    Average,
    /// <summary>Count of rows.</summary>
    Count,
    /// <summary>Minimum value.</summary>
    Min,
    /// <summary>Maximum value.</summary>
    Max
}

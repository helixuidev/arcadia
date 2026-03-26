namespace Arcadia.DataGrid.Core;

/// <summary>
/// Describes a filter applied to a grid column.
/// </summary>
public class FilterDescriptor
{
    /// <summary>The column key being filtered.</summary>
    public string Property { get; set; } = "";

    /// <summary>Filter operator.</summary>
    public FilterOperator Operator { get; set; } = FilterOperator.Contains;

    /// <summary>Filter value (user-entered text).</summary>
    public string Value { get; set; } = "";
}

/// <summary>
/// Filter operators for grid column filtering.
/// </summary>
public enum FilterOperator
{
    /// <summary>Cell value contains the filter text (case-insensitive).</summary>
    Contains,
    /// <summary>Cell value equals the filter text (case-insensitive).</summary>
    Equals,
    /// <summary>Cell value starts with the filter text.</summary>
    StartsWith,
    /// <summary>Cell value ends with the filter text.</summary>
    EndsWith,
    /// <summary>Cell value is greater than the filter value (numeric).</summary>
    GreaterThan,
    /// <summary>Cell value is less than the filter value (numeric).</summary>
    LessThan,
    /// <summary>Cell value does not equal the filter text.</summary>
    NotEquals,
    /// <summary>Cell value is null or empty.</summary>
    IsEmpty,
    /// <summary>Cell value is not null or empty.</summary>
    IsNotEmpty
}

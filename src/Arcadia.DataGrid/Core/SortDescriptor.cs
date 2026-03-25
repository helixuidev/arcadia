namespace Arcadia.DataGrid.Core;

/// <summary>
/// Describes a sort applied to a grid column.
/// </summary>
public class SortDescriptor
{
    /// <summary>The property name or column identifier being sorted.</summary>
    public string Property { get; set; } = "";

    /// <summary>Sort direction.</summary>
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
}

/// <summary>
/// Sort direction for grid columns.
/// </summary>
public enum SortDirection
{
    /// <summary>No sort applied.</summary>
    None,
    /// <summary>Ascending (A-Z, 0-9).</summary>
    Ascending,
    /// <summary>Descending (Z-A, 9-0).</summary>
    Descending
}

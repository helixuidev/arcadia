namespace Arcadia.DataGrid.Core;

/// <summary>
/// Arguments passed to the LoadData callback for server-side data loading.
/// Contains the current sort, filter, and paging state so the server can
/// return the correct slice of data.
/// </summary>
public class DataGridLoadArgs
{
    /// <summary>Number of rows to skip (offset for paging).</summary>
    public int Skip { get; set; }

    /// <summary>Number of rows to take (page size).</summary>
    public int Take { get; set; }

    /// <summary>Sort property name (column key). Null if no sort applied.</summary>
    public string? SortProperty { get; set; }

    /// <summary>Sort direction. None if no sort applied.</summary>
    public SortDirection SortDirection { get; set; } = SortDirection.None;

    /// <summary>Active filters (column key → filter descriptor).</summary>
    public IReadOnlyList<FilterDescriptor> Filters { get; set; } = Array.Empty<FilterDescriptor>();
}

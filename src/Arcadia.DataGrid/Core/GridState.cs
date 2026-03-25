namespace Arcadia.DataGrid.Core;

/// <summary>
/// Encapsulates the current state of the grid: sorting, paging, filtering.
/// </summary>
public class GridState
{
    /// <summary>Current page index (0-based).</summary>
    public int PageIndex { get; set; }

    /// <summary>Rows per page.</summary>
    public int PageSize { get; set; } = 25;

    /// <summary>Active sort descriptors (multi-sort supported).</summary>
    public List<SortDescriptor> Sorts { get; set; } = new();

    /// <summary>Total number of items (for server-side paging).</summary>
    public int TotalCount { get; set; }

    /// <summary>Total number of pages.</summary>
    public int PageCount => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;

    /// <summary>Number of items to skip for current page.</summary>
    public int Skip => PageIndex * PageSize;
}

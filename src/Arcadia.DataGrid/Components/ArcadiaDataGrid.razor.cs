using Microsoft.AspNetCore.Components;
using Arcadia.Core.Base;
using Arcadia.DataGrid.Core;

namespace Arcadia.DataGrid.Components;

/// <summary>
/// A data grid component with sorting, paging, and column templates.
/// </summary>
public partial class ArcadiaDataGrid<TItem> : ArcadiaComponentBase
{
    /// <summary>Client-side data source.</summary>
    [Parameter] public IReadOnlyList<TItem>? Data { get; set; }

    /// <summary>Rows per page. 0 = show all (no paging).</summary>
    [Parameter] public int PageSize { get; set; } = 25;

    /// <summary>Page size options for the dropdown.</summary>
    [Parameter] public int[] PageSizeOptions { get; set; } = new[] { 10, 25, 50, 100 };

    /// <summary>Enable column sorting.</summary>
    [Parameter] public bool Sortable { get; set; } = true;

    /// <summary>Alternate row shading.</summary>
    [Parameter] public bool Striped { get; set; } = true;

    /// <summary>Highlight row on hover.</summary>
    [Parameter] public bool Hoverable { get; set; } = true;

    /// <summary>Compact row height.</summary>
    [Parameter] public bool Dense { get; set; }

    /// <summary>Sticky header on scroll.</summary>
    [Parameter] public bool FixedHeader { get; set; } = true;

    /// <summary>Fixed height with scroll. Null = auto height.</summary>
    [Parameter] public string? Height { get; set; }

    /// <summary>Show skeleton loading state.</summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>Message shown when data is empty.</summary>
    [Parameter] public string EmptyMessage { get; set; } = "No data available";

    /// <summary>Column definitions (ArcadiaColumn child components).</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Fired when sort changes.</summary>
    [Parameter] public EventCallback<SortDescriptor?> SortChanged { get; set; }

    // ── Internal state ──
    internal ArcadiaDataGridColumnCollector<TItem> Collector { get; } = new();
    private SortDescriptor? _currentSort;
    private int _pageIndex;
    private bool _columnsCollected;

    private IReadOnlyList<ArcadiaColumn<TItem>> Columns => Collector.Columns;
    private bool HasData => Data is not null && Data.Count > 0;
    private int TotalCount => Data?.Count ?? 0;
    private int PageCount => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && !_columnsCollected && Columns.Count > 0)
        {
            _columnsCollected = true;
            StateHasChanged();
        }
    }

    /// <summary>Get the current page of data, sorted.</summary>
    internal IEnumerable<TItem> GetPagedData()
    {
        if (Data is null) return Enumerable.Empty<TItem>();

        IEnumerable<TItem> result = Data;

        // Apply sort
        if (_currentSort is not null && _currentSort.Direction != SortDirection.None)
        {
            var sortCol = Columns.FirstOrDefault(c => c.Key == _currentSort.Property);
            if (sortCol?.Field is not null)
            {
                result = _currentSort.Direction == SortDirection.Ascending
                    ? result.OrderBy(item => sortCol.Field(item))
                    : result.OrderByDescending(item => sortCol.Field(item));
            }
        }

        // Apply paging
        if (PageSize > 0)
        {
            result = result.Skip(_pageIndex * PageSize).Take(PageSize);
        }

        return result;
    }

    internal SortDirection GetSortDirection(string columnKey)
    {
        if (_currentSort is not null && _currentSort.Property == columnKey)
            return _currentSort.Direction;
        return SortDirection.None;
    }

    internal async Task HandleHeaderClick(ArcadiaColumn<TItem> column)
    {
        var isSortable = column.Sortable ?? Sortable;
        if (!isSortable || column.Field is null) return;

        var key = column.Key;
        if (_currentSort?.Property == key)
        {
            // Cycle: Ascending → Descending → None
            _currentSort.Direction = _currentSort.Direction switch
            {
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _ => SortDirection.Ascending
            };
            if (_currentSort.Direction == SortDirection.None)
                _currentSort = null;
        }
        else
        {
            _currentSort = new SortDescriptor { Property = key, Direction = SortDirection.Ascending };
        }

        _pageIndex = 0; // Reset to first page on sort change
        if (SortChanged.HasDelegate)
            await SortChanged.InvokeAsync(_currentSort);
    }

    internal void GoToPage(int page)
    {
        if (page < 0) page = 0;
        if (page >= PageCount) page = PageCount - 1;
        _pageIndex = page;
    }

    internal void SetPageSize(int size)
    {
        PageSize = size;
        _pageIndex = 0;
    }

    internal void HandlePageSizeChange(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var size))
            SetPageSize(size);
    }

    internal string GetSortAriaLabel(ArcadiaColumn<TItem> column)
    {
        var dir = GetSortDirection(column.Key);
        return dir switch
        {
            SortDirection.Ascending => $"{column.Title}, sorted ascending",
            SortDirection.Descending => $"{column.Title}, sorted descending",
            _ => $"{column.Title}, click to sort"
        };
    }
}

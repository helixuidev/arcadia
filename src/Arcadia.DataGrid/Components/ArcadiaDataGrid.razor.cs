using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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

    /// <summary>Enable column filtering (filter row below header).</summary>
    [Parameter] public bool Filterable { get; set; }

    /// <summary>Enable row selection.</summary>
    [Parameter] public bool Selectable { get; set; }

    /// <summary>Allow multi-row selection (checkbox column). Requires Selectable=true.</summary>
    [Parameter] public bool MultiSelect { get; set; }

    /// <summary>Currently selected items. Two-way bindable.</summary>
    [Parameter] public HashSet<TItem>? SelectedItems { get; set; }

    /// <summary>Fired when selection changes.</summary>
    [Parameter] public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>Server-side data loading callback. When set, grid delegates sort/filter/page to the server.</summary>
    [Parameter] public EventCallback<DataGridLoadArgs> LoadData { get; set; }

    /// <summary>Total row count for server-side paging. Required when using LoadData.</summary>
    [Parameter] public int? ServerTotalCount { get; set; }

    /// <summary>Detail row template. When set, rows become expandable.</summary>
    [Parameter] public RenderFragment<TItem>? DetailTemplate { get; set; }

    /// <summary>Column definitions (ArcadiaColumn child components).</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Fired when sort changes.</summary>
    [Parameter] public EventCallback<SortDescriptor?> SortChanged { get; set; }

    /// <summary>Fired when a row is edited (inline edit commits).</summary>
    [Parameter] public EventCallback<TItem> OnRowEdit { get; set; }

    /// <summary>Column key to group rows by. Null = no grouping.</summary>
    [Parameter] public string? GroupBy { get; set; }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    private IJSObjectReference? _jsModule;

    // ── Internal state ──
    internal ArcadiaDataGridColumnCollector<TItem> Collector { get; } = new();
    private SortDescriptor? _currentSort;
    private int _pageIndex;
    private bool _columnsCollected;
    private Dictionary<string, FilterDescriptor> _filters = new();
    private HashSet<TItem> _selectedItems = new();
    private bool _showFilters;
    private bool _showColumnPicker;
    private HashSet<TItem> _expandedRows = new();
    private TItem? _editingRow;
    private string? _editingColumn;
    private Dictionary<object, bool> _groupExpanded = new();
    private bool _isServerMode => LoadData.HasDelegate;

    private IReadOnlyList<ArcadiaColumn<TItem>> Columns => Collector.Columns;
    private bool HasData => _isServerMode || (Data is not null && Data.Count > 0);
    private int FilteredCount => _isServerMode ? (ServerTotalCount ?? 0) : GetFilteredData().Count();
    private int TotalCount => _isServerMode ? (ServerTotalCount ?? 0) : (Filterable && _filters.Values.Any(f => !string.IsNullOrEmpty(f.Value)) ? GetFilteredData().Count() : (Data?.Count ?? 0));
    private int PageCount => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && !_columnsCollected && Columns.Count > 0)
        {
            _columnsCollected = true;
            StateHasChanged();
        }
    }

    /// <summary>Get filtered data (before paging).</summary>
    internal IEnumerable<TItem> GetFilteredData()
    {
        if (Data is null) return Enumerable.Empty<TItem>();
        IEnumerable<TItem> result = Data;

        // Apply filters
        foreach (var filter in _filters.Values.Where(f => !string.IsNullOrEmpty(f.Value)))
        {
            var col = Columns.FirstOrDefault(c => c.Key == filter.Property);
            if (col?.Field is null) continue;

            var filterValue = filter.Value;
            var op = filter.Operator;
            result = result.Where(item =>
            {
                var cellValue = col.Field(item)?.ToString() ?? "";
                return op switch
                {
                    FilterOperator.Contains => cellValue.Contains(filterValue, StringComparison.OrdinalIgnoreCase),
                    FilterOperator.Equals => cellValue.Equals(filterValue, StringComparison.OrdinalIgnoreCase),
                    FilterOperator.StartsWith => cellValue.StartsWith(filterValue, StringComparison.OrdinalIgnoreCase),
                    FilterOperator.EndsWith => cellValue.EndsWith(filterValue, StringComparison.OrdinalIgnoreCase),
                    FilterOperator.GreaterThan => double.TryParse(cellValue, out var cv) && double.TryParse(filterValue, out var fv) && cv > fv,
                    FilterOperator.LessThan => double.TryParse(cellValue, out var cv2) && double.TryParse(filterValue, out var fv2) && cv2 < fv2,
                    _ => true
                };
            });
        }

        return result;
    }

    /// <summary>Get the current page of data, filtered and sorted.</summary>
    internal IEnumerable<TItem> GetPagedData()
    {
        var result = GetFilteredData();

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

    // ── Filter methods ──

    internal void SetFilter(string columnKey, string value)
    {
        if (!_filters.ContainsKey(columnKey))
            _filters[columnKey] = new FilterDescriptor { Property = columnKey };
        _filters[columnKey].Value = value;
        _pageIndex = 0;
    }

    internal string GetFilterValue(string columnKey)
    {
        return _filters.TryGetValue(columnKey, out var f) ? f.Value : "";
    }

    internal void ToggleFilters()
    {
        _showFilters = !_showFilters;
        if (!_showFilters) _filters.Clear();
    }

    // ── Selection methods ──

    internal bool IsSelected(TItem item) => _selectedItems.Contains(item);

    internal async Task ToggleSelection(TItem item)
    {
        if (!Selectable) return;

        if (MultiSelect)
        {
            if (!_selectedItems.Add(item))
                _selectedItems.Remove(item);
        }
        else
        {
            _selectedItems.Clear();
            _selectedItems.Add(item);
        }

        SelectedItems = _selectedItems;
        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(_selectedItems);
    }

    internal async Task ToggleSelectAll()
    {
        var pageData = GetPagedData().ToList();
        if (pageData.All(i => _selectedItems.Contains(i)))
        {
            foreach (var item in pageData) _selectedItems.Remove(item);
        }
        else
        {
            foreach (var item in pageData) _selectedItems.Add(item);
        }

        SelectedItems = _selectedItems;
        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(_selectedItems);
    }

    internal bool IsAllSelected()
    {
        var pageData = GetPagedData().ToList();
        return pageData.Count > 0 && pageData.All(i => _selectedItems.Contains(i));
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

    // ── Server-side data ──

    internal async Task InvokeLoadData()
    {
        if (!LoadData.HasDelegate) return;
        var args = new DataGridLoadArgs
        {
            Skip = _pageIndex * PageSize,
            Take = PageSize,
            SortProperty = _currentSort?.Property,
            SortDirection = _currentSort?.Direction ?? SortDirection.None,
            Filters = _filters.Values.Where(f => !string.IsNullOrEmpty(f.Value)).ToList()
        };
        await LoadData.InvokeAsync(args);
    }

    internal async Task GoToPageAsync(int page)
    {
        GoToPage(page);
        if (_isServerMode) await InvokeLoadData();
    }

    internal async Task SetPageSizeAsync(int size)
    {
        SetPageSize(size);
        if (_isServerMode) await InvokeLoadData();
    }

    // ── Detail expansion ──

    internal bool IsExpanded(TItem item) => _expandedRows.Contains(item);

    internal void ToggleDetail(TItem item)
    {
        if (!_expandedRows.Remove(item))
            _expandedRows.Add(item);
    }

    // ── Inline editing ──

    internal bool IsEditing(TItem item) => _editingRow is not null && EqualityComparer<TItem>.Default.Equals(_editingRow, item);
    internal bool IsEditingCell(TItem item, string colKey) => IsEditing(item) && _editingColumn == colKey;

    internal void StartEdit(TItem item, string colKey)
    {
        _editingRow = item;
        _editingColumn = colKey;
    }

    internal async Task CommitEdit()
    {
        if (_editingRow is not null && OnRowEdit.HasDelegate)
            await OnRowEdit.InvokeAsync(_editingRow);
        _editingRow = default;
        _editingColumn = null;
    }

    internal void CancelEdit()
    {
        _editingRow = default;
        _editingColumn = null;
    }

    // ── Grouping ──

    internal List<(object Key, string Label, List<TItem> Items)> GetGroupedData()
    {
        if (string.IsNullOrEmpty(GroupBy)) return new();
        var col = Columns.FirstOrDefault(c => c.Key == GroupBy);
        if (col?.Field is null) return new();

        var sorted = GetFilteredData();
        if (_currentSort is not null && _currentSort.Direction != SortDirection.None)
        {
            var sortCol = Columns.FirstOrDefault(c => c.Key == _currentSort.Property);
            if (sortCol?.Field is not null)
            {
                sorted = _currentSort.Direction == SortDirection.Ascending
                    ? sorted.OrderBy(item => sortCol.Field(item))
                    : sorted.OrderByDescending(item => sortCol.Field(item));
            }
        }

        return sorted
            .GroupBy(item => col.Field(item)?.ToString() ?? "")
            .Select(g => ((object)g.Key, g.Key.ToString() ?? "", g.ToList()))
            .ToList();
    }

    internal bool IsGroupExpanded(object key)
    {
        return !_groupExpanded.ContainsKey(key) || _groupExpanded[key];
    }

    internal void ToggleGroup(object key)
    {
        if (_groupExpanded.ContainsKey(key))
            _groupExpanded[key] = !_groupExpanded[key];
        else
            _groupExpanded[key] = false; // was expanded (default), now collapsed
    }

    // ── CSV Export ──

    internal string ToCsv()
    {
        var sb = new System.Text.StringBuilder();
        var visibleCols = Columns.Where(c => c.IsVisible && c.Field is not null).ToList();

        // Header
        sb.AppendLine(string.Join(",", visibleCols.Select(c => CsvQuote(c.Title))));

        // Data (all filtered+sorted, not just current page)
        var allData = GetFilteredData();
        if (_currentSort is not null && _currentSort.Direction != SortDirection.None)
        {
            var sortCol = Columns.FirstOrDefault(c => c.Key == _currentSort.Property);
            if (sortCol?.Field is not null)
            {
                allData = _currentSort.Direction == SortDirection.Ascending
                    ? allData.OrderBy(item => sortCol.Field(item))
                    : allData.OrderByDescending(item => sortCol.Field(item));
            }
        }

        foreach (var item in allData)
        {
            sb.AppendLine(string.Join(",", visibleCols.Select(c => CsvQuote(c.FormatValue(c.Field!(item))))));
        }

        return sb.ToString();
    }

    internal async Task ExportCsvDownload()
    {
        try
        {
            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Arcadia.DataGrid/js/datagrid-interop.js");
            var csv = ToCsv();
            await _jsModule.InvokeVoidAsync("downloadCsv", csv, "export.csv");
        }
        catch (JSException) { }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { }
#endif
    }

    private static string CsvQuote(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    // ── Column picker ──

    internal void ToggleColumnPicker()
    {
        _showColumnPicker = !_showColumnPicker;
    }

    internal void ToggleColumnVisibility(ArcadiaColumn<TItem> col)
    {
        col.ToggleVisible();
    }

    // ── Aggregate computation ──

    internal double ComputeAggregate(ArcadiaColumn<TItem> col, AggregateType type)
    {
        if (col.Field is null || Data is null) return 0;
        var values = Data.Select(item =>
        {
            var raw = col.Field(item);
            return raw switch
            {
                double d => d,
                int i => (double)i,
                long l => (double)l,
                decimal m => (double)m,
                float f => (double)f,
                _ => double.TryParse(raw?.ToString(), out var p) ? p : double.NaN
            };
        }).Where(v => !double.IsNaN(v)).ToList();

        if (values.Count == 0) return 0;
        return type switch
        {
            AggregateType.Sum => values.Sum(),
            AggregateType.Average => values.Average(),
            AggregateType.Count => values.Count,
            AggregateType.Min => values.Min(),
            AggregateType.Max => values.Max(),
            _ => 0
        };
    }
}

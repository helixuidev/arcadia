using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Arcadia.Core.Base;
using Arcadia.DataGrid.Core;

namespace Arcadia.DataGrid.Components;

/// <summary>
/// A data grid component with sorting, paging, filtering, selection, grouping,
/// inline editing, master-detail, and CSV export.
/// </summary>
public partial class ArcadiaDataGrid<TItem> : ArcadiaComponentBase, IAsyncDisposable
{
    /// <summary>In-memory collection of row items. When set alongside LoadData, client-side sorting/filtering is bypassed. When null, the grid shows EmptyMessage.</summary>
    [Parameter] public IReadOnlyList<TItem>? Data { get; set; }

    /// <summary>Rows per page. 0 = show all (no paging).</summary>
    [Parameter] public int PageSize { get; set; } = 25;

    /// <summary>Page size options for the dropdown.</summary>
    [Parameter] public int[] PageSizeOptions { get; set; } = new[] { 10, 25, 50, 100 };

    /// <summary>Enable click-to-sort on column headers. Individual columns can override via their Sortable parameter. Default is true.</summary>
    [Parameter] public bool Sortable { get; set; } = true;

    /// <summary>Apply alternating background shading to even/odd rows for easier horizontal scanning. Default is true.</summary>
    [Parameter] public bool Striped { get; set; } = true;

    /// <summary>Apply a background highlight to the row under the cursor. Default is true.</summary>
    [Parameter] public bool Hoverable { get; set; } = true;

    /// <summary>Reduce row height and cell padding for a denser layout showing more rows. Default is false.</summary>
    [Parameter] public bool Dense { get; set; }

    /// <summary>Keep column headers fixed at the top during vertical scroll. Requires Height to be set. Default is true.</summary>
    [Parameter] public bool FixedHeader { get; set; } = true;

    /// <summary>Fixed height with scroll. Null = auto height.</summary>
    [Parameter] public string? Height { get; set; }

    /// <summary>Render animated skeleton rows instead of data while awaiting an async fetch. Default is false.</summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>Text shown when Data is null or empty and Loading is false. Default is 'No data available'.</summary>
    [Parameter] public string EmptyMessage { get; set; } = "No data available";

    /// <summary>Show row selector column with row numbers and state indicators.</summary>
    [Parameter] public bool ShowRowSelector { get; set; }

    /// <summary>Display sequential row numbers in the row selector column. Only takes effect when ShowRowSelector is true. Default is true.</summary>
    [Parameter] public bool ShowRowNumbers { get; set; } = true;

    /// <summary>Add a filter input row below headers. Users can type per-column filter values. Default is false.</summary>
    [Parameter] public bool Filterable { get; set; }

    /// <summary>Allow rows to be selected by clicking. Combine with MultiSelect for checkbox-based multi-select. Default is false.</summary>
    [Parameter] public bool Selectable { get; set; }

    /// <summary>Add a checkbox column for selecting multiple rows. Requires Selectable=true. Includes select-all header checkbox. Default is false.</summary>
    [Parameter] public bool MultiSelect { get; set; }

    /// <summary>Currently selected items. Two-way bindable.</summary>
    [Parameter] public HashSet<TItem>? SelectedItems { get; set; }

    /// <summary>Callback invoked when the selection set changes. Receives the updated HashSet of selected items.</summary>
    [Parameter] public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>Callback invoked when the grid needs data from the server (on page, sort, or filter change). When set, the grid operates in server-side mode.</summary>
    [Parameter] public EventCallback<DataGridLoadArgs> LoadData { get; set; }

    /// <summary>Total matching rows on the server for page count calculation. Required alongside LoadData. Ignored in client-side mode.</summary>
    [Parameter] public int? ServerTotalCount { get; set; }

    /// <summary>Detail row template. When set, rows become expandable.</summary>
    [Parameter] public RenderFragment<TItem>? DetailTemplate { get; set; }

    /// <summary>Child content slot for ArcadiaColumn components that define the grid's columns.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Callback invoked when column sorting changes. Receives a SortDescriptor with column key and direction, or null when cleared.</summary>
    [Parameter] public EventCallback<SortDescriptor?> SortChanged { get; set; }

    /// <summary>Callback invoked when inline edit is committed (Enter or blur). Receives the edited item for persistence.</summary>
    [Parameter] public EventCallback<TItem> OnRowEdit { get; set; }

    /// <summary>Key of the column whose values group rows into collapsible sections. Set to null to disable. Groups show expand/collapse toggles.</summary>
    [Parameter] public string? GroupBy { get; set; }

    /// <summary>Enable virtual scrolling for large datasets. Requires Height to be set. Disables pagination.</summary>
    [Parameter] public bool VirtualizeRows { get; set; }

    /// <summary>Estimated row height in pixels for the virtualizer to calculate scroll position. Match your actual row height. Default is 40.</summary>
    [Parameter] public float ItemSize { get; set; } = 40;

    /// <summary>Extra rows rendered outside the viewport to reduce flicker during fast scrolling. Default is 5.</summary>
    [Parameter] public int OverscanCount { get; set; } = 5;

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    private IJSObjectReference? _jsModule;
    private bool _disposed;

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
    private bool _editCommitting; // guard against double-commit
    private TItem? _currentRow;
    private Dictionary<object, bool> _groupExpanded = new();
    private bool _isServerMode => LoadData.HasDelegate;

    // ── Keyboard navigation ──
    private int _focusRow;
    private int _focusCol;
    private bool _gridHasFocus;
    private string? _liveAnnouncement;

    // ── Cached filtered data to avoid multiple LINQ enumerations ──
    private List<TItem>? _filteredDataCache;
    private int _lastDataHash;
    private int _lastFilterHash;

    internal ElementReference TableRef;
    private bool _resizeInitialized;

    private IReadOnlyList<ArcadiaColumn<TItem>> Columns => Collector.Columns;
    private bool HasData => _isServerMode || (Data is not null && Data.Count > 0);

    private int TotalCount
    {
        get
        {
            if (_isServerMode) return ServerTotalCount ?? 0;
            return GetCachedFilteredData().Count;
        }
    }

    private int PageCount => PageSize > 0 ? Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize)) : 1;

    protected override async void OnAfterRender(bool firstRender)
    {
        if (_disposed) return;
        if (firstRender && !_columnsCollected && Columns.Count > 0)
        {
            _columnsCollected = true;
            StateHasChanged();
        }

        if (_columnsCollected && !_resizeInitialized)
        {
            _resizeInitialized = true;
            try
            {
                _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/Arcadia.DataGrid/js/datagrid-interop.js");
                await _jsModule.InvokeVoidAsync("initResizeHandles", TableRef, 50);
            }
            catch { /* JS not available in SSR */ }
        }
    }

    // ── Filtered data cache ──

    private List<TItem> GetCachedFilteredData()
    {
        var dataHash = Data?.Count ?? 0;
        var filterHash = _filters.Values.Sum(f => f.Value?.GetHashCode() ?? 0);
        if (_filteredDataCache is null || dataHash != _lastDataHash || filterHash != _lastFilterHash)
        {
            _filteredDataCache = GetFilteredDataInternal().ToList();
            _lastDataHash = dataHash;
            _lastFilterHash = filterHash;
        }
        return _filteredDataCache;
    }

    private void InvalidateCache() { _filteredDataCache = null; }

    /// <summary>Get filtered data (before paging). Uses cache.</summary>
    internal IEnumerable<TItem> GetFilteredData() => GetCachedFilteredData();

    private IEnumerable<TItem> GetFilteredDataInternal()
    {
        if (Data is null) return Enumerable.Empty<TItem>();
        IEnumerable<TItem> result = Data;

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

    /// <summary>Get all filtered and sorted data (no paging). Used by virtual scrolling and export.</summary>
    internal IList<TItem> GetAllSortedData()
    {
        if (_isServerMode) return (IList<TItem>)(Data ?? (IReadOnlyList<TItem>)Array.Empty<TItem>());

        IEnumerable<TItem> result = GetCachedFilteredData();

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

        return result.ToList();
    }

    /// <summary>Get the current page of data, filtered and sorted.</summary>
    internal IEnumerable<TItem> GetPagedData()
    {
        if (VirtualizeRows) return GetAllSortedData(); // virtual scrolling shows all

        if (_isServerMode) return Data ?? Enumerable.Empty<TItem>();

        IEnumerable<TItem> result = GetCachedFilteredData();

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
        InvalidateCache();
        if (_isServerMode) _ = InvokeLoadData();
    }

    internal string GetFilterValue(string columnKey)
    {
        return _filters.TryGetValue(columnKey, out var f) ? f.Value : "";
    }

    internal void ToggleFilters()
    {
        _showFilters = !_showFilters;
        // Don't clear filters on toggle — preserve filter state when hidden
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

        _pageIndex = 0;
        CancelEdit(); // Clear edit state on sort change
        if (SortChanged.HasDelegate)
            await SortChanged.InvokeAsync(_currentSort);
        if (_isServerMode) await InvokeLoadData();
    }

    internal void GoToPage(int page)
    {
        if (page < 0) page = 0;
        if (page >= PageCount) page = PageCount - 1;
        _pageIndex = page;
        CancelEdit(); // Clear edit state on page change
        if (_isServerMode) _ = InvokeLoadData();
    }

    internal void SetPageSize(int size)
    {
        PageSize = size;
        _pageIndex = 0;
        CancelEdit();
        if (_isServerMode) _ = InvokeLoadData();
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

    // ── Current row (focus) ──

    internal bool IsCurrent(TItem item) => _currentRow is not null && EqualityComparer<TItem>.Default.Equals(_currentRow, item);

    internal void SetCurrentRow(TItem item) { _currentRow = item; }

    /// <summary>Get the row selector glyph for a given item.</summary>
    internal string GetRowSelectorGlyph(TItem item)
    {
        if (IsEditing(item)) return "\u270E"; // ✎ pencil
        if (IsCurrent(item)) return "\u25B6"; // ▶ right arrow
        return "";
    }

    // ── Detail expansion ──

    internal bool IsExpanded(TItem item) => _expandedRows.Contains(item);

    internal void ToggleDetail(TItem item)
    {
        if (!_expandedRows.Remove(item))
            _expandedRows.Add(item);
    }

    // ── Inline editing (with double-commit guard) ──

    internal bool IsEditing(TItem item) => _editingRow is not null && EqualityComparer<TItem>.Default.Equals(_editingRow, item);
    internal bool IsEditingCell(TItem item, string colKey) => IsEditing(item) && _editingColumn == colKey;

    internal void StartEdit(TItem item, string colKey)
    {
        _editingRow = item;
        _editingColumn = colKey;
    }

    internal async Task CommitEdit()
    {
        if (_editCommitting) return; // guard against double-commit from blur + Enter
        _editCommitting = true;
        try
        {
            if (_editingRow is not null && OnRowEdit.HasDelegate)
                await OnRowEdit.InvokeAsync(_editingRow);
        }
        finally
        {
            _editingRow = default;
            _editingColumn = null;
            _editCommitting = false;
        }
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

        IEnumerable<TItem> sorted = GetCachedFilteredData();
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
            _groupExpanded[key] = false;
    }

    // ── CSV Export ──

    internal string ToCsv()
    {
        var sb = new System.Text.StringBuilder();
        var visibleCols = Columns.Where(c => c.IsVisible && c.Field is not null).ToList();

        sb.AppendLine(string.Join(",", visibleCols.Select(c => CsvQuote(c.Title))));

        IEnumerable<TItem> allData = GetCachedFilteredData();
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
        StateHasChanged(); // FIX: was missing — column picker didn't update UI
    }

    // ── Aggregate computation (uses filtered data, not raw Data) ──

    internal double ComputeAggregate(ArcadiaColumn<TItem> col, AggregateType type)
    {
        if (col.Field is null) return 0;
        var source = GetCachedFilteredData(); // FIX: was using Data, now uses filtered
        if (source.Count == 0) return 0;

        var values = source.Select(item =>
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

    // ── Keyboard Navigation (WAI-ARIA Grid Pattern) ──

    internal void HandleGridKeyDown(KeyboardEventArgs e)
    {
        if (_editingRow is not null && e.Key != "Escape" && e.Key != "Enter" && e.Key != "Tab")
            return; // let edit input handle its own keys

        var visibleCols = Columns.Where(c => c.IsVisible).ToList();
        var maxCol = visibleCols.Count - 1;
        var pageRows = GetPagedData().ToList();
        var maxRow = pageRows.Count - 1;

        switch (e.Key)
        {
            case "ArrowDown":
                _focusRow = Math.Min(_focusRow + 1, maxRow);
                UpdateCurrentFromFocus(pageRows);
                break;
            case "ArrowUp":
                _focusRow = Math.Max(_focusRow - 1, 0);
                UpdateCurrentFromFocus(pageRows);
                break;
            case "ArrowRight":
                _focusCol = Math.Min(_focusCol + 1, maxCol);
                break;
            case "ArrowLeft":
                _focusCol = Math.Max(_focusCol - 1, 0);
                break;
            case "Home":
                _focusCol = 0;
                if (e.CtrlKey) _focusRow = 0;
                UpdateCurrentFromFocus(pageRows);
                break;
            case "End":
                _focusCol = maxCol;
                if (e.CtrlKey) _focusRow = maxRow;
                UpdateCurrentFromFocus(pageRows);
                break;
            case "PageDown":
                GoToPage(_pageIndex + 1);
                _focusRow = 0;
                Announce($"Page {_pageIndex + 1} of {PageCount}");
                break;
            case "PageUp":
                GoToPage(_pageIndex - 1);
                _focusRow = 0;
                Announce($"Page {_pageIndex + 1} of {PageCount}");
                break;
            case "Enter":
                if (_editingRow is not null)
                {
                    _ = CommitEdit();
                    _focusRow = Math.Min(_focusRow + 1, maxRow);
                }
                else if (_focusRow >= 0 && _focusRow < pageRows.Count)
                {
                    var col = _focusCol >= 0 && _focusCol < visibleCols.Count ? visibleCols[_focusCol] : null;
                    if (col?.Editable == true)
                    {
                        StartEdit(pageRows[_focusRow], col.Key);
                        Announce($"Editing {col.Title}");
                    }
                }
                break;
            case "Escape":
                if (_editingRow is not null)
                {
                    CancelEdit();
                    Announce("Edit cancelled");
                }
                break;
            case " ":
                if (Selectable && _focusRow >= 0 && _focusRow < pageRows.Count)
                {
                    _ = ToggleSelection(pageRows[_focusRow]);
                    var selected = IsSelected(pageRows[_focusRow]);
                    Announce(selected ? "Row selected" : "Row deselected");
                }
                break;
            default:
                return; // don't prevent default for unhandled keys
        }
    }

    private void UpdateCurrentFromFocus(List<TItem> pageRows)
    {
        if (_focusRow >= 0 && _focusRow < pageRows.Count)
            SetCurrentRow(pageRows[_focusRow]);
    }

    internal bool IsFocusedCell(int rowIdx, int colIdx) => _gridHasFocus && rowIdx == _focusRow && colIdx == _focusCol;

    internal string GetCellId(int rowIdx, int colIdx) => $"cell-{rowIdx}-{colIdx}";

    internal string? GetActiveDescendant()
    {
        if (!_gridHasFocus) return null;
        return GetCellId(_focusRow, _focusCol);
    }

    // ── ARIA Live Announcements ──

    private void Announce(string message)
    {
        _liveAnnouncement = message;
        StateHasChanged();
        _ = ClearAnnouncementAsync();
    }

    private async Task ClearAnnouncementAsync()
    {
        await Task.Delay(150);
        _liveAnnouncement = null;
        await InvokeAsync(StateHasChanged);
    }

    // ── Column Reorder (Drag & Drop) ──

    private int _dragSourceCol = -1;
    private int _dropTargetCol = -1;

    internal void StartColumnDrag(int colIndex) { _dragSourceCol = colIndex; }
    internal void SetDropTarget(int colIndex) { _dropTargetCol = colIndex; }
    internal void EndColumnDrag() { _dragSourceCol = -1; _dropTargetCol = -1; }

    internal void DropColumn(int targetIndex)
    {
        if (_dragSourceCol >= 0 && _dragSourceCol != targetIndex)
        {
            Collector.MoveColumn(_dragSourceCol, targetIndex);
            Announce($"Column moved to position {targetIndex + 1}");
        }
        EndColumnDrag();
    }

    // ── Disposal ──

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        try
        {
            if (_jsModule is not null)
                await _jsModule.DisposeAsync();
        }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { }
#endif
        catch (ObjectDisposedException) { }
        GC.SuppressFinalize(this);
    }
}

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Arcadia.Core.Base;
using Arcadia.Core.Utilities;
using Arcadia.DataGrid.Core;

namespace Arcadia.DataGrid.Components;

/// <summary>
/// A data grid component with sorting, paging, filtering, selection, grouping,
/// inline editing, master-detail, and CSV export.
/// </summary>
public partial class ArcadiaDataGrid<TItem> : ArcadiaComponentBase, IAsyncDisposable
{
    /// <summary>In-memory collection of row items. When set alongside LoadData, client-side sorting/filtering is bypassed. When null, the grid shows EmptyMessage.</summary>
    /// <example>
    /// <code>
    /// &lt;ArcadiaDataGrid Data="@employees"&gt;
    ///     &lt;ArcadiaColumn Property="Name" /&gt;
    ///     &lt;ArcadiaColumn Property="Department" /&gt;
    /// &lt;/ArcadiaDataGrid&gt;
    /// </code>
    /// </example>
    [Parameter] public IReadOnlyList<TItem>? Data { get; set; }

    /// <summary>Rows per page. 0 = show all (no paging).</summary>
    [Parameter] public int PageSize { get; set; } = 25;

    /// <summary>Page size options for the dropdown.</summary>
    [Parameter] public int[] PageSizeOptions { get; set; } = new[] { 10, 25, 50, 100 };

    /// <summary>Enable click-to-sort on column headers. Individual columns can override via their Sortable parameter. Default is true.</summary>
    /// <remarks>Shift+Click column headers for multi-column sort with priority badges.</remarks>
    [Parameter] public bool Sortable { get; set; } = true;

    /// <summary>Apply alternating background shading to even/odd rows for easier horizontal scanning. Default is true.</summary>
    [Parameter] public bool Striped { get; set; } = true;

    /// <summary>Apply a background highlight to the row under the cursor. Default is true.</summary>
    [Parameter] public bool Hoverable { get; set; } = true;

    /// <summary>Reduce row height and cell padding for a denser layout showing more rows. Default is false.</summary>
    [Parameter] public bool Dense { get; set; }

    /// <summary>Keep column headers fixed at the top during vertical scroll. Requires Height to be set. Default is true.</summary>
    [Parameter] public bool FixedHeader { get; set; } = true;

    /// <summary>Keep the footer aggregate row fixed at the bottom during vertical scroll. Requires Height to be set. Default is false.</summary>
    [Parameter] public bool StickyFooter { get; set; }

    /// <summary>Fixed height with scroll. Null = auto height.</summary>
    [Parameter] public string? Height { get; set; }

    /// <summary>Render animated skeleton rows instead of data while awaiting an async fetch. Default is false.</summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>Text shown when Data is null or empty and Loading is false. Default is 'No data available'.</summary>
    [Parameter] public string EmptyMessage { get; set; } = "No data available";

    /// <summary>Custom template for the empty state. Overrides EmptyMessage when set.</summary>
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }

    /// <summary>Selection behavior: None (default), Single (click to select one row), Multiple (checkbox column with select-all). Overrides Selectable/MultiSelect when set explicitly.</summary>
    /// <example>
    /// <code>
    /// &lt;ArcadiaDataGrid Data="@items"
    ///     SelectionMode="DataGridSelectionMode.Multiple"
    ///     @bind-SelectedItems="selected" /&gt;
    /// </code>
    /// </example>
    [Parameter] public DataGridSelectionMode SelectionMode { get; set; } = DataGridSelectionMode.None;

    // ── Localization strings (override for i18n) ──

    /// <summary>Search input placeholder text. Default: "Search..."</summary>
    [Parameter] public string TextSearch { get; set; } = "Search...";

    /// <summary>Filter toggle button text. Default: "Filter"</summary>
    [Parameter] public string TextFilter { get; set; } = "Filter";

    /// <summary>Column picker button text. Default: "Columns"</summary>
    [Parameter] public string TextColumns { get; set; } = "Columns";

    /// <summary>CSV export button text. Default: "CSV"</summary>
    [Parameter] public string TextCsv { get; set; } = "CSV";

    /// <summary>Excel export button text. Default: "Excel"</summary>
    [Parameter] public string TextExcel { get; set; } = "Excel";

    /// <summary>PDF export button text. Default: "PDF"</summary>
    [Parameter] public string TextPdf { get; set; } = "PDF";

    /// <summary>Page info format. Use {0} for current page and {1} for total pages. Default: "Page {0} of {1}"</summary>
    [Parameter] public string TextPageInfo { get; set; } = "Page {0} of {1}";

    /// <summary>Rows info format. Use {0} for start, {1} for end, {2} for total. Default: "{0}–{1} of {2}"</summary>
    [Parameter] public string TextRowsInfo { get; set; } = "{0}\u2013{1} of {2}";

    /// <summary>Batch save button text. Default: "Save"</summary>
    [Parameter] public string TextSave { get; set; } = "Save";

    /// <summary>Batch discard button text. Default: "Discard"</summary>
    [Parameter] public string TextDiscard { get; set; } = "Discard";

    /// <summary>Pending changes format. Use {0} for count. Default: "{0} pending"</summary>
    [Parameter] public string TextPending { get; set; } = "{0} pending";

    /// <summary>Enable drag-and-drop column reordering by dragging column headers. Default is false.</summary>
    [Parameter] public bool AllowColumnReorder { get; set; }

    /// <summary>Callback fired after columns are reordered via drag-and-drop.</summary>
    [Parameter] public EventCallback OnColumnsReordered { get; set; }

    /// <summary>Enable drag-and-drop row reordering. Shows a drag handle column on the left. Requires Data to be a mutable IList.</summary>
    /// <remarks>Fires OnRowReordered with old and new index after a row is dropped. Data must be an IList (not IReadOnlyList) for in-place reorder.</remarks>
    [Parameter] public bool AllowRowReorder { get; set; }

    /// <summary>Callback fired after a row is dragged to a new position. Receives the old and new index.</summary>
    [Parameter] public EventCallback<(int OldIndex, int NewIndex)> OnRowReordered { get; set; }

    /// <summary>Pin column menu item. Default: "Pin to Left"</summary>
    [Parameter] public string TextPinColumn { get; set; } = "Pin to Left";

    /// <summary>Unpin column menu item. Default: "Unpin Column"</summary>
    [Parameter] public string TextUnpinColumn { get; set; } = "Unpin Column";

    /// <summary>Hide column menu item. Default: "Hide Column"</summary>
    [Parameter] public string TextHideColumn { get; set; } = "Hide Column";

    /// <summary>Sort ascending menu item. Default: "Sort Ascending"</summary>
    [Parameter] public string TextSortAscending { get; set; } = "Sort Ascending";

    // ── Command Column ──

    /// <summary>Command template rendered as an extra column at the end of each row. Use for action buttons (Edit, Delete, etc.).</summary>
    /// <example>
    /// <code>
    /// &lt;ArcadiaDataGrid Data="@items" CommandTemplate="@(item => @&lt;div&gt;&lt;button @onclick="() => Edit(item)"&gt;Edit&lt;/button&gt;&lt;/div&gt;)"&gt;
    /// </code>
    /// </example>
    [Parameter] public RenderFragment<TItem>? CommandTemplate { get; set; }

    /// <summary>Header text for the command column. Default is "Actions".</summary>
    [Parameter] public string CommandColumnTitle { get; set; } = "Actions";

    /// <summary>CSS width for the command column. Default is "120px".</summary>
    [Parameter] public string CommandColumnWidth { get; set; } = "120px";

    // ── Inline Row Add ──

    /// <summary>Enable infinite scroll (load-more-on-scroll) as an alternative to pagination. When true, pagination is hidden and OnLoadMore fires as the user scrolls to the bottom.</summary>
    [Parameter] public bool InfiniteScroll { get; set; }

    /// <summary>Number of rows to load per batch when infinite scrolling. Default is 50.</summary>
    [Parameter] public int InfiniteScrollPageSize { get; set; } = 50;

    /// <summary>Callback fired when the infinite scroll sentinel becomes visible. Receives the next page number (1-based).</summary>
    [Parameter] public EventCallback<int> OnLoadMore { get; set; }

    /// <summary>Localization text for the copy-with-headers keyboard shortcut. Default: "Copy with Headers"</summary>
    [Parameter] public string TextCopyWithHeaders { get; set; } = "Copy with Headers";

    /// <summary>Show an "Add Row" button in the toolbar. Requires Data to be an IList for in-place insertion. Default is false.</summary>
    [Parameter] public bool ShowAddRow { get; set; }

    /// <summary>Callback invoked when the Add Row button is clicked. Use to initialize the new row or handle server-side creation.</summary>
    [Parameter] public EventCallback OnRowAdd { get; set; }

    /// <summary>Position where the new row is inserted: "top" or "bottom". Default is "top".</summary>
    [Parameter] public string NewRowPosition { get; set; } = "top";

    /// <summary>Add Row button text. Default: "Add Row"</summary>
    [Parameter] public string TextAddRow { get; set; } = "Add Row";

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

    /// <summary>Enable batch editing mode. Modified cells are tracked and can be committed or discarded as a group. Default is false.</summary>
    /// <example>
    /// <code>
    /// &lt;ArcadiaDataGrid Data="@items" BatchEdit="true"
    ///     OnBatchCommit="@HandleBatchSave"&gt;
    ///     &lt;ArcadiaColumn Property="Name" Editable="true" /&gt;
    /// &lt;/ArcadiaDataGrid&gt;
    /// </code>
    /// </example>
    [Parameter] public bool BatchEdit { get; set; }

    /// <summary>Callback invoked when batch changes are committed. Receives the list of changes.</summary>
    [Parameter] public EventCallback<List<BatchEditChange<TItem>>> OnBatchCommit { get; set; }

    /// <summary>Context menu template shown on right-click. Receives the clicked row item.</summary>
    [Parameter] public RenderFragment<TItem>? ContextMenuTemplate { get; set; }

    /// <summary>Callback invoked when a row is right-clicked.</summary>
    [Parameter] public EventCallback<TItem> OnContextMenu { get; set; }

    /// <summary>Group rows by this column key or property name. When set, rows are organized into collapsible sections. Can reference a displayed column key OR any property on TItem (e.g., "Department" groups by the Department property even if no Department column is shown).</summary>
    [Parameter] public string? GroupBy { get; set; }

    /// <summary>Lambda accessor for the group value. Takes precedence over GroupBy string when set. Use for computed grouping (e.g., GroupByField="@(e => e.Salary > 100000 ? "Senior" : "Junior")").</summary>
    [Parameter] public Func<TItem, object>? GroupByField { get; set; }

    /// <summary>Show aggregate summaries (Sum, Avg, etc.) in group header rows for columns that have Aggregate set. Default is false.</summary>
    [Parameter] public bool ShowGroupAggregates { get; set; }

    /// <summary>Enable virtual scrolling for large datasets. Requires Height to be set. Disables pagination.</summary>
    /// <remarks>Requires Height to be set. Renders only visible rows plus OverscanCount buffer rows. Disables pagination when enabled.</remarks>
    [Parameter] public bool VirtualizeRows { get; set; }

    /// <summary>Estimated row height in pixels for the virtualizer to calculate scroll position. Match your actual row height. Default is 40.</summary>
    [Parameter] public float ItemSize { get; set; } = 40;

    /// <summary>Extra rows rendered outside the viewport to reduce flicker during fast scrolling. Default is 5.</summary>
    [Parameter] public int OverscanCount { get; set; } = 5;

    /// <summary>localStorage key for persisting grid state (sort, filter, column order, visibility, page size). Null = no persistence.</summary>
    /// <remarks>Saves sort, filter, page size, and column visibility to localStorage. Use OnStateChanged for server-side persistence.</remarks>
    [Parameter] public string? StateKey { get; set; }

    /// <summary>Callback invoked when grid state changes. Use for server-side persistence.</summary>
    [Parameter] public EventCallback<DataGridState> OnStateChanged { get; set; }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    private IJSObjectReference? _jsModule;
    private bool _disposed;
    private CollectionObserver<TItem>? _collectionObserver;

    /// <summary>Display toolbar with search, filter toggle, and export. Default is false.</summary>
    /// <example>
    /// <code>
    /// &lt;ArcadiaDataGrid Data="@data" ShowToolbar="true"&gt;
    ///     &lt;ArcadiaColumn Property="Name" /&gt;
    ///     &lt;ArcadiaColumn Property="Email" /&gt;
    /// &lt;/ArcadiaDataGrid&gt;
    /// </code>
    /// </example>
    [Parameter] public bool ShowToolbar { get; set; }

    /// <summary>Quick filter text that searches across all visible columns. Two-way bindable.</summary>
    [Parameter] public string? QuickFilter { get; set; }

    /// <summary>Callback invoked when QuickFilter text changes.</summary>
    [Parameter] public EventCallback<string?> QuickFilterChanged { get; set; }

    // ── Internal state ──
    internal ArcadiaDataGridColumnCollector<TItem> Collector { get; } = new();
    internal ArcadiaDataGridCommandCollector<TItem> CommandCollector { get; } = new();
    private List<SortDescriptor> _sortStack = new();
    private int _pageIndex;
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
    private List<BatchEditChange<TItem>> _batchChanges = new();
    private TItem? _contextMenuItem;
    private double _contextMenuX, _contextMenuY;
    private bool _showContextMenu;
    private bool _isServerMode => LoadData.HasDelegate;

    // ── Infinite scroll state ──
    private int _infiniteScrollPage;
    private bool _loadingMore;
    private IJSObjectReference? _infiniteScrollObserver;
    private ElementReference _infiniteScrollSentinel;

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

    protected override void OnInitialized()
    {
        Collector.OnColumnsChanged = () => InvokeAsync(StateHasChanged);
        CommandCollector.OnCommandColumnChanged = () => InvokeAsync(StateHasChanged);
    }

    private DotNetObjectReference<ArcadiaDataGrid<TItem>>? _dotNetRef;

    protected override void OnParametersSet()
    {
        // SelectionMode overrides Selectable/MultiSelect when set
        if (SelectionMode != DataGridSelectionMode.None)
        {
            Selectable = true;
            MultiSelect = SelectionMode == DataGridSelectionMode.Multiple;
        }

        // Calculate min/max for columns with ConditionalFormat
        CalculateConditionalFormatRanges();

        // Observable collection auto-refresh
        if (!LoadData.HasDelegate) // Skip for server-side mode
        {
            _collectionObserver ??= new CollectionObserver<TItem>(
                () =>
                {
                    CalculateConditionalFormatRanges();
                    StateHasChanged();
                    return Task.CompletedTask;
                },
                InvokeAsync
            );
            _collectionObserver.Attach(Data);
        }
    }

    /// <summary>Calculate min/max values for columns that have ConditionalFormat set.</summary>
    private void CalculateConditionalFormatRanges()
    {
        if (Data is null || Data.Count == 0) return;

        foreach (var col in Columns.Where(c => !string.IsNullOrEmpty(c.ConditionalFormat) && c.ResolvedField is not null))
        {
            double min = double.MaxValue;
            double max = double.MinValue;
            foreach (var item in Data)
            {
                var raw = col.ResolvedField!(item);
                double val;
                switch (raw)
                {
                    case double d: val = d; break;
                    case int i: val = i; break;
                    case long l: val = l; break;
                    case decimal m: val = (double)m; break;
                    case float f: val = f; break;
                    default:
                        if (raw is not null && double.TryParse(raw.ToString(), out var p)) val = p;
                        else continue;
                        break;
                }
                if (val < min) min = val;
                if (val > max) max = val;
            }
            if (min != double.MaxValue)
            {
                col.ColumnMin = min;
                col.ColumnMax = max;
            }
        }
    }

    protected override async void OnAfterRender(bool firstRender)
    {
        if (_disposed) return;

        if (firstRender && !string.IsNullOrEmpty(StateKey))
            await LoadStateAsync();

        if (!_resizeInitialized && Columns.Count > 0)
        {
            _resizeInitialized = true;
            try
            {
                _dotNetRef ??= DotNetObjectReference.Create(this);
                _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/Arcadia.DataGrid/js/datagrid-interop.js");
                await _jsModule.InvokeVoidAsync("initResizeHandles", TableRef, 50, _dotNetRef);
            }
            catch (JSException) { /* JS module load failed — non-critical during SSR or circuit disconnect */ }
#if NET6_0_OR_GREATER
            catch (JSDisconnectedException) { /* Blazor Server circuit disconnected */ }
#endif
            catch (InvalidOperationException) { /* JS interop unavailable during static prerendering */ }
        }

        // Initialize infinite scroll observer
        if (InfiniteScroll && _infiniteScrollObserver is null && _infiniteScrollSentinel.Id is not null)
        {
            try
            {
                _dotNetRef ??= DotNetObjectReference.Create(this);
                _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/Arcadia.DataGrid/js/datagrid-interop.js");
                _infiniteScrollObserver = await _jsModule.InvokeAsync<IJSObjectReference>(
                    "observeInfiniteScroll", _infiniteScrollSentinel, _dotNetRef);
            }
            catch (JSException) { /* JS module load failed — non-critical during SSR or circuit disconnect */ }
#if NET6_0_OR_GREATER
            catch (JSDisconnectedException) { /* Blazor Server circuit disconnected */ }
#endif
            catch (InvalidOperationException) { /* JS interop unavailable during static prerendering */ }
        }
    }

    // ── Filtered data cache ──

    private List<TItem> GetCachedFilteredData()
    {
        var dataHash = Data?.Count ?? 0;
        var filterHash = unchecked(_filters.Values.Aggregate(0, (h, f) => h ^ (f.Value?.GetHashCode() ?? 0) ^ (f.Operator.GetHashCode() << 16)));
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
            var col = Columns.FirstOrDefault(c => c.ResolvedKey == filter.Property);
            if (col?.ResolvedField is null) continue;

            var filterValue = filter.Value;
            var op = filter.Operator;
            result = result.Where(item =>
            {
                var cellValue = col.ResolvedField(item)?.ToString() ?? "";
                return op switch
                {
                    FilterOperator.Contains => cellValue.Contains(filterValue, StringComparison.OrdinalIgnoreCase),
                    FilterOperator.Equals => cellValue.Equals(filterValue, StringComparison.OrdinalIgnoreCase),
                    FilterOperator.StartsWith => cellValue.StartsWith(filterValue, StringComparison.OrdinalIgnoreCase),
                    FilterOperator.EndsWith => cellValue.EndsWith(filterValue, StringComparison.OrdinalIgnoreCase),
                    FilterOperator.GreaterThan => double.TryParse(cellValue, out var cv) && double.TryParse(filterValue, out var fv) && cv > fv,
                    FilterOperator.LessThan => double.TryParse(cellValue, out var cv2) && double.TryParse(filterValue, out var fv2) && cv2 < fv2,
                    FilterOperator.NotEquals => !cellValue.Equals(filterValue, StringComparison.OrdinalIgnoreCase),
                    FilterOperator.IsEmpty => string.IsNullOrEmpty(cellValue),
                    FilterOperator.IsNotEmpty => !string.IsNullOrEmpty(cellValue),
                    _ => true
                };
            });
        }

        // Apply quick filter across all visible columns
        if (!string.IsNullOrEmpty(QuickFilter))
        {
            var qf = QuickFilter;
            var searchCols = Columns.Where(c => c.IsVisible && c.ResolvedField is not null).ToList();
            result = result.Where(item => searchCols.Any(c =>
                (c.ResolvedField!(item)?.ToString() ?? "").Contains(qf, StringComparison.OrdinalIgnoreCase)));
        }

        return result;
    }

    internal async Task SetQuickFilter(string? value)
    {
        QuickFilter = value;
        InvalidateCache();
        _pageIndex = 0;
        if (QuickFilterChanged.HasDelegate)
            await QuickFilterChanged.InvokeAsync(value);
    }

    /// <summary>Get all filtered and sorted data (no paging). Used by virtual scrolling and export.</summary>
    internal IList<TItem> GetAllSortedData()
    {
        if (_isServerMode) return (IList<TItem>)(Data ?? (IReadOnlyList<TItem>)Array.Empty<TItem>());

        IEnumerable<TItem> result = ApplySort(GetCachedFilteredData());
        return result.ToList();
    }

    /// <summary>Get the current page of data, filtered and sorted.</summary>
    internal IEnumerable<TItem> GetPagedData()
    {
        if (VirtualizeRows) return GetAllSortedData(); // virtual scrolling shows all

        if (_isServerMode) return Data ?? Enumerable.Empty<TItem>();

        IEnumerable<TItem> result = ApplySort(GetCachedFilteredData());

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

    internal string GetFilterOperator(string columnKey)
    {
        return _filters.TryGetValue(columnKey, out var f) ? f.Operator.ToString() : "Contains";
    }

    internal void SetFilterOperator(string columnKey, string operatorName)
    {
        if (!_filters.ContainsKey(columnKey))
            _filters[columnKey] = new FilterDescriptor { Property = columnKey };
        if (Enum.TryParse<FilterOperator>(operatorName, out var op))
            _filters[columnKey].Operator = op;
        InvalidateCache();
        _pageIndex = 0;
        if (_isServerMode) _ = InvokeLoadData();
    }

    internal void SetBooleanFilter(string columnKey, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            // "All" selected — clear filter
            _filters.Remove(columnKey);
        }
        else
        {
            _filters[columnKey] = new FilterDescriptor
            {
                Property = columnKey,
                Operator = FilterOperator.Equals,
                Value = value
            };
        }
        _pageIndex = 0;
        InvalidateCache();
        if (_isServerMode) _ = InvokeLoadData();
    }

    internal void SetDateFilter(string columnKey, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            _filters.Remove(columnKey);
        }
        else
        {
            _filters[columnKey] = new FilterDescriptor
            {
                Property = columnKey,
                Operator = FilterOperator.Equals,
                Value = value
            };
        }
        _pageIndex = 0;
        InvalidateCache();
        if (_isServerMode) _ = InvokeLoadData();
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
        var match = _sortStack.FirstOrDefault(s => s.Property == columnKey);
        return match?.Direction ?? SortDirection.None;
    }

    internal int GetSortPriority(string columnKey)
    {
        var idx = _sortStack.FindIndex(s => s.Property == columnKey);
        return idx >= 0 ? idx + 1 : 0;
    }

    internal async Task HandleHeaderClick(ArcadiaColumn<TItem> column, bool addToStack = false)
    {
        var isSortable = column.Sortable ?? Sortable;
        if (!isSortable || column.ResolvedField is null) return;

        var key = column.ResolvedKey;
        var existing = _sortStack.FirstOrDefault(s => s.Property == key);

        if (existing is not null)
        {
            existing.Direction = existing.Direction switch
            {
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _ => SortDirection.Ascending
            };
            if (existing.Direction == SortDirection.None)
                _sortStack.Remove(existing);
        }
        else
        {
            if (!addToStack) _sortStack.Clear();
            _sortStack.Add(new SortDescriptor { Property = key, Direction = SortDirection.Ascending });
        }

        _pageIndex = 0;
        InvalidateCache();
        CancelEdit();
        if (SortChanged.HasDelegate)
            await SortChanged.InvokeAsync(_sortStack.Count > 0 ? _sortStack[0] : null);
        if (_isServerMode) await InvokeLoadData();
        _ = SaveStateAsync();
    }

    private IEnumerable<TItem> ApplySort(IEnumerable<TItem> data)
    {
        if (_sortStack.Count == 0) return data;

        IOrderedEnumerable<TItem>? ordered = null;
        foreach (var sort in _sortStack)
        {
            var sortCol = Columns.FirstOrDefault(c => c.ResolvedKey == sort.Property);
            if (sortCol?.ResolvedField is null) continue;

            if (ordered is null)
            {
                ordered = sort.Direction == SortDirection.Ascending
                    ? data.OrderBy(item => sortCol.ResolvedField(item))
                    : data.OrderByDescending(item => sortCol.ResolvedField(item));
            }
            else
            {
                ordered = sort.Direction == SortDirection.Ascending
                    ? ordered.ThenBy(item => sortCol.ResolvedField(item))
                    : ordered.ThenByDescending(item => sortCol.ResolvedField(item));
            }
        }
        return ordered ?? data;
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
        var dir = GetSortDirection(column.ResolvedKey);
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
            SortProperty = _sortStack.Count > 0 ? _sortStack[0].Property : null,
            SortDirection = _sortStack.Count > 0 ? _sortStack[0].Direction : SortDirection.None,
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

    // ── Inline editing (with double-commit guard + validation) ──

    internal bool IsEditing(TItem item) => _editingRow is not null && EqualityComparer<TItem>.Default.Equals(_editingRow, item);
    internal bool IsEditingCell(TItem item, string colKey) => IsEditing(item) && _editingColumn == colKey;

    /// <summary>Pending edit input value, captured from the edit cell for validation.</summary>
    private string? _editValue;

    internal void StartEdit(TItem item, string colKey)
    {
        // Clear any previous validation errors
        ClearAllValidationErrors();
        _editingRow = item;
        _editingColumn = colKey;
        // Initialize edit value from current cell value
        var col = Columns.FirstOrDefault(c => c.ResolvedKey == colKey);
        _editValue = col?.ResolvedField is not null ? col.FormatValue(col.ResolvedField(item)) : "";
    }

    /// <summary>Update the pending edit value (called from input oninput).</summary>
    internal void UpdateEditValue(string? value)
    {
        _editValue = value;
        // Live validation: validate as user types
        if (_editingColumn is not null)
        {
            var col = Columns.FirstOrDefault(c => c.ResolvedKey == _editingColumn);
            if (col?.Validator is not null)
            {
                col.ValidationError = col.Validator(value ?? "");
            }
        }
    }

    /// <summary>Get the current validation error for the editing column.</summary>
    internal string? GetEditValidationError()
    {
        if (_editingColumn is null) return null;
        var col = Columns.FirstOrDefault(c => c.ResolvedKey == _editingColumn);
        return col?.ValidationError;
    }

    internal async Task CommitEdit()
    {
        if (_editCommitting) return; // guard against double-commit from blur + Enter
        _editCommitting = true;
        try
        {
            // Validate before committing
            if (_editingColumn is not null)
            {
                var col = Columns.FirstOrDefault(c => c.ResolvedKey == _editingColumn);
                if (col?.Validator is not null)
                {
                    var error = col.Validator(_editValue ?? "");
                    col.ValidationError = error;
                    if (!string.IsNullOrEmpty(error))
                    {
                        Announce($"Validation error: {error}");
                        _editCommitting = false;
                        return; // Don't commit — stay in edit mode
                    }
                }
            }

            if (_editingRow is not null && OnRowEdit.HasDelegate)
                await OnRowEdit.InvokeAsync(_editingRow);
        }
        finally
        {
            if (_editCommitting) // only clear if we actually committed
            {
                ClearAllValidationErrors();
                _editingRow = default;
                _editingColumn = null;
                _editValue = null;
                _editCommitting = false;
            }
        }
    }

    internal void CancelEdit()
    {
        ClearAllValidationErrors();
        _editingRow = default;
        _editingColumn = null;
        _editValue = null;
    }

    private void ClearAllValidationErrors()
    {
        foreach (var col in Columns)
            col.ValidationError = null;
    }

    // ── Grouping ──

    internal List<(object Key, string Label, List<TItem> Items)> GetGroupedData()
    {
        var groupAccessor = ResolveGroupAccessor();
        if (groupAccessor is null) return new();

        IEnumerable<TItem> sorted = ApplySort(GetCachedFilteredData());

        return sorted
            .GroupBy(item => groupAccessor(item)?.ToString() ?? "")
            .Select(g => ((object)g.Key, g.Key.ToString() ?? "", g.ToList()))
            .ToList();
    }

    private Func<TItem, object>? ResolveGroupAccessor()
    {
        // 1. Explicit lambda takes precedence
        if (GroupByField is not null) return GroupByField;

        if (string.IsNullOrEmpty(GroupBy)) return null;

        // 2. Try matching a displayed column by key
        var col = Columns.FirstOrDefault(c => c.ResolvedKey == GroupBy);
        if (col?.ResolvedField is not null) return col.ResolvedField;

        // 3. Fall back to reflection on TItem property
        var prop = typeof(TItem).GetProperty(GroupBy,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
        if (prop is not null) return item => prop.GetValue(item)!;

        return null;
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

    // ── Export (CSV + Excel) ──

    /// <summary>Export grid data as an Excel (.xlsx) file download. Respects current sort, filter, and column visibility.</summary>
    public async Task ExportToExcelAsync(string filename = "export.xlsx")
    {
        try
        {
            var visibleCols = Columns.Where(c => c.IsVisible && c.ResolvedField is not null).ToList();
            IEnumerable<TItem> allData = ApplySort(GetCachedFilteredData());
            var bytes = Services.ExcelExportService.ToXlsx(visibleCols, allData);

            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Arcadia.DataGrid/js/datagrid-interop.js");
            await _jsModule.InvokeVoidAsync("downloadBlob", bytes, filename,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        catch (JSException ex) { System.Diagnostics.Debug.WriteLine($"[ArcadiaDataGrid] Excel export failed: {ex.Message}"); }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { /* Blazor Server circuit disconnected */ }
#endif
    }

    /// <summary>Export grid data as a PDF file download. Respects current sort, filter, and column visibility.</summary>
    /// <param name="filename">Download filename. Default is "export.pdf".</param>
    /// <param name="title">Optional title displayed above the table in the PDF.</param>
    /// <param name="orientation">Page orientation: "portrait" or "landscape". Default is "landscape".</param>
    public async Task ExportToPdfAsync(string filename = "export.pdf", string? title = null, string orientation = "landscape")
    {
        try
        {
            var visibleCols = Columns.Where(c => c.IsVisible && c.ResolvedField is not null).ToList();
            IEnumerable<TItem> allData = ApplySort(GetCachedFilteredData());
            var bytes = Services.PdfExportService.ToPdf(visibleCols, allData, title, orientation);

            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Arcadia.DataGrid/js/datagrid-interop.js");
            await _jsModule.InvokeVoidAsync("downloadBlob", bytes, filename, "application/pdf");
        }
        catch (JSException ex) { System.Diagnostics.Debug.WriteLine($"[ArcadiaDataGrid] PDF export failed: {ex.Message}"); }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { /* Blazor Server circuit disconnected */ }
#endif
    }

    internal string ToCsv()
    {
        var sb = new System.Text.StringBuilder();
        var visibleCols = Columns.Where(c => c.IsVisible && c.ResolvedField is not null).ToList();

        sb.AppendLine(string.Join(",", visibleCols.Select(c => CsvQuote(c.Title))));

        IEnumerable<TItem> allData = ApplySort(GetCachedFilteredData());

        foreach (var item in allData)
        {
            sb.AppendLine(string.Join(",", visibleCols.Select(c => CsvQuote(c.FormatValue(c.ResolvedField!(item))))));
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
        catch (JSException ex) { System.Diagnostics.Debug.WriteLine($"[ArcadiaDataGrid] CSV export failed: {ex.Message}"); }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { /* Blazor Server circuit disconnected */ }
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
        if (col.ResolvedField is null) return 0;
        var source = GetCachedFilteredData(); // FIX: was using Data, now uses filtered
        if (source.Count == 0) return 0;

        var values = source.Select(item =>
        {
            var raw = col.ResolvedField(item);
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

    /// <summary>Compute an aggregate value for a column over a specific subset of items (e.g., a group).</summary>
    internal double ComputeAggregateForItems(ArcadiaColumn<TItem> col, AggregateType type, IReadOnlyList<TItem> items)
    {
        if (col.ResolvedField is null || items.Count == 0) return 0;

        var values = items.Select(item =>
        {
            var raw = col.ResolvedField(item);
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

    /// <summary>Format an aggregate type label.</summary>
    internal static string AggregateLabel(AggregateType type) => type switch
    {
        AggregateType.Sum => "Sum",
        AggregateType.Average => "Avg",
        AggregateType.Count => "Count",
        AggregateType.Min => "Min",
        AggregateType.Max => "Max",
        _ => ""
    };

    // ── Virtual scroll helpers ──

    /// <summary>Generate CSS grid-template-columns from visible column widths.</summary>
    internal string GetGridTemplateColumns()
    {
        var cols = Columns.Where(c => c.IsVisible).ToList();
        var template = string.Join(" ", cols.Select(c => c.EffectiveWidth ?? "1fr"));
        return $"grid-template-columns: {template};";
    }

    /// <summary>Get all sorted data as a list for Virtualize Items binding.</summary>
    internal IList<TItem> GetAllSortedDataList() => GetAllSortedData();

#if NET6_0_OR_GREATER
    /// <summary>ItemsProvider for server-side virtual scrolling.</summary>
    internal async ValueTask<Microsoft.AspNetCore.Components.Web.Virtualization.ItemsProviderResult<TItem>> ProvideItems(
        Microsoft.AspNetCore.Components.Web.Virtualization.ItemsProviderRequest request)
    {
        if (!_isServerMode)
        {
            var allData = GetAllSortedDataList();
            var page = allData.Skip(request.StartIndex).Take(request.Count).ToList();
            return new Microsoft.AspNetCore.Components.Web.Virtualization.ItemsProviderResult<TItem>(page, allData.Count);
        }

        var args = new DataGridLoadArgs
        {
            Skip = request.StartIndex,
            Take = request.Count,
            SortProperty = _sortStack.Count > 0 ? _sortStack[0].Property : null,
            SortDirection = _sortStack.Count > 0 ? _sortStack[0].Direction : SortDirection.None,
            Filters = _filters.Values.Where(f => !string.IsNullOrEmpty(f.Value)).ToList()
        };
        await LoadData.InvokeAsync(args);
        return new Microsoft.AspNetCore.Components.Web.Virtualization.ItemsProviderResult<TItem>(
            Data?.ToList() ?? new List<TItem>(),
            ServerTotalCount ?? 0);
    }
#endif

    // ── Keyboard Navigation (WAI-ARIA Grid Pattern) ──

    internal void HandleGridKeyDown(KeyboardEventArgs e)
    {
        if (_editingRow is not null && e.Key != "Escape" && e.Key != "Enter" && e.Key != "Tab")
            return; // let edit input handle its own keys

        // Keyboard interaction means grid has focus
        _gridHasFocus = true;

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
                        StartEdit(pageRows[_focusRow], col.ResolvedKey);
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
            case "c" when (e.CtrlKey || e.MetaKey) && e.ShiftKey:
                _ = CopyToClipboardWithHeaders(visibleCols);
                break;
            case "c" when (e.CtrlKey || e.MetaKey) && !e.ShiftKey:
                _ = CopyToClipboard(visibleCols);
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

    internal void SetFocusCell(int rowIdx, int colIdx)
    {
        _focusRow = rowIdx;
        _focusCol = colIdx;
        _gridHasFocus = true;
    }

    internal bool IsFocusedCell(int rowIdx, int colIdx) => _gridHasFocus && rowIdx == _focusRow && colIdx == _focusCol;

    internal string GetCellId(int rowIdx, int colIdx) => $"cell-{rowIdx}-{colIdx}";

    internal string? GetActiveDescendant()
    {
        if (!_gridHasFocus) return null;
        return GetCellId(_focusRow, _focusCol);
    }

    // ── Clipboard ──

    private async Task CopyToClipboard(List<ArcadiaColumn<TItem>> visibleCols, bool includeHeaders = false)
    {
        try
        {
            var cols = visibleCols.Where(c => c.ResolvedField is not null).ToList();
            var sb = new System.Text.StringBuilder();

            if (includeHeaders)
            {
                sb.AppendLine(string.Join("\t", cols.Select(c => c.Title)));
            }

            IEnumerable<TItem> rows = _selectedItems.Count > 0
                ? _selectedItems
                : GetPagedData();

            foreach (var item in rows)
            {
                sb.AppendLine(string.Join("\t", cols.Select(c => c.FormatValue(c.ResolvedField!(item)))));
            }

            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Arcadia.DataGrid/js/datagrid-interop.js");
            await _jsModule.InvokeVoidAsync("copyToClipboard", sb.ToString());
            var suffix = includeHeaders ? " with headers" : "";
            Announce($"Copied {(_selectedItems.Count > 0 ? _selectedItems.Count : "all")} rows{suffix} to clipboard");
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ArcadiaDataGrid] Clipboard copy failed: {ex.Message}"); }
    }

    private Task CopyToClipboardWithHeaders(List<ArcadiaColumn<TItem>> visibleCols)
        => CopyToClipboard(visibleCols, includeHeaders: true);

    // ── Column Header Menu (Pin/Hide) ──

    private ArcadiaColumn<TItem>? _columnMenuTarget;
    private double _columnMenuX, _columnMenuY;
    private bool _showColumnMenu;

    internal void ShowColumnMenu(ArcadiaColumn<TItem> col, MouseEventArgs e)
    {
        _columnMenuTarget = col;
        _columnMenuX = e.ClientX;
        _columnMenuY = e.ClientY;
        _showColumnMenu = true;
    }

    internal void CloseColumnMenu() { _showColumnMenu = false; _columnMenuTarget = null; }

    internal void TogglePin(ArcadiaColumn<TItem> col)
    {
        col.IsFrozen = !col.IsFrozen;
        CloseColumnMenu();
        StateHasChanged();
    }

    internal void HideColumn(ArcadiaColumn<TItem> col)
    {
        col.IsVisible = false;
        CloseColumnMenu();
        StateHasChanged();
    }

    // ── Context Menu ──

    internal async Task HandleRowContextMenu(MouseEventArgs e, TItem item)
    {
        if (ContextMenuTemplate is null && !OnContextMenu.HasDelegate) return;
        _contextMenuItem = item;
        _contextMenuX = e.ClientX;
        _contextMenuY = e.ClientY;
        _showContextMenu = true;
        if (OnContextMenu.HasDelegate) await OnContextMenu.InvokeAsync(item);
    }

    internal void CloseContextMenu() { _showContextMenu = false; _contextMenuItem = default; }

    // ── Batch Editing ──

    /// <summary>Track a cell change for batch commit.</summary>
    internal void TrackBatchChange(TItem item, string columnKey, object? oldValue, object? newValue)
    {
        if (!BatchEdit) return;

        // Suppress observer on first batch change to avoid mid-edit rerenders
        if (_batchChanges.Count == 0)
            _collectionObserver?.Suppress();

        var existing = _batchChanges.FirstOrDefault(c =>
            EqualityComparer<TItem>.Default.Equals(c.Item, item) && c.ColumnKey == columnKey);
        if (existing is not null)
        {
            existing.NewValue = newValue;
            if (Equals(existing.OldValue, newValue))
                _batchChanges.Remove(existing); // reverted to original
        }
        else
        {
            _batchChanges.Add(new BatchEditChange<TItem>
            {
                Item = item, ColumnKey = columnKey, OldValue = oldValue, NewValue = newValue
            });
        }
    }

    /// <summary>Get the count of pending batch changes.</summary>
    public int PendingChangeCount => _batchChanges.Count;

    /// <summary>Check if a cell has been modified in batch mode.</summary>
    internal bool IsBatchModified(TItem item, string columnKey) =>
        _batchChanges.Any(c => EqualityComparer<TItem>.Default.Equals(c.Item, item) && c.ColumnKey == columnKey);

    /// <summary>Commit all batch changes and invoke OnBatchCommit callback.</summary>
    public async Task CommitBatchAsync()
    {
        if (_batchChanges.Count == 0) return;
        if (OnBatchCommit.HasDelegate)
            await OnBatchCommit.InvokeAsync(_batchChanges.ToList());
        _batchChanges.Clear();
        _collectionObserver?.Resume(triggerImmediately: true);
        Announce($"Saved {_batchChanges.Count} changes");
        StateHasChanged();
    }

    /// <summary>Discard all batch changes.</summary>
    public void DiscardBatch()
    {
        _batchChanges.Clear();
        _collectionObserver?.Resume(triggerImmediately: true);
        Announce("Changes discarded");
        StateHasChanged();
    }

    // ── Print ──

    /// <summary>Open a print-friendly version of the grid in a new window and trigger the browser print dialog.</summary>
    public async Task PrintAsync()
    {
        try
        {
            var visibleCols = Columns.Where(c => c.IsVisible).ToList();
            IEnumerable<TItem> allData = ApplySort(GetCachedFilteredData());

            var sb = new System.Text.StringBuilder();
            sb.Append("<table style=\"border-collapse:collapse;width:100%;font-family:system-ui,sans-serif;font-size:12px;\">");
            sb.Append("<thead><tr>");
            foreach (var col in visibleCols)
            {
                sb.Append($"<th style=\"border:1px solid #ddd;padding:8px 12px;background:#f5f5f5;text-align:{col.Align};font-weight:600;\">");
                sb.Append(System.Net.WebUtility.HtmlEncode(col.Title));
                sb.Append("</th>");
            }
            sb.Append("</tr></thead><tbody>");

            foreach (var item in allData)
            {
                sb.Append("<tr>");
                foreach (var col in visibleCols)
                {
                    var val = col.ResolvedField is not null ? col.FormatValue(col.ResolvedField(item)) : "";
                    sb.Append($"<td style=\"border:1px solid #ddd;padding:6px 12px;text-align:{col.Align};\">");
                    sb.Append(System.Net.WebUtility.HtmlEncode(val));
                    sb.Append("</td>");
                }
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");

            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Arcadia.DataGrid/js/datagrid-interop.js");
            await _jsModule.InvokeVoidAsync("printGrid", sb.ToString());
        }
        catch (JSException ex) { System.Diagnostics.Debug.WriteLine($"[ArcadiaDataGrid] Print failed: {ex.Message}"); }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { /* Blazor Server circuit disconnected */ }
#endif
    }

    // ── Column Resize Persist (JS → .NET callback) ──

    /// <summary>Called from JS when a column resize completes. Updates the column width and persists state.</summary>
    [JSInvokable]
    public async Task OnColumnResized(int columnIndex, string newWidth)
    {
        var visibleCols = Columns.Where(c => c.IsVisible).ToList();
        if (columnIndex >= 0 && columnIndex < visibleCols.Count)
        {
            visibleCols[columnIndex].RuntimeWidth = newWidth;
            await SaveStateAsync();
        }
    }

    // ── State Persistence ──

    /// <summary>Get the current grid state for persistence.</summary>
    public DataGridState GetState() => new()
    {
        Sorts = _sortStack.Count > 0 ? _sortStack.ToList() : null,
        Filters = _filters.Values.Where(f => !string.IsNullOrEmpty(f.Value)).ToList() is { Count: > 0 } fl ? fl : null,
        PageSize = PageSize,
        ColumnVisibility = Columns.Any(c => !c.IsVisible)
            ? Columns.ToDictionary(c => c.ResolvedKey, c => c.IsVisible)
            : null,
        ColumnWidths = Columns.Any(c => c.EffectiveWidth is not null)
            ? Columns.Where(c => c.EffectiveWidth is not null).ToDictionary(c => c.ResolvedKey, c => c.EffectiveWidth!)
            : null,
    };

    /// <summary>Restore grid state from a previously saved state object.</summary>
    public void RestoreState(DataGridState state)
    {
        if (state.Sorts is { Count: > 0 }) _sortStack = state.Sorts.ToList();
        if (state.Filters is { Count: > 0 })
        {
            _filters.Clear();
            foreach (var f in state.Filters) _filters[f.Property] = f;
        }
        if (state.PageSize.HasValue) PageSize = state.PageSize.Value;
        if (state.ColumnVisibility is not null)
        {
            foreach (var col in Columns)
            {
                if (state.ColumnVisibility.TryGetValue(col.ResolvedKey, out var vis))
                    col.IsVisible = vis;
            }
        }
        if (state.ColumnWidths is not null)
        {
            foreach (var col in Columns)
            {
                if (state.ColumnWidths.TryGetValue(col.ResolvedKey, out var width))
                    col.RuntimeWidth = width;
            }
        }
        InvalidateCache();
        StateHasChanged();
    }

    private async Task SaveStateAsync()
    {
        if (string.IsNullOrEmpty(StateKey) && !OnStateChanged.HasDelegate) return;

        var state = GetState();
        if (OnStateChanged.HasDelegate)
            await OnStateChanged.InvokeAsync(state);

        if (!string.IsNullOrEmpty(StateKey))
        {
            try
            {
                _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/Arcadia.DataGrid/js/datagrid-interop.js");
                await _jsModule.InvokeVoidAsync("saveState", StateKey, state.ToJson());
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ArcadiaDataGrid] Save state failed: {ex.Message}"); }
        }
    }

    private async Task LoadStateAsync()
    {
        if (string.IsNullOrEmpty(StateKey)) return;
        try
        {
            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Arcadia.DataGrid/js/datagrid-interop.js");
            var json = await _jsModule.InvokeAsync<string?>("loadState", StateKey);
            if (json is not null)
            {
                var state = DataGridState.FromJson(json);
                if (state is not null) RestoreState(state);
            }
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ArcadiaDataGrid] Load state failed: {ex.Message}"); }
    }

    // ── Infinite Scroll ──

    /// <summary>Called from JS IntersectionObserver when the infinite scroll sentinel becomes visible.</summary>
    [JSInvokable]
    public async Task OnInfiniteScrollTrigger()
    {
        if (_loadingMore || !InfiniteScroll || !OnLoadMore.HasDelegate) return;
        _loadingMore = true;
        StateHasChanged();
        _infiniteScrollPage++;
        await OnLoadMore.InvokeAsync(_infiniteScrollPage);
        _loadingMore = false;
        StateHasChanged();
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

    // ── Stacked / Grouped Column Headers ──

    /// <summary>Whether any visible column has a non-null Group parameter.</summary>
    internal bool HasColumnGroups() => Columns.Any(c => c.IsVisible && !string.IsNullOrEmpty(c.Group));

    /// <summary>Build the list of group header cells with their colspan values. Adjacent columns with the same Group are merged; ungrouped columns get an empty header.</summary>
    internal List<(string Title, int Span)> GetColumnGroupHeaders()
    {
        var result = new List<(string Title, int Span)>();
        var visibleCols = Columns.Where(c => c.IsVisible).ToList();

        string? currentGroup = null;
        int currentSpan = 0;

        foreach (var col in visibleCols)
        {
            var group = col.Group;
            if (group == currentGroup)
            {
                currentSpan++;
            }
            else
            {
                if (currentSpan > 0)
                    result.Add((currentGroup ?? "", currentSpan));
                currentGroup = group;
                currentSpan = 1;
            }
        }

        if (currentSpan > 0)
            result.Add((currentGroup ?? "", currentSpan));

        return result;
    }

    // ── Column Reorder (Drag & Drop) ──

    private int _dragSourceCol = -1;
    private int _dropTargetCol = -1;

    internal void StartColumnDrag(int colIndex) { if (AllowColumnReorder) _dragSourceCol = colIndex; }
    internal void SetDropTarget(int colIndex) { if (AllowColumnReorder) _dropTargetCol = colIndex; }
    internal void EndColumnDrag() { _dragSourceCol = -1; _dropTargetCol = -1; }

    internal async Task DropColumn(int targetIndex)
    {
        if (!AllowColumnReorder) { EndColumnDrag(); return; }
        if (_dragSourceCol >= 0 && _dragSourceCol != targetIndex)
        {
            Collector.MoveColumn(_dragSourceCol, targetIndex);
            Announce($"Column moved to position {targetIndex + 1}");
            if (OnColumnsReordered.HasDelegate)
                await OnColumnsReordered.InvokeAsync();
        }
        EndColumnDrag();
    }

    // ── Row Reorder (Drag & Drop) ──

    private int _dragSourceRow = -1;
    private int _dropTargetRow = -1;

    internal void StartRowDrag(int rowIndex) { _dragSourceRow = rowIndex; }
    internal void SetRowDropTarget(int rowIndex) { _dropTargetRow = rowIndex; }
    internal void EndRowDrag() { _dragSourceRow = -1; _dropTargetRow = -1; }

    internal async Task DropRow(int targetIndex)
    {
        if (_dragSourceRow >= 0 && _dragSourceRow != targetIndex && Data is System.Collections.IList mutableList)
        {
            var item = mutableList[_dragSourceRow];
            mutableList.RemoveAt(_dragSourceRow);
            mutableList.Insert(targetIndex, item!);
            InvalidateCache();
            Announce($"Row moved from {_dragSourceRow + 1} to {targetIndex + 1}");
            if (OnRowReordered.HasDelegate)
                await OnRowReordered.InvokeAsync((_dragSourceRow, targetIndex));
        }
        EndRowDrag();
    }

    // ── Command Column ──

    /// <summary>Whether a command column should be rendered (either via CommandTemplate parameter or registered ArcadiaCommandColumn child).</summary>
    internal bool HasCommandColumn => CommandTemplate is not null || CommandCollector.HasCommandColumn;

    /// <summary>Render the command cell for a given item.</summary>
    internal RenderFragment RenderCommandCell(TItem item)
    {
        if (CommandTemplate is not null)
            return CommandTemplate(item);
        if (CommandCollector.CommandColumn is not null)
            return CommandCollector.CommandColumn.RenderCell(item);
        return builder => { };
    }

    /// <summary>Get the command column title.</summary>
    internal string ResolvedCommandColumnTitle =>
        CommandCollector.CommandColumn?.Title ?? CommandColumnTitle;

    /// <summary>Get the command column width.</summary>
    internal string ResolvedCommandColumnWidth =>
        CommandCollector.CommandColumn?.Width ?? CommandColumnWidth;

    // ── Inline Row Add ──

    /// <summary>Insert a new default row and optionally start editing it.</summary>
    internal async Task AddRowAsync()
    {
        if (OnRowAdd.HasDelegate)
            await OnRowAdd.InvokeAsync();

        if (Data is System.Collections.IList mutableList)
        {
            try
            {
                var newItem = Activator.CreateInstance<TItem>();
                if (string.Equals(NewRowPosition, "bottom", StringComparison.OrdinalIgnoreCase))
                    mutableList.Add(newItem!);
                else
                    mutableList.Insert(0, newItem!);

                InvalidateCache();

                // Navigate to the new row's page and set it as current
                if (string.Equals(NewRowPosition, "bottom", StringComparison.OrdinalIgnoreCase))
                {
                    // Go to last page
                    if (PageSize > 0 && PageCount > 0)
                        _pageIndex = PageCount - 1;
                }
                else
                {
                    _pageIndex = 0;
                }

                _currentRow = newItem;

                // Start editing the first editable column if available
                var firstEditable = Columns.FirstOrDefault(c => c.IsVisible && c.Editable);
                if (firstEditable is not null && newItem is not null)
                    StartEdit(newItem, firstEditable.ResolvedKey);

                Announce("New row added");
            }
            catch (MissingMethodException)
            {
                // TItem has no parameterless constructor — caller should handle via OnRowAdd
            }
        }

        StateHasChanged();
    }

    // ── Disposal ──

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        try
        {
            if (_infiniteScrollObserver is not null)
                await _infiniteScrollObserver.InvokeVoidAsync("disconnect");
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ArcadiaDataGrid] Dispose scroll observer failed: {ex.Message}"); }
        try
        {
            if (_jsModule is not null)
                await _jsModule.DisposeAsync();
        }
#if NET6_0_OR_GREATER
        catch (JSDisconnectedException) { /* Blazor Server circuit disconnected — expected during teardown */ }
#endif
        catch (ObjectDisposedException) { /* Module already disposed — safe to ignore */ }
        _dotNetRef?.Dispose();
        _collectionObserver?.Dispose();
        GC.SuppressFinalize(this);
    }
}

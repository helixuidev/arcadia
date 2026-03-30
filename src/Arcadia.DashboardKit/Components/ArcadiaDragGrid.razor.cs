using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Arcadia.Core.Base;
using Arcadia.Core.Utilities;
using Arcadia.DashboardKit.Models;
using System.Text.Json;

namespace Arcadia.DashboardKit.Components;

/// <summary>
/// A CSS Grid-based drag-and-drop dashboard layout component.
/// Renders child <see cref="ArcadiaDragGridItem"/> components in a responsive grid
/// with optional drag reordering and resize support.
/// </summary>
public partial class ArcadiaDragGrid : ArcadiaInteropBase
{
    private readonly List<ArcadiaDragGridItem> _items = new();
    private DotNetObjectReference<ArcadiaDragGrid>? _dotNetRef;

    // Keyboard drag state
    private string? _keyboardDragItemId;
    private string _liveAnnouncement = "";

    // Wiggle mode state (iOS-style long-press to enter edit)
    private bool _wiggleActive;

    /// <summary>
    /// Gets or sets the child content (should contain <see cref="ArcadiaDragGridItem"/> components).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the number of columns in the grid. Default is 4.
    /// </summary>
    [Parameter]
    public int Columns { get; set; } = 4;

    /// <summary>
    /// Gets or sets the height of each row in pixels. Default is 120.
    /// </summary>
    [Parameter]
    public int RowHeight { get; set; } = 120;

    /// <summary>
    /// Gets or sets the gap between grid items in pixels. Default is 16.
    /// </summary>
    [Parameter]
    public int Gap { get; set; } = 16;

    /// <summary>
    /// Gets or sets the current layout state. Use with <see cref="LayoutChanged"/> for two-way binding.
    /// </summary>
    [Parameter]
    public DragGridLayout? Layout { get; set; }

    /// <summary>
    /// Callback invoked when the layout changes due to drag or resize operations.
    /// </summary>
    [Parameter]
    public EventCallback<DragGridLayout> LayoutChanged { get; set; }

    /// <summary>
    /// Gets or sets whether edit mode is enabled (allows dragging and resizing). Default is true.
    /// </summary>
    [Parameter]
    public bool EditMode { get; set; } = true;

    /// <summary>
    /// Gets or sets the drag interaction mode.
    /// "direct" (default) — drag immediately from the handle.
    /// "longpress" — iOS-style: long-press any card to enter wiggle mode, then drag to reorder. Tap "Done" to exit.
    /// </summary>
    [Parameter]
    public string DragMode { get; set; } = "direct";

    /// <summary>
    /// Gets or sets the long-press duration in milliseconds before entering wiggle mode. Default is 500.
    /// </summary>
    [Parameter]
    public int LongPressDuration { get; set; } = 500;

    /// <summary>Whether wiggle mode is currently active (iOS-style jiggle).</summary>
    internal bool IsWiggling => _wiggleActive;

    /// <summary>Exits wiggle mode (called by "Done" button).</summary>
    private async Task ExitWiggleMode()
    {
        _wiggleActive = false;
        if (Module is not null)
            await Module.InvokeVoidAsync("setWiggleMode", ElementRef, false);
        StateHasChanged();
    }

    /// <summary>Called from JS when long-press triggers wiggle mode.</summary>
    [JSInvokable]
    public void OnWiggleStart()
    {
        _wiggleActive = true;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>Called from JS when wiggle mode is exited.</summary>
    [JSInvokable]
    public void OnWiggleEnd()
    {
        _wiggleActive = false;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Gets or sets whether items can be resized. Default is true.
    /// </summary>
    [Parameter]
    public bool AllowResize { get; set; } = true;

    /// <summary>
    /// Gets or sets the scale factor applied to items while being dragged. Default is 1.03.
    /// </summary>
    [Parameter]
    public double DragScaleUp { get; set; } = 1.03;

    /// <summary>
    /// Gets or sets the spring stiffness for drag animations. Default is 170.
    /// </summary>
    [Parameter]
    public int SpringStiffness { get; set; } = 170;

    /// <summary>
    /// Gets or sets the spring damping for drag animations. Default is 26.
    /// </summary>
    [Parameter]
    public int SpringDamping { get; set; } = 26;

    /// <summary>
    /// Gets or sets whether the layout is automatically saved to localStorage on every drop or resize.
    /// When true, the layout is persisted using the key "arcadia-draggrid-layout" (or a custom key via <see cref="SaveLayoutAsync"/>).
    /// </summary>
    [Parameter]
    public bool AutoSave { get; set; }

    /// <summary>
    /// Gets or sets whether the "Add Panel" button is displayed above the grid.
    /// </summary>
    [Parameter]
    public bool ShowAddButton { get; set; }

    /// <summary>
    /// Callback invoked when the "Add Panel" button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnAddPanel { get; set; }

    /// <summary>
    /// Gets or sets whether each panel displays a remove ("x") button in its header.
    /// </summary>
    [Parameter]
    public bool AllowRemove { get; set; }

    /// <summary>
    /// Callback invoked when a panel's remove button is clicked.
    /// The string parameter is the ID of the panel being removed.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnRemovePanel { get; set; }

    /// <summary>
    /// Callback invoked when an item is reordered via drag-and-drop.
    /// </summary>
    [Parameter]
    public EventCallback<DragGridReorderEventArgs> OnReorder { get; set; }

    /// <summary>
    /// Callback invoked when an item is resized.
    /// </summary>
    [Parameter]
    public EventCallback<DragGridResizeEventArgs> OnItemResized { get; set; }

    /// <inheritdoc />
    protected override string ModulePath => "./_content/Arcadia.DashboardKit/js/arcadia-draggrid.js";

    /// <summary>
    /// Element reference for the grid container.
    /// </summary>
    protected ElementReference ElementRef;

    // Tracks the visual display order of items (may differ from DOM order)
    private readonly List<string> _order = new();

    // Explicit grid positions computed by occupancy grid algorithm
    // Key = item ID, Value = (col, row) 1-based
    private readonly Dictionary<string, (int Col, int Row)> _gridPositions = new();

    internal void RegisterItem(ArcadiaDragGridItem item)
    {
        if (!_items.Contains(item))
        {
            _items.Add(item);
            if (item.Id is not null && !_order.Contains(item.Id))
                _order.Add(item.Id);
        }
    }

    internal void UnregisterItem(ArcadiaDragGridItem item)
    {
        _items.Remove(item);
        if (item.Id is not null) _order.Remove(item.Id);
    }

    /// <summary>Returns the explicit grid column start for a given item ID (1-based).</summary>
    internal int GetGridCol(string? id) => id is not null && _gridPositions.TryGetValue(id, out var pos) ? pos.Col : 0;

    /// <summary>Returns the explicit grid row start for a given item ID (1-based).</summary>
    internal int GetGridRow(string? id) => id is not null && _gridPositions.TryGetValue(id, out var pos) ? pos.Row : 0;

    /// <summary>Recomputes explicit grid positions for all items using a 2D occupancy grid.</summary>
    private void RecomputeGridPositions()
    {
        _gridPositions.Clear();
        var grid = new string?[50, Columns]; // [row, col]

        foreach (var id in _order)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item is null) continue;

            // Skip floating items — they are positioned absolutely
            if (item.Floating) continue;

            var cs = Math.Min(item.EffectiveColSpan, Columns);
            var rs = item.EffectiveRowSpan;
            var placed = false;

            for (var r = 0; !placed; r++)
            {
                for (var c = 0; c <= Columns - cs; c++)
                {
                    var fits = true;
                    for (var dr = 0; dr < rs && fits; dr++)
                        for (var dc = 0; dc < cs && fits; dc++)
                            if (r + dr < 50 && grid[r + dr, c + dc] is not null)
                                fits = false;

                    if (fits)
                    {
                        for (var dr = 0; dr < rs; dr++)
                            for (var dc = 0; dc < cs; dc++)
                                if (r + dr < 50)
                                    grid[r + dr, c + dc] = id;

                        _gridPositions[id] = (c + 1, r + 1); // CSS Grid is 1-based
                        placed = true;
                        break; // Exit column loop
                    }
                }
            }
        }
    }

    private bool _lastEditMode;
    private string _lastDragMode = "direct";

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Let the base class load the JS module first
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            // All children have registered by now — compute the layout
            RecomputeGridPositions();
            _lastEditMode = EditMode;
            _lastDragMode = DragMode;
            StateHasChanged();
        }

        // Sync EditMode changes to JS
        if (!firstRender && Module is not null && EditMode != _lastEditMode)
        {
            _lastEditMode = EditMode;
            await Module.InvokeVoidAsync("setEditMode", ElementRef, EditMode);
        }

        // Sync DragMode changes to JS
        if (!firstRender && Module is not null && DragMode != _lastDragMode)
        {
            _lastDragMode = DragMode;
            var wasWiggling = _wiggleActive;
            _wiggleActive = false;
            await Module.InvokeVoidAsync("setDragMode", ElementRef, DragMode, LongPressDuration);
            if (wasWiggling) StateHasChanged(); // Re-render to hide Done button
        }

        if (firstRender && Module is not null)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await Module.InvokeVoidAsync("initialize", ElementRef, _dotNetRef, new
            {
                columns = Columns,
                rowHeight = RowHeight,
                gap = Gap,
                editMode = EditMode,
                allowResize = AllowResize,
                dragScaleUp = DragScaleUp,
                springStiffness = SpringStiffness,
                springDamping = SpringDamping,
                dragMode = DragMode,
                longPressDuration = LongPressDuration
            });
        }
    }

    // ══════════════════════════════════════════
    // STATE PERSISTENCE
    // ══════════════════════════════════════════

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the current layout state and saves it to localStorage via JS interop.
    /// </summary>
    /// <param name="key">The localStorage key to save under. Defaults to "arcadia-draggrid-layout".</param>
    public async Task SaveLayoutAsync(string key = "arcadia-draggrid-layout")
    {
        if (Module is null) return;

        UpdateLayout();
        var json = JsonSerializer.Serialize(Layout, _jsonOptions);
        await Module.InvokeVoidAsync("saveLayout", key, json);
    }

    /// <summary>
    /// Loads a layout from localStorage and restores the grid order and item spans.
    /// </summary>
    /// <param name="key">The localStorage key to load from. Defaults to "arcadia-draggrid-layout".</param>
    public async Task LoadLayoutAsync(string key = "arcadia-draggrid-layout")
    {
        if (Module is null) return;

        var json = await Module.InvokeAsync<string?>("loadLayout", key);
        if (string.IsNullOrEmpty(json)) return;

        var layout = JsonSerializer.Deserialize<DragGridLayout>(json, _jsonOptions);
        if (layout is null) return;

        // Restore order
        _order.Clear();
        foreach (var pos in layout.Items.OrderBy(i => i.Order))
        {
            _order.Add(pos.Id);

            // Restore spans
            var item = _items.FirstOrDefault(i => i.Id == pos.Id);
            if (item is not null)
            {
                item.SetSpans(pos.ColSpan, pos.RowSpan);
            }
        }

        // Add any items not in the saved layout at the end
        foreach (var item in _items)
        {
            if (item.Id is not null && !_order.Contains(item.Id))
                _order.Add(item.Id);
        }

        RecomputeGridPositions();
        Layout = layout;
        await LayoutChanged.InvokeAsync(layout);
        StateHasChanged();
    }

    /// <summary>
    /// Performs auto-save if <see cref="AutoSave"/> is enabled.
    /// </summary>
    private async Task AutoSaveIfEnabled()
    {
        if (AutoSave)
        {
            await SaveLayoutAsync();
        }
    }

    // ══════════════════════════════════════════
    // KEYBOARD ACCESSIBILITY
    // ══════════════════════════════════════════

    /// <summary>
    /// Processes keyboard navigation events from a grid item.
    /// Supports Enter/Space to pick up and drop items, arrow keys to move, and Escape to cancel.
    /// </summary>
    /// <param name="itemId">The ID of the item that received the keyboard event.</param>
    /// <param name="e">The keyboard event arguments.</param>
    internal async Task HandleItemKeyDown(string itemId, KeyboardEventArgs e)
    {
        if (!EditMode) return;

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null || item.Locked) return;

        if (e.Key is "Enter" or " ")
        {
            if (_keyboardDragItemId is null)
            {
                // Pick up the item
                _keyboardDragItemId = itemId;
                var idx = _order.IndexOf(itemId);
                Announce($"{GetItemLabel(itemId)} picked up. Position {idx + 1} of {_order.Count}. Use arrow keys to move, Enter to drop, Escape to cancel.");
            }
            else if (_keyboardDragItemId == itemId)
            {
                // Drop the item
                var idx = _order.IndexOf(itemId);
                Announce($"{GetItemLabel(itemId)} dropped at position {idx + 1}.");
                _keyboardDragItemId = null;
                await AutoSaveIfEnabled();
            }
        }
        else if (e.Key == "Escape" && _keyboardDragItemId is not null)
        {
            Announce($"{GetItemLabel(_keyboardDragItemId)} move cancelled.");
            _keyboardDragItemId = null;
        }
        else if (_keyboardDragItemId == itemId)
        {
            var currentIdx = _order.IndexOf(itemId);
            var targetIdx = -1;

            switch (e.Key)
            {
                case "ArrowRight":
                    if (currentIdx < _order.Count - 1) targetIdx = currentIdx + 1;
                    break;
                case "ArrowLeft":
                    if (currentIdx > 0) targetIdx = currentIdx - 1;
                    break;
                case "ArrowDown":
                    // Move down by Columns positions (next row)
                    targetIdx = Math.Min(currentIdx + Columns, _order.Count - 1);
                    if (targetIdx == currentIdx) targetIdx = -1;
                    break;
                case "ArrowUp":
                    // Move up by Columns positions (previous row)
                    targetIdx = Math.Max(currentIdx - Columns, 0);
                    if (targetIdx == currentIdx) targetIdx = -1;
                    break;
            }

            if (targetIdx >= 0 && targetIdx != currentIdx)
            {
                // Swap
                (_order[currentIdx], _order[targetIdx]) = (_order[targetIdx], _order[currentIdx]);
                RecomputeGridPositions();
                UpdateLayout();

                if (OnReorder.HasDelegate)
                {
                    await OnReorder.InvokeAsync(new DragGridReorderEventArgs
                    {
                        ItemId = itemId,
                        OldIndex = currentIdx,
                        NewIndex = targetIdx
                    });
                }

                Announce($"{GetItemLabel(itemId)} moved to position {targetIdx + 1} of {_order.Count}.");
                StateHasChanged();
            }
        }
    }

    /// <summary>
    /// Gets whether the specified item is currently being keyboard-dragged.
    /// </summary>
    /// <param name="itemId">The item ID to check.</param>
    /// <returns>True if the item is in keyboard drag mode.</returns>
    internal bool IsKeyboardDragging(string? itemId) => itemId is not null && _keyboardDragItemId == itemId;

    private string GetItemLabel(string itemId)
    {
        // Use a human-readable label if available via HeaderContent, fallback to ID
        return itemId;
    }

    private void Announce(string message)
    {
        _liveAnnouncement = message;
        StateHasChanged();
    }

    // ══════════════════════════════════════════
    // ADD / REMOVE PANELS
    // ══════════════════════════════════════════

    private async Task HandleAddPanel()
    {
        if (OnAddPanel.HasDelegate)
        {
            await OnAddPanel.InvokeAsync();
        }
    }

    /// <summary>
    /// Handles the remove button click for a panel.
    /// Invoked by child <see cref="ArcadiaDragGridItem"/> components.
    /// </summary>
    /// <param name="itemId">The ID of the panel to remove.</param>
    internal async Task HandleRemovePanel(string itemId)
    {
        if (OnRemovePanel.HasDelegate)
        {
            await OnRemovePanel.InvokeAsync(itemId);
        }
    }

    // ══════════════════════════════════════════
    // JS INVOKABLE CALLBACKS
    // ══════════════════════════════════════════

    /// <summary>
    /// Called from JavaScript when a drag-and-drop reorder completes.
    /// </summary>
    /// <param name="itemId">The ID of the dragged item.</param>
    /// <param name="newIndex">The new index position.</param>
    [JSInvokable]
    public async Task OnDropComplete(string itemId, int newIndex)
    {
        var oldIndex = _order.IndexOf(itemId);
        if (oldIndex < 0 || oldIndex == newIndex || newIndex < 0 || newIndex >= _order.Count) return;

        // Swap the two items in the order list (mirrors JS swap)
        (_order[oldIndex], _order[newIndex]) = (_order[newIndex], _order[oldIndex]);
        RecomputeGridPositions();

        UpdateLayout();

        if (OnReorder.HasDelegate)
        {
            await OnReorder.InvokeAsync(new DragGridReorderEventArgs
            {
                ItemId = itemId,
                OldIndex = oldIndex,
                NewIndex = newIndex
            });
        }

        StateHasChanged();
        await AutoSaveIfEnabled();
    }

    /// <summary>
    /// Called from JavaScript when a resize operation completes.
    /// </summary>
    /// <param name="itemId">The ID of the resized item.</param>
    /// <param name="newColSpan">The new column span.</param>
    /// <param name="newRowSpan">The new row span.</param>
    [JSInvokable]
    public async Task OnResizeComplete(string itemId, int newColSpan, int newRowSpan)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return;

        var oldCol = item.ColSpan;
        var oldRow = item.RowSpan;
        item.SetSpans(newColSpan, newRowSpan);

        RecomputeGridPositions();
        UpdateLayout();

        if (OnItemResized.HasDelegate)
        {
            await OnItemResized.InvokeAsync(new DragGridResizeEventArgs
            {
                ItemId = itemId,
                OldColSpan = oldCol,
                OldRowSpan = oldRow,
                NewColSpan = newColSpan,
                NewRowSpan = newRowSpan
            });
        }

        StateHasChanged();
        await AutoSaveIfEnabled();
    }

    /// <summary>
    /// Called from JavaScript when a drag operation starts.
    /// </summary>
    /// <param name="itemId">The ID of the item being dragged.</param>
    [JSInvokable]
    public void OnDragStart(string itemId) { }

    /// <summary>
    /// Called from JavaScript when a drag operation ends.
    /// </summary>
    /// <param name="itemId">The ID of the item that was dragged.</param>
    [JSInvokable]
    public void OnDragEnd(string itemId) { }

    private void UpdateLayout()
    {
        var layout = new DragGridLayout();
        for (var i = 0; i < _order.Count; i++)
        {
            var item = _items.FirstOrDefault(it => it.Id == _order[i]);
            if (item is null) continue;
            layout.Items.Add(new DragGridItemPosition
            {
                Id = _order[i],
                Order = i,
                ColSpan = item.EffectiveColSpan,
                RowSpan = item.EffectiveRowSpan,
                Locked = item.Locked
            });
        }

        Layout = layout;
        _ = LayoutChanged.InvokeAsync(layout);
    }

    private string GridStyle
    {
        get
        {
            var s = $"grid-template-columns:repeat({Columns},1fr);grid-auto-rows:{RowHeight}px;gap:{Gap}px;";
            if (!string.IsNullOrEmpty(Style))
            {
                s += Style;
            }

            return s;
        }
    }

    private string? CssClass => CssBuilder.Default("arcadia-draggrid")
        .AddClass("arcadia-draggrid--edit", EditMode && DragMode != "longpress")
        .AddClass("arcadia-draggrid--edit", DragMode == "longpress" && _wiggleActive)
        .AddClass("arcadia-draggrid--wiggle", _wiggleActive)
        .AddClass($"arcadia-draggrid--mode-{DragMode}")
        .AddClass(Class)
        .Build();

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore()
    {
        if (Module is not null)
        {
            try
            {
                await Module.InvokeVoidAsync("dispose", ElementRef);
            }
            catch
            {
                // Swallow errors during disposal (circuit may be disconnected)
            }
        }

        _dotNetRef?.Dispose();
        await base.DisposeAsyncCore();
    }
}

# DataGrid Column Reorder (Drag & Drop) — PRP

## Problem

Users cannot rearrange column order. Enterprise grids allow dragging column headers to new positions.

## Approach

HTML5 Drag and Drop API on column headers. No external JS library — native browser support.

## Behavior

1. User grabs a column header (mousedown/pointerdown)
2. Header gets ghost opacity (0.4), cursor changes to `grabbing`
3. As user drags over other headers, a blue insertion line appears at the drop position
4. On drop, columns reorder in the internal list
5. Animation: remaining columns slide to new positions (CSS transition on `transform`)

## Implementation

### Blazor Side

```razor
<th draggable="true"
    @ondragstart="@(() => StartColumnDrag(colIndex))"
    @ondragover:preventDefault
    @ondragover="@(() => SetDropTarget(colIndex))"
    @ondrop="@(() => DropColumn(colIndex))"
    @ondragend="EndColumnDrag"
    class="@(colIndex == _dragSourceCol ? "arcadia-grid__th--dragging" : "")
           @(colIndex == _dropTargetCol ? "arcadia-grid__th--drop-target" : "")">
```

### State

```csharp
private int _dragSourceCol = -1;
private int _dropTargetCol = -1;

void StartColumnDrag(int colIndex) { _dragSourceCol = colIndex; }
void SetDropTarget(int colIndex) { _dropTargetCol = colIndex; }
void EndColumnDrag() { _dragSourceCol = -1; _dropTargetCol = -1; }
void DropColumn(int targetIndex)
{
    if (_dragSourceCol >= 0 && _dragSourceCol != targetIndex)
    {
        var col = Collector.Columns[_dragSourceCol];
        Collector.MoveColumn(_dragSourceCol, targetIndex);
    }
    EndColumnDrag();
}
```

### CSS

```css
.arcadia-grid__th--dragging { opacity: 0.4; }
.arcadia-grid__th--drop-target { box-shadow: inset -3px 0 0 var(--_accent); }
.arcadia-grid__th { transition: transform 0.2s ease; }
```

### Column Collector Change

Add `MoveColumn(int from, int to)` to `ArcadiaDataGridColumnCollector`:

```csharp
public void MoveColumn(int fromIndex, int toIndex)
{
    if (fromIndex < 0 || fromIndex >= _columns.Count) return;
    if (toIndex < 0 || toIndex >= _columns.Count) return;
    var col = _columns[fromIndex];
    _columns.RemoveAt(fromIndex);
    _columns.Insert(toIndex, col);
}
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Reorderable` | `bool` | `false` | Enable column drag-and-drop reorder |

## Files to Modify

| File | Changes |
|------|---------|
| `ArcadiaDataGrid.razor` | Add drag attributes to th elements |
| `ArcadiaDataGrid.razor.cs` | Drag state, StartDrag/Drop/End methods, Reorderable param |
| `ArcadiaDataGridColumnCollector.cs` | MoveColumn method |
| `arcadia-datagrid.css` | Already has drag state classes |

## Testing

- E2E: Drag column A to position B, verify new order
- E2E: Verify data cells follow header reorder
- Manual: Verify animation smoothness

# DataGrid Column Freeze / Pinning — PRP

## Problem

When grid has many columns and horizontal scroll, the identifier column (name, ID) scrolls out of view. Users need to pin columns to the left (or right) edge.

## Approach

CSS `position: sticky` on individual `<th>` and `<td>` cells. No JS needed.

## Behavior

1. User sets `Frozen="true"` on a column definition
2. That column's th and td get `position: sticky; left: 0; z-index: 1`
3. Multiple frozen columns stack: first at `left: 0`, second at `left: {firstWidth}px`
4. Frozen columns have a subtle right border/shadow to indicate the freeze edge
5. Non-frozen columns scroll horizontally underneath

## Parameters

### On ArcadiaColumn

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Frozen` | `bool` | `false` | Pin this column to the left edge |
| `FrozenPosition` | `string` | `"left"` | `"left"` or `"right"` |

## Implementation

### Razor Template

```razor
<th style="@(col.Frozen ? $"position:sticky;left:{GetFrozenOffset(colIndex)}px;z-index:3;" : "")">
<td style="@(col.Frozen ? $"position:sticky;left:{GetFrozenOffset(colIndex)}px;z-index:1;" : "")">
```

### Offset Calculation

```csharp
internal double GetFrozenOffset(int colIndex)
{
    double offset = 0;
    for (var i = 0; i < colIndex; i++)
    {
        if (Columns[i].Frozen)
            offset += GetColumnWidth(i); // needs JS interop or stored width
    }
    return offset;
}
```

### CSS

```css
.arcadia-grid__td--frozen,
.arcadia-grid__th--frozen {
  position: sticky;
  z-index: 1;
  background: var(--_bg); /* must be opaque to cover scrolling content */
}
.arcadia-grid__td--frozen::after,
.arcadia-grid__th--frozen::after {
  content: "";
  position: absolute;
  right: -4px;
  top: 0;
  bottom: 0;
  width: 4px;
  background: linear-gradient(to right, rgba(0,0,0,0.08), transparent);
}
```

### Challenge: Column Width

Sticky positioning with `left` offset requires knowing the width of preceding frozen columns. Options:

1. **Require explicit `Width` on frozen columns** — simplest, user sets `Width="200px"`
2. **JS interop to measure** — read `offsetWidth` after first render
3. **CSS-only with `left: 0`** — only supports one frozen column

**Recommendation: Option 1 for v1.** Require Width on frozen columns. Add warning in docs.

## Files to Modify

| File | Changes |
|------|---------|
| `ArcadiaColumn.razor` | Add Frozen, FrozenPosition params |
| `ArcadiaDataGrid.razor` | Add sticky styles to frozen th/td |
| `ArcadiaDataGrid.razor.cs` | GetFrozenOffset calculation |
| `arcadia-datagrid.css` | Frozen cell styles with shadow edge |

## Testing

- E2E: Scroll horizontally, verify frozen column stays visible
- E2E: Verify frozen column has opaque background (no bleed-through)
- Manual: Verify shadow edge is visible at freeze boundary

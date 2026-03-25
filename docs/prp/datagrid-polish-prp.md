# DataGrid Visual Polish — PRP

## Problems Identified

### 1. Vapor theme contrast — CRITICAL
Row text is unreadable: white text on near-white translucent background. The `rgba(255,255,255,0.02)` surface doesn't provide enough contrast against a light or transparent page background. Need explicit text color fallbacks and higher-contrast surface values for Vapor.

Also: the "Group by" dropdown and status text below the grid inherit page styles (white on white). These controls are OUTSIDE the grid component but need to follow the theme.

### 2. Column resizing — MISSING
Users expect to drag column borders to resize. Every serious grid has this.

Implementation:
- Drag handle on the right edge of each `<th>`
- JS interop for pointer events (pointerdown → pointermove → pointerup)
- Update column width in real-time during drag
- Cursor changes to `col-resize` on hover
- Min width constraint (50px default)
- Double-click auto-sizes to content width

### 3. Column reordering (drag & drop) — MISSING
Drag a column header to a new position. The column order updates live.

Implementation:
- JS interop for HTML5 drag events (dragstart, dragover, drop)
- Visual feedback: dragged header gets ghost overlay, drop target shows insertion line
- On drop: reorder the internal column list, re-render
- Animated transition for columns shifting position

### 4. Animations — LACKING
The grid feels static. Modern grids have:
- **Row enter/exit** — fade + slide on page change
- **Sort transition** — rows animate to new positions when sorted
- **Filter transition** — filtered-out rows fade, remaining rows slide up
- **Expand/collapse** — detail rows slide open/closed
- **Selection** — background color transitions (already partially done)
- **Column reorder** — columns slide to new positions

Realistic scope for now:
- Row fade-in on page change (CSS animation on tbody tr)
- Detail row slide-open (max-height transition)
- Smooth background transitions (already have 0.1s on hover)
- Column header drag visual feedback

### 5. Demo controls styling
The "Group by" dropdown below the grid uses inline styles with hardcoded colors. Should use grid theme variables or at least proper dark-mode colors.

---

## Implementation Plan

### Fix 1: Vapor contrast (10 min)
- Increase Vapor surface to `rgba(255,255,255,0.05)` minimum for bg
- Set explicit `--_text` to `#E4E4E7` (not inherited)
- Add `.arcadia-grid--vapor .arcadia-grid__table` explicit background
- Ensure table cells get background from the grid, not transparent

### Fix 2: Column resizing (45 min)
- Add `Resizable` bool parameter on ArcadiaColumn
- Add resize handle `<div>` at right edge of each th
- JS interop: `startResize(element, colIndex)` → track pointermove → update width
- CSS: `.arcadia-grid__resize-handle` positioned absolutely at right edge
- Store widths in a `Dictionary<string, double>` in grid state
- Apply widths via inline style on th and td

### Fix 3: Column reordering (30 min)
- Add `Reorderable` bool parameter on grid
- HTML5 drag on column headers: `draggable="true"`, `@ondragstart`, `@ondragover`, `@ondrop`
- Track drag source and drop target column indices
- On drop: swap columns in the collector's list
- CSS: `.arcadia-grid__th--dragging` (opacity: 0.5), `.arcadia-grid__th--drop-target` (left border highlight)

### Fix 4: Row animations (20 min)
- Add `@keyframes arcadia-grid-row-enter` (fade-in + slide-up, 0.15s)
- Apply to `tbody tr` on render
- Detail row: `max-height` transition (0 → 300px, 0.2s ease)
- Page change: tbody gets a brief opacity transition

### Fix 5: Demo controls (5 min)
- Move inline styles to use CSS variables
- Or wrap controls in a styled div that reads `--_text` and `--_border`

---

## Files to modify

| File | Changes |
|------|---------|
| `arcadia-datagrid.css` | Vapor fix, resize handle, drag states, row animations, detail transition |
| `ArcadiaDataGrid.razor` | Resize handles in th, drag attributes on headers |
| `ArcadiaDataGrid.razor.cs` | Resizable/Reorderable params, column width state, drag state |
| `datagrid-interop.js` | Column resize pointer tracking |
| `TestDataGrid.razor` | Fix demo controls styling |
| `vapor.css` (Theme) | Higher contrast surface values |

## Priority Order
1. Vapor contrast fix (blocks usability)
2. Row animations (quick visual win)
3. Column resizing (expected feature)
4. Column reordering (nice to have)
5. Demo controls styling (polish)

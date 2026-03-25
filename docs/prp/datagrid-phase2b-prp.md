# DataGrid Phase 2B — Mini PRP

## Scope

Complete the remaining Phase 2 features plus the highest-impact Phase 3 items that are feasible without JS interop.

## What we're building (in order)

### 1. Server-side data (LoadData callback)
**Why:** Enterprise grids almost always load from an API. This is the #1 pattern people test.

- Add `LoadData` EventCallback<DataGridLoadArgs> parameter
- Add `DataGridLoadArgs` type: Skip, Take, SortProperty, SortDirection, Filters
- Add `TotalCount` parameter for server-side paging
- When `LoadData` is set, grid calls it on sort/filter/page change instead of client-side logic
- Demo with simulated async delay

### 2. Footer aggregates
**Why:** Quick visual — "Sum: $4,250,000" at the bottom of a salary column.

- Add `FooterTemplate` RenderFragment on ArcadiaColumn
- Add built-in `Aggregate` parameter: None, Sum, Average, Count, Min, Max
- Render `<tfoot>` row below `<tbody>`
- Format using column's Format string

### 3. Column visibility toggle
**Why:** Power users expect to show/hide columns.

- Add toolbar button "Columns" that shows a dropdown checklist
- Toggle `Visible` property on each column
- Persist in component state

### 4. Master-detail (expandable rows)
**Why:** The "wow" feature in grid demos.

- Add `DetailTemplate` RenderFragment<TItem> on ArcadiaDataGrid
- Click expand icon in first column → renders detail row spanning all columns
- Collapse on second click
- Track expanded row set internally

### 5. Cell editing (inline)
**Why:** The feature that separates "display grid" from "real grid."

- Add `Editable` bool on ArcadiaColumn
- Add `EditTemplate` RenderFragment<TItem> on ArcadiaColumn
- Double-click cell → switches to edit mode (shows EditTemplate or auto-input)
- Enter/Tab → commits, Escape → cancels
- Add `OnRowEdit` EventCallback<TItem> for save handling

## What we're NOT doing yet
- Column resizing (needs JS interop — Phase 3)
- Frozen columns (needs sticky positioning + JS scroll sync — Phase 3)
- Column reordering drag (needs JS drag API — Phase 3)
- Clipboard (needs JS navigator.clipboard — Phase 3)
- State persistence (needs JS localStorage — Phase 3)

## Files to create/modify

| File | Action |
|------|--------|
| `src/Arcadia.DataGrid/Core/DataGridLoadArgs.cs` | NEW — server-side callback args |
| `src/Arcadia.DataGrid/Components/ArcadiaDataGrid.razor.cs` | ADD — LoadData, TotalCount, DetailTemplate, expanded rows, edit state |
| `src/Arcadia.DataGrid/Components/ArcadiaDataGrid.razor` | ADD — footer, detail rows, edit mode, column picker |
| `src/Arcadia.DataGrid/Components/ArcadiaColumn.razor` | ADD — Aggregate, Editable, EditTemplate, FooterTemplate |
| `src/Arcadia.DataGrid/wwwroot/css/arcadia-datagrid.css` | ADD — detail row, edit mode, column picker styles |
| `samples/.../TestDataGrid.razor` | UPDATE — show all new features |

## Success criteria
- Server-side demo loads with visible loading state and works with sort/filter/page
- Footer shows aggregate values
- Detail row expands/collapses with content
- Double-click cell enters edit mode
- All existing Phase 1+2 features still work (no regressions)

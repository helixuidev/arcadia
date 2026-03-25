# DataGrid Bugfix PRP — Audit Response

## Source: Comprehensive audit found 48 issues across 7 categories

## Phase 1 — Ship-Blocking Fixes (do NOW)

### 1. IAsyncDisposable (Critical)
- Add `IAsyncDisposable` to `ArcadiaDataGrid<TItem>`
- Dispose `_jsModule` on cleanup
- Wrap in try/catch for JSDisconnectedException

### 2. Wire Server-Side LoadData (High)
- Call `InvokeLoadData()` in `HandleHeaderClick()` after sort change
- Call `InvokeLoadData()` in `SetFilter()` after filter change
- Call `InvokeLoadData()` in `GoToPage()` after page change
- In server mode, `GetPagedData()` should return `Data` directly (server already filtered/sorted)

### 3. Fix Column Visibility Toggle (High)
- Add `StateHasChanged()` after `col.ToggleVisible()`

### 4. Fix Aggregates to Use Filtered Data (High)
- Change `ComputeAggregate` to use `GetFilteredData()` instead of `Data`

### 5. Fix Edit Race Condition (Critical)
- Add `_editCommitting` guard flag
- Check flag in `CommitEdit()` — if already committing, return early
- Clear edit state on page change

### 6. Cache Filtered Data (High)
- Store result of `GetFilteredData()` in a field `_cachedFilteredData`
- Invalidate on data change, filter change, or sort change
- `TotalCount`, `GetPagedData()`, `ComputeAggregate()` all read from cache

### 7. Fix Grouping Null Reference (Critical)
- Add null check for `col.Field` inside the GroupBy LINQ chain
- Return empty list if Field is null

## Phase 2 — High Impact (do NEXT)

### 8. Don't Clear Filters on Toggle
- Remove `_filters.Clear()` from `ToggleFilters()`
- Filters persist when filter row is hidden

### 9. Clear Edit State on Page Change
- In `GoToPage()`, call `CancelEdit()`

### 10. Remove Unused GridState Class
- Delete `Core/GridState.cs`

### 11. Add Aria Labels to Checkboxes
- Header: `aria-label="Select all rows"`
- Row: `aria-label="Select row {rowNumber}"`

### 12. Fix Sticky Header in Scrollable Container
- Add `overflow: auto` to a wrapper div, not the grid root
- thead sticky needs the scroll container to be the parent

## Not Fixing Now (deferred)
- Keyboard navigation — separate feature, needs full PRP
- Virtual scrolling — separate feature, needs architecture change
- Column reorder DnD — needs JS interop work
- Server multi-sort — breaking change to DataGridLoadArgs
- Global search — new feature
- Column freeze — CSS position:sticky per column

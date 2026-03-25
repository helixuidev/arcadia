# DataGrid Virtual Scrolling — PRP

## Problem

Grid renders ALL rows on the current page. With PageSize=1000 or PageSize=0 (show all), DOM becomes huge and rendering is slow. Need virtualization — only render visible rows.

## Approach

Use Blazor's built-in `Virtualize<T>` component (net6.0+). For net5.0, fall back to full render (acceptable since net5 is legacy).

## Architecture

### Current Flow
```
Data → Filter → Sort → Skip/Take → Render ALL rows in <tbody>
```

### Virtualized Flow
```
Data → Filter → Sort → Virtualize<T> (renders only visible rows) → <tbody>
```

### Key Decision: Replace `<tbody>` loop with `<Virtualize>`

```razor
@if (UseVirtualization)
{
    <Virtualize Items="@GetSortedFilteredData()" Context="item" OverscanCount="5">
        <tr role="row">...</tr>
    </Virtualize>
}
else
{
    @foreach (var item in GetPagedData()) { <tr>...</tr> }
}
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Virtualize` | `bool` | `false` | Enable virtual scrolling |
| `ItemSize` | `float` | `40` | Estimated row height in pixels |
| `OverscanCount` | `int` | `5` | Extra rows rendered above/below viewport |

### Requirements

- `Height` must be set (virtual scroll needs a fixed container)
- Pagination is disabled in virtual mode (all rows are scrollable)
- `overflow-y: auto` on the scroll container
- `Virtualize<T>` handles the item provider

### Server-Side Virtual Scrolling

For server mode, use `Virtualize<T>.ItemsProvider` instead of `Items`:

```csharp
private async ValueTask<ItemsProviderResult<TItem>> ProvideItems(ItemsProviderRequest request)
{
    var args = new DataGridLoadArgs
    {
        Skip = request.StartIndex,
        Take = request.Count,
        SortProperty = _currentSort?.Property,
        SortDirection = _currentSort?.Direction ?? SortDirection.None,
        Filters = _filters.Values.Where(f => !string.IsNullOrEmpty(f.Value)).ToList()
    };
    await LoadData.InvokeAsync(args);
    return new ItemsProviderResult<TItem>(Data ?? new List<TItem>(), ServerTotalCount ?? 0);
}
```

## Performance Targets

| Scenario | Target |
|----------|--------|
| 100 rows, no virtualization | < 10ms render |
| 10,000 rows, virtualized | < 50ms initial, 60fps scroll |
| 100,000 rows, virtualized + server | < 100ms initial |

## Conditional Compilation

```csharp
#if NET6_0_OR_GREATER
    // Use Virtualize<T>
#else
    // Fall back to full render
#endif
```

## Files to Modify

| File | Changes |
|------|---------|
| `ArcadiaDataGrid.razor` | Conditional Virtualize vs foreach |
| `ArcadiaDataGrid.razor.cs` | Virtualize/ItemSize/OverscanCount params, ItemsProvider |
| `arcadia-datagrid.css` | Scroll container height management |

## Testing

- E2E: Render 10K rows, verify scroll performance
- E2E: Verify only ~20 DOM rows exist at any time
- Unit: ItemsProvider returns correct slice

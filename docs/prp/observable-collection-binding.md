# PRP: Live Data Binding via ObservableCollection

**Author:** Arcadia Controls Team
**Date:** 2026-04-01
**Status:** Draft
**Priority:** P0 — Sprint Quick Win (Score: 31/60)
**Effort:** 2 days

---

## 1. Problem Statement

Arcadia components (DataGrid, Charts, DashboardKit) accept `IReadOnlyList<T>` as their `Data` parameter. When the underlying collection changes (items added, removed, replaced), the component does NOT re-render automatically. The developer must manually call `StateHasChanged()` or reassign the entire `Data` parameter.

This is a critical gap for:
- **Real-time dashboards** (SignalR pushing stock tickers, IoT sensor readings, log streams)
- **CRUD applications** (adding a row to a DataGrid should show it immediately)
- **Collaborative apps** (multiple users editing the same dataset)

### Current behavior (broken)
```csharp
// This does NOT update the DataGrid or Chart
_items.Add(new Sale { Month = "Dec", Revenue = 50000 });

// Developer must do this workaround:
_items = new List<Sale>(_items); // force reference change
StateHasChanged();
```

### Desired behavior
```csharp
// ObservableCollection<T> auto-notifies the component
_items.Add(new Sale { Month = "Dec", Revenue = 50000 });
// DataGrid and Chart automatically re-render
```

### Competitive position
- **Syncfusion:** Supports `ObservableCollection` on DataGrid and Charts — auto-refresh on add/remove/replace
- **DevExpress:** Supports `ObservableCollection` on DataGrid — auto-refresh
- **MudBlazor:** Does NOT support it (manual StateHasChanged required)
- **Radzen:** Partial support on some components

---

## 2. Goals

1. All components that accept `Data` parameter should detect `INotifyCollectionChanged` and auto-rerender
2. No breaking changes — `IReadOnlyList<T>` still works as before
3. Performance: batch multiple rapid changes into a single re-render (debounce)
4. Thread safety: SignalR callbacks happen on different threads in Blazor Server
5. Proper cleanup: unsubscribe from collection events on disposal

## 3. Non-Goals

- We are NOT building a reactive state management system (no Redux/Flux)
- We are NOT adding `INotifyPropertyChanged` support (individual property changes within items)
- We are NOT changing the `Data` parameter type — it remains `IReadOnlyList<T>`

---

## 4. Technical Design

### 4a. Core pattern

`ObservableCollection<T>` implements both `IReadOnlyList<T>` (via `Collection<T>`) and `INotifyCollectionChanged`. Our components already accept `IReadOnlyList<T>`, so `ObservableCollection<T>` already compiles as a Data source — we just need to subscribe to the change event.

### 4b. Implementation approach

In each component's lifecycle:

```csharp
// In OnParametersSet or OnInitialized
private INotifyCollectionChanged? _observableData;

protected override void OnParametersSet()
{
    // Unsubscribe from old collection
    if (_observableData is not null)
    {
        _observableData.CollectionChanged -= OnCollectionChanged;
        _observableData = null;
    }

    // Subscribe to new collection if it supports change notification
    if (Data is INotifyCollectionChanged observable)
    {
        _observableData = observable;
        _observableData.CollectionChanged += OnCollectionChanged;
    }
}

private async void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
{
    // InvokeAsync ensures we're on the correct synchronization context
    // (critical for Blazor Server where SignalR callbacks come from different threads)
    await InvokeAsync(StateHasChanged);
}

// In DisposeAsync
if (_observableData is not null)
{
    _observableData.CollectionChanged -= OnCollectionChanged;
}
```

### 4c. Debouncing for bulk updates

When SignalR pushes 100 items in rapid succession, we don't want 100 re-renders. Add a debounce:

```csharp
private CancellationTokenSource? _debounceCts;

private async void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
{
    _debounceCts?.Cancel();
    _debounceCts = new CancellationTokenSource();
    try
    {
        await Task.Delay(16, _debounceCts.Token); // ~1 frame at 60fps
        await InvokeAsync(StateHasChanged);
    }
    catch (TaskCanceledException) { } // debounced
}
```

### 4d. Components to update

| Component | File | Data parameter | Notes |
|-----------|------|---------------|-------|
| `ArcadiaDataGrid<T>` | `src/Arcadia.DataGrid/Components/ArcadiaDataGrid.razor.cs` | `Data` | Must also recalculate conditional format ranges, pagination |
| `ChartBase<T>` | `src/Arcadia.Charts/Core/ChartBase.cs` | `Data` | Must recalculate scales, layout, animations |
| `ArcadiaDragGrid` | `src/Arcadia.DashboardKit/Components/ArcadiaDragGrid.razor.cs` | N/A — uses ChildContent, not Data | Skip |
| All chart types | Inherit from ChartBase | Inherited | Covered by ChartBase change |

### 4e. Edge cases

1. **Collection replaced entirely** — `OnParametersSet` handles unsubscribe/resubscribe
2. **Component disposed while change pending** — `DisposeAsync` cancels the debounce CTS
3. **Blazor Server thread safety** — `InvokeAsync` marshals to the renderer's sync context
4. **WASM (single-threaded)** — `InvokeAsync` is a no-op wrapper, no perf overhead
5. **Batch edit mode on DataGrid** — should NOT auto-refresh during active batch (pending changes would be lost)
6. **Server-side DataGrid (LoadData mode)** — ObservableCollection not applicable, skip subscription

---

## 5. Affected Files

### Must change:
- `src/Arcadia.DataGrid/Components/ArcadiaDataGrid.razor.cs` — subscribe/unsubscribe in OnParametersSet, debounced handler, disposal cleanup
- `src/Arcadia.Charts/Core/ChartBase.cs` — same pattern, plus recalculate layout on change

### Must add:
- `src/Arcadia.Core/Utilities/CollectionObserver.cs` — shared helper to avoid duplicating the subscribe/debounce/dispose pattern in every component

### Tests to add:
- `tests/Arcadia.Tests.Unit/DataGrid/DataGridObservableTests.cs`
- `tests/Arcadia.Tests.Unit/Charts/ChartObservableTests.cs`
- `tests/Arcadia.Tests.Unit/Core/CollectionObserverTests.cs`

### Docs to update:
- `website/src/pages/docs/datagrid/index.mdx` — add "Live Data" section
- `website/src/pages/docs/charts/index.mdx` — add "Real-time Data" section

### Demo to add:
- Server demo: streaming DataGrid page with simulated SignalR data
- Server demo: live-updating chart with timer

---

## 6. Acceptance Criteria

- [ ] `ObservableCollection<T>` as `Data` parameter auto-refreshes DataGrid on Add/Remove/Replace/Reset
- [ ] `ObservableCollection<T>` as `Data` parameter auto-refreshes all chart types
- [ ] Regular `List<T>` still works exactly as before (no regression)
- [ ] 100 rapid adds in 50ms result in ≤ 3 re-renders (debounce works)
- [ ] No memory leaks: unsubscribe on Data change and component disposal
- [ ] Thread-safe: SignalR callback on background thread doesn't crash
- [ ] Batch edit mode suppresses auto-refresh (pending changes preserved)
- [ ] Unit tests cover: add, remove, replace, reset, clear, dispose-during-pending, thread safety

---

## 7. Rollout Plan

1. Build `CollectionObserver` utility in Core
2. Integrate into ChartBase (affects all 20 chart types at once)
3. Integrate into DataGrid (with batch-edit guard)
4. Add unit tests
5. Add demo pages
6. Update docs
7. Ship in beta.20

---

## 8. Risks

| Risk | Mitigation |
|------|-----------|
| Memory leaks if event handler not unsubscribed | Shared `CollectionObserver` handles lifecycle; unit test verifies disposal |
| Perf degradation from excessive re-renders | 16ms debounce (1 frame); configurable via future parameter |
| Breaking change if Data type signature changes | NOT changing type — `IReadOnlyList<T>` stays, we just detect the interface at runtime |
| Blazor Server thread marshaling bugs | `InvokeAsync` is the standard pattern; well-tested by framework |

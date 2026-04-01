# Epics: Live Data Binding via ObservableCollection

Parent PRP: [observable-collection-binding.md](./observable-collection-binding.md)

---

## Epic 1: Core — CollectionObserver Utility

**Goal:** Shared reusable helper that any component can use to subscribe to `INotifyCollectionChanged`, debounce rapid changes, and auto-dispose.

### Tasks

| # | Task | File | Est |
|---|------|------|-----|
| 1.1 | Create `CollectionObserver<T>` class | `src/Arcadia.Core/Utilities/CollectionObserver.cs` | 1h |
| 1.2 | Implement `Attach(IReadOnlyList<T>)` — detects `INotifyCollectionChanged`, subscribes | Same | 30m |
| 1.3 | Implement debounced `OnChanged` callback with configurable delay (default 16ms) | Same | 30m |
| 1.4 | Implement `Detach()` — unsubscribes, cancels pending debounce | Same | 15m |
| 1.5 | Implement `IDisposable` — calls Detach | Same | 15m |
| 1.6 | Thread safety: wrap callback in caller-provided `Func<Func<Task>, Task>` (maps to `InvokeAsync`) | Same | 30m |
| 1.7 | Unit tests: attach, detach, add triggers callback, debounce batches, dispose cancels pending, reattach to new collection | `tests/Arcadia.Tests.Unit/Core/CollectionObserverTests.cs` | 1h |

**Acceptance:** `CollectionObserverTests` all pass. No component integration yet.

**Dependencies:** None. This is the foundation.

---

## Epic 2: Charts — Live Data on All Chart Types

**Goal:** All 20 chart types auto-rerender when an `ObservableCollection<T>` changes.

### Tasks

| # | Task | File | Est |
|---|------|------|-----|
| 2.1 | Add `CollectionObserver<T>` field to `ChartBase<T>` | `src/Arcadia.Charts/Core/ChartBase.cs` | 15m |
| 2.2 | In `OnParametersSet`: call `_observer.Attach(Data)` | Same | 15m |
| 2.3 | Wire observer callback to `InvokeAsync(StateHasChanged)` | Same | 15m |
| 2.4 | In `DisposeAsync`: dispose observer | Same | 10m |
| 2.5 | Verify: existing `AppendAndSlide` on LineChart still works alongside observer | Manual test | 15m |
| 2.6 | Unit tests: Line chart with ObservableCollection, add item → SVG updates; Bar chart same; remove item → re-renders; replace collection → resubscribes | `tests/Arcadia.Tests.Unit/Charts/ChartObservableTests.cs` | 1h |

**Acceptance:** Add an item to an `ObservableCollection<T>` bound to any chart → chart re-renders within 16ms. No manual `StateHasChanged` needed.

**Dependencies:** Epic 1

---

## Epic 3: DataGrid — Live Data with Guard Rails

**Goal:** DataGrid auto-refreshes on collection changes, with proper handling for batch edit, pagination, grouping, and conditional formatting.

### Tasks

| # | Task | File | Est |
|---|------|------|-----|
| 3.1 | Add `CollectionObserver<T>` field to `ArcadiaDataGrid<T>` | `src/Arcadia.DataGrid/Components/ArcadiaDataGrid.razor.cs` | 15m |
| 3.2 | In `OnParametersSet`: call `_observer.Attach(Data)`, skip if `LoadData` is set (server-side mode) | Same | 20m |
| 3.3 | Wire observer callback: `InvokeAsync` → recalculate pagination, conditional format ranges, then `StateHasChanged` | Same | 30m |
| 3.4 | Add batch edit guard: if `_pendingBatchChanges.Count > 0`, suppress auto-refresh and queue for after commit/discard | Same | 30m |
| 3.5 | In `DisposeAsync`: dispose observer | Same | 10m |
| 3.6 | Handle `NotifyCollectionChangedAction.Reset` (Clear): reset page to 1, clear selection | Same | 20m |
| 3.7 | Handle `NotifyCollectionChangedAction.Remove`: if selected item removed, deselect it | Same | 20m |
| 3.8 | Unit tests: add row → appears in grid; remove row → disappears; clear → resets page; batch edit suppresses; server-mode skips; dispose unsubscribes | `tests/Arcadia.Tests.Unit/DataGrid/DataGridObservableTests.cs` | 1.5h |

**Acceptance:** `ObservableCollection<T>` bound to DataGrid, adding/removing items auto-updates the rendered rows. Batch edit in progress is NOT disrupted. Server-side mode is unaffected.

**Dependencies:** Epic 1

---

## Epic 4: Demo — Real-Time Dashboard Showcase

**Goal:** A compelling demo page that shows live data updating in both a chart and a DataGrid, simulating a real-time dashboard (stock ticker / IoT sensor).

### Tasks

| # | Task | File | Est |
|---|------|------|-----|
| 4.1 | Create Server demo page `/test/live-data` with a timer that adds data every 500ms | `samples/Arcadia.Demo.Server/Components/Pages/Tests/TestLiveData.razor` | 45m |
| 4.2 | Page contains: Line chart (streaming stock price), DataGrid (live order feed), Gauge (CPU usage updating) | Same | 30m |
| 4.3 | Add "Live Data" sidebar nav entry in both Server and WASM demos | `ChartsDemo.razor` (both) | 15m |
| 4.4 | WASM demo: same page at `/live-data` (use `System.Threading.Timer` instead of SignalR) | `samples/Arcadia.Demo.Wasm/Pages/LiveDataDemo.razor` | 30m |
| 4.5 | Verify: Playwright screenshot shows data updating over 3 seconds | Manual/script | 15m |

**Acceptance:** Demo page shows chart, grid, and gauge all updating in real time from a shared `ObservableCollection<T>`. No manual `StateHasChanged` calls in the demo code.

**Dependencies:** Epics 2 + 3

---

## Epic 5: Documentation

**Goal:** Update docs so developers know how to use live data binding.

### Tasks

| # | Task | File | Est |
|---|------|------|-----|
| 5.1 | Add "Live Data" section to DataGrid overview doc | `website/src/pages/docs/datagrid/index.mdx` | 20m |
| 5.2 | Add "Real-Time Data" section to Charts overview doc | `website/src/pages/docs/charts/index.mdx` | 20m |
| 5.3 | Code samples showing ObservableCollection + Timer pattern | Same files | 15m |
| 5.4 | Code sample showing SignalR → ObservableCollection pattern | Same files | 15m |
| 5.5 | Update README.md feature list to mention live data binding | Root `README.md` | 5m |
| 5.6 | Update NuGet package descriptions | `src/Arcadia.DataGrid/Arcadia.DataGrid.csproj`, `src/Arcadia.Charts/Arcadia.Charts.csproj` | 10m |

**Acceptance:** A developer reading the docs can implement a live-updating dashboard without searching Stack Overflow.

**Dependencies:** Epics 2 + 3

---

## Implementation Order

```
Epic 1 (Core utility)
  ├── Epic 2 (Charts integration)  ← can parallel
  └── Epic 3 (DataGrid integration) ← can parallel
        └── Epic 4 (Demo pages)
            └── Epic 5 (Documentation)
```

**Estimated total:** 1.5 days
- Day 1: Epics 1 + 2 + 3 (core + both integrations)
- Day 2 AM: Epic 4 (demos)
- Day 2 PM: Epic 5 (docs)

---

## Definition of Done

- [ ] All unit tests pass (CollectionObserver, Chart, DataGrid)
- [ ] Demo page shows live updates without `StateHasChanged`
- [ ] Existing tests (1404) still pass (no regressions)
- [ ] Docs updated
- [ ] WASM builds and runs
- [ ] Playground E2E smoke test passes

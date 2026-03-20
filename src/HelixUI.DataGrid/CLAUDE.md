# CLAUDE.md — HelixUI DataGrid (JS Interop Specialist Context)

## Role: JavaScript Interop Specialist

You are the JS↔Blazor bridge expert. You own the DataGrid (AG Grid wrapper) and any component requiring JavaScript interop.

## Responsibilities
- Build and maintain the AG Grid Blazor wrapper
- Design clean C# APIs that abstract away JS complexity
- Handle memory management and disposal in interop scenarios
- Bundle and manage JavaScript/TypeScript modules
- Optimize interop performance (batch calls, minimize marshaling)

## DataGrid Architecture
```
HelixUI.DataGrid/
├── wwwroot/
│   ├── js/
│   │   ├── helix-grid.ts      # TypeScript wrapper around AG Grid API
│   │   ├── helix-grid.js      # Compiled JS (Vite build)
│   │   └── helix-grid.d.ts    # Type definitions
│   └── css/
│       └── helix-grid.css     # Grid-specific styles using HelixUI tokens
├── Components/
│   ├── HelixGrid.razor         # Main grid component
│   ├── HelixGrid.razor.cs      # Code-behind
│   ├── GridColumn.razor        # Column definition component
│   └── GridToolbar.razor       # Optional toolbar
├── Models/
│   ├── GridOptions.cs          # C# grid configuration
│   ├── ColumnDefinition.cs     # Column config model
│   ├── SortModel.cs            # Sort state
│   ├── FilterModel.cs          # Filter state
│   └── GridEvent.cs            # Event args
├── Services/
│   ├── GridInteropService.cs   # JS interop bridge
│   └── IGridDataSource.cs      # Server-side data source interface
└── HelixUI.DataGrid.csproj
```

## JS Interop Rules
1. **Isolate JS modules** — use `IJSObjectReference` from `./_content/HelixUI.DataGrid/js/helix-grid.js`
2. **Never use IJSRuntime directly** in components — go through `GridInteropService`
3. **Batch operations** — collect multiple grid API calls and send as single interop call
4. **Dispose properly** — implement `IAsyncDisposable`, release JS object references
5. **Serialize minimally** — only send changed data to JS, not full state
6. **Error boundaries** — catch JSDisconnectedException in Server mode gracefully

## AG Grid Integration Strategy
- Use AG Grid Community Edition as the base (MIT license)
- Wrap AG Grid's Column API, Row API, and Event API
- Map AG Grid events to Blazor EventCallbacks
- Support AG Grid themes but prefer HelixUI token-based theming
- Server-side row model for large datasets (lazy loading via IGridDataSource)

## Two-Way Binding Pattern
```csharp
[Parameter] public List<TItem>? Items { get; set; }
[Parameter] public EventCallback<List<TItem>> ItemsChanged { get; set; }

[Parameter] public SortModel? Sort { get; set; }
[Parameter] public EventCallback<SortModel> SortChanged { get; set; }

[Parameter] public FilterModel? Filter { get; set; }
[Parameter] public EventCallback<FilterModel> FilterChanged { get; set; }

[Parameter] public TItem? SelectedItem { get; set; }
[Parameter] public EventCallback<TItem> SelectedItemChanged { get; set; }
```

## Performance Targets
- Initial render: < 100ms for 1000 rows
- Sort/filter: < 50ms for 10,000 rows (client-side)
- Scroll (virtualized): 60fps
- JS interop calls per user action: ≤ 3
- Memory: no growth on repeated sort/filter cycles

## TypeScript Build
- Bundler: Vite
- Output: ES module, single file
- Source maps: included in debug, stripped in release
- AG Grid imported as external (peer dependency)

<p align="center">
  <strong>Arcadia.DataGrid</strong><br>
  <em>A high-performance Blazor DataGrid with sorting, filtering, grouping, virtual scrolling, and 6 built-in themes</em>
</p>

## Why Arcadia DataGrid?

- **Pure Blazor** — no JavaScript dependency, no npm, no webpack. Renders natively in C#.
- **Virtual scrolling** — handles 100,000+ rows at 60fps using Blazor's `Virtualize<T>` with CSS Grid layout.
- **Declarative columns** — define columns as child components with templates, not config objects.
- **Accessible** — WCAG 2.1 AA: ARIA grid roles, keyboard navigation, screen reader announcements.

## Features

| Feature | Details |
|---------|---------|
| Sorting | Multi-column sort with Shift+Click and priority badges |
| Filtering | 9 operators per column + quick filter toolbar search |
| Paging | Built-in pager with configurable page sizes |
| Selection | Single, multi-select, checkbox column, `SelectionMode` enum |
| Grouping | Group by any column with collapsible headers |
| Inline editing | Double-click to edit, custom edit templates |
| Batch editing | Track changes, save/discard all with `OnBatchCommit` |
| Virtual scrolling | CSS Grid layout for 100K+ rows |
| Column templates | Cell, header, footer, and detail row templates |
| CSV export | One-click export with column/row filtering |
| Clipboard copy | Ctrl+C copies selected rows as TSV; Ctrl+Shift+C copies with headers |
| Column reorder | Drag-and-drop column reordering |
| Stacked headers | Multi-row header groups for complex column layouts |
| Infinite scroll | Continuous data loading as user scrolls |
| Cell tooltips | Hover tooltips on truncated cell content |
| Conditional formatting | Data-driven cell styles via `CellClass` |
| ObservableCollection | Live data binding with automatic 16ms debounced re-renders |
| Context menu | Custom right-click menus via `ContextMenuTemplate` |
| State persistence | Auto-save sort/filter/columns to localStorage |
| Keyboard nav | Arrow keys, Enter to edit, Escape to cancel, Ctrl+C to copy |
| 6 Themes | Obsidian, Vapor, Carbon, Aurora, Slate, Midnight |
| 3 Density modes | Comfortable, Compact, Dense |

## 60-Second Quick Start

```bash
dotnet add package Arcadia.DataGrid
dotnet add package Arcadia.Theme
```

Add to your `App.razor` `<head>`:
```html
<link href="_content/Arcadia.Theme/css/arcadia.css" rel="stylesheet" />
<link href="_content/Arcadia.DataGrid/css/arcadia-datagrid.css" rel="stylesheet" />
```

Drop in a grid:
```razor
@using Arcadia.DataGrid.Components

<ArcadiaDataGrid TItem="Employee" Data="@employees" Sortable="true" Filterable="true" ShowToolbar="true">
    <ArcadiaColumn TItem="Employee" Property="Name" />
    <ArcadiaColumn TItem="Employee" Property="Department" />
    <ArcadiaColumn TItem="Employee" Property="Salary" Format="C0" Align="right" />
</ArcadiaDataGrid>
```

## Render Mode Support

| Mode | Status |
|------|--------|
| Blazor Server | Fully supported |
| Blazor WebAssembly | Fully supported |
| Blazor Auto (net8+) | Fully supported |

Multi-targets .NET 5 through .NET 10.

**[Live Demo](https://arcadiaui.com/playground/)** · **[Documentation](https://arcadiaui.com/docs/datagrid)** · **[Changelog](https://github.com/ArcadiaUIDev/arcadia/blob/main/CHANGELOG.md)** · **[Benchmarks](https://arcadiaui.com/docs/benchmarks)** · **[GitHub](https://github.com/ArcadiaUIDev/arcadia)**

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
| Sorting | Single and multi-column, configurable per column |
| Filtering | Text, number, date, and boolean filter operators per column |
| Paging | Built-in pager with configurable page sizes |
| Selection | Single, multi-select, and checkbox column |
| Grouping | Group by any column with collapsible headers |
| Inline editing | Double-click to edit, custom edit templates |
| Virtual scrolling | CSS Grid layout for 100K+ rows |
| Column templates | Cell, header, footer, and detail row templates |
| CSV export | One-click export with column/row filtering |
| Keyboard nav | Arrow keys, Enter to edit, Escape to cancel |
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

<ArcadiaDataGrid TItem="Employee" Data="@employees" Sortable="true" Filterable="true">
    <ArcadiaColumn TItem="Employee" Field="@(e => e.Name)" Title="Name" />
    <ArcadiaColumn TItem="Employee" Field="@(e => e.Department)" Title="Department" />
    <ArcadiaColumn TItem="Employee" Field="@(e => e.Salary)" Title="Salary" Format="C0" Align="right" />
</ArcadiaDataGrid>
```

## Render Mode Support

| Mode | Status |
|------|--------|
| Blazor Server | Fully supported |
| Blazor WebAssembly | Fully supported |
| Blazor Auto (net8+) | Fully supported |

Multi-targets .NET 5 through .NET 9.

**[Live Demo](https://arcadiaui.com/playground/)** · **[Documentation](https://arcadiaui.com/docs/datagrid)** · **[GitHub](https://github.com/ArcadiaUIDev/arcadia)**

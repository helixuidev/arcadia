# Arcadia DataGrid — Product Requirements Plan

## 1. Vision

**The missing piece that turns Arcadia from "chart library" into "component suite."**

Every enterprise Blazor app needs a data grid. It's the #1 most requested component type. Teams currently standardize on Syncfusion ($995/dev/year) or Telerik ($979/dev/year) primarily for their grid — the charts are a bonus. By shipping `ArcadiaDataGrid`, we give teams a reason to standardize on Arcadia at $299/dev/year instead.

**Target users:** The same enterprise .NET developers already using Arcadia Charts — building admin panels, internal tools, reporting dashboards, and CRUD apps.

**Why this, why now:** The review critique said "Arcadia has 14 chart types with solid basics but shallow depth." A DataGrid changes the conversation from "chart library comparison" to "component suite comparison" — and at $299 vs $995, we win on value.

---

## 2. Competitive Landscape

### Open-Source Foundations (all MIT — legal to study and adapt)

| Library | Grid LOC | Features | Extractable? | Our Use |
|---------|----------|----------|-------------|---------|
| **QuickGrid** (Microsoft) | ~1,500 | Sort, page, virtualize only | Yes (standalone pkg) | **Architectural scaffold** — column model, virtualization pattern, generic `<TItem>` |
| **Radzen.Blazor** | ~4,100 | Full (filter, group, edit, export) | Moderate (100+ component lib) | **API reference** — study their parameter names, filter operators, grouping model |
| **MudBlazor** | ~2,700 | Full (filter, group, edit, no export) | Hard (monolithic + Material) | **Pattern reference** — study their EditTemplate approach |

### Commercial Competitors

| Product | Price | Grid LOC | Notable Strengths |
|---------|-------|----------|------------------|
| Syncfusion | $995/dev/year | N/A | 100+ column types, PDF/Excel export, adaptive layout, master-detail, lazy loading, clipboard |
| Telerik | $979/dev/year | N/A | Incell/inline/popup editing, column menu, column virtualization, state persistence |
| DevExpress | $1,078/dev/year | N/A | Unmatched filtering UX, summary footers, band columns, master-detail |

### Our Positioning

- **Price:** $299/dev/year for Charts + Grid + FormBuilder + Notifications
- **Bundle:** ~350KB total (vs 3-5MB for Syncfusion)
- **DX:** Clean API — `SeriesConfig<T>` lambda pattern extends naturally to grid column definitions
- **Render mode:** Server, WASM, Auto (net5-net10). QuickGrid only works on net8+.

---

## 3. Phased Delivery

### Phase 1 — "Ship a usable grid" (MVP)

The grid people actually evaluate in the first 10 minutes. Must be solid.

| Feature | Priority | Notes |
|---------|----------|-------|
| `ArcadiaDataGrid<TItem>` component | P0 | Generic, inherits `ArcadiaComponentBase` |
| `ArcadiaColumn<TItem>` child component | P0 | Declarative column definitions via `RenderFragment` |
| **Sorting** (click header, asc/desc/none) | P0 | Multi-column sort with Shift+Click |
| **Pagination** (page size selector, nav) | P0 | Configurable page sizes: 10, 25, 50, 100 |
| **Virtual scrolling** | P0 | Blazor `Virtualize<T>` for 10K+ rows |
| **Column templates** | P0 | `<Template>` and `<HeaderTemplate>` on columns |
| **Loading state** | P0 | Skeleton shimmer, consistent with chart loading |
| **Empty state** | P0 | Configurable "no data" message |
| **Striped/hover rows** | P0 | CSS via `--arcadia-*` custom properties |
| **Responsive** | P0 | Horizontal scroll on narrow viewports |
| **Accessibility** | P0 | `role="grid"`, `aria-sort`, keyboard nav (arrow keys, Enter) |
| **Dark/Light theme** | P0 | CSS custom properties, matches Arcadia Theme |
| **Pro watermark** | P0 | `<CommunityWatermark />` — this is a Pro component |

### Phase 2 — "Looks like a real grid"

Features that enterprise evaluators check for in the first hour.

| Feature | Priority | Notes |
|---------|----------|-------|
| **Filtering** (per-column) | P1 | Filter row below header, operators: contains, equals, >, <, between, starts with |
| **Column resizing** (drag) | P1 | CSS resize + JS interop for drag handle |
| **Row selection** (single + multi) | P1 | Checkbox column, `SelectedItems` two-way binding |
| **Server-side data** | P1 | `LoadData` callback with `DataGridLoadArgs` (skip, take, sort, filter) for API-backed grids |
| **Column visibility toggle** | P1 | Dropdown menu to show/hide columns |
| **Frozen columns** (left) | P1 | `Frozen="true"` on column, sticky positioning |
| **Footer aggregates** | P1 | Sum, Avg, Count, Min, Max per column |
| **Custom cell styling** | P1 | `CellClass` / `CellStyle` callbacks per row |

### Phase 3 — "Enterprise-grade"

Features that close deals and compete with Syncfusion/Telerik.

| Feature | Priority | Notes |
|---------|----------|-------|
| **Cell editing** (inline) | P2 | `Editable="true"` on column, EditTemplate |
| **Grouping** (drag header to group bar) | P2 | Collapse/expand, group header template |
| **Master-detail** (expandable rows) | P2 | `DetailTemplate` renders child content on expand |
| **Excel/CSV export** | P2 | Server-side generation, respects current sort/filter |
| **Column reordering** (drag) | P2 | Drag column headers to reorder |
| **Row drag and drop** | P2 | Reorder rows, move between grids |
| **Clipboard** (copy selection) | P2 | Ctrl+C copies selected cells/rows |
| **State persistence** | P2 | Save/restore sort, filter, column order, page to localStorage |
| **Column pinning** (right) | P2 | Pin columns to right edge |

---

## 4. Architecture

```
src/Arcadia.DataGrid/
├── Arcadia.DataGrid.csproj
├── Components/
│   ├── ArcadiaDataGrid.razor          # Main grid component
│   ├── ArcadiaDataGrid.razor.cs       # Code-behind
│   ├── ArcadiaColumn.razor            # Column definition (child component)
│   ├── ArcadiaColumn.razor.cs         # Column code-behind
│   ├── GridPagination.razor           # Pagination bar
│   ├── GridToolbar.razor              # Optional toolbar (filter toggle, export, column picker)
│   └── GridSkeleton.razor             # Loading state
├── Core/
│   ├── GridState.cs                   # Sort, filter, page state container
│   ├── SortDescriptor.cs              # Column + direction
│   ├── FilterDescriptor.cs            # Column + operator + value
│   ├── DataGridLoadArgs.cs            # Args for server-side LoadData callback
│   └── ColumnDefinition.cs            # Internal column metadata
├── wwwroot/
│   ├── css/arcadia-datagrid.css       # Grid styles
│   └── js/datagrid-interop.js         # Column resize, clipboard (minimal)
└── _Imports.razor
```

### Component Hierarchy

```razor
<ArcadiaDataGrid TItem="Employee" Data="@employees"
                 PageSize="25" Sortable="true">
    <ArcadiaColumn TItem="Employee" Field="@(e => e.Name)"
                   Title="Name" Sortable="true" />
    <ArcadiaColumn TItem="Employee" Field="@(e => e.Department)"
                   Title="Department" />
    <ArcadiaColumn TItem="Employee" Field="@(e => e.Salary)"
                   Title="Salary" Format="C0" Align="right" />
    <ArcadiaColumn TItem="Employee" Title="Actions">
        <Template Context="emp">
            <button @onclick="() => Edit(emp)">Edit</button>
        </Template>
    </ArcadiaColumn>
</ArcadiaDataGrid>
```

### Key Design Decisions

**1. Column definitions via child components (not configuration objects)**

```razor
<!-- YES — declarative, discoverable, template-friendly -->
<ArcadiaColumn TItem="Employee" Field="@(e => e.Name)" Title="Name" />

<!-- NO — opaque, harder to add templates -->
Columns="@(new[] { new ColumnDef<Employee>(e => e.Name, "Name") })"
```

Child components match the Blazor idiom (like Telerik and Radzen). They allow `<Template>`, `<HeaderTemplate>`, and `<FooterTemplate>` as child content. Parameters are discoverable via IntelliSense.

**2. Data via `Data` parameter (client-side) or `LoadData` callback (server-side)**

```razor
<!-- Client-side: pass IReadOnlyList<T>, grid handles sort/filter/page -->
<ArcadiaDataGrid TItem="Employee" Data="@employees" />

<!-- Server-side: grid calls your API with sort/filter/page args -->
<ArcadiaDataGrid TItem="Employee" LoadData="@LoadEmployees" TotalCount="@_total" />

@code {
    async Task LoadEmployees(DataGridLoadArgs args)
    {
        var result = await Http.GetFromJsonAsync<PagedResult<Employee>>(
            $"/api/employees?skip={args.Skip}&take={args.Take}&sort={args.SortField}");
        _employees = result.Items;
        _total = result.TotalCount;
    }
}
```

This is the pattern used by Radzen, Telerik, and Syncfusion. It's the one enterprise developers expect.

**3. Sorting state managed internally, exposed via `SortChanged` event**

The grid manages its own sort/filter/page state. Users can observe changes via `EventCallback` or provide initial state via parameters. This avoids forcing users to manage a state object externally (which is a common complaint about MudBlazor's grid).

**4. CSS via custom properties (no Material/Bootstrap dependency)**

```css
.arcadia-grid {
  --arcadia-grid-header-bg: var(--arcadia-color-surface-raised, #1e1b2e);
  --arcadia-grid-row-hover: var(--arcadia-color-surface-sunken, rgba(139,92,246,0.06));
  --arcadia-grid-border: var(--arcadia-color-border, #2d2a3e);
  --arcadia-grid-stripe: var(--arcadia-color-surface-sunken, rgba(0,0,0,0.02));
}
```

Users can override any visual via CSS variables or the `Class`/`Style` parameters.

**5. Minimal JS interop (Blazor-first)**

JS used only for:
- Column resize drag (pointer events + style updates)
- Clipboard (navigator.clipboard API)
- Sticky column positioning (IntersectionObserver)

Everything else is pure C# rendering, consistent with the Charts approach.

---

## 5. Parameter Reference (Phase 1)

### ArcadiaDataGrid<TItem>

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Data` | `IReadOnlyList<TItem>?` | null | Client-side data source |
| `LoadData` | `EventCallback<DataGridLoadArgs>` | — | Server-side data callback |
| `TotalCount` | `int?` | null | Total rows for server-side paging |
| `PageSize` | `int` | 25 | Rows per page (0 = no paging) |
| `PageSizeOptions` | `int[]` | `[10,25,50,100]` | Page size dropdown options |
| `Sortable` | `bool` | true | Enable column sorting |
| `Striped` | `bool` | true | Alternate row shading |
| `Hoverable` | `bool` | true | Highlight row on hover |
| `Dense` | `bool` | false | Compact row height |
| `FixedHeader` | `bool` | true | Sticky header on scroll |
| `Height` | `string?` | null | Fixed height (enables scroll). Null = auto |
| `Loading` | `bool` | false | Show skeleton loading state |
| `EmptyMessage` | `string` | "No data available" | Empty state text |
| `Class` | `string?` | null | Additional CSS classes |
| `Style` | `string?` | null | Inline styles |
| `ChildContent` | `RenderFragment` | — | Column definitions |

### ArcadiaColumn<TItem>

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Field` | `Func<TItem, object>?` | null | Value accessor lambda |
| `Title` | `string` | "" | Column header text |
| `Sortable` | `bool?` | null | Override grid-level Sortable |
| `Width` | `string?` | null | CSS width (e.g., "200px", "30%") |
| `MinWidth` | `string?` | null | Minimum width |
| `Align` | `string` | "left" | Text alignment: left, center, right |
| `Format` | `string?` | null | .NET format string (e.g., "C2", "d") |
| `Template` | `RenderFragment<TItem>?` | null | Custom cell template |
| `HeaderTemplate` | `RenderFragment?` | null | Custom header template |
| `Visible` | `bool` | true | Column visibility |
| `CellClass` | `Func<TItem, string?>?` | null | Dynamic cell CSS class |

---

## 6. Accessibility Requirements

| Requirement | Implementation |
|-------------|---------------|
| Grid landmark | `role="grid"` on table element |
| Column headers | `role="columnheader"`, `aria-sort="ascending/descending/none"` |
| Row identification | `role="row"` on `<tr>` |
| Cell identification | `role="gridcell"` on `<td>` |
| Keyboard navigation | Arrow keys between cells, Enter to activate sort, Tab to pagination |
| Page announcement | `aria-live="polite"` region announces "Page 2 of 10, showing 25 items" |
| Sort announcement | Screen reader announces sort direction change |
| Loading state | `aria-busy="true"` during data load |
| Focus management | Focus visible outline on all interactive elements |

---

## 7. Testing Strategy

### Unit Tests (bUnit)
- Renders correct number of rows for given data
- Sorting: click header toggles asc/desc/none
- Pagination: correct page slice, page navigation
- Virtual scrolling: renders only visible rows
- Empty state: shows message when data is empty/null
- Loading state: shows skeleton
- Column templates: custom content renders
- Column visibility: hidden columns don't render
- Accessibility: role attributes present

### E2E Tests (Playwright)
- Full sort → filter → page workflow
- Keyboard navigation (arrow keys, Enter on headers)
- Column resize drag
- Visual regression baselines
- Performance: render 10K rows, measure FPS

### Performance Benchmarks
- 100 rows: < 10ms render
- 1,000 rows (paged): < 15ms render
- 10,000 rows (virtualized): < 50ms initial, smooth scroll at 60fps
- 100,000 rows (virtualized + server): < 100ms initial

---

## 8. Documentation Requirements

| Page | Content |
|------|---------|
| `/docs/datagrid` | Installation + first grid in 2 minutes |
| `/docs/datagrid/sorting` | Sort configuration, multi-sort, custom comparers |
| `/docs/datagrid/paging` | Page size, server-side paging, "load more" pattern |
| `/docs/datagrid/filtering` | Filter operators, custom filter templates, server-side filters |
| `/docs/datagrid/selection` | Single/multi select, checkbox column, SelectedItems binding |
| `/docs/datagrid/editing` | Inline edit, edit templates, validation integration |
| `/docs/datagrid/templates` | Cell, header, footer, detail, group templates |
| `/docs/datagrid/virtualization` | Setup, performance tips, server-side virtual scroll |
| `/docs/datagrid/export` | Excel, CSV, PDF export configuration |
| `/docs/datagrid/styling` | CSS variables, conditional formatting, themes |

---

## 9. NuGet Package

```xml
<PackageId>Arcadia.DataGrid</PackageId>
<Description>Blazor DataGrid with sorting, filtering, paging, virtual scrolling,
editing, and Excel export. Part of Arcadia Controls. arcadiaui.com</Description>
<PackageTags>blazor;datagrid;data-grid;table;sorting;filtering;paging;
virtual-scroll;blazor-components;dotnet</PackageTags>
```

Dependency: `Arcadia.Core` only. No third-party runtime dependencies.

---

## 10. Success Criteria

| Metric | Target |
|--------|--------|
| Phase 1 ship | Working sort + page + virtualize in under 1 week |
| Demo quality | Grid looks professional in the playground within 10 seconds of loading |
| Performance | 10K rows virtualized, smooth 60fps scroll |
| API approval | A senior .NET dev can render a sorted, paged grid in < 10 lines of code |
| Review response | Next external review says "the grid is solid" instead of "no grid" |

---

## 11. Open Questions

1. **IQueryable support?** QuickGrid supports `IQueryable<T>` for EF Core integration. Do we want this in Phase 1 or Phase 2?
2. **Column auto-generation?** Generate columns from `TItem` properties via reflection. Convenient for prototyping but harder to control. Phase 2?
3. **Inline editing validation?** Integrate with `EditContext` / `DataAnnotations`? Or custom validation model? Phase 3.
4. **Multi-select UX?** Checkbox column vs row click vs Ctrl+Click? Need to decide before Phase 2.

---

## 12. Legal Notes

- Architecture inspired by Microsoft QuickGrid (MIT License, Copyright .NET Foundation)
- API patterns studied from Radzen.Blazor (MIT License, Copyright Radzen Ltd)
- All implementation is original Arcadia code
- Include `THIRD-PARTY-NOTICES.md` in NuGet package acknowledging inspirations
- No code is copied — only patterns and naming conventions are referenced

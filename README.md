<p align="center">
  <strong>Arcadia Controls</strong><br>
  <em>20 chart types, 46 UI components, high-performance DataGrid, drag-and-drop dashboards, form builder & notifications for Blazor</em>
</p>

<p align="center">
  <a href="https://www.nuget.org/packages?q=Arcadia">
    <img src="https://img.shields.io/nuget/v/Arcadia.Charts?label=NuGet&color=8b5cf6" alt="NuGet" />
  </a>
  <a href="https://arcadiaui.com">
    <img src="https://img.shields.io/badge/docs-arcadiaui.com-blue" alt="Documentation" />
  </a>
  <a href="https://arcadiaui.com/playground/">
    <img src="https://img.shields.io/badge/demo-playground-green" alt="Live Demo" />
  </a>
</p>

---

## What is Arcadia Controls?

A commercial Blazor component library for enterprise .NET developers. Pure SVG chart rendering with zero JavaScript dependencies for core charting. Multi-targets **.NET 5 through .NET 10** and supports **Server, WebAssembly, and Auto** render modes.

**[Live Demo](https://arcadiaui.com/playground/)** · **[Documentation](https://arcadiaui.com/docs/)** · **[Why Arcadia?](https://arcadiaui.com/why-arcadia/)**

## Charts (20 Types)

| Chart | Component | Tier |
|-------|-----------|------|
| Line / Area | `ArcadiaLineChart<T>` | Community (Free) |
| Bar / Column | `ArcadiaBarChart<T>` | Community (Free) |
| Pie / Donut | `ArcadiaPieChart<T>` | Community (Free) |
| Scatter / Bubble | `ArcadiaScatterChart<T>` | Community (Free) |
| Candlestick (OHLC) | `ArcadiaCandlestickChart<T>` | Pro |
| Radar / Spider | `ArcadiaRadarChart<T>` | Pro |
| Gauge | `ArcadiaGaugeChart` | Pro (or free via `Arcadia.Gauge`) |
| Heatmap | `ArcadiaHeatmap<T>` | Pro |
| Funnel | `ArcadiaFunnelChart<T>` | Pro |
| Treemap | `ArcadiaTreemapChart<T>` | Pro |
| Waterfall | `ArcadiaWaterfallChart<T>` | Pro |
| Rose / Polar | `ArcadiaRoseChart<T>` | Pro |
| Range Area | `ArcadiaRangeAreaChart<T>` | Pro |
| Box Plot | `ArcadiaBoxPlot<T>` | Pro |
| Sankey | `ArcadiaSankeyChart` | Pro |
| Chord | `ArcadiaChordChart` | Pro |
| Area | `ArcadiaAreaChart<T>` | Community (Free) |
| Donut | `ArcadiaDonutChart<T>` | Community (Free) |
| Stacked Bar | `ArcadiaStackedBarChart<T>` | Community (Free) |
| Bubble | `ArcadiaBubbleChart<T>` | Community (Free) |

**Plus:** DataGrid with virtual scrolling, column reorder, stacked headers, infinite scroll, and ObservableCollection binding. 7 dashboard widgets (KPI Card, Sparkline, Progress Bar, Delta Indicator, Bar List, Tracker, Category Bar). Drag-and-drop dashboard grid (DashboardKit). 46 general-purpose UI components. Form Builder with 21 field types. Toast Notifications.

## Quick Start

**1. Install packages:**

```bash
dotnet add package Arcadia.Charts
dotnet add package Arcadia.DataGrid
dotnet add package Arcadia.Theme
```

**2. Add stylesheets** to your `App.razor` (or `_Host.cshtml` / `index.html`):

```html
<link href="_content/Arcadia.Theme/css/arcadia.css" rel="stylesheet" />
<link href="_content/Arcadia.Charts/css/arcadia-charts.css" rel="stylesheet" />
```

**3. Use a chart:**

```razor
@using Arcadia.Charts.Core
@using Arcadia.Charts.Components.Charts

<ArcadiaLineChart TItem="SalesRecord" Data="@data"
                  XField="@(d => (object)d.Month)"
                  Series="@series"
                  Height="350" Width="0"
                  ShowPoints="true" AnimateOnLoad="true" />

@code {
    record SalesRecord(string Month, double Revenue, double Target);

    List<SalesRecord> data = new()
    {
        new("Jan", 45000, 50000), new("Feb", 52000, 50000),
        new("Mar", 48000, 52000), new("Apr", 61000, 55000),
    };

    List<SeriesConfig<SalesRecord>> series = new()
    {
        new() { Name = "Revenue", Field = d => d.Revenue, Color = "success",
                ShowArea = true, AreaOpacity = 0.1, CurveType = "smooth" },
        new() { Name = "Target", Field = d => d.Target, Color = "warning",
                Dashed = true },
    };
}
```

## Key Features

- **Pure SVG rendering** — no JavaScript dependencies for chart rendering
- **20 chart types** — from line charts to Sankey flows, chord diagrams to heatmaps
- **Responsive** — set `Width="0"` and charts auto-fill their container
- **Dark/Light themes** — full theme support via CSS custom properties
- **Streaming data** — real-time updates with `AppendAndSlide` and sliding window
- **Export** — PNG/SVG download toolbar on every chart
- **Crosshair & Annotations** — interactive data exploration
- **Smooth curves** — Catmull-Rom interpolation for elegant line charts
- **Anti-collision layout engine** — labels never overlap
- **Accessibility** — WCAG 2.1 AA, screen reader tables, `prefers-reduced-motion`
- **DataGrid** — sorting, filtering, paging, grouping, inline editing, CSV/Excel/PDF export
- **Virtual scrolling** — DataGrid handles 100K+ rows at 60fps
- **Column reorder & stacked headers** — drag columns, multi-row header groups
- **Infinite scroll** — continuous data loading as user scrolls
- **Cell tooltips & copy with headers** — Ctrl+Shift+C copies with column headers
- **Conditional formatting & cell merge** — data-driven cell styles and spans
- **ObservableCollection binding** — live data on Charts and DataGrid with 16ms debounce
- **Selection modes** — single, multi, and checkbox selection in the DataGrid
- **Column templates** — cell, header, footer, and detail row templates
- **6 built-in grid themes** — Obsidian, Vapor, Carbon, Aurora, Slate, Midnight + 3 density modes
- **DashboardKit** — drag-and-drop dashboard grid with FLIP animations, spring physics, iOS wiggle mode
- **46 UI components** — Dialog, Tabs, Card, CommandPalette, HoverCard, Popover, and more
- **Form Builder** — 21 field types, schema-driven or model-driven, wizard mode
- **Toast Notifications** — fire-and-forget with auto-dismiss and stacking

## NuGet Packages

| Package | Description |
|---------|-------------|
| `Arcadia.Core` | Base classes, theming engine, accessibility utilities |
| `Arcadia.Theme` | Design tokens, CSS custom properties, Tailwind plugin |
| `Arcadia.Charts` | All 20 chart types + 7 dashboard widgets |
| `Arcadia.DataGrid` | High-performance data grid with sorting, filtering, virtual scrolling, inline editing, CSV/Excel/PDF export, column reorder, stacked headers, infinite scroll |
| `Arcadia.DashboardKit` | Drag-and-drop dashboard grid with FLIP animations, spring physics, iOS wiggle mode |
| `Arcadia.UI` | 46 general-purpose UI components (Dialog, Tabs, Card, CommandPalette, etc.) |
| `Arcadia.FormBuilder` | Dynamic forms, validation, wizards |
| `Arcadia.Notifications` | Toast notification system |
| `Arcadia.Gauge` | Free standalone gauge component (MIT) — zero dependencies, under 15 KB |
| `Arcadia.Analyzers` | Roslyn analyzers for Arcadia API usage |

## AI Integration

Arcadia Controls is the first Blazor component library with a built-in **MCP server** for AI-assisted development. Connect it to Claude Code, Claude Desktop, or any MCP-compatible tool:

```bash
claude mcp add arcadia-controls node tools/mcp-server/index.js
```

Then ask: *"Create a bar chart showing quarterly revenue by region, stacked"* — and get production-ready Blazor code instantly.

[MCP Server Documentation](https://arcadiaui.com/docs/mcp-server)

## IDE Support

**Visual Studio** and **JetBrains Rider** snippet packs included — type `arcline`, `arcbar`, `arcpie`, etc. for instant chart scaffolding.

See [tools/snippets/README.md](tools/snippets/README.md) for installation.

## Pricing

| Tier | Price | Includes |
|------|-------|----------|
| **Community** | Free (MIT) | Line, Bar, Pie, Scatter + 4 aliases, Gauge, 46 UI components, Notifications |
| **Pro** | $299/dev/year | All 20 chart types + DataGrid + DashboardKit + Form Builder + Notifications |
| **Enterprise** | $799/dev/year | Pro + priority support + source code access |

[View pricing](https://arcadiaui.com/#pricing)

## Design Principles

- **Render mode agnostic** — Server, WASM, and Auto (net8+)
- **Accessibility first** — WCAG 2.1 AA minimum
- **Zero Bootstrap dependency** — Tailwind CSS compatible via custom properties
- **Performance budgeted** — render time, memory, and interop calls tracked
- **Tree-shakeable** — each package is independent, Core is the only shared dependency

## What's New (beta.19)

- **Arcadia.DashboardKit** — drag-and-drop dashboard grid with FLIP animations, spring physics, iOS wiggle mode
- **23 new UI components** — CommandPalette, HoverCard, Popover, ContextMenu, Switch, Slider, Rating, and more
- **DataGrid enhancements** — column reorder, stacked headers, infinite scroll, cell tooltips, copy with headers, conditional formatting, cell merge, PDF export
- **Card upgrades** — glassmorphism variants, 6 elevation levels, gradient border, skeleton loading, collapsible
- **ObservableCollection binding** — live data on Charts and DataGrid with 16ms debounce
- **1,700+ tests** — 1,525 unit + 198 E2E + 11 performance
- **100+ components** across 10 packages

See [CHANGELOG.md](CHANGELOG.md) for all releases.

## Links

- [Website & Documentation](https://arcadiaui.com)
- [Live Playground](https://arcadiaui.com/playground/)
- [Changelog](CHANGELOG.md)
- [Performance Benchmarks](https://arcadiaui.com/docs/benchmarks)
- [NuGet Packages](https://www.nuget.org/packages?q=Arcadia)
- [Comparison: Arcadia vs Telerik vs Syncfusion](https://arcadiaui.com/blog/blazor-chart-library-comparison-2026/)

---

© 2026 Arcadia Controls. All rights reserved.

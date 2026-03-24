<p align="center">
  <strong>Arcadia Controls</strong><br>
  <em>12 chart types, dashboard widgets, form builder & notifications for Blazor</em>
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

## Charts (12 Types)

| Chart | Component | Tier |
|-------|-----------|------|
| Line / Area | `ArcadiaLineChart<T>` | Community (Free) |
| Bar / Column | `ArcadiaBarChart<T>` | Community (Free) |
| Pie / Donut | `ArcadiaPieChart<T>` | Community (Free) |
| Scatter / Bubble | `ArcadiaScatterChart<T>` | Community (Free) |
| Candlestick (OHLC) | `ArcadiaCandlestickChart<T>` | Pro |
| Radar / Spider | `ArcadiaRadarChart<T>` | Pro |
| Gauge | `ArcadiaGaugeChart` | Pro |
| Heatmap | `ArcadiaHeatmap<T>` | Pro |
| Funnel | `ArcadiaFunnelChart<T>` | Pro |
| Treemap | `ArcadiaTreemapChart<T>` | Pro |
| Waterfall | `ArcadiaWaterfallChart<T>` | Pro |
| Rose / Polar | `ArcadiaRoseChart<T>` | Pro |

**Plus:** 7 dashboard widgets (KPI Card, Sparkline, Progress Bar, Delta Indicator, Bar List, Tracker, Category Bar), Form Builder with 21 field types, and Toast Notifications.

## Quick Start

```bash
dotnet add package Arcadia.Charts
dotnet add package Arcadia.Theme
```

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
- **12 chart types** — from line charts to candlesticks, heatmaps to rose charts
- **Responsive** — set `Width="0"` and charts auto-fill their container
- **Dark/Light themes** — full theme support via CSS custom properties
- **Streaming data** — real-time updates with `AppendAndSlide` and sliding window
- **Export** — PNG/SVG download toolbar on every chart
- **Crosshair & Annotations** — interactive data exploration
- **Smooth curves** — Catmull-Rom interpolation for elegant line charts
- **Anti-collision layout engine** — labels never overlap
- **Accessibility** — WCAG 2.1 AA, screen reader tables, `prefers-reduced-motion`
- **Form Builder** — 21 field types, schema-driven or model-driven, wizard mode
- **Toast Notifications** — fire-and-forget with auto-dismiss and stacking

## NuGet Packages

| Package | Description |
|---------|-------------|
| `Arcadia.Core` | Base classes, theming engine, accessibility utilities |
| `Arcadia.Theme` | Design tokens, CSS custom properties, Tailwind plugin |
| `Arcadia.Charts` | All 12 chart types + 7 dashboard widgets |
| `Arcadia.FormBuilder` | Dynamic forms, validation, wizards |
| `Arcadia.Notifications` | Toast notification system |

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
| **Community** | Free (MIT) | Line, Bar, Pie, Scatter charts + Sparklines |
| **Pro** | $299/dev/year | All 12 chart types + Form Builder + Notifications |
| **Enterprise** | $799/dev/year | Pro + priority support + source code access |

[View pricing](https://arcadiaui.com/#pricing)

## Design Principles

- **Render mode agnostic** — Server, WASM, and Auto (net8+)
- **Accessibility first** — WCAG 2.1 AA minimum
- **Zero Bootstrap dependency** — Tailwind CSS compatible via custom properties
- **Performance budgeted** — render time, memory, and interop calls tracked
- **Tree-shakeable** — each package is independent, Core is the only shared dependency

## Links

- [Website & Documentation](https://arcadiaui.com)
- [Live Playground](https://arcadiaui.com/playground/)
- [NuGet Packages](https://www.nuget.org/packages?q=Arcadia)
- [Comparison: Arcadia vs Telerik vs Syncfusion](https://arcadiaui.com/blog/blazor-chart-library-comparison-2026/)

---

© 2026 Arcadia Controls. All rights reserved.

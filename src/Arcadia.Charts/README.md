# Arcadia.Charts

Dashboard analytics toolkit for Blazor — 8 chart types + 7 dashboard widgets. Native SVG rendering with zero JavaScript dependencies.

## Charts

Line, Bar, Pie/Donut, Scatter, Candlestick, Radar, Gauge, Heatmap

## Dashboard Widgets

KPI Card, Sparkline, Delta Indicator, Progress Bar, Bar List, Tracker, Category Bar

## Quick Start

```csharp
<HelixKpiCard Title="Revenue" Value="$142,500"
              Delta="+12.3%" DeltaType="DeltaType.Increase"
              Sparkline="@(new double[] { 42, 48, 45, 52, 55, 58, 62 })" />

<HelixLineChart TItem="SalesRecord" Data="@data"
                XField="@(d => (object)d.Month)"
                Series="@series" Height="300" />
```

## Key Features

- Native Blazor SVG rendering — zero JS for core charts
- Anti-collision layout engine — labels never overlap
- LTTB downsampling — handles 100K+ data points
- On-load animations — lines draw, bars grow, points pop
- WCAG 2.1 AA — hidden data table for screen readers
- 7 color palettes including color-blind safe (Okabe-Ito)
- Works in Server, WASM, Auto, and static SSR

## Installation

```
dotnet add package Arcadia.Charts
```

## Links

- [Documentation](https://arcadiaui.com/docs/charts)
- [Live Demo](https://arcadiaui.com/demo)
- [GitHub](https://github.com/helixuidev/helixui)

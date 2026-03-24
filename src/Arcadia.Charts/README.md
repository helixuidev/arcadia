# Arcadia.Charts

12 Blazor chart types + 7 dashboard widgets. Pure SVG rendering, zero JavaScript dependencies.

## Charts

| Community (Free) | Pro ($299/dev/year) |
|-----------------|---------------------|
| Line / Area | Candlestick (OHLC) |
| Bar / Column | Radar / Spider |
| Pie / Donut | Gauge |
| Scatter / Bubble | Heatmap, Funnel, Treemap, Waterfall, Rose |

## Quick Start

```bash
dotnet add package Arcadia.Charts
dotnet add package Arcadia.Theme
```

```razor
<ArcadiaLineChart TItem="DataRecord" Data="@data"
                  XField="@(d => (object)d.Label)" Series="@series"
                  Height="350" Width="0" ShowPoints="true" />
```

## Key Features

Pure SVG rendering · Responsive (`Width="0"`) · Dark/Light themes · Export PNG/SVG · Streaming data · Crosshair & annotations · Smooth curves · Anti-collision labels · WCAG 2.1 AA accessible

**[Docs](https://arcadiaui.com/docs/charts)** · **[Demo](https://arcadiaui.com/playground/)** · **[GitHub](https://github.com/ArcadiaUIDev/arcadia)**

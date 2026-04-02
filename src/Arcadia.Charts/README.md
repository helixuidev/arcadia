<p align="center">
  <strong>Arcadia.Charts</strong><br>
  <em>20 chart types + 7 dashboard widgets for Blazor — 244KB total, zero JavaScript</em>
</p>

## Why Arcadia Charts?

- **Pure SVG** — no JavaScript runtime, no npm, no webpack. Charts render in C# on the server.
- **244KB total** — compare to Syncfusion (3MB+) or Telerik (1.5MB+)
- **Responsive** — charts fill their container automatically. Set `Width="0"` and done.
- **Accessible** — WCAG 2.1 AA: screen reader tables, `prefers-reduced-motion`, keyboard nav

## 20 Chart Types

Line · Area · Bar · Stacked Bar · Pie · Donut · Scatter · Bubble · Candlestick · Radar · Gauge · Heatmap · Funnel · Treemap · Waterfall · Rose · Range Area · Box Plot · Sankey · Chord

4 base charts + 4 aliases are **free forever** (MIT). Pro adds 12 more for $299/dev/year.

## 60-Second Quick Start

```bash
dotnet add package Arcadia.Charts
dotnet add package Arcadia.Theme
```

Add to your `App.razor` `<head>`:
```html
<link href="_content/Arcadia.Theme/css/arcadia.css" rel="stylesheet" />
<link href="_content/Arcadia.Charts/css/arcadia-charts.css" rel="stylesheet" />
```

Drop in a chart:
```razor
@using Arcadia.Charts.Core
@using Arcadia.Charts.Components.Charts

<ArcadiaLineChart TItem="Sale" Data="@sales"
                  XField="@(d => (object)d.Month)"
                  Series="@(new List<SeriesConfig<Sale>> {
                      new() { Name = "Revenue", Field = d => d.Revenue,
                              Color = "success", ShowArea = true, CurveType = "smooth" }
                  })"
                  Height="350" ShowPoints="true" />
```

## What's Included

| Feature | Details |
|---------|---------|
| Streaming | `AppendAndSlide` for real-time data with slide animation |
| Export | PNG/SVG download toolbar on every chart |
| Crosshair | Vertical tracking line on hover |
| Annotations | Mark events, thresholds, targets on the chart |
| Trendlines | Linear regression and moving average overlays |
| Click events | `OnPointClick` returns item + index + series context |
| Custom tooltips | `TooltipTemplate` for rich HTML hover content |
| ObservableCollection | Live data binding with 16ms debounce — no `StateHasChanged` needed |
| Dark/Light | Full theme support via CSS custom properties |
| Smooth curves | Catmull-Rom interpolation for elegant lines |

**[Live Demo](https://arcadiaui.com/playground/)** · **[Documentation](https://arcadiaui.com/docs/charts)** · **[GitHub](https://github.com/ArcadiaUIDev/arcadia)**

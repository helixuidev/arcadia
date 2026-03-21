# HelixUI Charts — Product Requirements Plan

## 1. Vision

A native Blazor SVG charting engine with zero JS dependencies, a composable Recharts-style API, and dashboard-first components. The first Blazor charting library that combines full native rendering with a rich feature set and WCAG 2.1 AA accessibility.

**Target users:** Enterprise .NET developers building dashboards, analytics pages, reporting interfaces, and KPI displays.

---

## 2. Competitive Gap

### What exists today

| Capability | Syncfusion | Telerik | Radzen | MudBlazor | ApexCharts Blazor |
|---|---|---|---|---|---|
| Chart types | 55+ | ~15 | ~8 | ~5 | ~20 |
| Rendering | SVG+Canvas | SVG (Kendo JS) | SVG native | SVG native | SVG (JS lib) |
| JS dependency | Minimal | Heavy (Kendo) | None | None | Heavy |
| Composable API | No | No | No | No | No |
| Dashboard components | No | No | No | No | No |
| Accessibility (WCAG) | AA | AA | No | No | No |
| CSS custom property theming | No | No | No | Partial | No |
| Synchronized charts | Yes | No | No | No | No |
| Server-side SVG export | No | No | No | No | No |
| Streaming/real-time | Yes | Yes | No | No | Yes |

### Our strategic position

**Native + Feature-rich + Accessible + Dashboard-first.** Nobody offers all four.

- Radzen/MudBlazor prove native SVG works — but they're feature-poor
- Syncfusion has features — but uses JS interop and costs $$$
- ApexCharts wrapper is feature-rich — but heavy JS, no SSR, no accessibility
- **Nobody** offers Recharts-style composable APIs or Tremor-style dashboard components

---

## 3. Architecture

```
HelixUI.Charts/
├── Core/
│   ├── ChartBase.cs                  # Base class for all chart types
│   ├── Series/
│   │   ├── ISeries.cs                # Series data contract
│   │   ├── LineSeries.cs             # Line/area data
│   │   ├── BarSeries.cs              # Bar/column data
│   │   ├── PieSeries.cs              # Pie/donut data
│   │   ├── ScatterSeries.cs          # Scatter/bubble data
│   │   └── GaugeSeries.cs            # Gauge value data
│   ├── Axes/
│   │   ├── IAxis.cs                  # Axis contract
│   │   ├── NumericAxis.cs            # Linear/logarithmic numeric
│   │   ├── CategoryAxis.cs           # String categories
│   │   ├── DateTimeAxis.cs           # Time-based
│   │   └── AxisRenderer.cs           # SVG axis rendering (ticks, labels, gridlines)
│   ├── Scales/
│   │   ├── LinearScale.cs            # Maps data range → pixel range
│   │   ├── LogScale.cs               # Logarithmic mapping
│   │   ├── BandScale.cs              # Category bands (bar charts)
│   │   └── TimeScale.cs              # DateTime mapping
│   ├── Layout/
│   │   ├── ChartLayout.cs            # Margin/padding/legend position calc
│   │   └── ResponsiveContainer.cs    # Auto-resize on container change
│   └── Palette/
│       ├── ChartPalette.cs           # Color palette from theme tokens
│       └── DefaultPalettes.cs        # Built-in palettes (10+)
├── Components/
│   ├── Charts/
│   │   ├── HelixLineChart.razor      # Line + Area chart
│   │   ├── HelixBarChart.razor       # Vertical + Horizontal bar
│   │   ├── HelixPieChart.razor       # Pie + Donut
│   │   ├── HelixScatterChart.razor   # Scatter + Bubble
│   │   ├── HelixRadarChart.razor     # Radar/spider
│   │   ├── HelixGaugeChart.razor     # Radial gauge / KPI meter
│   │   └── HelixHeatmap.razor        # Heatmap grid
│   ├── Elements/                      # Composable sub-components
│   │   ├── HelixXAxis.razor          # X-axis config
│   │   ├── HelixYAxis.razor          # Y-axis config
│   │   ├── HelixLine.razor           # Line series element
│   │   ├── HelixArea.razor           # Area fill element
│   │   ├── HelixBar.razor            # Bar series element
│   │   ├── HelixPie.razor            # Pie/donut slice element
│   │   ├── HelixScatter.razor        # Scatter point element
│   │   ├── HelixTooltip.razor        # Hover tooltip
│   │   ├── HelixLegend.razor         # Chart legend
│   │   ├── HelixGridLines.razor      # Background grid
│   │   ├── HelixReferenceLine.razor  # Horizontal/vertical reference
│   │   ├── HelixAnnotation.razor     # Data point annotation
│   │   └── HelixCrosshair.razor      # Crosshair cursor
│   ├── Dashboard/                     # Pre-composed dashboard widgets
│   │   ├── HelixSparkline.razor      # Inline mini chart
│   │   ├── HelixKpiCard.razor        # KPI with value, delta, sparkline
│   │   ├── HelixProgressBar.razor    # Linear/circular progress
│   │   ├── HelixDeltaIndicator.razor # Up/down percentage change
│   │   ├── HelixBarList.razor        # Horizontal bar ranking list
│   │   ├── HelixTracker.razor        # Day/status grid (GitHub-style)
│   │   └── HelixCategoryBar.razor    # Segmented category bar
│   └── Shared/
│       ├── HelixChartGroup.razor     # Synchronized chart container
│       └── ChartContainer.razor      # Responsive SVG wrapper
├── wwwroot/
│   ├── css/helix-charts.css          # Chart styles with theme tokens
│   └── js/chart-interop.js           # Minimal JS: resize observer, tooltip positioning
└── HelixUI.Charts.csproj
```

---

## 4. API Design — Composable Pattern

Inspired by Recharts. Each chart element is an independent Blazor component:

```razor
<HelixLineChart Data="@salesData" Height="300">
    <HelixXAxis Field="@(d => d.Month)" />
    <HelixYAxis Label="Revenue ($)" />
    <HelixLine Field="@(d => d.Revenue)" Color="primary" StrokeWidth="2" />
    <HelixLine Field="@(d => d.Target)" Color="secondary" StrokeDasharray="5,5" />
    <HelixArea Field="@(d => d.Revenue)" Opacity="0.1" />
    <HelixReferenceLine Y="50000" Label="Goal" Color="success" />
    <HelixTooltip />
    <HelixLegend Position="Bottom" />
    <HelixGridLines Horizontal="true" />
</HelixLineChart>
```

### Data binding
```csharp
// Strongly-typed with lambda field selectors
var salesData = new List<SalesRecord>
{
    new("Jan", 45000, 50000),
    new("Feb", 52000, 50000),
    // ...
};

record SalesRecord(string Month, double Revenue, double Target);
```

### Bar chart
```razor
<HelixBarChart Data="@data" Height="300">
    <HelixXAxis Field="@(d => d.Category)" />
    <HelixYAxis />
    <HelixBar Field="@(d => d.Value2024)" Label="2024" Color="primary" />
    <HelixBar Field="@(d => d.Value2025)" Label="2025" Color="secondary" />
    <HelixTooltip />
    <HelixLegend />
</HelixBarChart>
```

### Pie / Donut
```razor
<HelixPieChart Data="@segments" Height="300">
    <HelixPie NameField="@(d => d.Label)"
              ValueField="@(d => d.Amount)"
              InnerRadius="60"
              Labels="true" />
    <HelixTooltip />
    <HelixLegend Position="Right" />
</HelixPieChart>
```

### Dashboard components
```razor
<HelixKpiCard Title="Monthly Revenue"
              Value="$142,500"
              Delta="+12.3%"
              DeltaType="Increase"
              Sparkline="@revenueHistory" />

<HelixSparkline Data="@last30Days" Height="32" Width="120"
                Color="primary" ShowArea="true" />

<HelixBarList Data="@topProducts" NameField="@(d => d.Name)"
              ValueField="@(d => d.Sales)" ValueFormat="$#,##0" />

<HelixTracker Data="@commitHistory" Height="120"
              StartDate="@sixMonthsAgo" />
```

---

## 5. Feature Requirements by Phase

### Phase 1 — Core Engine + 4 Chart Types
*Ship the foundation and most-used charts.*

**SVG Rendering Engine:**
- [ ] Linear scale (data range → pixel range mapping)
- [ ] Band scale (category-based for bar charts)
- [ ] Time scale (DateTime mapping)
- [ ] Axis renderer (ticks, labels, gridlines, auto-tick calculation)
- [ ] Chart layout calculator (margins, padding, legend space)
- [ ] Responsive container (resize observer via minimal JS)
- [ ] Color palette system integrated with HelixUI.Theme tokens
- [ ] 10+ built-in palettes (Default, Cool, Warm, Monochrome, Pastel, etc.)

**Chart Components:**
- [ ] HelixLineChart — single/multi-series line, curved/linear/stepped interpolation, area fill
- [ ] HelixBarChart — vertical/horizontal, grouped/stacked, rounded corners
- [ ] HelixPieChart — pie + donut with configurable inner radius, labels, explode on hover
- [ ] HelixScatterChart — scatter + bubble (size-mapped)

**Composable Elements:**
- [ ] HelixXAxis — position, label, tick format, rotation
- [ ] HelixYAxis — position, label, tick format, dual axis support
- [ ] HelixLine — series config (color, width, dash, curve type, data points)
- [ ] HelixArea — fill below line (gradient support)
- [ ] HelixBar — series config (color, width, corner radius)
- [ ] HelixPie — series config (inner/outer radius, padding angle, labels)
- [ ] HelixScatter — point config (color, size, shape: circle/square/triangle)
- [ ] HelixTooltip — hover tooltip with customizable template
- [ ] HelixLegend — position (top/bottom/left/right), interactive toggle
- [ ] HelixGridLines — horizontal/vertical, dashed, color
- [ ] HelixReferenceLine — horizontal/vertical reference with label

**Interactivity:**
- [ ] Hover highlight (series/point)
- [ ] Click events (OnPointClick, OnSeriesClick)
- [ ] Tooltip with smart positioning (follows cursor, stays in bounds)
- [ ] Legend toggle (click to show/hide series)
- [ ] CSS transitions for smooth updates

**Accessibility:**
- [ ] SVG `role="img"` with `aria-label` on chart container
- [ ] Hidden data table for screen readers (`<table>` inside `aria-hidden` SVG)
- [ ] Keyboard navigation between data points
- [ ] High contrast mode (pattern fills + increased stroke width)
- [ ] `prefers-reduced-motion` respected for all animations

### Phase 2 — Dashboard Components + Advanced Charts
*Ship the Tremor-inspired dashboard widgets that nobody else has.*

**Dashboard Components:**
- [ ] HelixSparkline — inline mini line/bar/area chart, no axes
- [ ] HelixKpiCard — value, delta indicator, mini sparkline, trend text
- [ ] HelixDeltaIndicator — up/down arrow with percentage and color
- [ ] HelixProgressBar — linear and circular variants, label, color by threshold
- [ ] HelixBarList — horizontal bar ranking (name + value + bar)
- [ ] HelixTracker — day/status grid (GitHub contribution graph style)
- [ ] HelixCategoryBar — segmented horizontal bar (budget allocation, etc.)

**Additional Charts:**
- [ ] HelixRadarChart — radar/spider with fill, multi-series
- [ ] HelixGaugeChart — radial gauge with min/max/thresholds
- [ ] HelixHeatmap — 2D color grid with scale legend

**Advanced Features:**
- [ ] HelixAnnotation — label/callout on specific data point
- [ ] HelixCrosshair — vertical/horizontal crosshair on hover
- [ ] HelixChartGroup — synchronized hover/zoom across multiple charts
- [ ] Zoom + pan (minimal JS interop for mouse/touch events)
- [ ] Data point selection (click to select, shift-click multi-select)

### Phase 3 — Enterprise
*Production-grade features for serious deployments.*

- [ ] Log scale axis
- [ ] Dual/multiple Y-axes
- [ ] Trendlines (linear regression, moving average)
- [ ] Data downsampling for large datasets (LTTB algorithm)
- [ ] Real-time streaming data with efficient incremental SVG updates
- [ ] Server-side SVG export (render chart to string for PDF/email)
- [ ] PNG export (via Canvas conversion, minimal JS)
- [ ] Stacked area charts
- [ ] Range area/bar charts
- [ ] Waterfall chart
- [ ] Custom tooltip templates (RenderFragment)
- [ ] Data labels on points/bars
- [ ] Axis label formatting (currency, percentage, date formats)
- [ ] Null/missing data handling (gap, connect, zero)
- [ ] Click-to-drill-down events with filter context

---

## 6. Non-Functional Requirements

- **Zero JS for core rendering** — SVG generated entirely in C#/Blazor
- **Minimal JS for interactivity** — resize observer, tooltip positioning, zoom/pan only
- **Render mode agnostic** — Server, WASM, Auto, and static SSR (charts render as static SVG)
- **Multi-target** — net5.0 through net9.0
- **Performance** — render 10,000 data points in < 200ms; 100,000 with downsampling
- **Bundle** — < 30KB CSS, < 5KB JS (before gzip)
- **WCAG 2.1 AA** — all chart types
- **Theme integration** — all colors from `--helix-*` CSS custom properties
- **Test coverage** — ≥ 80% unit tests

---

## 7. Data Model

```csharp
// Charts accept IEnumerable<T> with lambda field selectors
public interface IChartData<T>
{
    IEnumerable<T> Data { get; }
}

// Series reference fields via expressions
<HelixLine Field="@(d => d.Revenue)" />  // Func<T, double>
<HelixXAxis Field="@(d => d.Month)" />   // Func<T, string> or Func<T, DateTime>

// Dashboard components accept simple arrays
<HelixSparkline Data="@(new double[] { 4, 7, 2, 8, 5, 9, 3 })" />
```

---

## 8. Milestones

| Phase | Scope | Components | Target |
|---|---|---|---|
| **Phase 1** | Core engine + 4 charts | ~25 files | Foundation |
| **Phase 2** | Dashboard + 3 charts | ~15 files | Differentiation |
| **Phase 3** | Enterprise features | ~10 files | Production-ready |

---

## 9. Open Questions

1. **Curve interpolation:** Ship catmull-rom (smooth), monotone, linear, step — or just linear + smooth?
2. **Responsive strategy:** Resize observer (needs JS) vs. CSS container queries (newer browsers only)?
3. **Animation:** CSS transitions, SMIL (deprecated but widely supported), or skip animations for v1?
4. **Tooltip positioning:** Pure CSS (limited) or minimal JS for smart boundary detection?
5. **Data table for accessibility:** Render as hidden `<table>` inside SVG, or as a sibling element?

# HelixUI Charts — Product Requirements Plan

## 1. Vision

**A dashboard analytics toolkit for Blazor** — not just another chart library.

Pre-composed, opinionated dashboard widgets with beautiful defaults, backed by a solid charting engine. A developer drops in `<HelixKpiCard>` or `<HelixBarList>` and it looks great immediately. When they need a full chart, the composable API makes it effortless.

Think Tremor for React, but for Blazor — and with the accessibility compliance that enterprise and government buyers require.

**Target users:** Enterprise .NET developers building analytics dashboards, KPI displays, admin panels, and reporting interfaces.

**Why this, why now:** Every Blazor chart library is either a $1,000/year suite tax (Syncfusion, Telerik, DevExpress) or a free-but-basic option (MudBlazor, Radzen). Nobody sells a focused, premium dashboard toolkit at an accessible price point.

---

## 2. Pricing & Packaging

| Tier | Price | Includes |
|---|---|---|
| **Community Edition** | Free / MIT | Core + Theme + 3 chart types (Line, Bar, Pie) + Sparkline. Full functionality, no watermark, no limits. Gets developers in the door. |
| **Pro** | $299/dev/year | All chart types + all dashboard widgets + pan/zoom + animations + live data + LTTB downsampling + export |
| **Enterprise** | $799/dev/year | All HelixUI packages (Charts + FormBuilder + Notifications + future packages). Still cheaper than one Telerik license. |

### Community Edition Strategy
- Core, Theme, and basic charts are always free and open source
- Developers discover HelixUI through free packages, build with them, hit the growth point where they need KpiCard, BarList, Tracker, advanced interactivity
- $299 upgrade is a no-brainer — doesn't need VP approval, cheaper than one day of developer time
- Targets the Syncfusion community license cliff: companies that outgrow the free tier ($1M revenue / 5 devs) face $995/dev. We're there at $299.

---

## 3. Honest Competitive Position

### What we can't claim as unique
These features exist in multiple competitors. We must execute them well, but they're not differentiators:

| Feature | Who already has it |
|---|---|
| Native SVG rendering | Radzen, MudBlazor v9 |
| Composable child-component API | Telerik, Radzen, Syncfusion |
| Label collision handling | Syncfusion (7+ strategies + smart labels) |
| Pan/zoom | Syncfusion, Telerik, DevExpress |
| Incremental live data updates | DevExpress |
| On-load & hover animations | Everyone |

### What is genuinely ours

| Differentiator | Why it matters | Who else has it |
|---|---|---|
| **Dashboard widget kit** (KpiCard, BarList, Tracker, DeltaIndicator, CategoryBar, ProgressBar) | Pre-composed, beautiful defaults. Drop in and it works. Tremor proved this in React. | Nobody in Blazor |
| **Hidden data table for screen readers** | Government/enterprise procurement requires WCAG AA. We pass audits others can't. | Nobody in Blazor |
| **LTTB downsampling** | Throw 100K points at it, it just works. No manual optimization needed. | Nobody in Blazor |
| **Data-morph transitions** | Values animate smoothly to new positions. The "wow factor" in demos and sales pitches. | Rare — most libraries destroy and recreate |
| **$299 standalone pricing** | Focused package, not a $1,000 suite tax. Easy procurement approval. | Nobody sells Blazor charting standalone |
| **Developer experience** | Cleanest API, best defaults, least configuration. The Stripe of Blazor charts. | Subjective — must execute |

### Threats to watch
- **MudBlazor v9** — free, growing fast, chart rewrite is decent. But shallow feature set, no accessibility, no dashboard widgets, community-maintained (slower iteration).
- **Syncfusion community license** — free for small teams. But cliff at $995 once you grow.

---

## 4. Architecture

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
│   │   └── AxisRenderer.cs           # SVG axis rendering
│   ├── Scales/
│   │   ├── LinearScale.cs            # Data range → pixel range
│   │   ├── LogScale.cs               # Logarithmic mapping
│   │   ├── BandScale.cs              # Category bands (bar charts)
│   │   └── TimeScale.cs              # DateTime mapping
│   ├── Layout/
│   │   ├── ChartLayoutEngine.cs      # THE anti-collision engine
│   │   ├── ChartLayoutInput.cs       # Layout inputs
│   │   ├── ChartLayoutResult.cs      # Resolved positions
│   │   ├── TextMeasure.cs            # Text width estimation
│   │   ├── TickGenerator.cs          # Smart tick calculation (Wilkinson's)
│   │   ├── CollisionDetector.cs      # AABB overlap detection
│   │   └── ResponsiveBreakpoints.cs  # Structural layout per width tier
│   ├── Data/
│   │   ├── LttbDownsampler.cs        # Largest Triangle Three Buckets
│   │   ├── DataViewport.cs           # Visible data window
│   │   └── DataPointKey.cs           # Identity key for Blazor diffing
│   ├── Animation/
│   │   ├── AnimationConfig.cs        # Duration, easing, reduced motion
│   │   └── MorphCalculator.cs        # Interpolation between old/new values
│   └── Palette/
│       ├── ChartPalette.cs           # Color palette from theme tokens
│       └── DefaultPalettes.cs        # 10+ built-in palettes
├── Components/
│   ├── Charts/
│   │   ├── HelixLineChart.razor      # Line + Area (Community)
│   │   ├── HelixBarChart.razor       # Bar/Column (Community)
│   │   ├── HelixPieChart.razor       # Pie + Donut (Community)
│   │   ├── HelixScatterChart.razor   # Scatter + Bubble (Pro)
│   │   ├── HelixRadarChart.razor     # Radar/spider (Pro)
│   │   ├── HelixGaugeChart.razor     # Radial gauge (Pro)
│   │   └── HelixHeatmap.razor        # Heatmap grid (Pro)
│   ├── Elements/                      # Composable sub-components
│   │   ├── HelixXAxis.razor
│   │   ├── HelixYAxis.razor
│   │   ├── HelixLine.razor
│   │   ├── HelixArea.razor
│   │   ├── HelixBar.razor
│   │   ├── HelixPie.razor
│   │   ├── HelixScatter.razor
│   │   ├── HelixTooltip.razor
│   │   ├── HelixLegend.razor
│   │   ├── HelixGridLines.razor
│   │   ├── HelixReferenceLine.razor
│   │   ├── HelixAnnotation.razor
│   │   └── HelixCrosshair.razor
│   ├── Dashboard/                     # THE differentiator — Tremor-style widgets
│   │   ├── HelixSparkline.razor      # Inline mini chart (Community)
│   │   ├── HelixKpiCard.razor        # KPI: value + delta + sparkline (Pro)
│   │   ├── HelixDeltaIndicator.razor # Up/down percentage arrow (Pro)
│   │   ├── HelixProgressBar.razor    # Linear + circular progress (Pro)
│   │   ├── HelixBarList.razor        # Horizontal bar ranking (Pro)
│   │   ├── HelixTracker.razor        # GitHub-style contribution grid (Pro)
│   │   └── HelixCategoryBar.razor    # Segmented category bar (Pro)
│   └── Shared/
│       ├── HelixChartGroup.razor     # Synchronized chart container
│       └── ChartContainer.razor      # Responsive SVG wrapper
├── Accessibility/
│   ├── HiddenDataTable.razor         # Screen reader data table
│   ├── ChartKeyboardNav.cs           # Keyboard navigation logic
│   └── HighContrastMode.cs           # Pattern fills for color-blind users
├── wwwroot/
│   ├── css/helix-charts.css
│   └── js/chart-interop.js           # ~10-15KB: resize, tooltips, pan/zoom, morph
└── HelixUI.Charts.csproj
```

---

## 5. API Design

### Dashboard widgets (the hero experience)
```razor
@* Drop in. Looks great. Done. *@
<HelixKpiCard Title="Monthly Revenue"
              Value="$142,500"
              Delta="+12.3%"
              DeltaType="Increase"
              Sparkline="@revenueHistory" />

<HelixSparkline Data="@last30Days" Height="32" Width="120"
                Color="primary" ShowArea="true" />

<HelixBarList Data="@topProducts"
              NameField="@(d => d.Name)"
              ValueField="@(d => d.Sales)"
              ValueFormat="$#,##0" />

<HelixTracker Data="@commitHistory" Height="120"
              StartDate="@sixMonthsAgo"
              ColorScale="success" />

<HelixDeltaIndicator Value="+12.3%" Type="Increase" />

<HelixProgressBar Value="73" Max="100" Label="Storage used"
                  Thresholds="@(new[] { (60, "warning"), (90, "danger") })" />

<HelixCategoryBar Segments="@budgetBreakdown"
                  NameField="@(d => d.Category)"
                  ValueField="@(d => d.Amount)" />
```

### Full charts (composable, when you need them)
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

### Live data
```razor
<HelixLineChart @ref="chartRef" Data="@sensorData" Height="300"
                SlidingWindow="100" FollowLatest="true"
                EnableZoom="true" EnablePan="true" ZoomMode="X">
    <HelixXAxis Field="@(d => d.Timestamp)" />
    <HelixYAxis Label="Temperature (°C)" />
    <HelixLine Field="@(d => d.Value)" Color="primary" />
    <HelixTooltip />
</HelixLineChart>

@code {
    // SignalR push — one line to add a point
    hubConnection.On<SensorReading>("Reading", r =>
        InvokeAsync(() => chartRef.AppendPoint(r)));
}
```

### Data binding (strongly typed)
```csharp
// Charts accept IEnumerable<T> with lambda selectors
record SalesRecord(string Month, double Revenue, double Target);
record SensorReading(DateTime Timestamp, double Value);

// Dashboard widgets accept simple arrays too
<HelixSparkline Data="@(new double[] { 4, 7, 2, 8, 5, 9, 3 })" />
```

---

## 6. Layout Engine — Anti-Collision System

**The most critical infrastructure.** Runs as a separate pure C# pass before SVG rendering.

### Architecture

```csharp
public class ChartLayoutEngine
{
    public ChartLayoutResult Calculate(ChartLayoutInput input);
}
```

Inputs: chart dimensions, data ranges, series count, tick labels, legend config.
Outputs: resolved tick positions, label positions with rotations, legend layout, final margins — all non-overlapping.

### Collision Strategies

**Axis Ticks — Priority Cascade:**
1. Calculate nice round numbers (Wilkinson's algorithm)
2. If labels overlap → reduce tick count
3. If still overlapping → rotate 0° → 45° → 90°
4. If still tight → abbreviate ("January" → "Jan" → "J")
5. Skip every Nth label (keep tick marks)
6. Last resort → first, middle, last only

**Time Axis Intelligence:**
- < 7 days → day names
- < 3 months → "Jan 15"
- < 2 years → "Jan", "Feb"
- < 10 years → "2024", "2025"
- 10+ years → "2020", "2025", "2030"

**Data Labels:** AABB overlap detection → nudge vertically → hide lower-value label → switch to hover-only at high density.

**Pie Labels:** Inside for slices > 10%, leader lines for small slices, two-column layout sorted vertically, group slices < 2° into "Other."

**Legend Adaptive Layout:** Horizontal → wrapped rows → vertical stack → truncated with "+N more" → popover at extreme counts.

**Responsive Breakpoints (structural, not just CSS scaling):**

| Width | Ticks | Labels | Legend | Grid | Axis Titles | Data Labels |
|---|---|---|---|---|---|---|
| > 600px | Full | Horizontal | Below/right | Both | Yes | Yes |
| 400-600px | Reduced | Rotated 45° | Below compact | H only | No | Hover only |
| 300-400px | Minimal | Rotated 90° | Hidden | None | No | No |
| < 300px | First/last | Abbreviated | Hidden | None | No | No |

**Auto-Margins:** Never hardcoded. Calculated from resolved label sizes. If margins would consume > 50% of chart area, the engine drops elements (titles first, then labels).

### Why This Architecture
- **Pure C#** — no DOM, fully unit testable
- **Deterministic** — same input always produces same output (snapshot testable)
- **Separate from rendering** — chart components just draw resolved positions, no layout logic in Razor files

---

## 7. Animation System

Animations are core infrastructure, not optional.

### On-Load
| Element | Animation | Technique |
|---|---|---|
| Line | Draws left-to-right | SVG `stroke-dashoffset` |
| Bar | Grows from baseline | CSS `transform: scaleY(0→1)` |
| Pie slice | Sweeps clockwise | SVG `stroke-dashoffset` on arc |
| Scatter | Scale in with stagger | CSS `scale(0→1)` + staggered delay |
| Dashboard widgets | Fade + slide up | CSS `opacity` + `translateY` |

### Hover
Point scale up, bar lighten, pie explode outward, series dim-others to 30% opacity.

### Data Morph (our differentiator)
When data changes, elements **morph** to new positions — they don't destroy and recreate:
- Lines: path `d` attribute interpolates from old to new coordinates
- Bars: height/width transition to new values
- Points: translate to new positions
- New data: scale in from zero
- Removed data: scale out, then remove

### Configuration
```razor
<HelixLineChart AnimateOnLoad="true" AnimationDuration="600"
                AnimationEasing="ease-out" AnimateUpdates="true">
```

`prefers-reduced-motion: reduce` → all durations set to 0.

---

## 8. Pan, Zoom & Drill-Down

### DataViewport
```csharp
public class DataViewport
{
    public double XMin { get; set; }
    public double XMax { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public bool AutoFitY { get; set; }  // Auto-adjust Y to fit visible data
}
```

### Interactions
- Mouse wheel → zoom centered on cursor
- Click-drag → pan
- Pinch-to-zoom on touch
- Box zoom (drag rectangle)
- Keyboard: arrows pan, +/- zoom
- Reset button to fit all data
- `ZoomMode`: X, Y, XY, None

### Drill-Down
```csharp
<HelixBarChart OnDrillDown="HandleDrillDown">

void HandleDrillDown(DrillDownContext<SalesRecord> ctx)
{
    // ctx.DataItem — the clicked item
    // ctx.SeriesName — which series
    // ctx.Viewport — current visible range
}
```

---

## 9. Live Data & Incremental Updates

### Streaming API
```csharp
chartRef.AppendPoint(newReading);      // Add one point — O(1) DOM update
chartRef.RemoveFirst();                // Sliding window — O(1) DOM update
chartRef.UpdateData(newBatch, animate: true);  // Batch with morph
```

### How It Works
- `@key="point.Id"` on SVG elements → Blazor diffs by identity, not position
- AppendPoint inserts one keyed element, siblings untouched
- SlidingWindow removes out-of-viewport elements from DOM (virtualization)
- FollowLatest auto-scrolls viewport to newest data

### Performance Budget
| Scenario | Target |
|---|---|
| Initial render, 1,000 points | < 100ms |
| Initial render, 10,000 points (LTTB) | < 200ms |
| Append 1 point (live) | < 16ms (60fps) |
| Sliding window shift | < 16ms |
| Pan/zoom viewport change | < 16ms |
| Full data replacement with morph | < 300ms |

---

## 10. Accessibility — Our Compliance Moat

This is a genuine differentiator. Government and enterprise buyers have WCAG AA requirements in procurement RFPs.

### Hidden Data Table
Every chart renders an invisible `<table>` alongside the SVG with all data values. Screen readers see structured data, not just "image." This is the Highcharts pattern — nobody in Blazor does it.

```html
<div class="helix-chart" role="figure" aria-label="Monthly Revenue, 2024-2026">
  <svg><!-- visible chart --></svg>
  <table class="helix-sr-only" aria-label="Chart data">
    <thead><tr><th>Month</th><th>Revenue</th><th>Target</th></tr></thead>
    <tbody>
      <tr><td>Jan</td><td>$45,000</td><td>$50,000</td></tr>
      <!-- ... -->
    </tbody>
  </table>
</div>
```

### Keyboard Navigation
- Tab to chart → focus ring visible
- Arrow keys navigate between data points
- Enter/Space to select/activate a point
- Escape to deselect

### High Contrast Mode
- Pattern fills (stripes, dots, crosshatch) in addition to colors
- Increased stroke widths
- Respects `prefers-contrast: more`

### Screen Reader Announcements
- Live region announces tooltip content on keyboard navigation
- Axis labels and scale read on chart focus

---

## 11. Testing Strategy

### Layout Collision Tests (the crown jewels)

**Fuzz testing — 1,000 random configs, assert invariant: no overlaps.**
```csharp
[Fact]
public void FuzzTest_NoOverlaps_AtAnySize()
{
    var random = new Random(42);
    for (var i = 0; i < 1000; i++)
    {
        var input = GenerateRandomInput(random,
            width: random.Next(150, 1200),
            tickCount: random.Next(2, 50),
            seriesCount: random.Next(1, 15));

        var result = layoutEngine.Calculate(input);

        AssertNoOverlaps(result.XTicks);
        AssertNoOverlaps(result.YTicks);
        Assert.True(result.PlotArea.Width > 0);
        Assert.True(result.PlotArea.Height > 0);
    }
}
```

**Specific collision scenarios:**
- Tick labels at every chart width (300, 400, 600, 800, 1200)
- Long label text ("Very Long Category Name") at narrow widths
- Pie labels with many small slices (8+ slices under 5%)
- Legend with 1, 5, 10, 20 series items
- Responsive breakpoint element visibility at each tier

**Animation tests:**
- `prefers-reduced-motion` → all durations zero
- Incremental update → only new DOM elements created
- Data morph → old and new values interpolate correctly

**Accessibility tests:**
- Hidden data table matches chart data
- ARIA labels present on all chart elements
- Keyboard navigation cycles through all data points

---

## 12. Feature Requirements by Phase

### Phase 1 — Dashboard Widgets + Core Engine
*Lead with what nobody else has. Ship the Tremor angle first.*

**Dashboard Widgets (the hero):**
- [ ] HelixSparkline — inline mini line/bar/area, no axes (Community)
- [ ] HelixKpiCard — value + delta + sparkline + trend text (Pro)
- [ ] HelixDeltaIndicator — up/down arrow with percentage and color (Pro)
- [ ] HelixProgressBar — linear + circular, thresholds, label (Pro)
- [ ] HelixBarList — horizontal bar ranking with name + value (Pro)
- [ ] HelixTracker — GitHub contribution grid (Pro)
- [ ] HelixCategoryBar — segmented horizontal bar (Pro)

**Core Engine:**
- [ ] ChartLayoutEngine with full anti-collision pipeline
- [ ] Smart tick generator (Wilkinson's)
- [ ] Text width estimation
- [ ] Tick collision cascade (reduce → rotate → abbreviate → skip)
- [ ] Legend adaptive layout
- [ ] Responsive breakpoints (4 tiers)
- [ ] Auto-margin calculation
- [ ] Linear, Band, Time scales
- [ ] LTTB downsampler
- [ ] Color palette system (theme token integration)
- [ ] 10+ built-in palettes

**Community Charts (4 types):**
- [ ] HelixLineChart — single/multi-series, curved/linear, area fill
- [ ] HelixBarChart — vertical/horizontal, grouped/stacked
- [ ] HelixPieChart — pie + donut, labels, hover explode
- [ ] HelixScatterChart — scatter + bubble (size-mapped)

**Composable Elements:**
- [ ] HelixXAxis, HelixYAxis
- [ ] HelixLine, HelixArea, HelixBar, HelixPie
- [ ] HelixTooltip (smart positioning)
- [ ] HelixLegend (interactive toggle)
- [ ] HelixGridLines, HelixReferenceLine

**Animation (core):**
- [ ] On-load: line draw, bar grow, pie sweep
- [ ] Hover: highlight, dim others
- [ ] Data morph: value-to-value transitions
- [ ] `prefers-reduced-motion` support

**Interactivity:**
- [ ] Hover highlight + click events
- [ ] Pan + zoom (mouse, touch, keyboard)
- [ ] DataViewport windowing
- [ ] Tooltip smart positioning

**Live Data:**
- [ ] AppendPoint / RemoveFirst streaming API
- [ ] @key-based Blazor diffing
- [ ] SlidingWindow virtualization
- [ ] FollowLatest mode

**Accessibility:**
- [ ] Hidden data table for screen readers
- [ ] Keyboard navigation
- [ ] High contrast pattern fills
- [ ] ARIA labels + live region announcements
- [ ] `prefers-reduced-motion`

**JS Interop Module (~10-15KB):**
- [ ] ResizeObserver
- [ ] Tooltip positioning
- [ ] Pan/zoom event handling
- [ ] Path morph orchestration

### Phase 2 — Pro Charts + Advanced Features

**Additional Chart Types (Pro):**
- [ ] HelixRadarChart — radar/spider with fill
- [ ] HelixGaugeChart — radial gauge with thresholds
- [ ] HelixHeatmap — 2D color grid with scale legend

**Advanced Features:**
- [ ] HelixAnnotation — callout on specific data point
- [ ] HelixCrosshair — crosshair on hover
- [ ] HelixChartGroup — synchronized hover/zoom across charts
- [ ] Box zoom (drag rectangle)
- [ ] Data point selection (click + shift-click multi-select)
- [ ] Drill-down events with typed context
- [ ] Custom tooltip templates (RenderFragment)

**Enterprise Features:**
- [ ] RTL support
- [ ] Localization (locale-based number/date formatting)
- [ ] Export: PNG, SVG (via JS Canvas conversion + SVG serialization)
- [ ] Print support
- [ ] Log scale axis
- [ ] Dual/multiple Y-axes
- [ ] Trendlines (linear regression, moving average)

### Phase 3 — Enterprise+

- [ ] Export: PDF, CSV, XLSX
- [ ] Server-side SVG export (render to string for email/PDF reports)
- [ ] Stacked area charts
- [ ] Range area/bar charts
- [ ] Waterfall chart
- [ ] Data labels with collision-free positioning
- [ ] Axis label formatting (currency, percentage, custom)
- [ ] Null/missing data handling (gap, connect, zero)
- [ ] Synchronized chart groups with linked brushing
- [ ] Canvas rendering fallback for > 10K visible points

---

## 13. Non-Functional Requirements

- **SVG rendering in C#** — core markup generated without JS
- **JS module** — ~10-15KB for resize, tooltips, pan/zoom, morph
- **Render mode agnostic** — Server, WASM, Auto; static SSR renders non-interactive SVG
- **Multi-target** — net5.0 through net10.0
- **Performance:** per budget table in Section 9
- **Bundle** — < 30KB CSS, < 15KB JS (before gzip)
- **WCAG 2.1 AA** — all chart types, auditable
- **Theme integration** — all colors from `--helix-*` CSS custom properties
- **Test coverage** — ≥ 80%, mandatory layout collision fuzz tests
- **No label overlaps guaranteed** — layout engine invariant

---

## 14. Resolved Decisions

| Question | Decision | Rationale |
|---|---|---|
| **Path morphing** | Web Animations API (JS) | Modern standard, already in our JS module budget. C# renders, JS animates. Chart works without JS — just no animation. |
| **Hidden data table** | Sibling `<table>` element | `<foreignObject>` has inconsistent browser/screen reader support. Sibling table is universally accessible. |
| **Viewport state** | Bindable `@bind-Viewport` | Developers need URL-persistable zoom/pan state for shareable dashboard links. Internal default, opt-in binding. |
| **Live data backpressure** | Buffer + batch at 60fps | Never drop data. Buffer incoming points, flush once per animation frame. LTTB handles visual density. |
| **Community/Pro split** | Line, Bar, Pie, Scatter, Sparkline = free | 4 basic charts feels generous, drives adoption. Radar, Gauge, Heatmap + dashboard widgets = Pro. |
| **Localization** | .NET CultureInfo | Accept `CultureInfo`/`IFormatProvider` on axes and tooltips. Don't reinvent — `ToString("C", culture)` already works. |

### Design Principle
**C# renders, JS animates.** The chart is fully functional without JS (static SVG). JS makes it beautiful and interactive. This means static SSR works out of the box — charts render as plain SVG markup with no JS required.

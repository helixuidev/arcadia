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

## 5. Layout Engine — Anti-Collision System

**This is the most critical piece of the entire charting library.** The #1 complaint with chart libraries is overlapping labels, crowded ticks, and legends that overflow. We solve this with a dedicated layout engine that runs as a separate pass before any SVG is rendered.

### Architecture

```csharp
public class ChartLayoutEngine
{
    // Runs BEFORE rendering. All collision resolution happens here.
    // The chart renderer receives pre-resolved, non-overlapping positions.
    public ChartLayoutResult Calculate(ChartLayoutInput input);
}

public class ChartLayoutInput
{
    public double Width { get; set; }
    public double Height { get; set; }
    public DataRange XRange { get; set; }
    public DataRange YRange { get; set; }
    public List<string> TickLabels { get; set; }
    public List<SeriesInfo> Series { get; set; }
    public LegendConfig Legend { get; set; }
    public string? Title { get; set; }
    public string? XAxisTitle { get; set; }
    public string? YAxisTitle { get; set; }
}

public class ChartLayoutResult
{
    public ChartMargins Margins { get; set; }       // Final resolved margins
    public PlotArea PlotArea { get; set; }           // Usable drawing area
    public List<TickMark> XTicks { get; set; }       // Resolved tick positions + labels
    public List<TickMark> YTicks { get; set; }
    public double TickLabelRotation { get; set; }    // 0, 45, or 90 degrees
    public LegendLayout Legend { get; set; }          // Resolved legend item positions
    public List<DataLabelPosition> DataLabels { get; set; } // Collision-free label positions
}
```

### Anti-Collision Strategies

**1. Axis Tick Labels**
The most common collision. Strategy cascade (each step tried in order):
1. **Ideal ticks** — calculate nice round numbers (Wilkinson's algorithm) that fit with spacing
2. **Reduce tick count** — if labels overlap, halve the number of ticks and recalculate
3. **Rotate labels** — 0° → 45° → 90° rotation, recalculate space needed
4. **Abbreviate** — "January" → "Jan" → "J"; "1,000,000" → "1M"
5. **Skip labels** — show every Nth label, keep all tick marks
6. **Last resort** — show only first, middle, and last labels

Text width estimation without DOM:
```csharp
// Approximate: average char width × character count × font size factor
// Calibrated per font family. Not pixel-perfect but sufficient for layout math.
public static double EstimateTextWidth(string text, double fontSize)
{
    const double avgCharWidth = 0.6; // ratio to fontSize for sans-serif
    return text.Length * fontSize * avgCharWidth;
}
```

Time axis special behavior:
- < 7 days visible → show day names ("Mon", "Tue")
- < 3 months → show "Jan 15", "Feb 1"
- < 2 years → show "Jan", "Feb", "Mar"
- < 10 years → show "2024", "2025"
- 10+ years → show "2020", "2025", "2030"

**2. Data Labels on Points/Bars**
- Calculate bounding box for each label at its ideal position (centered above point)
- Run pairwise AABB overlap detection
- Collision resolution priority:
  1. Nudge vertically (alternate above/below)
  2. Nudge horizontally (offset left/right)
  3. If still overlapping, hide the lower-value label
  4. At high density, switch to "hover-only" labels (show on tooltip instead)
- Maximum label density: no more than 1 label per 40px of horizontal space

**3. Pie/Donut Labels**
Pie labels are the hardest. Strategy:
- **Inside labels** — for slices > 10% of total, place label inside the slice
- **Outside labels** — for small slices, use leader lines
- **Two-column layout** — labels go in sorted vertical columns on left/right sides
- **Minimum slice angle** — slices below 2° get grouped into "Other"
- **Leader line routing** — lines route around other slices, never cross each other

**4. Legend**
Adaptive layout based on available space:
- **Horizontal (default)** — items in a row, if total width < chart width
- **Horizontal wrapped** — wraps to 2-3 rows if too wide
- **Vertical** — switch to vertical stack if > 6-8 items
- **Truncated** — if > 12 items, show first 10 + "+N more" with expand on click
- **Repositioned** — if legend at bottom would make chart too short, move to right side

**5. Responsive Breakpoints**
Not just CSS scaling — the layout engine makes **structural decisions** per width:

| Width | Ticks | Labels | Legend | Grid | Axis Titles | Data Labels |
|---|---|---|---|---|---|---|
| > 600px | Full | Horizontal | Below/right | Both | Yes | Yes |
| 400-600px | Reduced | Rotated 45° | Below, compact | Horizontal only | No | Hover only |
| 300-400px | Minimal | Rotated 90° | Hidden (tooltip) | None | No | No |
| < 300px | First/last only | Abbreviated | Hidden | None | No | No |

**6. Margin Auto-Calculation**
Margins are never hardcoded. The layout engine calculates them:
```
Left margin   = Y-axis label width + Y-axis title height + padding
Bottom margin = X-axis label height (accounting for rotation) + X-axis title height + padding
Top margin    = chart title height + padding
Right margin  = legend width (if positioned right) + padding
```
The plot area is whatever remains after margins. If margins would consume > 50% of chart area, the engine starts dropping elements (titles first, then reduces tick labels).

**7. Collision Detection Algorithm**
Pure C# AABB (Axis-Aligned Bounding Box):
```csharp
public static bool Overlaps(LabelBox a, LabelBox b, double padding = 2)
{
    return a.X < b.X + b.Width + padding
        && a.X + a.Width + padding > b.X
        && a.Y < b.Y + b.Height + padding
        && a.Y + a.Height + padding > b.Y;
}
```
Runs in O(n²) for labels — fine for the typical case (< 100 labels). For data labels on scatter charts with many points, we use a spatial grid for O(n) average case.

### Why This Matters
- **Every test includes layout assertions** — tick counts, label rotation, margin values
- **Layout engine is pure C#** — no DOM, no JS, fully unit testable
- **Deterministic** — same input always produces same layout (important for snapshot testing)
- **Runs before render** — chart components just draw what the engine tells them, no layout logic in Razor files

---

## 6. Animation System

Animations are core infrastructure, not a nice-to-have. Every chart element animates.

### On-Load Animations
| Element | Animation | Technique |
|---|---|---|
| Line | Draws itself left-to-right | SVG `stroke-dashoffset` transition |
| Area | Fades in after line draws | CSS `opacity` transition with delay |
| Bar | Grows from baseline (zero) | CSS `transform: scaleY(0→1)` with `transform-origin: bottom` |
| Pie slice | Sweeps in clockwise | SVG `stroke-dashoffset` on arc path |
| Donut | Sweeps in + center value counts up | Dash + JS counter |
| Scatter points | Scale in with stagger | CSS `transform: scale(0→1)` with staggered `animation-delay` |
| Axes/labels | Fade in | CSS `opacity` transition |
| Legend | Fade in after chart | CSS `opacity` with delay |

### Hover/Interaction Animations
| Interaction | Animation |
|---|---|
| Point hover | Scale up 1.5x, drop shadow appears |
| Bar hover | Lighten fill, subtle lift (translateY -2px) |
| Pie slice hover | Explode outward (translate along angle) |
| Series hover | Highlight series, dim all others to 30% opacity |
| Legend item hover | Highlight corresponding series |
| Tooltip appear | Fade in + slight translateY |

### Data Update Animations
When data changes, elements **morph** — they don't destroy and recreate:
- Lines: path morphs from old coordinates to new (interpolate `d` attribute)
- Bars: height/width transitions to new value
- Pie: slices sweep to new angles
- Points: translate to new positions
- New points: scale in from zero
- Removed points: scale out to zero, then remove from DOM

### Animation Configuration
```razor
<HelixLineChart Data="@data" Height="300"
                AnimateOnLoad="true"
                AnimationDuration="600"
                AnimationEasing="ease-out"
                AnimateUpdates="true">
```

`prefers-reduced-motion: reduce` → all durations set to 0, no motion.

### JS Interop Module (`chart-interop.js`)
Required for:
- `ResizeObserver` for responsive container
- Tooltip positioning (getBoundingClientRect, viewport bounds)
- Pan: mousedown → mousemove → mouseup tracking
- Zoom: wheel event with delta, pinch-to-zoom on touch
- Path morphing orchestration for data updates
- Counter animation for KPI values

Estimated size: ~10-15KB minified (single module, tree-shakeable per chart type).

---

## 7. Pan, Zoom & Drill-Down

### Data Viewport
Charts operate on a **viewport window** into the full dataset:

```csharp
public class DataViewport
{
    public double XMin { get; set; }   // Left edge of visible range
    public double XMax { get; set; }   // Right edge of visible range
    public double YMin { get; set; }   // Bottom edge
    public double YMax { get; set; }   // Top edge
    public bool AutoFitY { get; set; } // Auto-adjust Y to fit visible data
}
```

### Zoom
- **Mouse wheel** — zoom in/out centered on cursor position
- **Pinch gesture** — two-finger zoom on touch devices
- **Zoom toolbar** — +/- buttons, reset to fit all data
- **Box zoom** — drag to select a rectangular area to zoom into
- Zoom has min/max limits (can't zoom past individual data points or beyond full dataset)

### Pan
- **Click-drag** — pan the viewport in any direction
- **Touch drag** — single finger pan on touch devices
- **Keyboard** — arrow keys pan, +/- keys zoom
- Momentum/inertia scrolling on touch (deceleration after release)
- Pan is bounded — can't scroll past the data range

### Drill-Down
Click on a bar/point/slice fires `OnDrillDown` with context:
```csharp
public class DrillDownContext<T>
{
    public T DataItem { get; set; }         // The clicked data item
    public string? SeriesName { get; set; } // Which series was clicked
    public DataViewport Viewport { get; set; } // Current visible range
}
```

### API
```razor
<HelixLineChart Data="@data" Height="300"
                EnableZoom="true"
                EnablePan="true"
                ZoomMode="X"
                MinZoomRange="7"
                OnViewportChanged="HandleViewportChange"
                OnDrillDown="HandleDrillDown">
```

`ZoomMode`: `X` (horizontal only), `Y` (vertical only), `XY` (both), `None`.

---

## 8. Live Data & Incremental Updates

### The Problem
Naive approach: replace `Data` property → Blazor diffs entire SVG → re-renders everything → janky.

### The Solution: Incremental Update API

```csharp
// Instead of replacing the entire data list:
chartRef.AppendPoint(new SalesRecord("Apr", 67000, 50000));
chartRef.RemoveFirst();  // sliding window

// Or batch update:
chartRef.UpdateData(newData, animateTransition: true);
```

### How It Works Internally
1. Each data point's SVG element uses `@key="dataPoint.Id"` so Blazor's diffing matches by identity
2. `AppendPoint` adds one new keyed element — Blazor inserts it, doesn't re-render siblings
3. `RemoveFirst` removes the first keyed element — Blazor removes just that node
4. Path elements (`<path d="...">`) update their `d` attribute — Blazor patches the single attribute
5. The viewport shifts to include the new point (if in follow mode)

### Sliding Window Mode
For streaming data (sensor feeds, stock tickers):
```razor
<HelixLineChart @ref="chartRef"
                Data="@data"
                Height="300"
                SlidingWindow="100"
                FollowLatest="true">
```
- `SlidingWindow="100"` — only render the latest 100 points, discard older ones from DOM
- `FollowLatest="true"` — viewport auto-scrolls to show newest data
- Old SVG elements are removed from the DOM (virtualized out)
- New elements animate in from the right edge

### SignalR Integration Pattern
```csharp
hubConnection.On<SensorReading>("NewReading", reading =>
{
    InvokeAsync(() => chartRef.AppendPoint(reading));
});
```

### Performance Budget
| Scenario | Target |
|---|---|
| Initial render, 1,000 points | < 100ms |
| Initial render, 10,000 points (downsampled) | < 200ms |
| Append 1 point to live chart | < 16ms (60fps) |
| Sliding window shift (remove 1, add 1) | < 16ms |
| Pan/zoom viewport change | < 16ms |
| Full data replacement, 1,000 points with animation | < 300ms |

---

## 9. Testing Strategy for Layout Collisions

### Philosophy
Layout bugs are the most common charting issue. Our test suite must **prevent them by construction**.

### Test Categories

**A. Tick Label Collision Tests**
```csharp
[Theory]
[InlineData(800, 12, 0)]     // Wide chart, 12 months — no rotation needed
[InlineData(400, 12, 45)]    // Medium chart, 12 months — rotates to 45°
[InlineData(200, 12, 90)]    // Narrow chart, 12 months — rotates to 90°
[InlineData(300, 30, 90)]    // Narrow + many ticks — rotation + reduced count
public void TickLabels_NeverOverlap(int width, int tickCount, int expectedRotation)
{
    var result = layoutEngine.Calculate(new ChartLayoutInput
    {
        Width = width,
        TickLabels = GenerateMonthLabels(tickCount)
    });

    AssertNoOverlaps(result.XTicks);
    result.TickLabelRotation.Should().Be(expectedRotation);
}
```

**B. Margin Adequacy Tests**
```csharp
[Fact]
public void LongLabels_ExpandMargins_PlotAreaNeverNegative()
{
    var labels = new[] { "Very Long Category Name A", "Another Extremely Long Name B" };
    var result = layoutEngine.Calculate(input with { TickLabels = labels, Width = 300 });

    result.PlotArea.Width.Should().BeGreaterThan(50); // Usable area remains
    result.Margins.Left.Should().BeGreaterThan(0);
}
```

**C. Legend Overflow Tests**
```csharp
[Theory]
[InlineData(5, "horizontal")]   // Few items — horizontal
[InlineData(10, "wrapped")]     // Many items — wrapped rows
[InlineData(20, "truncated")]   // Too many — truncated with "+N more"
public void Legend_AdaptsToItemCount(int seriesCount, string expectedLayout)
{
    var result = layoutEngine.Calculate(input with
    {
        Series = GenerateSeries(seriesCount),
        Width = 600
    });

    result.Legend.LayoutMode.Should().Be(expectedLayout);
}
```

**D. Pie Label Tests**
```csharp
[Fact]
public void PieLabels_SmallSlices_GroupedIntoOther()
{
    var slices = new[] { 50.0, 30.0, 10.0, 5.0, 2.0, 1.5, 0.8, 0.7 };
    var result = layoutEngine.CalculatePieLayout(slices, radius: 150);

    result.Slices.Count(s => s.Label != "Other").Should().BeLessThan(slices.Length);
    result.Slices.Should().Contain(s => s.Label == "Other");
}

[Fact]
public void PieLabels_LeaderLines_NeverCross()
{
    var slices = new[] { 20.0, 20.0, 15.0, 15.0, 10.0, 10.0, 5.0, 5.0 };
    var result = layoutEngine.CalculatePieLayout(slices, radius: 150);

    AssertNoLeaderLineCrossings(result.LeaderLines);
}
```

**E. Responsive Breakpoint Tests**
```csharp
[Theory]
[InlineData(800, true, true, true, true)]   // Wide: all elements shown
[InlineData(400, true, false, true, false)]  // Medium: no axis titles, no data labels
[InlineData(250, false, false, false, false)] // Narrow: minimal
public void Responsive_ElementVisibility(int width, bool showLegend, bool showAxisTitles, bool showGridLines, bool showDataLabels)
{
    var result = layoutEngine.Calculate(input with { Width = width });

    result.ShowLegend.Should().Be(showLegend);
    result.ShowAxisTitles.Should().Be(showAxisTitles);
    result.ShowGridLines.Should().Be(showGridLines);
    result.ShowDataLabels.Should().Be(showDataLabels);
}
```

**F. Fuzz Testing**
Generate random chart configurations and assert invariants always hold:
```csharp
[Fact]
public void FuzzTest_NoOverlaps_AtAnySize()
{
    var random = new Random(42); // Deterministic seed
    for (var i = 0; i < 1000; i++)
    {
        var width = random.Next(150, 1200);
        var tickCount = random.Next(2, 50);
        var seriesCount = random.Next(1, 15);

        var result = layoutEngine.Calculate(GenerateRandomInput(random, width, tickCount, seriesCount));

        AssertNoOverlaps(result.XTicks);
        AssertNoOverlaps(result.YTicks);
        Assert.True(result.PlotArea.Width > 0);
        Assert.True(result.PlotArea.Height > 0);
    }
}
```

**G. Animation Tests**
```csharp
[Fact]
public void ReducedMotion_AllDurationsZero()
{
    var config = new AnimationConfig { ReducedMotion = true };
    config.OnLoadDuration.Should().Be(0);
    config.UpdateDuration.Should().Be(0);
    config.HoverDuration.Should().Be(0);
}

[Fact]
public void IncrementalUpdate_OnlyNewElementsAdded()
{
    // Render with 5 points, then add 1
    // Assert only 1 new SVG element created, existing 5 unchanged
}
```

### Test Helpers
```csharp
static void AssertNoOverlaps(List<TickMark> ticks)
{
    for (int i = 0; i < ticks.Count - 1; i++)
    {
        var a = ticks[i].BoundingBox;
        var b = ticks[i + 1].BoundingBox;
        Assert.False(Overlaps(a, b),
            $"Tick '{ticks[i].Label}' at {a} overlaps '{ticks[i+1].Label}' at {b}");
    }
}
```

---

## 10. Feature Requirements by Phase

### Phase 1 — Core Engine + 4 Chart Types
*Ship the foundation and most-used charts.*

**Layout Engine (the foundation):**
- [ ] ChartLayoutEngine with full collision detection pipeline
- [ ] Text width estimation (sans-serif calibrated)
- [ ] Smart tick generator (Wilkinson's algorithm for nice numbers)
- [ ] Tick label collision cascade (reduce → rotate → abbreviate → skip)
- [ ] Data label AABB collision detection and resolution
- [ ] Legend adaptive layout (horizontal → wrapped → vertical → truncated)
- [ ] Responsive breakpoint system (4 tiers)
- [ ] Auto-margin calculation from resolved label sizes
- [ ] Pie label two-column layout with leader lines

**SVG Rendering Engine:**
- [ ] Linear scale (data range → pixel range mapping)
- [ ] Band scale (category-based for bar charts)
- [ ] Time scale (DateTime mapping)
- [ ] Axis renderer (ticks, labels, gridlines — positions from layout engine)
- [ ] Responsive container (resize observer via minimal JS)
- [ ] Color palette system integrated with HelixUI.Theme tokens
- [ ] 10+ built-in palettes (Default, Cool, Warm, Monochrome, Pastel, etc.)
- [ ] Data downsampling (LTTB algorithm) for datasets > 1,000 points

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

**Animation (core, not optional):**
- [ ] On-load animations: line draw, bar grow, pie sweep, scatter scale-in
- [ ] Hover animations: point scale, bar lighten, pie explode, series dim-others
- [ ] Data update morphing: path interpolation, bar height transition, point translate
- [ ] Animation config: duration, easing, enable/disable, stagger delay
- [ ] `prefers-reduced-motion` → all durations 0

**Interactivity:**
- [ ] Hover highlight (series/point)
- [ ] Click events (OnPointClick, OnSeriesClick)
- [ ] Tooltip with smart positioning (follows cursor, stays in bounds)
- [ ] Legend toggle (click to show/hide series)
- [ ] Pan (click-drag, touch, keyboard arrows)
- [ ] Zoom (mouse wheel, pinch, box select, +/- buttons)
- [ ] DataViewport windowing (XMin/XMax/YMin/YMax)
- [ ] Zoom mode: X, Y, XY, None

**Live Data & Incremental Updates:**
- [ ] `AppendPoint` / `RemoveFirst` API for streaming data
- [ ] `@key` on all data point SVG elements for efficient Blazor diffing
- [ ] Sliding window mode (keep N latest points, virtualize older ones out of DOM)
- [ ] `FollowLatest` mode for auto-scrolling to newest data
- [ ] Batch `UpdateData` with animated transitions
- [ ] < 16ms per incremental update (60fps target)

**JS Interop Module (~10-15KB):**
- [ ] ResizeObserver for responsive container
- [ ] Tooltip getBoundingClientRect positioning
- [ ] Pan/zoom mouse + touch event handling
- [ ] Path morph orchestration for data updates
- [ ] Counter animation for KPI values

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

## Non-Functional Requirements

- **SVG rendering in C#** — core chart markup generated without JS
- **JS interop module** — ~10-15KB for resize, tooltips, pan/zoom, animations
- **Render mode agnostic** — Server, WASM, Auto; static SSR renders non-interactive SVG
- **Multi-target** — net5.0 through net9.0
- **Performance:**
  - Initial render, 1,000 points: < 100ms
  - Initial render, 10,000 points (downsampled): < 200ms
  - Incremental update (1 point): < 16ms (60fps)
  - Pan/zoom viewport change: < 16ms
  - Full data replacement with animation: < 300ms
- **Bundle** — < 30KB CSS, < 15KB JS (before gzip)
- **WCAG 2.1 AA** — all chart types
- **Theme integration** — all colors from `--helix-*` CSS custom properties
- **Test coverage** — ≥ 80% unit tests, mandatory layout collision fuzz tests
- **No label overlaps guaranteed** — layout engine invariant, tested with 1,000+ random configs

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

## Open Questions

1. **Curve interpolation:** Ship catmull-rom (smooth), monotone, linear, step — or just linear + smooth?
2. **Path morphing for data updates:** CSS can't animate SVG `d` attribute. Use Web Animations API (JS) or SMIL `<animate>` (deprecated but supported)?
3. **Data table for accessibility:** Render as hidden `<table>` inside SVG, or as a sibling element?
4. **Zoom/pan state:** Should viewport state be managed internally or exposed as a bindable parameter for URL persistence?
5. **Live data backpressure:** If data arrives faster than 60fps, buffer and batch? Or drop intermediate frames?

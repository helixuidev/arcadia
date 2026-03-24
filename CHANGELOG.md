# Changelog

## 1.0.0-beta.4.1 (2026-03-24) — Post-review fixes

### Bug Fixes
- Fix `Loading` property — now applies `arcadia-chart--loading` shimmer CSS class
- Fix `YValue` annotations — draws horizontal reference lines at Y position (was always vertical)
- Fix pan/zoom listener leak — `mousemove`/`mouseup` handlers properly removed in `disablePanZoom()`
- Fix NaN in screen reader tables — shows "—" instead of "NaN"
- Add `ShowToolbar` parameter (default `true`) to opt out of export toolbar
- Add `NoDataState` component — all 14 charts show "No data available" when empty
- All CSS variable references now have hardcoded fallbacks (pie, rose, funnel, treemap, etc.)
- Fix stale HelixUI path references in all CSS file comments

### Known API Inconsistencies (will be addressed in 1.0.0)
- `ArcadiaGaugeChart.Thresholds` uses `List<GaugeThreshold>` while `ArcadiaProgressBar.Thresholds` uses `IReadOnlyList<(double, string)>` — will be unified
- `OnPointClick` returns `EventCallback<T>` (item) while `OnSeriesClick` returns `EventCallback<int>` (index) — will add a unified `PointClickEventArgs`
- `TooltipTemplate` parameter exists but is not documented — it requires JS interop and only works in interactive render modes

## 1.0.0-beta.4 (2026-03-24)

### Breaking Changes
- **All component names renamed from `Helix*` to `Arcadia*`**
  - `HelixLineChart` → `ArcadiaLineChart`
  - `HelixBarChart` → `ArcadiaBarChart`
  - `HelixPieChart` → `ArcadiaPieChart`
  - `HelixScatterChart` → `ArcadiaScatterChart`
  - `HelixCandlestickChart` → `ArcadiaCandlestickChart`
  - `HelixRadarChart` → `ArcadiaRadarChart`
  - `HelixGaugeChart` → `ArcadiaGaugeChart`
  - `HelixHeatmap` → `ArcadiaHeatmap`
  - `HelixFunnelChart` → `ArcadiaFunnelChart`
  - `HelixTreemapChart` → `ArcadiaTreemapChart`
  - `HelixWaterfallChart` → `ArcadiaWaterfallChart`
  - `HelixRoseChart` → `ArcadiaRoseChart`
  - `HelixKpiCard` → `ArcadiaKpiCard`
  - `HelixSparkline` → `ArcadiaSparkline`
  - `HelixProgressBar` → `ArcadiaProgressBar`
  - `HelixFormBuilder` → `ArcadiaFormBuilder`
  - `HelixWizard` → `ArcadiaWizard`
  - `HelixToastContainer` → `ArcadiaToastContainer`
  - `HelixComponentBase` → `ArcadiaComponentBase`
  - `IHelixTheme` → `IArcadiaTheme`
- Theme CSS renamed from `helix.css` to `arcadia.css`:
  ```diff
  - <link href="_content/Arcadia.Theme/css/helix.css" rel="stylesheet" />
  + <link href="_content/Arcadia.Theme/css/arcadia.css" rel="stylesheet" />
  ```

### New Features
- **Rose/Polar area chart** (`ArcadiaRoseChart`)
- **Box Plot chart** (`ArcadiaBoxPlot`)
- **Range Area chart** (`ArcadiaRangeAreaChart`)
- **Export toolbar** on all 14 chart types (PNG/SVG download on hover)
- **Pan & Zoom** parameters (`EnableZoom`, `EnablePan`, `ZoomMode`)
- **Crosshair** tracking (`ShowCrosshair`)
- **Annotations** for marking data points (`Annotations` parameter)
- **Streaming data** with slide animation (`AppendAndSlide`, `SlidingWindow`)
- **Click events** wired on Bar, Pie, Line, Scatter (`OnPointClick`, `OnSliceClick`)
- **ShowToolbar** parameter to opt out of export toolbar
- **MCP server** for AI-assisted chart code generation
- **IDE snippet packs** for Visual Studio and JetBrains Rider
- **Empty data state** — charts show "No data available" when Data is empty
- **CSS variable fallbacks** — charts render correctly even without theme CSS loaded

### Bug Fixes
- Fix Blazor circuit crash on rapid tab switching (proper `IAsyncDisposable`)
- Fix heatmap/radar/pie/treemap rendering off-center (use `EffectiveWidth`)
- Fix candlestick label overlap (layout engine index-based positioning)
- Fix gauge animation (dynamic `stroke-dasharray` from arc path length)
- Fix export downloading toolbar icon instead of chart SVG
- Fix NaN values showing in screen reader tables (now shows "—")
- Fix gauge track bleed-through (thinner track with lower opacity)
- Fix label contrast in dark/light themes (`currentColor` with opacity)
- Fix widget contrast (KPI cards, progress bars adapt to theme)

### Internal
- 20+ implementation types changed from `public` to `internal`
- 102 Playwright E2E visual regression tests
- 565 bUnit unit tests
- CI workflow with E2E job and documentation check

## 1.0.0-beta.3 (2026-03-23)

### Features
- Smooth curves (Catmull-Rom interpolation)
- Real-time streaming API
- Combo charts (bar + line on same axes)
- Stacked area charts
- Trendlines (linear and moving average)

## 1.0.0-beta.2 (2026-03-22)

### Features
- Initial 8 chart types
- Dashboard widgets
- Theme engine
- Form Builder
- Toast notifications

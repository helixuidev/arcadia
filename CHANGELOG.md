# Changelog

All notable changes to Arcadia Controls are documented here. This project uses [Keep a Changelog](https://keepachangelog.com/) format and [Semantic Versioning](https://semver.org/).

## [1.0.0-beta.11] — 2026-03-26

### Fixed
- **Filter hash overflow** — multi-column filter cache hash used `Sum()` of `GetHashCode()` values, causing `OverflowException` with certain filter value combinations. Replaced with `unchecked` XOR aggregate.

### Changed
- **13 DataGrid documentation pages** — 3 new pages (state persistence, batch editing, context menu) plus multi-sort, quick filter, clipboard copy, and operator dropdown sections added to existing pages
- **Comprehensive CHANGELOG** — beta.10 entry expanded with all 17 features listed by name

## [1.0.0-beta.10] — 2026-03-26

### Added
- **Arcadia.DataGrid on NuGet** — first public release of the DataGrid package
- **`Property` parameter** on ArcadiaColumn — string-based column binding without boxing (`Property="Salary"` instead of `Field="@(e => (object)e.Salary)"`)
- **Multi-column sort** — Shift+Click to add secondary/tertiary sort columns with numbered priority badges on headers
- **Quick filter** — toolbar search input that filters across all visible columns (`ShowToolbar="true"`, `QuickFilter` parameter)
- **Clipboard copy** — Ctrl+C (Cmd+C) copies selected rows as TSV; if no selection, copies all visible rows with headers
- **Filter operator dropdown** — per-column filter operator selection (Contains, Equals, StartsWith, EndsWith, NotEquals, GreaterThan, LessThan, IsEmpty, IsNotEmpty)
- **Batch editing** — `BatchEdit="true"` enables change tracking with Save/Discard toolbar buttons and `OnBatchCommit` callback
- **Context menu** — `ContextMenuTemplate` for custom right-click menus, `OnContextMenu` callback
- **State persistence** — `StateKey` parameter auto-saves sort/filter/page/column state to localStorage; `OnStateChanged` callback for server-side persistence
- **`SelectionMode` enum** — `None`/`SingleRow`/`Multiple` as cleaner alternative to separate `Selectable`/`MultiSelect` booleans
- **`EmptyTemplate`** — custom RenderFragment for empty state content
- **4 financial indicators** — SMA, EMA, Bollinger Bands, RSI in `Arcadia.Charts.Core.Indicators`
- **`SyncGroup` parameter** — link charts together for synchronized crosshair and zoom
- **Rubber band zoom** — `ZoomMode="selection"` for click-drag region zoom
- **`OnPrint` callback** — print-optimized chart rendering
- **973 unit tests** — 162 DataGrid tests + 136 FormBuilder field type tests added (was 669)
- **13 DataGrid documentation pages** — overview, sorting, filtering, selection, editing, grouping, templates, virtual scrolling, themes, export, state persistence, batch editing, context menu
- **DataGrid in demo gallery** — showcases in both Server and WASM demos (in sync)
- **DataGrid on homepage** — feature card, pricing tiers, FAQ, JSON-LD structured data

### Fixed
- **DataGrid `Key` parameter** — was a computed property without `[Parameter]`, causing `InvalidOperationException` when set in Razor. Now a proper parameter with auto-generation fallback.
- **DataGrid SSR blank render** — grid rendered zero data during server-side prerendering because column collection was gated on `OnAfterRender`. Columns now render on first pass.
- **Delta indicator contrast** — replaced `opacity: 0.6` with explicit muted color for WCAG AA compliance
- **Sparkline missing alt text** — auto-generates `aria-label` with data range when none provided
- **Empty table headers in DataGrid** — added `aria-label` and sr-only text to utility columns
- **DataGrid missing from solution file** — caused CI pack to fail with NU5026
- **DataGrid missing from CI/release workflows** — pack step now includes all 6 packages

## [1.0.0-beta.9] — 2026-03-26

### Added
- **ArcadiaDataGrid** — full-featured data grid component with sorting, filtering, paging, row selection, master-detail, grouping, cell editing, CSV export, column picker, column resize, column reorder (drag-drop), frozen columns, row selector, keyboard navigation, ARIA live regions, and virtual scrolling (10K+ rows via Blazor Virtualize)
- **6 DataGrid themes** — Obsidian, Vapor, Carbon, Aurora, Slate, Midnight (each with distinct visual identity)
- **3 density modes** — Compact, Default, Relaxed
- **6 unified named themes** in Arcadia.Theme — Obsidian, Vapor, Carbon, Aurora, Slate, Midnight (C# classes + CSS, apply to Charts + DataGrid + all components)
- **DateTime X-axis** (`XAxisType="time"`) — continuous time-proportional positioning with auto-format tick labels
- **Secondary Y-axis** — dual-axis charts via `YAxisIndex` on SeriesConfig, with right-side labels and scale
- **Logarithmic Y-axis** (`YAxisType="log"`) — log10 scale with power-of-10 ticks
- **Interactive playground** at `/playground-builder` — live parameter editor with code generation
- **Performance benchmarks** — 8 tests measuring render time (LineChart 10K pts: 87ms)
- **Stress tests** — 18 tests for null, empty, NaN, extreme values, circular refs, special chars
- **30+ configurable visual parameters** across all 16 charts (opacity, stroke, colors, radii)

### Fixed
- Animation opacity override on Sankey, Chord, Range Area, Radar (from-only keyframes)
- Animation blink (backwards fill mode for delayed animations)
- LogScaleAdapter method hiding (virtual + override instead of new)
- Pan/zoom JS event listener leak in disposal
- ResizeObserver callback after disposal race condition
- TimeScale invert precision (Math.Round)
- Chord label angular wraparound
- LineChart null guard on _yScale (+ 6 other charts)
- SSR disposal exception (catch InvalidOperationException)
- Vapor theme contrast (readable text on dark backgrounds)
- Double scrollbar in virtual scroll mode
- 8 doc page mismatches (wrong defaults, missing params, wrong types)

### Changed
- .editorconfig with 20+ Roslyn analyzer rules, zero errors across all projects
- 100% XML doc coverage: 329 parameters across Charts, DataGrid, FormBuilder all documented
- Dead code purged: 4 unused methods, 5 unused CSS classes, 1 dead keyframe
- All bare catch blocks replaced with typed exceptions + comments
- CSS duplicates removed, split declarations consolidated
- Gallery defaults to dark theme with gradient title
- Test count: 677 (was 628)
- Bundle size: 452 KB total (382 KB DLLs + 59 KB CSS + 11 KB JS)

## [1.0.0-beta.8] — 2026-03-24

### Added
- **Sankey Diagram** (`ArcadiaSankeyChart`) — flow visualization with topological column layout, 4+ level support, cycle handling, input validation (negative values, self-links, duplicates)
- **Chord Diagram** (`ArcadiaChordChart`) — circular relationship visualization with arc ring, quadratic bezier ribbon paths, rotated labels, hover highlight, small-arc label suppression
- **30+ configurable visual parameters** across all 16 charts:
  - ChartBase: `GridOpacity`, `AxisLineOpacity`, `PointRadius`
  - Pie/Rose: `SliceStrokeWidth`, `SliceStrokeColor`
  - Treemap: `CellStrokeWidth`, `CellStrokeColor`, `CellLabelColor`
  - Funnel: `StageOpacity`, `StageLabelColor`
  - Scatter: `PointOpacity`, `TrendlineOpacity`
  - Candlestick: `WickWidth`, `CandleWidthRatio`
  - BoxPlot: `BoxFillOpacity`, `WhiskerOpacity`, `MedianLineColor`, `MedianLineWidth`
  - Waterfall: `ConnectorOpacity`, `ConnectorDashPattern`
  - RangeArea: `LineStrokeWidth`
  - Gauge: `TrackOpacity`
  - Heatmap: `CellGap`
  - Radar: `GridRingOpacity`, `HoverDimOpacity`, `HoverFillOpacity`
  - Sankey: `LinkOpacity`, `LinkHoverOpacity`
  - Chord: `ChordOpacity`, `ChordHoverOpacity`, `MinLabelAngle`
- Radar chart hover-dim effect (hovering one series dims all others)
- Chord chart hover highlight (ribbons at 0.75 default, 1.0 on hover)
- Blog post screenshots for dashboard tutorial
- 63 new unit tests (35 Sankey + 28 Chord), 18 new E2E tests with visual baselines

### Fixed
- **Animation opacity override** — Sankey, Chord, Range Area, and Radar fill animations no longer snap to `opacity: 1` after completion. Uses `from`-only keyframes respecting inline opacity.
- Gallery defaults to dark theme with gradient title shading

### Changed
- Community edition locked to 4 charts (Line, Bar, Pie, Scatter). Funnel, Treemap, and Waterfall moved to Pro tier.
- Total chart count: 14 → 16

## [1.0.0-beta.7] — 2026-03-24

### Added
- Custom tooltip templates (`TooltipTemplate` parameter) — render arbitrary Blazor content on hover
- Roslyn analyzers package (`Arcadia.Analyzers`)
- NuGet package READMEs rewritten

### Fixed
- Responsive charts finalized: `width="100%"` + viewBox scaling → ResizeObserver approach
- WASM playground synced with Server demo
- Deploy workflow fixes

## [1.0.0-beta.6] — 2026-03-23

### Added
- **Responsive width by default** — `Width="0"` fills container correctly

### Fixed
- BlogLayout not reading MDX frontmatter props

## [1.0.0-beta.5] — 2026-03-23

### Fixed
- `Loading` parameter shows skeleton shimmer correctly
- Annotations render at correct data index positions
- ResizeObserver listener leak on dispose
- All known API issues from external review addressed

### Breaking Changes
- `OnPointClick` changed from `EventCallback<T>` to `EventCallback<PointClickEventArgs<T>>` — now includes `Item`, `DataIndex`, `SeriesIndex`

## [1.0.0-beta.4] — 2026-03-22

### Added
- **Box Plot** (`ArcadiaBoxPlot<T>`) — statistical distribution chart
- **Range Area** (`ArcadiaRangeAreaChart<T>`) — confidence intervals and min/max bands
- Click events (`OnPointClick`) on Bar, Pie, Line, Scatter

### Fixed
- Empty state: all charts show `NoDataState` when data is null/empty
- NaN in screen reader tables replaced with "—"
- CSS variable fallbacks on all color references

### Breaking Changes
- `helix.css` renamed to `arcadia.css` — update `<link>` tags

## [1.0.0-beta.3] — 2026-03-22

### Added
- **Helix → Arcadia rename** across all components, namespaces, CSS, packages
- License key validation (`ArcadiaLicense.SetKey()`)
- Community watermark on Pro components
- WASM playground at `/playground/`
- MCP server for AI-assisted code generation
- IDE snippets (Visual Studio + JetBrains Rider)
- Rose/Polar chart, stacked area, combo charts
- Annotations, crosshair, export toolbar
- Interactive legend toggle
- Streaming data with `AppendAndSlide`
- Playwright E2E tests

### Breaking Changes
- **All names changed**: `HelixKpiCard` → `ArcadiaKpiCard`, `HelixLineChart` → `ArcadiaLineChart`, etc.
- **All CSS renamed**: `helix-theme.css` → `arcadia-theme.css`, `helix-charts.css` → `arcadia-charts.css`
- **All namespaces**: `HelixUI.*` → `Arcadia.*`
- **NuGet packages**: `HelixUI.Charts` → `Arcadia.Charts`, etc.

## [1.0.0-beta.2] — 2026-03-21

### Added
- Candlestick (OHLC) chart
- On-load animations for all chart types
- 30+ parameters on `ChartBase<T>` (axes, grid, margins, pan/zoom, crosshair)
- Pro charts: Radar, Gauge, Heatmap, Funnel, Treemap, Waterfall
- Trendlines (linear regression, moving average)
- Stacked bar mode
- Anti-collision layout engine
- 38 bUnit unit tests

## [1.0.0-beta.1] — 2026-03-20

### Added
- Initial release
- **Core**: base classes, CSS builder, focus trap, accessibility utilities
- **Theme**: design tokens, CSS custom properties, light/dark themes, Tailwind plugin
- **Charts**: Line, Bar, Pie, Scatter + 7 dashboard widgets
- **Form Builder**: 21 field types, schema-driven, wizard mode, validation
- **Notifications**: toast system with auto-dismiss and stacking
- Multi-target: .NET 5–10
- Server, WebAssembly, and Auto render mode support

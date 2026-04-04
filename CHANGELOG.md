# Changelog

All notable changes to Arcadia Controls are documented here. This project uses [Keep a Changelog](https://keepachangelog.com/) format and [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Fixed
- **Chart `ObservableCollection<T>` feature was silently broken** — `ChartBase.OnParametersSet` created the `CollectionObserver`, but 18 derived chart classes override `OnParametersSet` without calling `base`. Moved setup to `SetParametersAsync` so the observer is always created regardless of derived overrides. Live-data chart updates now actually work.
- **Screen-reader data tables leaked locale-dependent dates** — `<td>@XField(item)</td>` called `DateTime.ToString()` in the current culture, producing `"1/1/2026"` for en-US users. Added `FormatSrX` helper on `ChartBase` that uses invariant culture + `XAxisFormatString`. Applied to Line, Bar, RangeArea, and Scatter chart SR tables.
- Test race: `IdGeneratorTests.Generate_IncrementsSequentially` and `Reset_RestartsCounter` asserted absolute counter values from shared static state; rewritten to assert relative ordering.

### Security
- Added Content-Security-Policy, Strict-Transport-Security, and Permissions-Policy headers to website nginx.conf, Demo.Server middleware, and a new Demo.Wasm `web.config`. Blazor-compatible policy (`'wasm-unsafe-eval'`, `'unsafe-inline'` for styles).

### Accessibility
- **Dialog and Drawer** now wrap content in `<FocusTrap>` with auto-focus, fixing tab-out and missing initial focus.
- **WCAG 2.1 AA contrast fixes** — `--arcadia-color-danger` (#dc2626 → #b91c1c), `--arcadia-color-warning` (#d97706 → #b45309), `--arcadia-color-text-disabled` (#94a3b8 → #6b7280), dark-theme danger (#f87171 → #fca5a5). Updated tokens, themes, C# theme classes, and CSS fallbacks.
- **Arrow key navigation** added to `ArcadiaCommandPalette` and `ArcadiaContextMenu` (+ `aria-activedescendant`, roving `tabindex`, disabled-item skipping via `KeyboardNavigation` utility).

### Code Quality
- Replaced ~30 bare `catch` blocks in `ArcadiaDataGrid` and `ChartBase` with `Debug.WriteLine` diagnostics or explanatory comments for expected framework exceptions.

### Multi-targeting
- Added `net10.0` to `TargetFrameworks` and conditional `PackageReference` (`10.0.0-preview.4.25258.110`) across all 9 source projects. Documentation claims of ".NET 5 through .NET 10" are now backed by reality.

### Documentation
- 37 individual UI component doc pages under `docs/ui/` (parameters extracted from real source).
- 21 form field doc pages under `docs/forms/fields/` (TextField, SelectField, DateField, FileField, RatingField, etc.).
- `DocsLayout.astro` sidebar nav wired up for all new pages.

### Tests
- **+72 new chart tests** covering 10 previously-untested chart types: BoxPlot, Funnel, Treemap, Rose, Waterfall, RangeArea, plus smoke tests for Area/Bubble/Donut/StackedBar wrapper charts.
- 31 new `Arcadia.Gauge` tests, 12 new `Arcadia.Analyzers` tests.
- Added WASM E2E infrastructure (`WasmFixture`, `RenderMode` enum, `RenderModeTestBase`, `DualModeChartTestBase`) so tests can target both Server and WASM demos.
- **Unit test count: 1,453 → 1,525** (+72, +5.0%).

### Repository Hygiene
- Added `Arcadia.Analyzers` to `HelixUI.sln` (was missing).
- Removed unused `FluentValidation` and `Moq` from `Directory.Packages.props`.
- Added `.claude/commands/` slash commands: `/release-checklist`, `/recommend-feature`, `/playground-test`, `/super-test`.

## [1.0.0-beta.20] — 2026-04-02

### New Features
- **ObservableCollection binding** on Charts and DataGrid — auto-rerender on add/remove/clear with 16ms debounce and batch-edit suppression
- **CollectionObserver\<T\>** utility in Core — reusable, thread-safe, with Suppress/Resume API
- **Security practices page** at /security — documents XSS protection, supply chain, disposal patterns
- **Live data demo** at /test/live-data — streaming chart + live DataGrid + gauge, all via ObservableCollection

### Security
- Eliminated `innerHTML` in chart tooltip rendering — replaced with DOMParser + sanitizeHtml()
- Eliminated `document.write` in DataGrid print — replaced with DOM construction API
- Zero unsafe DOM operations remain in codebase

### Fixes
- SEO: all doc pages had empty `<title>` tags (MDX frontmatter not passed to DocsLayout)
- Documentation sync: README, CHANGELOG, CLAUDE.md, all package READMEs updated

### Tests
- 1,426 total (+22 from beta.19): 10 CollectionObserver, 5 Chart observable, 7 DataGrid observable

## [1.0.0-beta.19] — 2026-04-01

### New Packages
- **Arcadia.DashboardKit** — Drag-and-drop dashboard grid with FLIP animations, spring physics, iOS wiggle mode, state persistence

### New Components (23 UI)
- CommandPalette, HoverCard, Popover, ContextMenu, Switch, Slider, Rating, TagInput, ColorPicker, TextArea, Alert, Progress, CircularProgress, Spinner, EmptyState, Chip, Carousel, Pagination, Separator, AspectRatio, ScrollArea

### New Features
- **DataGrid**: Column reorder, stacked headers, infinite scroll, cell tooltips, copy with headers (Ctrl+Shift+C), cell validation, sticky footer, group aggregates, PDF export, command column, inline add row, conditional formatting
- **Card**: Glassmorphism variants (glass/outlined/ghost), 6 elevation levels, gradient border, skeleton loading, collapsible, status bar, horizontal layout
- **Charts**: Animation performance caps (200+ points stagger disabled), accessibility on 4 new chart types
- **ObservableCollection binding**: Live data on Charts and DataGrid with 16ms debounce, batch-edit suppression

### Fixed
- Desktop layout broken by mobile CSS
- WASM playground navigation 404s
- SEO: empty titles on all doc pages (MDX frontmatter not passed to layout)
- Security: eliminated innerHTML and document.write from JS interop

### Infrastructure
- Playground smoke test in CI pipeline
- DashboardKit + Gauge added to release.yml and ci.yml pack steps
- Mobile responsive playground with hamburger menu
- Free Controls section on homepage
- Security practices page at /security
- Railway deployment config (Dockerfile.website)

## [1.0.0-beta.18] — 2026-03-29

### Added
- **Charts GaugeChart parity** with standalone Gauge: needle, ticks, gradient, ranges, editable, custom angles, center template (41 params, was 17)
- **GaugeRange model** for colored arc bands (separate from GaugeThreshold)

### Fixed
- **ShowToolbar default: false** (was true, causing 60+ export buttons per dashboard)
- **Removed watermarks from 16 Pro components** (Charts, Dashboard widgets, DataGrid)
- **Helpful error messages** for Sankey/Heatmap/Candlestick missing fields
- **DataGrid SSR skeleton** auto-shows shimmer when Data is null

### Changed
- Watermarks now ONLY on free community charts (Line, Bar, Pie, Scatter)
- Gauge doc page updated with 6 new feature sections + standalone package callout

## [1.0.0-beta.17] — 2026-03-29

### Added
- **Arcadia.Gauge** free standalone package (MIT) with radial gauge, needle, ticks, gradient arcs, ranges, editable mode
- **14 gauge E2E tests** covering rendering, clipping, needle, ticks, gradients, ranges, visual regression
- **Helpful error messages** for Sankey ("Data/Links missing"), Heatmap ("XField not set"), Candlestick ("OpenField missing")
- **DataGrid SSR skeleton** auto-shows shimmer rows when Data is null (async loading)
- **Replay Animation** button on gauge demo
- **Medium article** "Your Blazor Components Aren't Actually Blazor" published on website + standalone HTML for Medium import

### Fixed
- DataGrid Community watermark removed (Pro component, not community)
- Gauge arc clipping at top of semi-circle
- Gauge needle overlapping center value text
- Playground-builder 404 on deployed WASM (relative href fix)
- Homepage "zero JavaScript" corrected to "<12 KB JavaScript"
- False "free" claims removed from 6 blog posts for Pro components
- Homepage package count: 7 -> 8 (Arcadia.Gauge added)

## [1.0.0-beta.16] — 2026-03-29

### Added
- **7 new UI components**: TreeView, Menu, Drawer, Skeleton, DatePicker, Stepper, Timeline (23 UI components total)
- **4 chart aliases**: Area, Donut, StackedBar, Bubble (20 chart types total)
- **Row reorder** drag-and-drop with `AllowRowReorder` parameter
- **Column resize UX** — wider handle (9px), visible indicator line on hover
- **11 DataGrid battle tests** — resize, reorder, rapid clicks, virtual scroll, edge cases
- **64 new unit tests** for all 7 UI components (1,161 total)
- **6 SEO blog posts** targeting DataGrid, Excel export, Dialog, virtual scroll, forms, tabs
- **Linear gauge doc page** + improved circular gauge SEO
- **XML doc examples** on top 20 parameters with `<example>` and `<remarks>`
- **What's New + Troubleshooting** documentation pages
- **15 blog posts** total (was 9)

### Fixed
- Homepage "zero JavaScript" → "<12 KB JavaScript" (accurate)
- Chart count updated from 16 to 20 everywhere

### Changed
- 86 components total (was 57)
- 20 chart types (was 16)
- 1,161 unit + 251 E2E + 11 perf = **1,423 total tests**

## [1.0.0-beta.15] — 2026-03-29

### Added
- **240 E2E Playwright tests** covering all controls: Charts, DataGrid, Forms, Notifications, UI Components, playground navigation, visual regression
- **8 playground-builder property tests** — width, height, palette, title, grid, legend, data labels, chart type switching
- **Cell focus regression test** — verifies no flash on previous cell when clicking another

### Fixed
- **Chart data labels showed scientific notation** (3.5E+2 instead of 350) — replaced G4 format with N0/N2
- **Width parameter ignored after responsive mode** — `Width > 0` now takes priority over `_measuredWidth`
- **Palette dropdown had no effect** — playground demo series used hardcoded colors overriding the Palette
- **Cell focus flashed previous cell** — removed `@onfocus` race condition, focus set only on cell click
- **Color contrast violations** (WCAG AA) — tick labels, subtitles, KPI footer, gallery text all fixed
- **Watermark marked `aria-hidden`** for accessibility compliance

### Changed
- Test count: 1,097 unit + 240 E2E + 11 performance = **1,348 total tests**
- Homepage stat updated to "1,350+ Tests"

## [1.0.0-beta.14] — 2026-03-28

### Added
- **Excel (XLSX) export** — zero-dependency SpreadsheetML writer, no DocumentFormat.OpenXml needed
- **Published benchmarks page** — 11 benchmarks with real numbers at /docs/benchmarks
- **Boolean + date typed filters** — auto-detects column type via reflection (boolean → tri-state dropdown, date → date picker, number → number input)
- **Column pinning UI** — right-click column header → Pin to Left / Unpin / Hide / Sort
- **Localization** — 14 Text* parameters for all UI strings (TextSearch, TextFilter, TextPageInfo, etc.)
- **Charts-in-grid demo** — sparklines, delta indicators, progress bars embedded in DataGrid cells
- **24 DataGrid demo scenarios** across 4 categories (Basics, Editing, Advanced, Enterprise) on separate pages
- **Collapsible sidebar** — section headers toggle open/closed with chevron + SVG icons
- **1,095 unit tests** — 122 new tests covering UI components, Excel export, typed filters, indicators
- **3 new DataGrid benchmarks** — 10K render (1ms), 10K sort (2ms), 10K filter (3ms)

### Changed
- DataGrid demos extracted to 4 separate routes (/datagrid/basics, /editing, /advanced, /enterprise) for faster loading
- Homepage updated: test count 1,095, DataGrid features include Excel export + typed filters

## [1.0.0-beta.13] — 2026-03-27

### Added
- **Arcadia.UI package** — 9 infrastructure components: Dialog, Tabs, Tooltip, Sidebar, Accordion, Breadcrumb, Card, Badge, Avatar
- **Playground home page** — marketing landing with product cards, stats, and quick install
- **UI Components demo tab** — interactive showcase for all 9 UI components in the gallery
- **Homepage rewrite** — "The complete UI framework Blazor deserves", 7 feature cards, stats bar, playground link

### Fixed
- **DataGrid grouping by non-displayed property** — GroupBy now resolves via reflection when no matching column is displayed
- **DataGrid no data in interactive mode** — column collector callback triggers re-render after registration
- **Watermark CSS not loaded** — arcadia-core.css was never linked in demo apps
- **Streaming chart animation jank** — pure CSS 3-state machine replaces JS race condition
- **Focus ring on playground title** — removed browser default outline on h1
- **Doc layout paths** — 13 DataGrid MDX files had wrong relative path to DocsLayout.astro

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

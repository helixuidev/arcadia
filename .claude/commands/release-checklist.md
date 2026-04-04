---
description: Run the Arcadia Controls pre-release checklist — validates build, tests, docs, demos, accessibility, and version consistency before a version bump.
---

# Release Checklist — Arcadia Controls

Run this before every version bump to ensure nothing is missed. Go through EVERY item. Do NOT skip any. Fix issues as you find them. Use automated checks (grep, diff, build) — do not eyeball.

## Pre-Release Validation

### 1. Build ALL Packages
- [ ] `dotnet build src/Arcadia.Core/Arcadia.Core.csproj` — all TFMs, 0 warnings, 0 errors
- [ ] `dotnet build src/Arcadia.Theme/Arcadia.Theme.csproj` — 0 errors
- [ ] `dotnet build src/Arcadia.Charts/Arcadia.Charts.csproj` — all TFMs (net5.0–net9.0), 0 warnings, 0 errors
- [ ] `dotnet build src/Arcadia.DataGrid/Arcadia.DataGrid.csproj` — all TFMs, 0 warnings, 0 errors
- [ ] `dotnet build src/Arcadia.FormBuilder/Arcadia.FormBuilder.csproj` — 0 errors
- [ ] `dotnet build src/Arcadia.Notifications/Arcadia.Notifications.csproj` — 0 errors
- [ ] `dotnet build samples/Arcadia.Demo.Server/Arcadia.Demo.Server.csproj --framework net9.0` — 0 errors
- [ ] `dotnet build samples/Arcadia.Demo.Wasm/Arcadia.Demo.Wasm.csproj` — 0 errors

### 2. Run ALL Tests
- [ ] `dotnet test tests/Arcadia.Tests.Unit` — all unit tests pass
- [ ] Start demo server, then run `dotnet test tests/Arcadia.Tests.E2E` — all E2E tests pass
- [ ] If visuals changed: `UPDATE_SNAPSHOTS=1 dotnet test tests/Arcadia.Tests.E2E` to update baselines, review diffs

### 3. Version Bump
- [ ] Update `<Version>` in `Directory.Build.props`
- [ ] Verify version appears in build output

### 4. Code Consistency
- [ ] Every new/changed `[Parameter]` has XML documentation
- [ ] Every Pro chart has `<CommunityWatermark />` — verify: `grep -rL CommunityWatermark src/Arcadia.Charts/Components/Charts/*.razor | grep -v '\.cs'` should return ONLY Line, Bar, Pie, Scatter
- [ ] Community charts (Line, Bar, Pie, Scatter ONLY) do NOT have watermarks
- [ ] New chart components inherit from `ChartBase<T>` and implement `IAsyncDisposable` (if using JS interop)
- [ ] New animation CSS classes added to `prefers-reduced-motion` media query in arcadia-charts.css
- [ ] Animation keyframes do NOT use `forwards` with `opacity: 1` (use from-only keyframes to respect inline opacity)

### 5. Accessibility Audit
- [ ] Every chart SVG has `role="figure"` and `aria-label`
- [ ] Every chart has a hidden `<table class="arcadia-sr-only">` with meaningful data
- [ ] Verify: `grep -L 'role="figure"' src/Arcadia.Charts/Components/Charts/*.razor | grep -v '\.cs'` returns nothing
- [ ] Verify: `grep -L 'arcadia-sr-only' src/Arcadia.Charts/Components/Charts/*.razor | grep -v '\.cs'` returns nothing

### 6. Breaking Change Check
- [ ] No public parameters renamed or removed without deprecation
- [ ] Default values unchanged for existing parameters (new params only)
- [ ] No namespace changes

## Content Updates — EVERY ITEM, EVERY FILE

### 7. Root README.md (shows on GitHub)
- [ ] Chart count in tagline (line 3)
- [ ] Chart table lists ALL chart types with correct Community/Pro tier
- [ ] "Key Features" bullet has correct chart count
- [ ] NuGet Packages table description has correct count
- [ ] Pricing table has correct chart count

### 8. Package README (src/Arcadia.Charts/README.md — shows on NuGet)
- [ ] Chart count in tagline
- [ ] Chart type list includes ALL charts
- [ ] Pro chart count ("X more for $299/dev/year")

### 9. NuGet Package Descriptions (csproj files)
- [ ] `src/Arcadia.Charts/Arcadia.Charts.csproj` — `<Description>` lists all chart types, count is correct, `<PackageTags>` includes new types
- [ ] `src/Arcadia.DataGrid/Arcadia.DataGrid.csproj` — `<Description>` lists all features, `<PackageTags>` is comprehensive
- [ ] Both packages have README.md with `<None Include="README.md" Pack="true" PackagePath="/" />`
- [ ] `release.yml` pack loop includes ALL packages (Core, Theme, Notifications, FormBuilder, Charts, DataGrid)
- [ ] `ci.yml` pack job includes ALL packages

### 10. Website Homepage (website/src/pages/index.astro)
- [ ] Stats section: component count, test count, package count are current
- [ ] Feature cards: DataGrid description lists latest features (Excel export, typed filters, etc.)
- [ ] Community pricing: lists correct free charts (Line, Bar, Pie, Scatter)
- [ ] Pro pricing: "All X chart types" matches actual count, DataGrid features current
- [ ] Enterprise pricing: consistent with Pro
- [ ] FAQ / JSON-LD structured data: component counts, test counts updated
- [ ] Verify: `grep -n 'chart type' website/src/pages/index.astro` — every occurrence is correct
- [ ] Verify: `grep -n '973\|1095\|57' website/src/pages/index.astro` — all stats are current

### 11. Documentation Site
- [ ] Charts overview (website/src/pages/docs/charts/index.mdx): chart count in title/description, all charts in table with correct tier
- [ ] DataGrid overview (website/src/pages/docs/datagrid/index.mdx): all features listed
- [ ] DocsLayout sidebar (website/src/layouts/DocsLayout.astro): nav entries for all charts AND all DataGrid pages
- [ ] Every chart has its own doc page in website/src/pages/docs/charts/
- [ ] Every DataGrid feature has its own doc page in website/src/pages/docs/datagrid/
- [ ] Doc pages cover: basic usage, parameters table, events, data model, input validation, accessibility, use cases

### 12. Blog Posts — grep ALL posts for stale counts
- [ ] `grep -rn 'chart type' website/src/pages/blog/` — every mention is current
- [ ] `grep -rn '12 chart\|11 chart\|13 chart\|14 chart\|15 chart' website/src/pages/blog/` — no stale counts
- [ ] Blog screenshots are current (if blog references the demo)

### 13. Demo Apps — Server & WASM MUST be in sync
- [ ] Both have identical sidebar nav buttons for every chart
- [ ] Both have identical chart content sections with same demo data
- [ ] Verify sync: `diff <(grep 'gallery__nav-btn' samples/Arcadia.Demo.Server/Components/Pages/ChartsDemo.razor) <(grep 'gallery__nav-btn' samples/Arcadia.Demo.Wasm/Pages/ChartsDemo.razor)` — empty diff
- [ ] Test pages exist at /test/<chart-name> for each chart type (E2E tests depend on these)
- [ ] WASM MainLayout has ThemeToggle button (playground needs theme switching)
- [ ] WASM Program.cs defaults to DarkTheme (matches Server demo)
- [ ] WASM builds successfully: `dotnet build samples/Arcadia.Demo.Wasm/Arcadia.Demo.Wasm.csproj`
- [ ] **Every NEW package has a demo in BOTH Server and WASM** — not just Server test pages
- [ ] **Every NEW package is referenced in WASM csproj** — check: `grep ProjectReference samples/Arcadia.Demo.Wasm/Arcadia.Demo.Wasm.csproj` lists ALL packages
- [ ] **Every NEW component has a sidebar nav entry in BOTH demos** — reviewer must be able to FIND it
- [ ] **Run WASM locally and visually verify new features render** — do NOT skip this

### 14. Tests — Coverage Audit
Every new or changed component/feature MUST have tests. Zero-coverage features do NOT ship.

#### 14a. Test existence check (automated)
- [ ] Run: `for f in src/Arcadia.UI/Components/Arcadia*.razor; do name=$(basename "$f" .razor | sed 's/Arcadia//'); test=$(find tests -name "*${name}*Tests.cs" 2>/dev/null | head -1); [ -z "$test" ] && echo "MISSING: $name"; done` — should return nothing
- [ ] Run: same for `src/Arcadia.DataGrid`, `src/Arcadia.Charts`, `src/Arcadia.DashboardKit`
- [ ] Every `.razor` component in `src/` has a corresponding `*Tests.cs` in `tests/`

#### 14b. Unit tests per package
- [ ] **Charts**: Unit tests for every chart type (rendering, accessibility, empty states, edge cases, colors, toolbar)
- [ ] **DataGrid**: Unit tests for every feature parameter — sort, filter, group, edit, batch edit, selection, export (CSV/Excel/PDF), virtual scroll, infinite scroll, column reorder, stacked headers, tooltips, copy with headers, cell validation, sticky footer, group aggregates, command column, inline add row, conditional formatting, cell merge, row reorder, state persistence
- [ ] **UI Components**: Unit tests for every component in `src/Arcadia.UI/Components/` — render, parameters, events, disabled state, accessibility attributes
- [ ] **DashboardKit**: Unit tests for DragGrid — render, item registration, layout computation, swap logic, resize, locked items, edit mode, drag mode
- [ ] **FormBuilder**: Unit tests for all field types and validation
- [ ] **Notifications**: Unit tests for toast service and container

#### 14c. E2E / Integration tests
- [ ] E2E tests exist for every chart (baseline screenshot, ARIA, SR table, element counts)
- [ ] `TestConstants.AllChartTypes` array in E2E project includes all chart type slugs
- [ ] DataGrid E2E tests cover: sorting click, filter input, pagination, column resize, row selection, export buttons
- [ ] DragGrid E2E tests cover: drag swap, resize, locked items, edit mode toggle, wiggle mode, bounds check, overlap check
- [ ] UI component E2E tests cover: dialog open/close, accordion expand, tabs switch, drawer toggle

#### 14d. Test count sanity check
- [ ] Total unit test count is >= previous release count (never decreases)
- [ ] Run `dotnet test --verbosity quiet` and record: `Passed: X, Failed: 0, Skipped: 0`
- [ ] If any test was DELETED, justify why in the commit message

#### 14e. New feature test checklist
For EACH new feature added in this release, verify:
- [ ] At least 1 unit test that verifies the feature renders correctly
- [ ] At least 1 unit test that verifies the feature parameter changes behavior
- [ ] At least 1 unit test for edge cases (null data, empty state, boundary values)
- [ ] At least 1 accessibility test (ARIA attributes, keyboard support)
- [ ] If the feature has JS interop: at least 1 E2E test that exercises it in a real browser

### 15. Documentation Coverage
Every public component and feature MUST be documented. Undocumented features are invisible to users.

#### 15a. XML Documentation (code-level)
- [ ] Every `[Parameter]` property has `<summary>` XML doc comment
- [ ] Run: `grep -rn '\[Parameter\]' src/ --include="*.cs" --include="*.razor" | grep -v obj | wc -l` and compare to `grep -rn '/// <summary>' src/ --include="*.cs" --include="*.razor" | grep -v obj | wc -l` — counts should be close
- [ ] Every public method (`SaveLayoutAsync`, `ExportToPdfAsync`, etc.) has XML docs
- [ ] Every public enum has XML docs on each value

#### 15b. Doc pages (website)
- [ ] Every chart type has a doc page in `website/src/pages/docs/charts/`
- [ ] Run: `for chart in Line Bar Pie Scatter Radar Gauge Candlestick Funnel Heatmap Treemap Waterfall BoxPlot Sankey Chord Rose RangeArea Area Bubble Donut StackedBar; do [ ! -f "website/src/pages/docs/charts/$(echo $chart | tr 'A-Z' 'a-z')-chart.mdx" ] && [ ! -f "website/src/pages/docs/charts/$(echo $chart | tr 'A-Z' 'a-z').mdx" ] && echo "MISSING DOC: $chart"; done`
- [ ] Every DataGrid feature has a doc page in `website/src/pages/docs/datagrid/`
- [ ] DashboardKit has doc pages: overview, drag-grid, drag-modes, state-persistence, accessibility
- [ ] UI Components have doc pages: at minimum an overview page listing all components with examples
- [ ] Every doc page includes: description, basic usage code sample, parameters table, events table, accessibility notes

#### 15c. Demo pages
- [ ] Every component has a working demo in the Server demo app (`samples/Arcadia.Demo.Server/`)
- [ ] Test pages exist at `/test/<component>` for E2E testing
- [ ] Demo data is deterministic (no `Random()` or `DateTime.Now` in demo pages)

#### 15d. API reference completeness
- [ ] `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is set in Directory.Build.props
- [ ] Build produces no CS1591 warnings (missing XML comments) — or they are explicitly suppressed with justification
- [ ] New packages (DashboardKit, Gauge) have README.md for NuGet display

#### 15e. Sidebar navigation
- [ ] `DocsLayout.astro` sidebar lists ALL chart types
- [ ] `DocsLayout.astro` sidebar lists ALL DataGrid feature pages
- [ ] `DocsLayout.astro` sidebar has entries for DashboardKit, UI Components
- [ ] No broken internal links — run: `grep -roh 'href="/docs/[^"]*"' website/src/ | sort -u` and verify each path has a corresponding .mdx file

### 16. Visual Verification
- [ ] Start demo server, check new charts render correctly
- [ ] Toggle dark/light theme — new charts look correct in both
- [ ] Hover interactions work (tooltips, dim effects)
- [ ] Animations play and settle to correct opacity

## Commit & Release

### 17. Git
- [ ] Commit: `chore: bump to X.Y.Z-beta.N` with release notes in body
- [ ] Create annotated tag: `git tag -a vX.Y.Z-beta.N` with changelog
- [ ] Push: `git push origin main --tags`

### 18. Post-Push Verification
- [ ] GitHub Actions deploy workflow triggers — check: `gh run list --limit 3`
- [ ] Deploy workflow SUCCEEDS — check: `gh run view <id> --log-failed` if it fails
- [ ] Website at arcadiaui.com reflects changes
- [ ] Playground at arcadiaui.com/playground/ shows new charts AND theme toggle works
- [ ] Playground defaults to dark theme
- [ ] GitHub repo page (README) displays correctly
- [ ] NuGet packages published (if applicable)

### 19. NEVER Make the Repo Private
- [ ] **DO NOT** change repo visibility — GitHub Pages dies, deploys break, and recovery takes 30+ minutes
- [ ] If hosting needs to move off GitHub Pages, migrate to Railway/Vercel/Cloudflare FIRST, then change visibility
- [ ] Verify `curl -sI https://arcadiaui.com` returns HTTP 200 (not from cache — check `x-proxy-cache` header)

### 20. CHANGELOG
- [ ] Update CHANGELOG.md (or release notes on GitHub) with:
  - New features (charts, parameters, widgets)
  - Bug fixes
  - Breaking changes (if any)
  - Migration notes (if any)

Report EVERY failed item. Fix ALL of them before the release ships.

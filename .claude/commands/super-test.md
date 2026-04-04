---
description: Run a comprehensive test audit — inventory existing tests, find gaps and fragile tests, write missing tests, verify all pass, and commit.
---

# Test Audit & Fix — Arcadia Controls

Run a comprehensive test audit. Do NOT just add tests blindly — first understand what exists, what's missing, what's broken, and what's testing the wrong thing. Fix everything you find.

## Phase 1: Inventory

### 1.1 Count existing tests
```bash
dotnet test tests/Arcadia.Tests.Unit/Arcadia.Tests.Unit.csproj --framework net9.0 --filter "Category!=Performance" --verbosity quiet
```
Record the total. This is the baseline.

### 1.2 List all test files and their test counts
```bash
for f in $(find tests/Arcadia.Tests.Unit -name "*Tests.cs" -not -path "*/obj/*"); do
  count=$(grep -c "\[Fact\]\|\[Theory\]" "$f" 2>/dev/null || echo 0)
  echo "$count  $f"
done | sort -rn
```

### 1.3 List all source components that LACK test files
For each .razor.cs or .razor file in src/, check if a corresponding test file exists. Report gaps.

### 1.4 List all public [Parameter] properties across all components
```bash
grep -rn "\[Parameter\]" src/ --include="*.cs" --include="*.razor" | grep -v obj | wc -l
```
Every public parameter should be tested for: default value, custom value, and edge cases.

## Phase 2: Quality Check

### 2.1 Run ALL tests (including performance)
```bash
dotnet test tests/Arcadia.Tests.Unit/Arcadia.Tests.Unit.csproj --framework net9.0 --verbosity quiet
```
Record pass/fail/skip. Investigate ANY failures.

### 2.2 Check for tests that pass but test nothing
Look for tests that:
- Assert on markup but use overly broad selectors (e.g., `Markup.Should().Contain("div")`)
- Never call the component under test
- Have empty test bodies
- Assert on hardcoded strings that could change
```bash
grep -rn "Should().Be(true)\|Should().BeTrue()\|Assert.True(true)" tests/ --include="*.cs" | grep -v obj
```

### 2.3 Check for tests that are fragile
Look for tests that depend on:
- Exact HTML structure (will break on any markup change)
- Specific CSS class names without semantic meaning
- Timing/ordering assumptions
- External state (static fields, singletons)

### 2.4 Check for missing edge case coverage
For each component, verify tests exist for:
- Null/empty data
- Single item
- Maximum/overflow values
- Rapid parameter changes
- Disposal (IAsyncDisposable components)

## Phase 3: Gap Analysis

### 3.1 Map features to tests
Go through each feature added in beta.10-13 and verify test coverage:

**DataGrid features needing tests:**
- [ ] `Property` parameter (reflection-based field resolution)
- [ ] `SelectionMode` enum (None/SingleRow/Multiple)
- [ ] `EmptyTemplate` renders custom content
- [ ] Multi-column sort (Shift+Click adds to stack, priority ordering)
- [ ] Quick filter (searches across all visible columns)
- [ ] Clipboard copy (Ctrl+C builds TSV string)
- [ ] Filter operator dropdown (9 operators including NotEquals, IsEmpty, IsNotEmpty)
- [ ] Batch editing (track changes, commit, discard)
- [ ] Context menu (shows/hides, receives correct item)
- [ ] State persistence (GetState/RestoreState round-trip)
- [ ] Excel export (produces valid XLSX bytes, respects filters/visibility)
- [ ] Boolean filter (tri-state dropdown)
- [ ] Date filter (date input renders for DateTime columns)
- [ ] Number filter (number input renders for numeric columns)
- [ ] Column pinning UI (IsFrozen toggle, sticky CSS)
- [ ] Column header context menu (show/close, pin/unpin/hide)
- [ ] Localization parameters (Text* params override defaults)
- [ ] GroupBy with reflection fallback (non-displayed property)
- [ ] Column notification callback (OnColumnsChanged triggers re-render)

**Charts features needing tests:**
- [ ] `SyncGroup` parameter exists on ChartBase
- [ ] Financial indicators (SMA, EMA, Bollinger, RSI calculations)
- [ ] `OnPrint` callback parameter
- [ ] Slide animation state machine (Idle/Offset/Animating transitions)

**UI Components needing tests:**
- [ ] Dialog (visible/hidden, close on Escape, close on overlay click)
- [ ] Tabs (active index, disabled tab, arrow key navigation)
- [ ] Tooltip (text renders, position classes)
- [ ] Card (title, subtitle, clickable)
- [ ] Badge (content, dot mode, color)
- [ ] Avatar (initials from name, size classes)
- [ ] Accordion (expand/collapse, single mode)
- [ ] Breadcrumb (items render, last item has aria-current)
- [ ] Sidebar (visible/collapsed, position)

## Phase 4: Write Missing Tests

For each gap found in Phase 3, write tests. Rules:
- Use bUnit `Render<T>()` with `ComponentParameterCollectionBuilder`
- Use FluentAssertions for readable assertions
- Test behavior, not implementation (assert on rendered output, not internal state)
- Each test file should have a descriptive name matching the feature
- Use `[Fact]` for single cases, `[Theory]` for parameterized
- Mark performance tests with `[Trait("Category", "Performance")]`
- Test accessibility: verify ARIA attributes, roles, labels

### Naming convention
```
FeatureName_Scenario_ExpectedBehavior
```
Example: `ExcelExport_WithFilters_OnlyExportsFilteredRows`

## Phase 5: Verify

### 5.1 Run full suite
```bash
dotnet test tests/Arcadia.Tests.Unit/Arcadia.Tests.Unit.csproj --framework net9.0 --verbosity quiet
```
ALL tests must pass. Zero failures. Zero skips.

### 5.2 Verify no regressions
Compare test count before and after. The count should only go UP.

### 5.3 Report
Output a summary table:
| Category | Before | After | Added |
|----------|--------|-------|-------|
| DataGrid | X | Y | +Z |
| Charts | X | Y | +Z |
| UI Components | X | Y | +Z |
| FormBuilder | X | Y | +Z |
| Total | X | Y | +Z |

## Phase 6: Commit
```
git add tests/
git commit -m "test: comprehensive audit — N new tests covering [features]"
git push origin main
```

Report EVERY gap you find. Fix ALL of them. Do not skip any phase.

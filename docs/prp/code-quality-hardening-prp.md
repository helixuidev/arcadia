# Code Quality Hardening — PRP

## Goal

Make the codebase indistinguishable from hand-crafted professional code. An experienced .NET developer reviewing the source should think "this team knows what they're doing" — not "this was generated."

## Definition of Done

- Zero Roslyn analyzer warnings on the public API surface
- Every public type/member has meaningful XML docs (not restating the name)
- Zero dead code (unused classes, methods, parameters, fields)
- Consistent naming across all 16 charts and the DataGrid
- No magic numbers in templates — all extracted to named constants or parameters
- No silent exception swallowing — every catch either logs, rethrows, or has a comment explaining why it's swallowed
- CSS has zero duplicate rules, zero unused classes
- Every doc page matches the actual code (parameter names, types, defaults)
- Test names describe behavior, not implementation

---

## Phase 1: Static Analysis Setup

### 1.1 Enable Roslyn Analyzers
- Add `<EnableNETAnalyzers>true</EnableNETAnalyzers>` to Directory.Build.props
- Add `<AnalysisLevel>latest-recommended</AnalysisLevel>`
- Add `.editorconfig` with team rules:
  - `dotnet_diagnostic.CA1062.severity = warning` (validate args)
  - `dotnet_diagnostic.CA1822.severity = suggestion` (mark static)
  - `dotnet_diagnostic.IDE0051.severity = warning` (unused private members)
  - `dotnet_diagnostic.IDE0052.severity = warning` (unread private members)
  - `dotnet_diagnostic.CS8618.severity = warning` (non-nullable uninitialized)
- Build all projects, fix every warning
- Gate: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` (already set)

### 1.2 Run dotnet format
- `dotnet format` across the solution for consistent whitespace/style
- Verify no functional changes in the diff

---

## Phase 2: Public API Surface Review

### 2.1 XML Documentation Audit
For every `[Parameter]` across all components, verify:
- Summary is NOT just the parameter name rephrased ("Gets or sets the height" → BAD)
- Summary explains WHAT it does and WHEN to use it
- Default value is mentioned if non-obvious
- Example usage shown for complex parameters

BAD:
```csharp
/// <summary>Gets or sets the height.</summary>
[Parameter] public double Height { get; set; } = 300;
```

GOOD:
```csharp
/// <summary>Chart height in pixels. The SVG viewBox scales to this height.</summary>
[Parameter] public double Height { get; set; } = 300;
```

### 2.2 Parameter Naming Consistency
Verify across ALL components:
- Two-way binding: `Value`, `ValueChanged`, `ValueExpression`
- Events: `On` prefix (`OnPointClick`, `OnRowEdit`, `OnChordClick`)
- Boolean: positive assertion (`ShowLabels` not `HideLabels`, `Sortable` not `DisableSort`)
- Collections: plural (`PageSizeOptions` not `PageSizeOption`)
- Color: `string` type, accepts hex/CSS/named colors
- Opacity: `double` 0.0-1.0

### 2.3 Dead Code Purge
Search for:
- `internal` methods never called from .razor files
- `private` fields never read
- `using` directives not needed
- Classes/types not referenced anywhere
- Parameters that exist but are never read in the component logic

---

## Phase 3: Code Pattern Consistency

### 3.1 Chart Lifecycle Pattern
Every chart component MUST follow this exact pattern:

```csharp
public partial class ArcadiaXxxChart<T> : ChartBase<T>
{
    // 1. Parameters (sorted: data, visual, behavior, events)
    // 2. Private fields (layout, scale, paths)
    // 3. OnParametersSet() — validate → compute layout → build paths
    // 4. Helper methods (format, tooltip, click handlers)
    // 5. Static F() formatter
    // 6. Private nested classes (layout types)
}
```

Verify all 16 charts follow this order.

### 3.2 Null Handling Pattern
Every chart template MUST guard with:
```razor
else if (HasData && RequiredFields && _yScale is not null)
```
Already done for 8 charts — verify remaining 8.

### 3.3 Exception Pattern
Replace all bare `catch { }` and `catch (Exception) { }` with:
```csharp
catch (JSException) { } // Tooltip interop unavailable in SSR
catch (JSDisconnectedException) { } // Circuit disconnected during Blazor Server navigation
catch (ObjectDisposedException) { } // Component disposed before async operation completed
catch (InvalidOperationException) { } // JS interop unavailable during static prerendering
```
Every catch MUST have a comment explaining why it's safe to swallow.

### 3.4 Magic Number Extraction
Find all hardcoded numbers in .razor files and extract:
- Padding values → use parameters or CSS variables
- Animation delays → calculate from index
- Font sizes → use CSS variables
- Opacity values → use parameters (already done for most)

---

## Phase 4: CSS Quality

### 4.1 Unused CSS Classes
- Grep all `.arcadia-` class names in CSS files
- Grep all `class="arcadia-` in .razor files
- Find classes defined in CSS but never used in templates
- Delete unused classes

### 4.2 Duplicate Rules
- Look for properties set multiple times on the same selector
- Look for selectors that are functionally identical

### 4.3 Specificity Audit
- No `!important` except for theme overrides and hover states
- Document every `!important` usage with a comment

---

## Phase 5: Documentation Accuracy

### 5.1 Parameter Table Verification
For each doc page in `website/src/pages/docs/charts/`:
- Every `[Parameter]` in the .cs file appears in the doc table
- Types match exactly
- Default values match exactly
- No parameters listed in docs that don't exist in code

### 5.2 Code Example Verification
- Every code example in docs should compile if pasted into a project
- Verify component names, parameter names, type names are current

---

## Phase 6: Test Quality

### 6.1 Test Naming Convention
Pattern: `{Method}_{Scenario}_{ExpectedResult}` or `{Feature}_{Condition}`

BAD: `Test1`, `Works`, `ShouldRender`
GOOD: `Renders_SvgWithNodesAndLinks`, `SkipsLinks_WithNegativeValues`

### 6.2 Test Coverage Gaps
- Every public parameter should have at least one test
- Every edge case (null, empty, NaN, negative) should be tested
- Every event callback should have a test verifying it fires

---

## Execution Order

1. Static analysis setup (.editorconfig + analyzers) — catches 50% of issues automatically
2. Dead code purge — smallest diff, biggest cleanup
3. Exception comments — quick sweep
4. XML doc quality — rewrite bad docs
5. CSS audit — delete unused
6. Doc accuracy — verify tables
7. Pattern consistency — verify all charts follow the template

## Files Affected

Every `.cs`, `.razor`, `.css`, and `.mdx` file in:
- `src/Arcadia.Charts/`
- `src/Arcadia.DataGrid/`
- `src/Arcadia.Core/`
- `website/src/pages/docs/`
- `tests/`

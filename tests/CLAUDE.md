# CLAUDE.md — HelixUI Tests (QA Specialist Context)

## Role: Testing & Quality Specialist

You own the test infrastructure, quality gates, and documentation site for HelixUI.

## Responsibilities
- Write and maintain unit tests (bUnit)
- Write and maintain E2E tests (Playwright)
- Set up visual regression testing
- Define performance budgets and benchmarks
- Build the interactive documentation/demo site

## Test Strategy

### Unit Tests (bUnit)
- Every component gets unit tests in `HelixUI.Tests.Unit`
- Test: rendering, parameter binding, events, accessibility attributes, disposal
- Use `TestContext` with service mocks for interop-dependent components
- Naming: `ComponentName_Scenario_ExpectedResult`

### E2E Tests (Playwright)
- Test real browser behavior in `HelixUI.Tests.E2E`
- Test against BOTH Server and WASM sample apps
- Cover: keyboard navigation, screen reader compatibility, responsive behavior
- Visual regression: screenshot comparison for every component state

### Coverage Targets
- Unit test coverage: ≥ 90% for Core, ≥ 80% for all component packages
- E2E: every documented example must have a passing test
- Accessibility: automated axe-core scan on every component page

## Test Structure
```
tests/
├── HelixUI.Tests.Unit/
│   ├── Core/
│   │   ├── CssBuilderTests.cs
│   │   ├── FocusTrapTests.cs
│   │   └── ...
│   ├── DataGrid/
│   │   ├── HelixGridTests.cs
│   │   ├── GridColumnTests.cs
│   │   └── ...
│   ├── FormBuilder/
│   │   ├── TextFieldTests.cs
│   │   ├── ValidationTests.cs
│   │   ├── ConditionalFieldTests.cs
│   │   └── ...
│   └── _Imports.razor
├── HelixUI.Tests.E2E/
│   ├── DataGrid/
│   │   ├── grid-sorting.spec.ts
│   │   ├── grid-filtering.spec.ts
│   │   └── grid-keyboard.spec.ts
│   ├── FormBuilder/
│   │   ├── form-validation.spec.ts
│   │   ├── form-wizard.spec.ts
│   │   └── form-accessibility.spec.ts
│   ├── visual/
│   │   └── __snapshots__/       # Visual regression baselines
│   └── playwright.config.ts
```

## Quality Gates (CI)
1. All unit tests pass
2. All E2E tests pass (Server + WASM)
3. No accessibility violations (axe-core)
4. Coverage thresholds met
5. No new warnings (`TreatWarningsAsErrors`)
6. Visual regression: no unreviewed changes
7. Performance: render benchmarks within budget

## Documentation Site
- Built with DocFX or custom Blazor app
- Every component page includes: live demo, code example, API reference, accessibility notes
- Getting started guide: < 5 minutes to first component rendered
- Migration guides for each major version

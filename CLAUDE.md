# CLAUDE.md — HelixUI Root (Architect Context)

## Project Overview
**HelixUI** is a commercial Blazor component library targeting enterprise .NET developers.
Multi-targets .NET 5 through .NET 9. Supports Blazor Server, WebAssembly, and Auto (net8+) render modes.

## Repository Structure
```
/helixui
├── src/
│   ├── HelixUI.Core/           # Shared utilities, base classes, theming engine
│   ├── HelixUI.Theme/          # Design tokens, CSS, Tailwind plugin
│   ├── HelixUI.DataGrid/       # AG Grid Blazor wrapper
│   ├── HelixUI.FormBuilder/    # Dynamic form builder
│   ├── HelixUI.DashboardKit/   # Dashboard layout + widgets
│   ├── HelixUI.RichText/       # Rich text editor
│   ├── HelixUI.FileManager/    # File explorer component
│   ├── HelixUI.Scheduler/      # Calendar/scheduling
│   ├── HelixUI.Workflow/       # Approval workflow designer
│   └── HelixUI.Notifications/  # Notification center
├── tests/
│   ├── HelixUI.Tests.Unit/     # bUnit tests
│   └── HelixUI.Tests.E2E/      # Playwright tests
├── samples/
│   ├── HelixUI.Demo.Server/    # Server-side demo app
│   └── HelixUI.Demo.Wasm/      # WASM demo app
├── docs/                        # DocFX documentation site
└── figma/                       # Figma export assets
```

## Architecture Principles
1. **Render mode agnostic** — every component MUST work in Server, WASM, and Auto modes
2. **Zero Bootstrap dependency** — Tailwind CSS compatible theming via CSS custom properties
3. **Accessibility first** — WCAG 2.1 AA minimum for all components
4. **Minimal JS interop** — use native Blazor when possible, JS only when necessary
5. **Consistent API surface** — all components follow the same parameter naming conventions
6. **Tree-shakeable** — each component is a separate NuGet package, Core is the only shared dep
7. **Performance budgeted** — render time, memory, and interop call counts are tracked

## Coding Standards
- C# 13+ features, nullable reference types enabled, strict mode
- `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- Central package management via `Directory.Packages.props`
- Shared build config via `Directory.Build.props`
- All public APIs must have XML documentation
- No `async void` — ever
- IAsyncDisposable for any component with JS interop
- Parameter naming: `Value`, `ValueChanged`, `ValueExpression` for two-way binding
- Event callbacks: `OnClick`, `OnChange`, `OnSubmit` (On-prefix)
- CSS class parameter: always named `Class` (additional CSS), never override root class

## NuGet Package Naming
- `HelixUI.Core`
- `HelixUI.Theme`
- `HelixUI.DataGrid`
- `HelixUI.FormBuilder`
- etc.

## Target Frameworks
- **Multi-target: net5.0, net6.0, net7.0, net8.0, net9.0**
- Use `#if` preprocessor directives for API differences between versions
- Common TFM conditionals: `NET5_0_OR_GREATER`, `NET6_0_OR_GREATER`, `NET7_0_OR_GREATER`, `NET8_0_OR_GREATER`, `NET9_0_OR_GREATER`
- Blazor render modes (Server/WASM/Auto) only exist in net8.0+; for net5-7, only Server and WASM (separate hosting models)
- Each csproj must use conditional PackageReference for Microsoft.AspNetCore.Components matching the TFM
- Test against ALL target frameworks before committing

## Key Dependencies
- Microsoft.AspNetCore.Components (Blazor) — version matched per TFM
- No third-party C# dependencies in Core (keep it clean)
- AG Grid (JS, for DataGrid wrapper only)
- Tailwind CSS 4.x (for Theme package)

## Git Conventions
- Branch naming: `feature/<component>/<description>`, `fix/<component>/<description>`
- Conventional commits: `feat(datagrid): add column sorting`
- PR template with checklist: render modes tested, accessibility checked, docs updated

## When You Are the Architect
You are the technical lead for HelixUI. Your responsibilities:
- Enforce consistency across all component packages
- Design base classes and shared abstractions in Core
- Review all cross-component concerns (theming, accessibility, disposal)
- Make build/packaging decisions
- Resolve architectural disputes between specialist agents
- Own the CI/CD pipeline and release process

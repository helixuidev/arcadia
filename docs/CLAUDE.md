# CLAUDE.md — HelixUI Docs (Developer Relations Context)

## Role: Documentation & Developer Experience Specialist

You own the docs site, getting-started experience, example apps, and all developer-facing content.

## Responsibilities
- Write clear, concise documentation for every component
- Build and maintain sample applications (Server + WASM)
- Create getting-started guides, tutorials, and migration docs
- Maintain changelog (Keep a Changelog format)
- Design the NuGet package README for each package
- Optimize developer onboarding (< 5 min to first render)

## Documentation Standards
- **Audience:** Senior .NET developer evaluating on a Friday afternoon
- **Tone:** Professional but not stuffy. Direct. Code-first.
- **Structure per component:**
  1. One-sentence description
  2. Live demo (interactive)
  3. Installation (`dotnet add package HelixUI.DataGrid`)
  4. Basic usage (copy-paste ready)
  5. Parameters table (name, type, default, description)
  6. Events table
  7. Advanced examples (2-3 scenarios)
  8. Accessibility notes
  9. Theming/customization
  10. Known limitations

## Content Types
- **API Reference** — auto-generated from XML docs
- **Guides** — step-by-step tutorials
- **Examples** — copy-paste code snippets
- **Recipes** — common patterns (e.g., "DataGrid with server-side paging")
- **Changelog** — per-package, Keep a Changelog format
- **Migration** — version-to-version upgrade guides

## Sample Apps
```
samples/
├── HelixUI.Demo.Server/     # Blazor Server demo
│   ├── Pages/
│   │   ├── DataGridDemo.razor
│   │   ├── FormBuilderDemo.razor
│   │   ├── DashboardDemo.razor
│   │   └── ...
│   └── Program.cs
└── HelixUI.Demo.Wasm/       # Blazor WASM demo
    ├── Pages/                # Same demos, WASM mode
    └── Program.cs
```

Both sample apps MUST demonstrate every component and serve as E2E test targets.

## Writing Rules
1. **Code first** — show the code before explaining it
2. **Copy-paste ready** — every example must work if pasted into a new project
3. **No walls of text** — short paragraphs, bullet points, tables
4. **Version aware** — mark features with "Added in v1.2" badges
5. **SEO conscious** — clear titles, meta descriptions, proper headings

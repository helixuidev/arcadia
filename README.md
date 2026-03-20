# HelixUI

**Premium Blazor components for enterprise .NET applications.**

HelixUI is a commercial component library built for .NET 9+ Blazor, supporting all render modes (Server, WebAssembly, Auto).

## Components

| Package | Description | Status |
|---------|-------------|--------|
| `HelixUI.Core` | Base classes, theming, accessibility utilities | 🔨 Building |
| `HelixUI.Theme` | Design tokens, Tailwind CSS integration | 🔨 Building |
| `HelixUI.DataGrid` | AG Grid wrapper with native Blazor bindings | 📋 Planned |
| `HelixUI.FormBuilder` | Dynamic forms, validation, wizards | 📋 Planned |
| `HelixUI.DashboardKit` | Drag-and-drop dashboards, charts, widgets | 📋 Planned |
| `HelixUI.RichText` | Rich text editor (Tiptap/ProseMirror) | 📋 Planned |
| `HelixUI.FileManager` | File explorer, upload, preview | 📋 Planned |
| `HelixUI.Scheduler` | Calendar, resource scheduling | 📋 Planned |
| `HelixUI.Workflow` | Visual approval workflow designer | 📋 Planned |
| `HelixUI.Notifications` | In-app notification center, toasts | 📋 Planned |

## Quick Start

```bash
dotnet add package HelixUI.Core
dotnet add package HelixUI.Theme
dotnet add package HelixUI.DataGrid  # or whichever component you need
```

```razor
@using HelixUI.Core
@using HelixUI.DataGrid

<HelixGrid Items="@employees" TItem="Employee">
    <GridColumn Field="@(e => e.Name)" Header="Name" Sortable />
    <GridColumn Field="@(e => e.Department)" Header="Department" Filterable />
    <GridColumn Field="@(e => e.Salary)" Header="Salary" Format="C2" />
</HelixGrid>
```

## Design Principles

- **Render mode agnostic** — works in Server, WASM, and Auto
- **Accessibility first** — WCAG 2.1 AA compliant
- **Tailwind compatible** — CSS custom properties, no Bootstrap
- **Performance budgeted** — every component has render time targets
- **Extensible** — clean APIs, pluggable adapters, custom renderers

## License

Commercial license. See [LICENSE](LICENSE) for details.

---

© 2026 HelixUI. All rights reserved.

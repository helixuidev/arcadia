<p align="center">
  <strong>Arcadia.UI</strong><br>
  <em>Essential Blazor UI components: Dialog, Tabs, Tooltip, Sidebar, Accordion, Breadcrumb, Card, Badge, Avatar</em>
</p>

## Why Arcadia UI?

- **Pure Blazor** — no JavaScript required for any component
- **Accessible** — WCAG 2.1 AA: focus traps, ARIA roles, keyboard navigation
- **Themeable** — CSS custom properties integrate with Arcadia.Theme
- **Lightweight** — each component is a single Razor file, tree-shakeable

## Components

| Component | Description |
|-----------|-------------|
| Dialog | Modal dialog with overlay, focus trap, Escape close |
| Tabs | Tabbed content with arrow key navigation |
| Tooltip | Hover tooltip with 4 positions |
| Sidebar | Collapsible navigation sidebar |
| Accordion | Expandable content sections |
| Breadcrumb | Navigation breadcrumb trail |
| Card | Content card with header/body/footer |
| Badge | Status indicator (count or dot) |
| Avatar | User avatar (image or initials) |

## Quick Start

```bash
dotnet add package Arcadia.UI
dotnet add package Arcadia.Theme
```

```html
<link href="_content/Arcadia.Theme/css/arcadia.css" rel="stylesheet" />
<link href="_content/Arcadia.UI/css/arcadia-ui.css" rel="stylesheet" />
```

```razor
@using Arcadia.UI.Components

<ArcadiaDialog @bind-Visible="showDialog" Title="Confirm Delete">
    <p>Are you sure you want to delete this item?</p>
    <FooterTemplate>
        <button @onclick="() => showDialog = false">Cancel</button>
        <button @onclick="Delete">Delete</button>
    </FooterTemplate>
</ArcadiaDialog>

<ArcadiaTabs>
    <ArcadiaTabPanel Title="Overview">Overview content</ArcadiaTabPanel>
    <ArcadiaTabPanel Title="Details">Details content</ArcadiaTabPanel>
</ArcadiaTabs>
```

**[Live Demo](https://arcadiaui.com/playground/)** · **[Documentation](https://arcadiaui.com/docs)** · **[GitHub](https://github.com/ArcadiaUIDev/arcadia)**

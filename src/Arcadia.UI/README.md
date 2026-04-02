<p align="center">
  <strong>Arcadia.UI</strong><br>
  <em>46 essential Blazor UI components: Dialog, Tabs, Card, CommandPalette, HoverCard, Popover, TreeView, and more</em>
</p>

## Why Arcadia UI?

- **Pure Blazor** — no JavaScript required for any component
- **Accessible** — WCAG 2.1 AA: focus traps, ARIA roles, keyboard navigation
- **Themeable** — CSS custom properties integrate with Arcadia.Theme
- **Lightweight** — each component is a single Razor file, tree-shakeable

## Components (46)

| Component | Description |
|-----------|-------------|
| Accordion | Expandable content sections |
| Alert | Contextual feedback messages |
| AspectRatio | Maintain width/height ratio for content |
| Avatar | User avatar (image or initials) |
| Badge | Status indicator (count or dot) |
| Breadcrumb | Navigation breadcrumb trail |
| Card | Content card with header/body/footer, glassmorphism variants |
| Carousel | Image/content slideshow |
| Chip | Compact element for tags and selections |
| CircularProgress | Circular progress indicator |
| ColorPicker | Color selection input |
| CommandPalette | Searchable command launcher (Ctrl+K) |
| ContextMenu | Right-click context menu |
| DatePicker | Date selection input |
| Dialog | Modal dialog with overlay, focus trap, Escape close |
| Drawer | Slide-out panel from any edge |
| EmptyState | Placeholder for empty content areas |
| HoverCard | Rich content preview on hover |
| Menu | Dropdown menu with items |
| Pagination | Page navigation controls |
| Popover | Floating content anchored to a trigger |
| Progress | Linear progress bar |
| Rating | Star rating input |
| ScrollArea | Custom scrollbar container |
| Separator | Visual divider between sections |
| Sidebar | Collapsible navigation sidebar |
| Skeleton | Loading placeholder with shimmer animation |
| Slider | Range input slider |
| Spinner | Loading spinner animation |
| Stepper | Multi-step workflow indicator |
| Switch | Toggle switch input |
| Tabs | Tabbed content with arrow key navigation |
| TagInput | Multi-value tag input field |
| TextArea | Multi-line text input |
| Timeline | Vertical event timeline |
| Tooltip | Hover tooltip with 4 positions |
| TreeView | Hierarchical tree navigation |

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

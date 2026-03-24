# Arcadia.Theme

Design tokens, CSS custom properties, and dark/light theme support for Arcadia Controls.

## Install

```bash
dotnet add package Arcadia.Theme
```

## Quick Start

Add the stylesheet to your `App.razor` or `_Host.cshtml`:

```html
<link rel="stylesheet" href="_content/Arcadia.Theme/arcadia-theme.css" />
```

Wrap your app in the theme provider and toggle themes programmatically:

```razor
<ThemeProvider>
    @Body
</ThemeProvider>

@inject ThemeService Theme
<button @onclick="() => Theme.SetMode(ThemeMode.Dark)">Dark Mode</button>
```

## What's Included

- **Design tokens** — colors, spacing, typography, border radius, elevation as CSS custom properties
- **Light and dark themes** — automatic OS detection with manual override, WCAG 2.1 AA contrast ratios
- **Three density modes** — compact, default, comfortable
- **Tailwind CSS 4.x plugin** — tokens map to Tailwind utilities with zero conflicts

## Key Features

CSS custom properties · OS-level dark mode detection · Programmatic theme switching · Custom theme creation · Multi-target .NET 5–10

**[Docs](https://arcadiaui.com/docs/theming)** · **[Demo](https://arcadiaui.com/playground/)** · **[GitHub](https://github.com/ArcadiaUIDev/arcadia)**

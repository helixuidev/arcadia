# Arcadia.Core

Shared base classes, theming engine, and accessibility utilities for the Arcadia Controls component library.

## Install

```bash
dotnet add package Arcadia.Core
```

## What's Included

- **HelixComponentBase** — base class with Class, Style, AdditionalAttributes for all Arcadia components
- **HelixInputBase\<T\>** — base for form inputs with Value/ValueChanged/ValueExpression two-way binding
- **CssBuilder / StyleBuilder** — fluent CSS class and inline style construction
- **Accessibility utilities** — FocusTrap, LiveRegion, AriaHelper, KeyboardNavigation, WCAG 2.1 AA compliance
- **IdGenerator** — unique IDs for aria-describedby, label-for linking

## Usage

Arcadia.Core is automatically included as a dependency of all Arcadia component packages. Install it directly only if building custom components that extend the Arcadia base classes:

```razor
@inherits HelixComponentBase

<div class="@CssClass" @attributes="AdditionalAttributes">
    @ChildContent
</div>
```

## Key Features

Zero external dependencies · Nullable reference types · IAsyncDisposable lifecycle · Multi-target .NET 5–10 · All Blazor render modes

**[Docs](https://arcadiaui.com/docs)** · **[Demo](https://arcadiaui.com/playground/)** · **[GitHub](https://github.com/ArcadiaUIDev/arcadia)**

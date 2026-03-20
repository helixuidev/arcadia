# CLAUDE.md — HelixUI Core

## Purpose
Core is the foundation package. Every other HelixUI package depends on it.
It contains base classes, shared utilities, theming abstractions, and accessibility helpers.

## What Goes Here
- `HelixComponentBase` — base class for all HelixUI components
- CSS class builder utilities
- Theme service abstractions
- Accessibility utilities (focus trap, live region, aria helpers)
- Common parameter interfaces (IHasClass, IHasStyle, IHasDisabled)
- Event argument models shared across components
- JS interop base helpers

## What Does NOT Go Here
- Any specific component implementation
- Third-party library dependencies (keep Core dependency-free)
- CSS/styles (those go in Theme)

## Core Structure
```
HelixUI.Core/
├── Base/
│   ├── HelixComponentBase.cs     # Base component (Class, Style, AdditionalAttributes)
│   ├── HelixInputBase.cs         # Base for form inputs (Value, ValueChanged, Validation)
│   └── HelixInteropBase.cs       # Base for JS interop components (disposal, module loading)
├── Utilities/
│   ├── CssBuilder.cs             # Fluent CSS class construction
│   ├── StyleBuilder.cs           # Inline style construction
│   └── IdGenerator.cs            # Unique ID generation for accessibility
├── Accessibility/
│   ├── FocusTrap.razor           # Focus trap component
│   ├── LiveRegion.razor          # ARIA live region for announcements
│   ├── AriaHelper.cs             # ARIA attribute helpers
│   └── KeyboardNavigation.cs     # Keyboard nav patterns (roving tabindex, etc.)
├── Abstractions/
│   ├── IHelixTheme.cs            # Theme provider interface
│   ├── IHasClass.cs              # Components with CSS class parameter
│   ├── IHasStyle.cs              # Components with inline style parameter
│   └── IHasDisabled.cs           # Components with disabled state
└── HelixUI.Core.csproj
```

## Rules
1. **Zero external dependencies** — only Microsoft.AspNetCore.Components
2. **No breaking changes** — Core is the most stable package
3. **XML docs on everything** — this is the public API surface
4. **Keep it small** — if it's not used by 3+ components, it doesn't belong here

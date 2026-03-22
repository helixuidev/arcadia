# Arcadia.FormBuilder

Dynamic form builder for Blazor — 21 field types, schema-driven rendering, wizard forms, and validation.

## Quick Start

```csharp
var schema = new FormSchema
{
    Title = "Contact Form",
    Fields = new()
    {
        new() { Name = "name", Type = FieldType.Text, Label = "Name", Required = true },
        new() { Name = "email", Type = FieldType.Text, Label = "Email",
                Validation = new() { Pattern = "email" } },
    }
};

<HelixFormBuilder Schema="@schema" OnValidSubmit="HandleSubmit" />
```

## Key Features

- 21 field types (text, number, select, date, rating, repeater, and more)
- Schema-driven, model-driven, or component-based forms
- Wizard/multi-step with per-step validation
- Conditional field visibility
- Cross-field validation (password confirm, date ranges)
- Auto-generate from C# classes with DataAnnotations
- Auto-save with undo/redo
- Full accessibility (aria-required, aria-invalid, aria-describedby)

## Installation

```
dotnet add package Arcadia.FormBuilder
```

## Links

- [Documentation](https://arcadiaui.com/docs/forms)
- [GitHub](https://github.com/helixuidev/helixui)

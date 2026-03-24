# Arcadia.FormBuilder

Dynamic form generation for Blazor — 21 field types, schema-driven and model-driven modes, multi-step wizards.

## Install

```bash
dotnet add package Arcadia.FormBuilder
```

## Quick Start

Generate a form from a JSON schema:

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
```

```razor
<HelixFormBuilder Schema="@schema" OnValidSubmit="HandleSubmit" />
```

Or generate directly from a C# model with DataAnnotations:

```razor
<HelixFormBuilder TModel="ContactForm" Model="@contact" OnValidSubmit="HandleSubmit" />
```

## Field Types

Text · Number · Email · Select · MultiSelect · Checkbox · Radio · Toggle · Date · DateTime · Time · File Upload · TextArea · Autocomplete · Switch · Slider · Rating · Color Picker · Rich Text · Repeater · Hidden

## Key Features

Schema-driven & model-driven · Multi-step wizard with per-step validation · Conditional field visibility · Async validation · Auto-save with undo/redo · WCAG 2.1 AA accessible · All Blazor render modes · .NET 5–10

**[Docs](https://arcadiaui.com/docs/form-builder)** · **[Demo](https://arcadiaui.com/playground/)** · **[GitHub](https://github.com/ArcadiaUIDev/arcadia)**

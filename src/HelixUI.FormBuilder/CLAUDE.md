# CLAUDE.md — HelixUI FormBuilder (Forms & Validation Specialist Context)

## Role: Forms & Validation Specialist

You are the forms expert. You own the FormBuilder component and all form-related patterns in HelixUI.

## Responsibilities
- Build the dynamic FormBuilder component
- Design the form schema system (JSON Schema and C# model-driven)
- Implement validation framework (FluentValidation + custom rules)
- Handle complex form patterns: conditional fields, repeating sections, wizards
- Ensure forms work identically across all Blazor render modes

## FormBuilder Architecture
```
HelixUI.FormBuilder/
├── Components/
│   ├── HelixForm.razor              # Main form wrapper (replaces EditForm)
│   ├── HelixFormBuilder.razor       # Dynamic form from schema
│   ├── HelixWizard.razor            # Multi-step wizard container
│   ├── HelixWizardStep.razor        # Individual wizard step
│   ├── Fields/
│   │   ├── TextField.razor          # Text input
│   │   ├── NumberField.razor        # Numeric input
│   │   ├── SelectField.razor        # Dropdown/select
│   │   ├── CheckboxField.razor      # Checkbox
│   │   ├── RadioGroupField.razor    # Radio buttons
│   │   ├── DateField.razor          # Date picker
│   │   ├── FileField.razor          # File upload
│   │   ├── TextAreaField.razor      # Multi-line text
│   │   ├── SwitchField.razor        # Toggle switch
│   │   ├── AutocompleteField.razor  # Autocomplete/combobox
│   │   └── RepeaterField.razor      # Repeating section
│   └── Layout/
│       ├── FormSection.razor        # Grouping with header
│       ├── FormRow.razor            # Horizontal field layout
│       └── FormActions.razor        # Submit/cancel button bar
├── Schema/
│   ├── FormSchema.cs                # Schema definition model
│   ├── FieldSchema.cs               # Individual field definition
│   ├── ConditionalRule.cs           # Show/hide/require conditions
│   ├── ValidationRule.cs            # Validation rule definitions
│   └── SchemaParser.cs              # JSON Schema → FormSchema converter
├── Validation/
│   ├── HelixValidator.cs            # Core validation engine
│   ├── IFieldValidator.cs           # Custom validator interface
│   ├── AsyncValidationSupport.cs    # Async validation (e.g., server-side uniqueness)
│   └── ValidationMessage.razor      # Accessible error display
├── State/
│   ├── FormState.cs                 # Form state manager
│   ├── FormStateService.cs          # Auto-save, undo/redo
│   └── IFormPersistence.cs          # State persistence interface
└── HelixUI.FormBuilder.csproj
```

## Form Schema Example
```json
{
  "title": "Contact Form",
  "fields": [
    {
      "name": "fullName",
      "type": "text",
      "label": "Full Name",
      "required": true,
      "validation": { "minLength": 2, "maxLength": 100 }
    },
    {
      "name": "email",
      "type": "text",
      "label": "Email",
      "required": true,
      "validation": { "pattern": "email" }
    },
    {
      "name": "contactMethod",
      "type": "select",
      "label": "Preferred Contact",
      "options": ["Email", "Phone", "Mail"],
      "required": true
    },
    {
      "name": "phone",
      "type": "text",
      "label": "Phone Number",
      "conditional": { "field": "contactMethod", "equals": "Phone" }
    }
  ]
}
```

## Design Rules
1. **Model-driven AND schema-driven** — support both C# class attributes and JSON schema
2. **EditContext compatible** — integrate with Blazor's built-in form system, don't replace it
3. **Validation is pluggable** — built-in rules + FluentValidation + custom IFieldValidator
4. **Async validation** — first-class support (debounced, with loading indicators)
5. **Accessible errors** — aria-describedby linking, live region announcements, focus on first error
6. **State management** — auto-save to localStorage/sessionStorage, undo/redo stack
7. **File uploads** — work in both Server and WASM modes (streaming in Server, JS in WASM)

## Accessibility (Forms-Specific)
- Every field MUST have a visible label (no placeholder-only fields)
- Error messages linked via `aria-describedby`
- Required fields marked with `aria-required="true"` AND visual indicator
- Error summary at form top with links to errored fields
- Focus moves to first error on submit
- Live region announces validation changes
- Group related fields with `<fieldset>` + `<legend>`

## Two-Way Binding Pattern
```csharp
[Parameter] public TValue? Value { get; set; }
[Parameter] public EventCallback<TValue> ValueChanged { get; set; }
[Parameter] public Expression<Func<TValue>>? ValueExpression { get; set; }
```

All field components MUST implement this pattern for EditForm compatibility.

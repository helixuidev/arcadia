# HelixUI FormBuilder — Product Requirements Plan

## 1. Vision

The most feature-complete, schema-driven form builder in the Blazor ecosystem. A FormIO-caliber engine for .NET — where no Blazor competitor offers repeating sections, wizards, conditional logic engines, computed fields, or auto-save.

**Target users:** Enterprise .NET developers building data-entry-heavy LOB apps, workflow systems, CRM/ERP interfaces, and admin panels.

---

## 2. Competitive Gap (Why This Matters)

| Capability | Telerik | Syncfusion | DevExpress | MudBlazor | **HelixUI** |
|---|---|---|---|---|---|
| Auto-gen from model | Yes | Yes | Partial | No | **Yes** |
| JSON Schema rendering | No | No | No | No | **Yes** |
| Conditional visibility | No | Yes | No | No | **Yes** |
| Repeating sections | No | No | No | No | **Yes** |
| Wizard/multi-step | No | No | No | No | **Yes** |
| Computed fields | No | No | No | No | **Yes** |
| Cross-field validation | No | No | No | No | **Yes** |
| Auto-save + undo/redo | No | No | No | No | **Yes** |
| Async validation | No | No | No | Yes | **Yes** |
| Visual designer | No | No | No | No | **Phase 4** |

**No Blazor library offers more than 3 of these. We will ship all of them.**

---

## 3. Architecture Overview

```
HelixUI.FormBuilder/
├── Components/
│   ├── HelixForm.razor              # Form wrapper (validation, state, error summary)
│   ├── HelixFormBuilder.razor       # Dynamic renderer from FormSchema
│   ├── HelixWizard.razor            # Multi-step wizard container
│   ├── HelixWizardStep.razor        # Individual wizard step
│   ├── Fields/
│   │   ├── FieldBase.cs             # Shared field logic (label, errors, aria)
│   │   ├── TextField.razor          # Single-line text
│   │   ├── NumberField.razor        # Numeric with min/max/step
│   │   ├── TextAreaField.razor      # Multi-line text
│   │   ├── SelectField.razor        # Dropdown (single select)
│   │   ├── MultiSelectField.razor   # Multi-select with tags
│   │   ├── CheckboxField.razor      # Single checkbox (boolean)
│   │   ├── CheckboxGroupField.razor # Multiple checkboxes (string[])
│   │   ├── RadioGroupField.razor    # Radio buttons
│   │   ├── DateField.razor          # Date picker
│   │   ├── TimeField.razor          # Time picker
│   │   ├── DateRangeField.razor     # Date range (start/end)
│   │   ├── SwitchField.razor        # Toggle switch
│   │   ├── SliderField.razor        # Range slider
│   │   ├── ColorField.razor         # Color picker
│   │   ├── PasswordField.razor      # Password with visibility toggle
│   │   ├── MaskedField.razor        # Input mask (phone, SSN, etc.)
│   │   ├── FileField.razor          # File upload (single/multi)
│   │   ├── AutocompleteField.razor  # Combobox with async search
│   │   ├── RichTextField.razor      # Rich text / markdown input
│   │   ├── SignatureField.razor     # Signature capture (canvas)
│   │   ├── RatingField.razor        # Star rating
│   │   ├── RepeaterField.razor      # Repeating row group
│   │   └── HiddenField.razor        # Hidden value carrier
│   └── Layout/
│       ├── FormSection.razor        # Fieldset with legend, collapsible
│       ├── FormTabs.razor           # Tabbed form sections
│       ├── FormRow.razor            # Horizontal multi-column row
│       ├── FormDivider.razor        # Visual separator
│       ├── FormAlert.razor          # Inline alert/info banner
│       └── FormActions.razor        # Submit/cancel/reset bar
├── Schema/
│   ├── FormSchema.cs               # Top-level form definition
│   ├── FieldSchema.cs              # Field definition + options
│   ├── FieldType.cs                # Enum of all 23 field types
│   ├── FieldOption.cs              # Select/radio/checkbox option
│   ├── ConditionalRule.cs          # Show/hide/require/disable/compute
│   ├── ValidationRule.cs           # Built-in constraint rules
│   ├── ComputedField.cs            # Derived value expressions
│   ├── FormSectionSchema.cs        # Section layout definition
│   └── SchemaParser.cs             # JSON string → FormSchema deserializer
├── Validation/
│   ├── FieldValidator.cs           # Built-in rule engine
│   ├── IFieldValidator.cs          # Sync custom validator interface
│   ├── IAsyncFieldValidator.cs     # Async custom validator interface
│   ├── CrossFieldValidator.cs      # Cross-field / form-level rules
│   ├── FluentValidationAdapter.cs  # FluentValidation integration
│   └── ValidationSummary.razor     # Accessible error summary with field links
├── State/
│   ├── FormState.cs                # Runtime value/error/dirty/touched tracking
│   ├── FormStateService.cs         # Auto-save, undo/redo stack
│   ├── IFormPersistence.cs         # Storage adapter interface
│   ├── LocalStoragePersistence.cs  # localStorage implementation
│   └── SessionStoragePersistence.cs # sessionStorage implementation
├── ModelBinding/
│   ├── ModelFormGenerator.cs       # Reflection: C# class → FormSchema
│   ├── HelixFieldAttribute.cs     # [HelixField] for field type hints
│   ├── HelixSectionAttribute.cs   # [HelixSection] for grouping
│   └── HelixConditionAttribute.cs # [HelixCondition] for visibility rules
├── wwwroot/
│   ├── css/helix-forms.css         # Component styles
│   └── js/
│       ├── forms.js                # Auto-save, focus management
│       ├── signature.js            # Signature pad canvas
│       └── mask.js                 # Input masking
└── HelixUI.FormBuilder.csproj
```

---

## 4. Feature Requirements by Phase

### Phase 1 — Foundation ✅ SHIPPED
*Match the Telerik/Syncfusion baseline.*

- [x] FormSchema / FieldSchema / FieldType definitions
- [x] 11 field types: Text, Number, TextArea, Select, Checkbox, RadioGroup, Date, Switch, File, Autocomplete, Repeater
- [x] FieldBase with label, placeholder, helperText, required, disabled, readonly
- [x] Built-in validation: required, minLength, maxLength, min, max, pattern (email/url/phone)
- [x] Custom validator interface (IFieldValidator, IAsyncFieldValidator)
- [x] ConditionalRule engine (7 operators, 4 actions)
- [x] FormState (values, errors, submitted flag, defaults)
- [x] HelixForm wrapper with error summary and cascading state
- [x] HelixFormBuilder dynamic renderer with conditional visibility
- [x] FormSection (fieldset/legend, collapsible, multi-column)
- [x] FormRow, FormActions layout components
- [x] Full accessibility: aria-required, aria-invalid, aria-describedby, label[for], role="alert"
- [x] CSS stylesheet with theme token integration
- [x] 40 tests

### Phase 2 — Beat the Competition
*Features no other Blazor library has.*

**2A: Additional Field Types (12 new)**

- [ ] MultiSelectField — multi-select dropdown with tag chips, search filtering
- [ ] CheckboxGroupField — multiple checkboxes bound to string[]
- [ ] TimeField — time picker input
- [ ] DateRangeField — start/end date pair
- [ ] SliderField — range slider with min/max/step, optional dual-handle
- [ ] ColorField — color picker with hex/rgb input
- [ ] PasswordField — password input with show/hide toggle, strength meter
- [ ] MaskedField — input mask (phone: `(###) ###-####`, SSN: `###-##-####`, custom)
- [ ] RichTextField — rich text / markdown editor (minimal JS interop)
- [ ] SignatureField — signature capture canvas
- [ ] RatingField — star/heart rating (1–N)
- [ ] HiddenField — hidden value carrier for computed/system values

**2B: Wizard / Multi-Step Forms**

- [ ] HelixWizard container component
- [ ] HelixWizardStep with title, icon, description
- [ ] Step navigation (next/previous/go-to)
- [ ] Per-step validation (block progression if current step invalid)
- [ ] Step status indicators (completed, current, upcoming, error)
- [ ] Linear mode (must complete in order) and free-navigation mode
- [ ] Step completion summary/review before final submit
- [ ] Progress bar with percentage
- [ ] Keyboard accessible (arrow keys between steps)

**2C: Computed / Derived Fields**

- [ ] ComputedField schema definition
- [ ] Expression evaluator: `"total = quantity * price"` syntax
- [ ] Built-in functions: `sum()`, `avg()`, `count()`, `min()`, `max()`, `concat()`, `if()`
- [ ] Auto-recalculate on dependency change
- [ ] Display-only computed fields (non-editable, shown as text)
- [ ] Computed fields can drive conditional rules

**2D: Cross-Field Validation**

- [ ] CrossFieldValidator — validate relationships between fields
- [ ] Declarative rules: `"endDate must be after startDate"`
- [ ] Password confirmation: `"confirmPassword must equal password"`
- [ ] At-least-one: `"at least one of phone or email required"`
- [ ] Form-level error messages (not attached to a single field)

**2E: Async Validation**

- [ ] Debounced async validation (configurable delay, default 300ms)
- [ ] Loading spinner while validating
- [ ] Cancellation of stale requests
- [ ] Server-side uniqueness check pattern (e.g., "email already taken")
- [ ] Validation caching (don't re-validate unchanged values)

**2F: JSON Schema Support**

- [ ] SchemaParser: deserialize JSON string → FormSchema
- [ ] Support standard JSON Schema draft-07 subset (type, required, enum, minLength, maxLength, minimum, maximum, pattern, format)
- [ ] Custom `x-helix-*` extensions for HelixUI-specific features (conditions, sections, field types)
- [ ] Round-trip: FormSchema → JSON → FormSchema

### Phase 3 — Enterprise Grade
*Premium features for serious production use.*

**3A: State Persistence & Auto-Save**

- [ ] FormStateService with auto-save to pluggable storage
- [ ] IFormPersistence interface
- [ ] LocalStoragePersistence implementation (JS interop)
- [ ] SessionStoragePersistence implementation
- [ ] Configurable auto-save interval (default 5s, debounced)
- [ ] Undo/redo stack (configurable depth, default 50)
- [ ] Dirty tracking per field (`touched`, `dirty`, `pristine` states)
- [ ] Unsaved changes warning on navigation
- [ ] Resume from saved state on page reload

**3B: Model-Driven Form Generation**

- [ ] ModelFormGenerator: reflect over C# class → FormSchema
- [ ] Respects DataAnnotations: [Required], [StringLength], [Range], [EmailAddress], [Display], [DataType]
- [ ] [HelixField] attribute: override field type, placeholder, helperText, order, columnSpan
- [ ] [HelixSection] attribute: group properties into sections
- [ ] [HelixCondition] attribute: conditional visibility from model
- [ ] [HelixIgnore] attribute: exclude properties
- [ ] Enum → Select/RadioGroup automatic mapping
- [ ] Nested object → FormSection mapping
- [ ] Collection property → RepeaterField mapping

**3C: FluentValidation Integration**

- [ ] FluentValidationAdapter: bridge FluentValidation rules to HelixUI validation
- [ ] Auto-extract rules from AbstractValidator<T>
- [ ] Support for RuleFor, Must, MustAsync, When, Unless
- [ ] Show FluentValidation messages in field error display
- [ ] No hard dependency — adapter loaded via DI if FluentValidation is present

**3D: Advanced Layout**

- [ ] FormTabs — tabbed form sections with tab bar
- [ ] Tab validation indicators (error dot/count per tab)
- [ ] FormDivider — visual horizontal rule with optional label
- [ ] FormAlert — inline info/warning/error banner within form
- [ ] Responsive column breakpoints (2-col on desktop → 1-col on mobile)
- [ ] Field-level column span override

**3E: Accessibility Enhancements**

- [ ] Focus management: auto-focus first error on submit
- [ ] Error summary with clickable links to errored fields
- [ ] Live region announcements for async validation results
- [ ] Field group descriptions via aria-describedby on fieldsets
- [ ] High contrast mode support
- [ ] Screen reader announcements for computed field changes
- [ ] Keyboard-operable repeater (add/remove via keyboard)

### Phase 4 — Differentiation (Long-Term)

**4A: Visual Form Designer**

- [ ] Drag-and-drop field placement
- [ ] Property panel for field configuration
- [ ] Live preview
- [ ] Outputs FormSchema JSON
- [ ] Import existing FormSchema for editing
- [ ] Section/tab drag arrangement

**4B: Form Analytics**

- [ ] Track time-per-field
- [ ] Completion rate tracking
- [ ] Drop-off point identification
- [ ] Field revision count (how many times was a field edited)
- [ ] Export analytics data

**4C: Advanced Features**

- [ ] Role-based field visibility (`[HelixRole("admin")]`)
- [ ] Form versioning with diff
- [ ] PDF generation from form submission
- [ ] Offline mode (IndexedDB) + sync-when-online
- [ ] Form templates marketplace / gallery
- [ ] Localization / i18n for labels and validation messages

---

## 5. API Surface Examples

### Schema-Driven (JSON)
```csharp
var schema = SchemaParser.Parse(jsonString);

<HelixFormBuilder Schema="@schema"
                  OnValidSubmit="HandleSubmit" />
```

### Schema-Driven (C# Fluent)
```csharp
var schema = new FormSchema
{
    Title = "Employee Onboarding",
    Columns = 2,
    Sections = new()
    {
        new()
        {
            Title = "Personal Information",
            Fields = new()
            {
                new() { Name = "firstName", Type = FieldType.Text, Label = "First Name", Required = true, ColumnSpan = 6 },
                new() { Name = "lastName", Type = FieldType.Text, Label = "Last Name", Required = true, ColumnSpan = 6 },
                new() { Name = "dob", Type = FieldType.Date, Label = "Date of Birth" },
                new() { Name = "department", Type = FieldType.Select, Label = "Department", Options = deptOptions },
            }
        },
        new()
        {
            Title = "Emergency Contact",
            Collapsible = true,
            Fields = new()
            {
                new() { Name = "emergencyName", Type = FieldType.Text, Label = "Contact Name" },
                new() { Name = "emergencyPhone", Type = FieldType.Masked, Label = "Phone", Placeholder = "(###) ###-####" },
            }
        }
    }
};
```

### Model-Driven (C# Class + Attributes)
```csharp
public class EmployeeForm
{
    [HelixSection("Personal Information")]
    [Required, StringLength(50)]
    [HelixField(Placeholder = "John", ColumnSpan = 6)]
    public string FirstName { get; set; }

    [HelixField(Placeholder = "Doe", ColumnSpan = 6)]
    [Required]
    public string LastName { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [HelixField(Type = FieldType.Select, Options = new[] { "Engineering", "Sales", "HR" })]
    public string Department { get; set; }

    [HelixSection("Preferences")]
    [HelixCondition(Field = "Department", Equals = "Engineering")]
    [HelixField(Type = FieldType.MultiSelect)]
    public string[] ProgrammingLanguages { get; set; }
}

// Usage:
<HelixFormBuilder Model="typeof(EmployeeForm)"
                  OnValidSubmit="HandleSubmit" />
```

### Wizard Form
```csharp
<HelixWizard OnComplete="HandleComplete">
    <HelixWizardStep Title="Personal Info" Icon="user">
        <TextField Label="Name" @bind-Value="model.Name" Required />
        <DateField Label="DOB" @bind-Value="model.DateOfBirth" />
    </HelixWizardStep>

    <HelixWizardStep Title="Contact" Icon="phone">
        <TextField Label="Email" @bind-Value="model.Email" Required />
        <TextField Label="Phone" @bind-Value="model.Phone" />
    </HelixWizardStep>

    <HelixWizardStep Title="Review" Icon="check">
        <!-- Summary of all entered data -->
    </HelixWizardStep>
</HelixWizard>
```

---

## 6. Non-Functional Requirements

- **Performance:** Form with 50 fields must render in < 100ms
- **Bundle size:** < 50KB CSS, < 20KB JS (before gzip)
- **Render mode agnostic:** Server, WASM, Auto (net8+)
- **Multi-target:** net5.0 through net9.0
- **Zero external C# dependencies** (except optional FluentValidation adapter)
- **WCAG 2.1 AA** for all field components
- **Test coverage:** ≥ 80% unit tests

---

## 7. Milestones

| Phase | Scope | Field Types | Est. Files |
|---|---|---|---|
| **Phase 1** ✅ | Foundation | 11 | 36 |
| **Phase 2** | Beat competition | +12 fields, wizard, computed, async validation, JSON parser | ~40 |
| **Phase 3** | Enterprise | Model binding, FluentValidation, auto-save, advanced layout | ~25 |
| **Phase 4** | Differentiation | Visual designer, analytics, PDF, offline | ~30 |

---

## 8. Open Questions

1. **Rich text editor:** Build our own minimal editor or wrap an existing JS library (Quill, TipTap)?
2. **Signature field:** Canvas-based — do we need to support touch + stylus on mobile?
3. **Visual designer:** Should this be a separate package (`HelixUI.FormDesigner`) or part of FormBuilder?
4. **JSON Schema standard:** Full draft-07 compliance or pragmatic subset with `x-helix` extensions?
5. **FluentValidation:** Hard dependency vs. optional adapter package (`HelixUI.FormBuilder.FluentValidation`)?

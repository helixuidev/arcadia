namespace Arcadia.FormBuilder.Schema;

/// <summary>
/// Defines the schema for a single form field, including its type,
/// label, validation rules, and conditional visibility.
/// </summary>
public class FieldSchema
{
    /// <summary>
    /// Gets or sets the unique field name (used as the form value key).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field type to render.
    /// </summary>
    public FieldType Type { get; set; } = FieldType.Text;

    /// <summary>
    /// Gets or sets the visible label for the field.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets placeholder text.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets helper text displayed below the field.
    /// </summary>
    public string? HelperText { get; set; }

    /// <summary>
    /// Gets or sets whether the field is required.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets whether the field is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the field is read-only.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the default value for the field.
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the available options for Select, RadioGroup, and Autocomplete fields.
    /// </summary>
    public List<FieldOption>? Options { get; set; }

    /// <summary>
    /// Gets or sets validation rules for this field.
    /// </summary>
    public ValidationRule? Validation { get; set; }

    /// <summary>
    /// Gets or sets conditional rules that control visibility/state of this field.
    /// </summary>
    public List<ConditionalRule>? Conditions { get; set; }

    /// <summary>
    /// Gets or sets the column span in a multi-column layout (1-12).
    /// </summary>
    public int ColumnSpan { get; set; } = 12;

    /// <summary>
    /// Gets or sets the child field schemas for Repeater fields.
    /// </summary>
    public List<FieldSchema>? Children { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for this field's container.
    /// </summary>
    public string? Class { get; set; }
}

/// <summary>
/// Represents a selectable option for Select, RadioGroup, and Autocomplete fields.
/// </summary>
public class FieldOption
{
    /// <summary>
    /// Gets or sets the display text.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the underlying value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this option is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Creates an option from a simple string (label and value are the same).
    /// </summary>
    public static implicit operator FieldOption(string value) => new() { Label = value, Value = value };
}

using HelixUI.FormBuilder.Schema;

namespace HelixUI.FormBuilder.ModelBinding;

/// <summary>
/// Configures how a property is rendered as a form field.
/// Overrides automatic type inference from the property type.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class HelixFieldAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the explicit field type. If not set, inferred from property type.
    /// </summary>
    public FieldType Type { get; set; } = (FieldType)(-1);

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the helper text displayed below the field.
    /// </summary>
    public string? HelperText { get; set; }

    /// <summary>
    /// Gets or sets the display order (lower = first). Default is int.MaxValue (natural order).
    /// </summary>
    public int Order { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets the column span in a multi-column layout (1–12).
    /// </summary>
    public int ColumnSpan { get; set; } = 12;

    /// <summary>
    /// Gets or sets whether the field type was explicitly set.
    /// </summary>
    internal bool HasExplicitType => (int)Type != -1;
}

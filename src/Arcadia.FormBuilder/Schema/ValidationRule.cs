namespace Arcadia.FormBuilder.Schema;

/// <summary>
/// Defines a validation constraint on a form field.
/// </summary>
public class ValidationRule
{
    /// <summary>
    /// Gets or sets the minimum length for string values.
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum length for string values.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the minimum value for numeric fields.
    /// </summary>
    public double? Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for numeric fields.
    /// </summary>
    public double? Max { get; set; }

    /// <summary>
    /// Gets or sets a regex pattern the value must match.
    /// Use "email", "url", "phone" for built-in patterns.
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Gets or sets a custom error message for this rule.
    /// </summary>
    public string? Message { get; set; }
}

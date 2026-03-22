namespace Arcadia.FormBuilder.Schema;

/// <summary>
/// Defines a condition that controls field visibility or required state
/// based on the value of another field.
/// </summary>
public class ConditionalRule
{
    /// <summary>
    /// Gets or sets the name of the field this condition depends on.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operator for comparison.
    /// </summary>
    public ConditionalOperator Operator { get; set; } = ConditionalOperator.Equals;

    /// <summary>
    /// Gets or sets the value to compare against.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the action to take when the condition is met.
    /// </summary>
    public ConditionalAction Action { get; set; } = ConditionalAction.Show;

    /// <summary>
    /// Evaluates this condition against a set of form values.
    /// </summary>
    /// <param name="formValues">The current form field values keyed by field name.</param>
    public bool Evaluate(IReadOnlyDictionary<string, object?> formValues)
    {
        if (!formValues.TryGetValue(Field, out var currentValue))
            return false;

        return Operator switch
        {
            ConditionalOperator.Equals => Equals(currentValue?.ToString(), Value?.ToString()),
            ConditionalOperator.NotEquals => !Equals(currentValue?.ToString(), Value?.ToString()),
            ConditionalOperator.Contains => currentValue?.ToString()?.Contains(Value?.ToString() ?? "", StringComparison.OrdinalIgnoreCase) == true,
            ConditionalOperator.IsEmpty => string.IsNullOrEmpty(currentValue?.ToString()),
            ConditionalOperator.IsNotEmpty => !string.IsNullOrEmpty(currentValue?.ToString()),
            ConditionalOperator.GreaterThan => CompareNumeric(currentValue, Value) > 0,
            ConditionalOperator.LessThan => CompareNumeric(currentValue, Value) < 0,
            _ => false
        };
    }

    private static int CompareNumeric(object? a, object? b)
    {
        if (double.TryParse(a?.ToString(), out var da) && double.TryParse(b?.ToString(), out var db))
            return da.CompareTo(db);
        return 0;
    }
}

/// <summary>
/// Comparison operators for conditional rules.
/// </summary>
public enum ConditionalOperator
{
    /// <summary>Field value equals the target value.</summary>
    Equals,
    /// <summary>Field value does not equal the target value.</summary>
    NotEquals,
    /// <summary>Field value contains the target value.</summary>
    Contains,
    /// <summary>Field value is null or empty.</summary>
    IsEmpty,
    /// <summary>Field value is not null or empty.</summary>
    IsNotEmpty,
    /// <summary>Field value is greater than the target value.</summary>
    GreaterThan,
    /// <summary>Field value is less than the target value.</summary>
    LessThan
}

/// <summary>
/// Actions to take when a conditional rule is satisfied.
/// </summary>
public enum ConditionalAction
{
    /// <summary>Show the field.</summary>
    Show,
    /// <summary>Hide the field.</summary>
    Hide,
    /// <summary>Make the field required.</summary>
    Require,
    /// <summary>Disable the field.</summary>
    Disable
}

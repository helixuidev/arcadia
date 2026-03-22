using System.Text.RegularExpressions;
using Arcadia.FormBuilder.Schema;

namespace Arcadia.FormBuilder.Validation;

/// <summary>
/// Built-in field validator that evaluates <see cref="ValidationRule"/> and required constraints.
/// </summary>
public static class FieldValidator
{
    private static readonly Dictionary<string, string> BuiltInPatterns = new()
    {
        ["email"] = @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        ["url"] = @"^https?://\S+$",
        ["phone"] = @"^[\+]?[\d\s\-\(\)]{7,}$"
    };

    /// <summary>
    /// Validates a field value against its schema definition.
    /// </summary>
    /// <param name="field">The field schema.</param>
    /// <param name="value">The current value.</param>
    /// <returns>A list of error messages, empty if valid.</returns>
    public static List<string> Validate(FieldSchema field, object? value)
    {
        var errors = new List<string>();
        var strValue = value?.ToString();
        var isEmpty = string.IsNullOrWhiteSpace(strValue);

        // Required check
        if (field.Required && isEmpty)
        {
            errors.Add(field.Validation?.Message ?? $"{field.Label} is required.");
            return errors; // No point validating further
        }

        if (isEmpty)
            return errors; // Not required and empty — valid

        var rules = field.Validation;
        if (rules is null)
            return errors;

        // MinLength
        if (rules.MinLength.HasValue && strValue!.Length < rules.MinLength.Value)
        {
            errors.Add(rules.Message ?? $"{field.Label} must be at least {rules.MinLength} characters.");
        }

        // MaxLength
        if (rules.MaxLength.HasValue && strValue!.Length > rules.MaxLength.Value)
        {
            errors.Add(rules.Message ?? $"{field.Label} must be at most {rules.MaxLength} characters.");
        }

        // Min (numeric)
        if (rules.Min.HasValue && double.TryParse(strValue, out var numVal) && numVal < rules.Min.Value)
        {
            errors.Add(rules.Message ?? $"{field.Label} must be at least {rules.Min}.");
        }

        // Max (numeric)
        if (rules.Max.HasValue && double.TryParse(strValue, out var numVal2) && numVal2 > rules.Max.Value)
        {
            errors.Add(rules.Message ?? $"{field.Label} must be at most {rules.Max}.");
        }

        // Pattern
        if (!string.IsNullOrEmpty(rules.Pattern))
        {
            var pattern = BuiltInPatterns.TryGetValue(rules.Pattern, out var builtIn)
                ? builtIn
                : rules.Pattern;

            if (!Regex.IsMatch(strValue!, pattern))
            {
                errors.Add(rules.Message ?? $"{field.Label} is not in a valid format.");
            }
        }

        return errors;
    }
}

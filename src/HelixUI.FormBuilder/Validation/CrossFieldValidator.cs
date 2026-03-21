namespace HelixUI.FormBuilder.Validation;

/// <summary>
/// Validates relationships between multiple form fields.
/// Add rules declaratively, evaluated on form submission.
/// </summary>
public class CrossFieldValidator : IFieldValidator
{
    private readonly List<CrossFieldRule> _rules = new();

    /// <summary>
    /// Adds a rule that a field must be after another field (for dates/numbers).
    /// </summary>
    public CrossFieldValidator MustBeAfter(string fieldName, string afterField, string? message = null)
    {
        _rules.Add(new CrossFieldRule
        {
            TargetField = fieldName,
            DependencyField = afterField,
            Type = CrossFieldRuleType.After,
            Message = message
        });
        return this;
    }

    /// <summary>
    /// Adds a rule that two fields must have equal values (e.g., password confirmation).
    /// </summary>
    public CrossFieldValidator MustEqual(string fieldName, string otherField, string? message = null)
    {
        _rules.Add(new CrossFieldRule
        {
            TargetField = fieldName,
            DependencyField = otherField,
            Type = CrossFieldRuleType.Equal,
            Message = message
        });
        return this;
    }

    /// <summary>
    /// Adds a rule that at least one of the specified fields must have a value.
    /// </summary>
    public CrossFieldValidator AtLeastOneRequired(string[] fieldNames, string? message = null)
    {
        foreach (var field in fieldNames)
        {
            _rules.Add(new CrossFieldRule
            {
                TargetField = field,
                DependencyFields = fieldNames,
                Type = CrossFieldRuleType.AtLeastOne,
                Message = message
            });
        }
        return this;
    }

    /// <inheritdoc />
    public IEnumerable<string> Validate(string fieldName, object? value, IReadOnlyDictionary<string, object?> formValues)
    {
        foreach (var rule in _rules.Where(r => r.TargetField == fieldName))
        {
            var error = EvaluateRule(rule, value, formValues);
            if (error is not null)
                yield return error;
        }
    }

    private static string? EvaluateRule(CrossFieldRule rule, object? value, IReadOnlyDictionary<string, object?> formValues)
    {
        switch (rule.Type)
        {
            case CrossFieldRuleType.Equal:
            {
                formValues.TryGetValue(rule.DependencyField!, out var otherValue);
                if (!Equals(value?.ToString(), otherValue?.ToString()))
                    return rule.Message ?? $"{rule.TargetField} must match {rule.DependencyField}.";
                break;
            }

            case CrossFieldRuleType.After:
            {
                formValues.TryGetValue(rule.DependencyField!, out var otherValue);
                if (DateTime.TryParse(value?.ToString(), out var date1) &&
                    DateTime.TryParse(otherValue?.ToString(), out var date2) &&
                    date1 <= date2)
                    return rule.Message ?? $"{rule.TargetField} must be after {rule.DependencyField}.";

                if (double.TryParse(value?.ToString(), out var num1) &&
                    double.TryParse(otherValue?.ToString(), out var num2) &&
                    num1 <= num2)
                    return rule.Message ?? $"{rule.TargetField} must be greater than {rule.DependencyField}.";
                break;
            }

            case CrossFieldRuleType.AtLeastOne:
            {
                if (rule.DependencyFields is not null)
                {
                    var anyFilled = rule.DependencyFields.Any(f =>
                        formValues.TryGetValue(f, out var v) && !string.IsNullOrWhiteSpace(v?.ToString()));
                    if (!anyFilled)
                        return rule.Message ?? $"At least one of {string.Join(", ", rule.DependencyFields)} is required.";
                }
                break;
            }
        }

        return null;
    }
}

internal class CrossFieldRule
{
    public string TargetField { get; set; } = string.Empty;
    public string? DependencyField { get; set; }
    public string[]? DependencyFields { get; set; }
    public CrossFieldRuleType Type { get; set; }
    public string? Message { get; set; }
}

internal enum CrossFieldRuleType
{
    Equal,
    After,
    AtLeastOne
}

namespace Arcadia.FormBuilder;

/// <summary>
/// Manages the runtime state of a dynamic form, including field values,
/// validation errors, and conditional visibility.
/// </summary>
public class FormState
{
    private readonly Dictionary<string, object?> _values = new();
    private readonly Dictionary<string, List<string>> _errors = new();

    /// <summary>
    /// Gets all current form values.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Values => _values;

    /// <summary>
    /// Gets all current validation errors.
    /// </summary>
    public IReadOnlyDictionary<string, List<string>> Errors => _errors;

    /// <summary>
    /// Gets whether the form has been submitted at least once.
    /// </summary>
    public bool IsSubmitted { get; private set; }

    /// <summary>
    /// Gets whether the form has any validation errors.
    /// </summary>
    public bool HasErrors => _errors.Values.Any(e => e.Count > 0);

    /// <summary>
    /// Raised when any form value changes.
    /// </summary>
    public event Action? OnValuesChanged;

    /// <summary>
    /// Raised when validation errors change.
    /// </summary>
    public event Action? OnErrorsChanged;

    /// <summary>
    /// Gets a field value by name.
    /// </summary>
    public object? GetValue(string fieldName)
    {
        return _values.TryGetValue(fieldName, out var value) ? value : null;
    }

    /// <summary>
    /// Gets a typed field value by name.
    /// </summary>
    public T? GetValue<T>(string fieldName)
    {
        var value = GetValue(fieldName);
        if (value is T typed)
            return typed;
        if (value is null)
            return default;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Sets a field value and notifies subscribers.
    /// </summary>
    public void SetValue(string fieldName, object? value)
    {
        _values[fieldName] = value;
        OnValuesChanged?.Invoke();
    }

    /// <summary>
    /// Gets the validation errors for a field.
    /// </summary>
    public IReadOnlyList<string> GetErrors(string fieldName)
    {
        return _errors.TryGetValue(fieldName, out var errors) ? errors : Array.Empty<string>();
    }

    /// <summary>
    /// Sets validation errors for a field.
    /// </summary>
    public void SetErrors(string fieldName, List<string> errors)
    {
        if (errors.Count == 0)
            _errors.Remove(fieldName);
        else
            _errors[fieldName] = errors;

        OnErrorsChanged?.Invoke();
    }

    /// <summary>
    /// Clears all validation errors.
    /// </summary>
    public void ClearErrors()
    {
        _errors.Clear();
        OnErrorsChanged?.Invoke();
    }

    /// <summary>
    /// Marks the form as submitted.
    /// </summary>
    public void MarkSubmitted()
    {
        IsSubmitted = true;
    }

    /// <summary>
    /// Initializes default values from a schema.
    /// </summary>
    public void InitializeDefaults(Schema.FormSchema schema)
    {
        foreach (var field in schema.AllFields)
        {
            if (field.DefaultValue is not null && !_values.ContainsKey(field.Name))
            {
                _values[field.Name] = field.DefaultValue;
            }
        }
    }
}

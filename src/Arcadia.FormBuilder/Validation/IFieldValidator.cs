namespace Arcadia.FormBuilder.Validation;

/// <summary>
/// Interface for custom field validators. Implement this to add
/// custom validation logic beyond the built-in rules.
/// </summary>
public interface IFieldValidator
{
    /// <summary>
    /// Validates a field value and returns any error messages.
    /// </summary>
    /// <param name="fieldName">The field name being validated.</param>
    /// <param name="value">The current field value.</param>
    /// <param name="formValues">All current form values for cross-field validation.</param>
    /// <returns>An enumerable of error messages, or empty if valid.</returns>
    IEnumerable<string> Validate(string fieldName, object? value, IReadOnlyDictionary<string, object?> formValues);
}

/// <summary>
/// Interface for async field validators (e.g., server-side uniqueness checks).
/// </summary>
public interface IAsyncFieldValidator
{
    /// <summary>
    /// Validates a field value asynchronously.
    /// </summary>
    /// <param name="fieldName">The field name being validated.</param>
    /// <param name="value">The current field value.</param>
    /// <param name="formValues">All current form values for cross-field validation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An enumerable of error messages, or empty if valid.</returns>
    Task<IEnumerable<string>> ValidateAsync(string fieldName, object? value, IReadOnlyDictionary<string, object?> formValues, CancellationToken cancellationToken = default);
}

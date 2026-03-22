namespace Arcadia.FormBuilder.State;

/// <summary>
/// Interface for persisting form state to external storage.
/// Implement this to provide custom storage backends (localStorage, API, etc.).
/// </summary>
public interface IFormPersistence
{
    /// <summary>
    /// Saves form state to storage.
    /// </summary>
    /// <param name="formId">A unique identifier for the form instance.</param>
    /// <param name="values">The form field values to persist.</param>
    Task SaveAsync(string formId, IReadOnlyDictionary<string, object?> values);

    /// <summary>
    /// Loads form state from storage.
    /// </summary>
    /// <param name="formId">The form identifier.</param>
    /// <returns>The stored values, or null if none exist.</returns>
    Task<Dictionary<string, object?>?> LoadAsync(string formId);

    /// <summary>
    /// Removes stored form state.
    /// </summary>
    /// <param name="formId">The form identifier.</param>
    Task ClearAsync(string formId);
}

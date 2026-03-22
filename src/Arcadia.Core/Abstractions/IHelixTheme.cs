namespace Arcadia.Core.Abstractions;

/// <summary>
/// Defines the contract for a HelixUI theme provider.
/// Themes are expressed as CSS custom properties applied to a root element.
/// </summary>
public interface IHelixTheme
{
    /// <summary>
    /// Gets the unique name of the theme (e.g., "light", "dark", "corporate").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the CSS custom properties that define this theme.
    /// Keys should include the -- prefix (e.g., "--arcadia-color-primary").
    /// </summary>
    IReadOnlyDictionary<string, string> Properties { get; }

    /// <summary>
    /// Gets a specific theme property value by name.
    /// </summary>
    /// <param name="propertyName">The CSS custom property name (e.g., "--arcadia-color-primary").</param>
    /// <returns>The property value, or null if not defined.</returns>
    string? GetProperty(string propertyName);
}

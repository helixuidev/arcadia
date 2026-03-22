using Arcadia.Core.Abstractions;

namespace Arcadia.Theme;

/// <summary>
/// Base implementation of <see cref="IHelixTheme"/> backed by a dictionary of CSS custom properties.
/// </summary>
public abstract class HelixThemeBase : IHelixTheme
{
    private readonly Dictionary<string, string> _properties = new();

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Properties => _properties;

    /// <inheritdoc />
    public string? GetProperty(string propertyName)
    {
        return _properties.TryGetValue(propertyName, out var value) ? value : null;
    }

    /// <summary>
    /// Sets a CSS custom property value. Used by derived themes to define their token values.
    /// </summary>
    /// <param name="propertyName">The CSS custom property name (e.g., "--arcadia-color-primary").</param>
    /// <param name="value">The property value.</param>
    protected void Set(string propertyName, string value)
    {
        _properties[propertyName] = value;
    }
}

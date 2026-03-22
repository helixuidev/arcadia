using System.Text;

namespace Arcadia.Core.Utilities;

/// <summary>
/// Fluent builder for constructing inline CSS style strings.
/// </summary>
public readonly struct StyleBuilder
{
    private readonly StringBuilder _builder;

    private StyleBuilder(string? initialStyle)
    {
        _builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(initialStyle))
        {
            _builder.Append(initialStyle);
            if (!initialStyle.EndsWith(";", StringComparison.Ordinal))
                _builder.Append(';');
        }
    }

    /// <summary>
    /// Creates a new <see cref="StyleBuilder"/> with an optional initial style.
    /// </summary>
    /// <param name="initialStyle">An initial style string.</param>
    public static StyleBuilder Default(string? initialStyle = null) => new(initialStyle);

    /// <summary>
    /// Adds a style property unconditionally.
    /// </summary>
    /// <param name="property">The CSS property name (e.g., "color").</param>
    /// <param name="value">The CSS property value (e.g., "red").</param>
    public StyleBuilder AddStyle(string property, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (_builder.Length > 0)
                _builder.Append(' ');
            _builder.Append(property).Append(": ").Append(value).Append(';');
        }

        return this;
    }

    /// <summary>
    /// Adds a style property when the condition is true.
    /// </summary>
    /// <param name="property">The CSS property name.</param>
    /// <param name="value">The CSS property value.</param>
    /// <param name="when">The condition that must be true to add the style.</param>
    public StyleBuilder AddStyle(string property, string? value, bool when)
    {
        return when ? AddStyle(property, value) : this;
    }

    /// <summary>
    /// Adds a style property when the condition function returns true.
    /// </summary>
    /// <param name="property">The CSS property name.</param>
    /// <param name="value">The CSS property value.</param>
    /// <param name="when">A function returning the condition.</param>
    public StyleBuilder AddStyle(string property, string? value, Func<bool> when)
    {
        return when() ? AddStyle(property, value) : this;
    }

    /// <summary>
    /// Adds a raw style string unconditionally.
    /// </summary>
    /// <param name="style">A raw CSS style string (e.g., "color: red; font-size: 12px").</param>
    public StyleBuilder AddRaw(string? style)
    {
        if (!string.IsNullOrWhiteSpace(style))
        {
            if (_builder.Length > 0)
                _builder.Append(' ');
            _builder.Append(style);
            if (!style.EndsWith(";", StringComparison.Ordinal))
                _builder.Append(';');
        }

        return this;
    }

    /// <summary>
    /// Builds the final inline style string.
    /// </summary>
    public string? Build()
    {
        var result = _builder.ToString().Trim();
        return result.Length == 0 ? null : result;
    }

    /// <inheritdoc />
    public override string ToString() => Build() ?? string.Empty;
}

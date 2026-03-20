using System.Text;

namespace HelixUI.Core.Utilities;

/// <summary>
/// Fluent builder for constructing CSS class strings.
/// </summary>
public readonly struct CssBuilder
{
    private readonly StringBuilder _builder;

    private CssBuilder(string? initialClass)
    {
        _builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(initialClass))
        {
            _builder.Append(initialClass);
        }
    }

    /// <summary>
    /// Creates a new <see cref="CssBuilder"/> with an optional default class.
    /// </summary>
    /// <param name="defaultClass">The default CSS class to start with.</param>
    public static CssBuilder Default(string? defaultClass = null) => new(defaultClass);

    /// <summary>
    /// Adds a CSS class unconditionally.
    /// </summary>
    /// <param name="cssClass">The CSS class to add.</param>
    public CssBuilder AddClass(string? cssClass)
    {
        if (!string.IsNullOrWhiteSpace(cssClass))
        {
            if (_builder.Length > 0)
                _builder.Append(' ');
            _builder.Append(cssClass);
        }

        return this;
    }

    /// <summary>
    /// Adds a CSS class when the condition is true.
    /// </summary>
    /// <param name="cssClass">The CSS class to add.</param>
    /// <param name="when">The condition that must be true to add the class.</param>
    public CssBuilder AddClass(string? cssClass, bool when)
    {
        return when ? AddClass(cssClass) : this;
    }

    /// <summary>
    /// Adds a CSS class when the condition function returns true.
    /// </summary>
    /// <param name="cssClass">The CSS class to add.</param>
    /// <param name="when">A function returning the condition.</param>
    public CssBuilder AddClass(string? cssClass, Func<bool> when)
    {
        return when() ? AddClass(cssClass) : this;
    }

    /// <summary>
    /// Adds a CSS class produced by a factory when the condition is true.
    /// </summary>
    /// <param name="cssClassFactory">A function that produces the CSS class.</param>
    /// <param name="when">The condition that must be true to add the class.</param>
    public CssBuilder AddClass(Func<string?> cssClassFactory, bool when)
    {
        return when ? AddClass(cssClassFactory()) : this;
    }

    /// <summary>
    /// Builds the final CSS class string.
    /// </summary>
    public string? Build()
    {
        var result = _builder.ToString().Trim();
        return result.Length == 0 ? null : result;
    }

    /// <inheritdoc />
    public override string ToString() => Build() ?? string.Empty;
}

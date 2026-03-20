namespace HelixUI.Core.Abstractions;

/// <summary>
/// Interface for components that accept an inline style parameter.
/// </summary>
public interface IHasStyle
{
    /// <summary>
    /// Gets or sets inline styles to apply to the component's root element.
    /// </summary>
    string? Style { get; set; }
}

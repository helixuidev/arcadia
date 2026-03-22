namespace Arcadia.Core.Abstractions;

/// <summary>
/// Interface for components that accept an additional CSS class parameter.
/// </summary>
public interface IHasClass
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the component's root element.
    /// </summary>
    string? Class { get; set; }
}

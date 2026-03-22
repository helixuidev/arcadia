namespace Arcadia.Core.Abstractions;

/// <summary>
/// Interface for components that support a disabled state.
/// </summary>
public interface IHasDisabled
{
    /// <summary>
    /// Gets or sets whether the component is disabled.
    /// </summary>
    bool Disabled { get; set; }
}

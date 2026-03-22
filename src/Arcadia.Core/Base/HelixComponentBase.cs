using Microsoft.AspNetCore.Components;

namespace Arcadia.Core.Base;

/// <summary>
/// Base class for all HelixUI components. Provides common parameters for
/// CSS classes, inline styles, and arbitrary HTML attributes.
/// </summary>
public abstract class HelixComponentBase : ComponentBase, Abstractions.IHasClass, Abstractions.IHasStyle
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the component's root element.
    /// These are appended to the component's default classes, never replacing them.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets inline styles to apply to the component's root element.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied
    /// to the component's root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the component's unique element ID for accessibility purposes.
    /// Lazily generated on first access.
    /// </summary>
    protected string ElementId => _elementId ??= Utilities.IdGenerator.Generate();
    private string? _elementId;
}

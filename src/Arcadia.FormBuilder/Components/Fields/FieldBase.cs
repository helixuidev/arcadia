using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.FormBuilder.Schema;

namespace Arcadia.FormBuilder.Components.Fields;

/// <summary>
/// Base class for all form field components. Provides common parameters
/// for labels, helper text, validation display, and accessibility.
/// </summary>
public abstract class FieldBase : Core.Base.HelixComponentBase
{
    /// <summary>
    /// Gets or sets the field schema definition.
    /// </summary>
    [Parameter]
    public FieldSchema? Schema { get; set; }

    /// <summary>
    /// Gets or sets the visible label.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets placeholder text.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets helper text below the field.
    /// </summary>
    [Parameter]
    public string? HelperText { get; set; }

    /// <summary>
    /// Gets or sets whether the field is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets whether the field is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the field is read-only.
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the validation errors to display.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? Errors { get; set; }

    /// <summary>
    /// Gets the effective label from parameter or schema.
    /// </summary>
    protected string EffectiveLabel => Label ?? Schema?.Label ?? string.Empty;

    /// <summary>
    /// Gets the effective placeholder from parameter or schema.
    /// </summary>
    protected string? EffectivePlaceholder => Placeholder ?? Schema?.Placeholder;

    /// <summary>
    /// Gets the effective helper text from parameter or schema.
    /// </summary>
    protected string? EffectiveHelperText => HelperText ?? Schema?.HelperText;

    /// <summary>
    /// Gets whether the field is effectively required from parameter or schema.
    /// </summary>
    protected bool EffectiveRequired => Required || (Schema?.Required ?? false);

    /// <summary>
    /// Gets whether the field is effectively disabled from parameter or schema.
    /// </summary>
    protected bool EffectiveDisabled => Disabled || (Schema?.Disabled ?? false);

    /// <summary>
    /// Gets whether there are any validation errors.
    /// </summary>
    protected bool HasErrors => Errors is { Count: > 0 };

    /// <summary>
    /// Gets the ID for the error message container (for aria-describedby).
    /// </summary>
    protected string ErrorId => $"{ElementId}-error";

    /// <summary>
    /// Gets the ID for the helper text (for aria-describedby).
    /// </summary>
    protected string HelperId => $"{ElementId}-helper";

    /// <summary>
    /// Builds the aria-describedby value from error and helper IDs.
    /// </summary>
    protected string? AriaDescribedBy
    {
        get
        {
            var ids = new List<string>();
            if (HasErrors) ids.Add(ErrorId);
            if (!string.IsNullOrEmpty(EffectiveHelperText)) ids.Add(HelperId);
            return ids.Count > 0 ? string.Join(" ", ids) : null;
        }
    }

    /// <summary>
    /// Builds the CSS class for the field container.
    /// </summary>
    protected string? ContainerClass => CssBuilder.Default("arcadia-field")
        .AddClass("arcadia-field--error", HasErrors)
        .AddClass("arcadia-field--disabled", EffectiveDisabled)
        .AddClass("arcadia-field--required", EffectiveRequired)
        .AddClass(Class)
        .Build();
}

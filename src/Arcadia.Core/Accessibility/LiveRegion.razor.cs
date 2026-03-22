using Microsoft.AspNetCore.Components;

namespace Arcadia.Core.Accessibility;

/// <summary>
/// Renders an ARIA live region that announces dynamic content changes to screen readers.
/// The region is visually hidden by default but accessible to assistive technology.
/// </summary>
public partial class LiveRegion : Base.HelixComponentBase
{
    /// <summary>
    /// Gets or sets the message to announce to screen readers.
    /// Updating this value triggers a new announcement.
    /// </summary>
    [Parameter]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets whether the announcement is assertive (interrupts current speech)
    /// or polite (waits for current speech to finish). Defaults to false (polite).
    /// </summary>
    [Parameter]
    public bool Assertive { get; set; }

    /// <summary>
    /// Gets or sets whether the live region is visually hidden.
    /// Defaults to true. Set to false to make the region visible on screen.
    /// </summary>
    [Parameter]
    public bool VisuallyHidden { get; set; } = true;

    private string? CssClass => Utilities.CssBuilder.Default("arcadia-live-region")
        .AddClass(Class)
        .Build();

    private string? VisuallyHiddenStyle => VisuallyHidden
        ? Utilities.StyleBuilder.Default("position:absolute;width:1px;height:1px;overflow:hidden;clip:rect(0,0,0,0);white-space:nowrap")
            .AddRaw(Style)
            .Build()
        : Style;
}

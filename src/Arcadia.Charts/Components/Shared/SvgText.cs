using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Arcadia.Charts.Components.Shared;

/// <summary>
/// Renders an SVG &lt;text&gt; element. Workaround for Blazor's Razor compiler
/// treating &lt;text&gt; as a directive keyword instead of an SVG element.
/// </summary>
public class SvgText : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "text");
        builder.AddMultipleAttributes(1, AdditionalAttributes!);
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}

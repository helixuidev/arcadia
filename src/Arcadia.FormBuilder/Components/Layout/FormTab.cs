using Microsoft.AspNetCore.Components;

namespace Arcadia.FormBuilder.Components.Layout;

/// <summary>
/// Represents a tab in a tabbed form layout.
/// </summary>
public class FormTab
{
    /// <summary>The tab title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>The tab's child content.</summary>
    public RenderFragment? Content { get; set; }

    /// <summary>Number of validation errors in this tab (shown as badge).</summary>
    public int ErrorCount { get; set; }
}

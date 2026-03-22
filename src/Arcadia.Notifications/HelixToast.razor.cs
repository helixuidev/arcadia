using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;

namespace Arcadia.Notifications;

/// <summary>
/// Renders a single toast notification with icon, message, and optional dismiss button.
/// </summary>
public partial class HelixToast : Core.Base.HelixComponentBase
{
    /// <summary>
    /// Gets or sets the toast data model.
    /// </summary>
    [Parameter]
    public ToastModel Toast { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback invoked when the toast is dismissed.
    /// </summary>
    [Parameter]
    public EventCallback OnDismiss { get; set; }

    private string? CssClass => CssBuilder.Default("arcadia-toast")
        .AddClass($"arcadia-toast--{Toast.Level.ToString().ToLowerInvariant()}")
        .AddClass(Class)
        .Build();
}

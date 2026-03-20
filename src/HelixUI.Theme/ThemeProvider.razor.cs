using Microsoft.AspNetCore.Components;
using HelixUI.Core.Utilities;

namespace HelixUI.Theme;

/// <summary>
/// Wraps content in a themed container. Applies the active theme's CSS custom properties
/// via a <c>data-helix-theme</c> attribute and cascades the <see cref="ThemeService"/>
/// to all descendant components.
/// </summary>
public partial class ThemeProvider : IDisposable
{
    /// <summary>
    /// Gets or sets the theme service instance. If not provided, falls back to DI.
    /// </summary>
    [Parameter]
    public ThemeService? Theme { get; set; }

    /// <summary>
    /// Gets or sets the density mode. Valid values: "compact", "default", "comfortable".
    /// </summary>
    [Parameter]
    public string Density { get; set; } = "default";

    /// <summary>
    /// Gets or sets the child content to render inside the themed container.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Inject]
    private ThemeService InjectedThemeService { get; set; } = default!;

    private ThemeService ThemeService => Theme ?? InjectedThemeService;

    private string? CssClass => CssBuilder.Default("helix-theme-provider")
        .AddClass(Class)
        .Build();

    private string? ComputedStyle => StyleBuilder.Default()
        .AddStyle("font-family", "var(--helix-font-sans)")
        .AddStyle("color", "var(--helix-color-text)")
        .AddStyle("background-color", "var(--helix-color-surface)")
        .AddRaw(Style)
        .Build();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        ThemeService.OnThemeChanged += HandleThemeChanged;
    }

    private void HandleThemeChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ThemeService.OnThemeChanged -= HandleThemeChanged;
    }
}

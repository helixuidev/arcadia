namespace Arcadia.Theme;

/// <summary>Vercel-inspired high-contrast true-black theme.</summary>
public class MidnightTheme : ArcadiaThemeBase
{
    /// <inheritdoc />
    public override string Name => "midnight";

    public MidnightTheme()
    {
        Set("--arcadia-color-primary", "#0070F3");
        Set("--arcadia-color-primary-hover", "#3291FF");
        Set("--arcadia-color-primary-active", "#0060D0");
        Set("--arcadia-color-primary-subtle", "rgba(0,112,243,0.12)");
        Set("--arcadia-color-on-primary", "#FFFFFF");
        Set("--arcadia-color-surface", "#000000");
        Set("--arcadia-color-surface-raised", "#0A0A0A");
        Set("--arcadia-color-surface-overlay", "#111111");
        Set("--arcadia-color-surface-sunken", "#000000");
        Set("--arcadia-color-text", "#EDEDED");
        Set("--arcadia-color-text-muted", "#888888");
        Set("--arcadia-color-text-inverse", "#000000");
        Set("--arcadia-color-border", "#1A1A1A");
        Set("--arcadia-color-border-hover", "#333333");
        Set("--arcadia-color-border-focus", "#0070F3");
        Set("--arcadia-color-danger", "#EE0000");
        Set("--arcadia-color-warning", "#F5A623");
        Set("--arcadia-color-success", "#0070F3");
        Set("--arcadia-color-info", "#0070F3");
        Set("--arcadia-color-focus-ring", "#0070F3");
        Set("--arcadia-color-focus-ring-offset", "#000000");
    }
}

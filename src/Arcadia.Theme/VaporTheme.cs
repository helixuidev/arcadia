namespace Arcadia.Theme;

/// <summary>Frosted glass theme inspired by Apple visionOS and Vercel.</summary>
public class VaporTheme : ArcadiaThemeBase
{
    /// <inheritdoc />
    public override string Name => "vapor";

    public VaporTheme()
    {
        Set("--arcadia-color-primary", "#818CF8");
        Set("--arcadia-color-primary-hover", "#A5B4FC");
        Set("--arcadia-color-primary-active", "#6366F1");
        Set("--arcadia-color-primary-subtle", "rgba(129,140,248,0.12)");
        Set("--arcadia-color-on-primary", "#0F0F12");
        Set("--arcadia-color-surface", "rgba(255,255,255,0.02)");
        Set("--arcadia-color-surface-raised", "rgba(255,255,255,0.05)");
        Set("--arcadia-color-surface-overlay", "rgba(255,255,255,0.08)");
        Set("--arcadia-color-surface-sunken", "rgba(0,0,0,0.2)");
        Set("--arcadia-color-text", "#E4E4E7");
        Set("--arcadia-color-text-muted", "#6B6B76");
        Set("--arcadia-color-text-inverse", "#050507");
        Set("--arcadia-color-border", "rgba(255,255,255,0.06)");
        Set("--arcadia-color-border-hover", "rgba(255,255,255,0.1)");
        Set("--arcadia-color-border-focus", "#818CF8");
        Set("--arcadia-color-danger", "#F87171");
        Set("--arcadia-color-warning", "#FBBF24");
        Set("--arcadia-color-success", "#34D399");
        Set("--arcadia-color-info", "#5DADEC");
        Set("--arcadia-color-focus-ring", "#818CF8");
        Set("--arcadia-color-focus-ring-offset", "rgba(0,0,0,0.5)");
    }
}

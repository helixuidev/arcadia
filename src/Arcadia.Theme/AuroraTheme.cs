namespace Arcadia.Theme;

/// <summary>Gradient accent theme inspired by Raycast and Arc Browser.</summary>
public class AuroraTheme : ArcadiaThemeBase
{
    /// <inheritdoc />
    public override string Name => "aurora";

    public AuroraTheme()
    {
        Set("--arcadia-color-primary", "#8B5CF6");
        Set("--arcadia-color-primary-hover", "#A78BFA");
        Set("--arcadia-color-primary-active", "#7C3AED");
        Set("--arcadia-color-primary-subtle", "rgba(139,92,246,0.12)");
        Set("--arcadia-color-on-primary", "#FFFFFF");
        Set("--arcadia-color-surface", "#0C0C0E");
        Set("--arcadia-color-surface-raised", "#141416");
        Set("--arcadia-color-surface-overlay", "#1C1C20");
        Set("--arcadia-color-surface-sunken", "#060607");
        Set("--arcadia-color-text", "#F5F5F7");
        Set("--arcadia-color-text-muted", "#6E6E78");
        Set("--arcadia-color-text-inverse", "#0C0C0E");
        Set("--arcadia-color-border", "#232326");
        Set("--arcadia-color-border-hover", "#2E2E32");
        Set("--arcadia-color-border-focus", "#8B5CF6");
        Set("--arcadia-color-danger", "#EF4444");
        Set("--arcadia-color-warning", "#F59E0B");
        Set("--arcadia-color-success", "#10B981");
        Set("--arcadia-color-info", "#3B82F6");
        Set("--arcadia-color-focus-ring", "#8B5CF6");
        Set("--arcadia-color-focus-ring-offset", "#0C0C0E");
    }
}

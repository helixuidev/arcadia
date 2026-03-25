namespace Arcadia.Theme;

/// <summary>Ultra-minimal theme inspired by Linear and shadcn/ui.</summary>
public class ObsidianTheme : ArcadiaThemeBase
{
    /// <inheritdoc />
    public override string Name => "obsidian";

    public ObsidianTheme()
    {
        Set("--arcadia-color-primary", "#6366F1");
        Set("--arcadia-color-primary-hover", "#818CF8");
        Set("--arcadia-color-primary-active", "#4F46E5");
        Set("--arcadia-color-primary-subtle", "#1E1B4B");
        Set("--arcadia-color-on-primary", "#FAFAFA");
        Set("--arcadia-color-surface", "#09090B");
        Set("--arcadia-color-surface-raised", "#0F0F12");
        Set("--arcadia-color-surface-overlay", "#18181B");
        Set("--arcadia-color-surface-sunken", "#050506");
        Set("--arcadia-color-text", "#FAFAFA");
        Set("--arcadia-color-text-muted", "#71717A");
        Set("--arcadia-color-text-inverse", "#09090B");
        Set("--arcadia-color-border", "#1F1F23");
        Set("--arcadia-color-border-hover", "#27272A");
        Set("--arcadia-color-border-focus", "#6366F1");
        Set("--arcadia-color-danger", "#EF4444");
        Set("--arcadia-color-warning", "#F59E0B");
        Set("--arcadia-color-success", "#22C55E");
        Set("--arcadia-color-info", "#3B82F6");
        Set("--arcadia-color-focus-ring", "#6366F1");
        Set("--arcadia-color-focus-ring-offset", "#09090B");
    }
}

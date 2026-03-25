namespace Arcadia.Theme;

/// <summary>Notion-inspired warm neutral theme.</summary>
public class SlateTheme : ArcadiaThemeBase
{
    /// <inheritdoc />
    public override string Name => "slate";

    public SlateTheme()
    {
        Set("--arcadia-color-primary", "#2EAADC");
        Set("--arcadia-color-primary-hover", "#4BC4F0");
        Set("--arcadia-color-primary-active", "#1A90C0");
        Set("--arcadia-color-primary-subtle", "rgba(46,170,220,0.12)");
        Set("--arcadia-color-on-primary", "#191919");
        Set("--arcadia-color-surface", "#191919");
        Set("--arcadia-color-surface-raised", "#202020");
        Set("--arcadia-color-surface-overlay", "#2C2C2C");
        Set("--arcadia-color-surface-sunken", "#111111");
        Set("--arcadia-color-text", "#E8E5E0");
        Set("--arcadia-color-text-muted", "#9B9A97");
        Set("--arcadia-color-text-inverse", "#37352F");
        Set("--arcadia-color-border", "#2E2E2E");
        Set("--arcadia-color-border-hover", "#3E3E3E");
        Set("--arcadia-color-border-focus", "#2EAADC");
        Set("--arcadia-color-danger", "#FF7369");
        Set("--arcadia-color-warning", "#FFA344");
        Set("--arcadia-color-success", "#4DAB9A");
        Set("--arcadia-color-info", "#529CCA");
        Set("--arcadia-color-focus-ring", "#2EAADC");
        Set("--arcadia-color-focus-ring-offset", "#191919");
    }
}

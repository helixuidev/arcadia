namespace Arcadia.Theme;

/// <summary>Stripe-inspired enterprise theme with professional density.</summary>
public class CarbonTheme : ArcadiaThemeBase
{
    /// <inheritdoc />
    public override string Name => "carbon";

    public CarbonTheme()
    {
        Set("--arcadia-color-primary", "#635BFF");
        Set("--arcadia-color-primary-hover", "#7A73FF");
        Set("--arcadia-color-primary-active", "#4B44E0");
        Set("--arcadia-color-primary-subtle", "rgba(99,91,255,0.12)");
        Set("--arcadia-color-on-primary", "#FFFFFF");
        Set("--arcadia-color-surface", "#111118");
        Set("--arcadia-color-surface-raised", "#19192A");
        Set("--arcadia-color-surface-overlay", "#22223A");
        Set("--arcadia-color-surface-sunken", "#0A0A0F");
        Set("--arcadia-color-text", "#F0F0F5");
        Set("--arcadia-color-text-muted", "#8888A0");
        Set("--arcadia-color-text-inverse", "#111118");
        Set("--arcadia-color-border", "#25253A");
        Set("--arcadia-color-border-hover", "#33334D");
        Set("--arcadia-color-border-focus", "#635BFF");
        Set("--arcadia-color-danger", "#E25C5C");
        Set("--arcadia-color-warning", "#F5A623");
        Set("--arcadia-color-success", "#3ECF8E");
        Set("--arcadia-color-info", "#5DADEC");
        Set("--arcadia-color-focus-ring", "#635BFF");
        Set("--arcadia-color-focus-ring-offset", "#111118");
    }
}

namespace Arcadia.Theme;

/// <summary>
/// The dark theme for Arcadia. All contrast ratios meet WCAG 2.1 AA.
/// </summary>
public class DarkTheme : ArcadiaThemeBase
{
    /// <inheritdoc />
    public override string Name => "dark";

    /// <summary>
    /// Creates a new instance of the dark theme with all token values.
    /// </summary>
    public DarkTheme()
    {
        // Primary
        Set("--arcadia-color-primary", "#60a5fa");
        Set("--arcadia-color-primary-hover", "#93bbfd");
        Set("--arcadia-color-primary-active", "#3b82f6");
        Set("--arcadia-color-primary-subtle", "#1e3a5f");
        Set("--arcadia-color-on-primary", "#0f172a");

        // Secondary
        Set("--arcadia-color-secondary", "#94a3b8");
        Set("--arcadia-color-secondary-hover", "#b0bec5");
        Set("--arcadia-color-secondary-active", "#78909c");
        Set("--arcadia-color-secondary-subtle", "#1e293b");
        Set("--arcadia-color-on-secondary", "#0f172a");

        // Surface
        Set("--arcadia-color-surface", "#0f172a");
        Set("--arcadia-color-surface-raised", "#1e293b");
        Set("--arcadia-color-surface-overlay", "#1e293b");
        Set("--arcadia-color-surface-sunken", "#020617");

        // Text
        Set("--arcadia-color-text", "#f1f5f9");
        Set("--arcadia-color-text-muted", "#94a3b8");
        Set("--arcadia-color-text-inverse", "#0f172a");
        Set("--arcadia-color-text-disabled", "#475569");

        // Border
        Set("--arcadia-color-border", "#334155");
        Set("--arcadia-color-border-hover", "#475569");
        Set("--arcadia-color-border-focus", "#60a5fa");

        // Status
        Set("--arcadia-color-danger", "#fca5a5");
        Set("--arcadia-color-danger-hover", "#fecaca");
        Set("--arcadia-color-danger-subtle", "#450a0a");
        Set("--arcadia-color-on-danger", "#0f172a");

        Set("--arcadia-color-warning", "#fbbf24");
        Set("--arcadia-color-warning-hover", "#fcd34d");
        Set("--arcadia-color-warning-subtle", "#451a03");
        Set("--arcadia-color-on-warning", "#0f172a");

        Set("--arcadia-color-success", "#4ade80");
        Set("--arcadia-color-success-hover", "#86efac");
        Set("--arcadia-color-success-subtle", "#052e16");
        Set("--arcadia-color-on-success", "#0f172a");

        Set("--arcadia-color-info", "#38bdf8");
        Set("--arcadia-color-info-hover", "#7dd3fc");
        Set("--arcadia-color-info-subtle", "#082f49");
        Set("--arcadia-color-on-info", "#0f172a");

        // Focus
        Set("--arcadia-color-focus-ring", "#60a5fa");
        Set("--arcadia-color-focus-ring-offset", "#0f172a");
    }
}

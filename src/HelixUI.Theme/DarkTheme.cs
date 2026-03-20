namespace HelixUI.Theme;

/// <summary>
/// The dark theme for HelixUI. All contrast ratios meet WCAG 2.1 AA.
/// </summary>
public class DarkTheme : HelixThemeBase
{
    /// <inheritdoc />
    public override string Name => "dark";

    /// <summary>
    /// Creates a new instance of the dark theme with all token values.
    /// </summary>
    public DarkTheme()
    {
        // Primary
        Set("--helix-color-primary", "#60a5fa");
        Set("--helix-color-primary-hover", "#93bbfd");
        Set("--helix-color-primary-active", "#3b82f6");
        Set("--helix-color-primary-subtle", "#1e3a5f");
        Set("--helix-color-on-primary", "#0f172a");

        // Secondary
        Set("--helix-color-secondary", "#94a3b8");
        Set("--helix-color-secondary-hover", "#b0bec5");
        Set("--helix-color-secondary-active", "#78909c");
        Set("--helix-color-secondary-subtle", "#1e293b");
        Set("--helix-color-on-secondary", "#0f172a");

        // Surface
        Set("--helix-color-surface", "#0f172a");
        Set("--helix-color-surface-raised", "#1e293b");
        Set("--helix-color-surface-overlay", "#1e293b");
        Set("--helix-color-surface-sunken", "#020617");

        // Text
        Set("--helix-color-text", "#f1f5f9");
        Set("--helix-color-text-muted", "#94a3b8");
        Set("--helix-color-text-inverse", "#0f172a");
        Set("--helix-color-text-disabled", "#475569");

        // Border
        Set("--helix-color-border", "#334155");
        Set("--helix-color-border-hover", "#475569");
        Set("--helix-color-border-focus", "#60a5fa");

        // Status
        Set("--helix-color-danger", "#f87171");
        Set("--helix-color-danger-hover", "#fca5a5");
        Set("--helix-color-danger-subtle", "#450a0a");
        Set("--helix-color-on-danger", "#0f172a");

        Set("--helix-color-warning", "#fbbf24");
        Set("--helix-color-warning-hover", "#fcd34d");
        Set("--helix-color-warning-subtle", "#451a03");
        Set("--helix-color-on-warning", "#0f172a");

        Set("--helix-color-success", "#4ade80");
        Set("--helix-color-success-hover", "#86efac");
        Set("--helix-color-success-subtle", "#052e16");
        Set("--helix-color-on-success", "#0f172a");

        Set("--helix-color-info", "#38bdf8");
        Set("--helix-color-info-hover", "#7dd3fc");
        Set("--helix-color-info-subtle", "#082f49");
        Set("--helix-color-on-info", "#0f172a");

        // Focus
        Set("--helix-color-focus-ring", "#60a5fa");
        Set("--helix-color-focus-ring-offset", "#0f172a");
    }
}

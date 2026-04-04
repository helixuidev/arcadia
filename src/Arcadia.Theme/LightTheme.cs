namespace Arcadia.Theme;

/// <summary>
/// The default light theme for Arcadia.
/// </summary>
public class LightTheme : ArcadiaThemeBase
{
    /// <inheritdoc />
    public override string Name => "light";

    /// <summary>
    /// Creates a new instance of the light theme with all default token values.
    /// </summary>
    public LightTheme()
    {
        // Primary
        Set("--arcadia-color-primary", "#2563eb");
        Set("--arcadia-color-primary-hover", "#1d4ed8");
        Set("--arcadia-color-primary-active", "#1e40af");
        Set("--arcadia-color-primary-subtle", "#dbeafe");
        Set("--arcadia-color-on-primary", "#ffffff");

        // Secondary
        Set("--arcadia-color-secondary", "#64748b");
        Set("--arcadia-color-secondary-hover", "#475569");
        Set("--arcadia-color-secondary-active", "#334155");
        Set("--arcadia-color-secondary-subtle", "#f1f5f9");
        Set("--arcadia-color-on-secondary", "#ffffff");

        // Surface
        Set("--arcadia-color-surface", "#ffffff");
        Set("--arcadia-color-surface-raised", "#ffffff");
        Set("--arcadia-color-surface-overlay", "#ffffff");
        Set("--arcadia-color-surface-sunken", "#f8fafc");

        // Text
        Set("--arcadia-color-text", "#0f172a");
        Set("--arcadia-color-text-muted", "#64748b");
        Set("--arcadia-color-text-inverse", "#ffffff");
        Set("--arcadia-color-text-disabled", "#6b7280");

        // Border
        Set("--arcadia-color-border", "#e2e8f0");
        Set("--arcadia-color-border-hover", "#cbd5e1");
        Set("--arcadia-color-border-focus", "#2563eb");

        // Status
        Set("--arcadia-color-danger", "#b91c1c");
        Set("--arcadia-color-danger-hover", "#991b1b");
        Set("--arcadia-color-danger-subtle", "#fef2f2");
        Set("--arcadia-color-on-danger", "#ffffff");

        Set("--arcadia-color-warning", "#b45309");
        Set("--arcadia-color-warning-hover", "#92400e");
        Set("--arcadia-color-warning-subtle", "#fffbeb");
        Set("--arcadia-color-on-warning", "#ffffff");

        Set("--arcadia-color-success", "#16a34a");
        Set("--arcadia-color-success-hover", "#15803d");
        Set("--arcadia-color-success-subtle", "#f0fdf4");
        Set("--arcadia-color-on-success", "#ffffff");

        Set("--arcadia-color-info", "#0284c7");
        Set("--arcadia-color-info-hover", "#0369a1");
        Set("--arcadia-color-info-subtle", "#f0f9ff");
        Set("--arcadia-color-on-info", "#ffffff");

        // Focus
        Set("--arcadia-color-focus-ring", "#2563eb");
        Set("--arcadia-color-focus-ring-offset", "#ffffff");
    }
}

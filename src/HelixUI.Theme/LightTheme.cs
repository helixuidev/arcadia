namespace HelixUI.Theme;

/// <summary>
/// The default light theme for HelixUI.
/// </summary>
public class LightTheme : HelixThemeBase
{
    /// <inheritdoc />
    public override string Name => "light";

    /// <summary>
    /// Creates a new instance of the light theme with all default token values.
    /// </summary>
    public LightTheme()
    {
        // Primary
        Set("--helix-color-primary", "#2563eb");
        Set("--helix-color-primary-hover", "#1d4ed8");
        Set("--helix-color-primary-active", "#1e40af");
        Set("--helix-color-primary-subtle", "#dbeafe");
        Set("--helix-color-on-primary", "#ffffff");

        // Secondary
        Set("--helix-color-secondary", "#64748b");
        Set("--helix-color-secondary-hover", "#475569");
        Set("--helix-color-secondary-active", "#334155");
        Set("--helix-color-secondary-subtle", "#f1f5f9");
        Set("--helix-color-on-secondary", "#ffffff");

        // Surface
        Set("--helix-color-surface", "#ffffff");
        Set("--helix-color-surface-raised", "#ffffff");
        Set("--helix-color-surface-overlay", "#ffffff");
        Set("--helix-color-surface-sunken", "#f8fafc");

        // Text
        Set("--helix-color-text", "#0f172a");
        Set("--helix-color-text-muted", "#64748b");
        Set("--helix-color-text-inverse", "#ffffff");
        Set("--helix-color-text-disabled", "#94a3b8");

        // Border
        Set("--helix-color-border", "#e2e8f0");
        Set("--helix-color-border-hover", "#cbd5e1");
        Set("--helix-color-border-focus", "#2563eb");

        // Status
        Set("--helix-color-danger", "#dc2626");
        Set("--helix-color-danger-hover", "#b91c1c");
        Set("--helix-color-danger-subtle", "#fef2f2");
        Set("--helix-color-on-danger", "#ffffff");

        Set("--helix-color-warning", "#d97706");
        Set("--helix-color-warning-hover", "#b45309");
        Set("--helix-color-warning-subtle", "#fffbeb");
        Set("--helix-color-on-warning", "#ffffff");

        Set("--helix-color-success", "#16a34a");
        Set("--helix-color-success-hover", "#15803d");
        Set("--helix-color-success-subtle", "#f0fdf4");
        Set("--helix-color-on-success", "#ffffff");

        Set("--helix-color-info", "#0284c7");
        Set("--helix-color-info-hover", "#0369a1");
        Set("--helix-color-info-subtle", "#f0f9ff");
        Set("--helix-color-on-info", "#ffffff");

        // Focus
        Set("--helix-color-focus-ring", "#2563eb");
        Set("--helix-color-focus-ring-offset", "#ffffff");
    }
}

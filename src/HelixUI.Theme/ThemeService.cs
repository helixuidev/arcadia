using HelixUI.Core.Abstractions;

namespace HelixUI.Theme;

/// <summary>
/// Manages the active theme and notifies subscribers when it changes.
/// Register as a scoped service in DI.
/// </summary>
public class ThemeService
{
    private IHelixTheme _currentTheme;

    /// <summary>
    /// Creates a new <see cref="ThemeService"/> with the specified default theme.
    /// </summary>
    /// <param name="defaultTheme">The initial active theme.</param>
    public ThemeService(IHelixTheme defaultTheme)
    {
        _currentTheme = defaultTheme;
    }

    /// <summary>
    /// Creates a new <see cref="ThemeService"/> with the light theme as default.
    /// </summary>
    public ThemeService() : this(new LightTheme())
    {
    }

    /// <summary>
    /// Gets the currently active theme.
    /// </summary>
    public IHelixTheme CurrentTheme => _currentTheme;

    /// <summary>
    /// Raised when the active theme changes.
    /// </summary>
    public event Action? OnThemeChanged;

    /// <summary>
    /// Sets the active theme and notifies all subscribers.
    /// </summary>
    /// <param name="theme">The new theme to activate.</param>
    public void SetTheme(IHelixTheme theme)
    {
        if (_currentTheme.Name == theme.Name)
            return;

        _currentTheme = theme;
        OnThemeChanged?.Invoke();
    }
}

using FluentAssertions;
using Arcadia.Theme;
using Xunit;

namespace Arcadia.Tests.Unit.Theme;

public class ThemeServiceTests
{
    [Fact]
    public void DefaultConstructor_UsesLightTheme()
    {
        var service = new ThemeService();

        service.CurrentTheme.Name.Should().Be("light");
    }

    [Fact]
    public void Constructor_WithTheme_UsesProvidedTheme()
    {
        var service = new ThemeService(new DarkTheme());

        service.CurrentTheme.Name.Should().Be("dark");
    }

    [Fact]
    public void SetTheme_ChangesCurrentTheme()
    {
        var service = new ThemeService();
        service.SetTheme(new DarkTheme());

        service.CurrentTheme.Name.Should().Be("dark");
    }

    [Fact]
    public void SetTheme_FiresOnThemeChanged()
    {
        var service = new ThemeService();
        var fired = false;
        service.OnThemeChanged += () => fired = true;

        service.SetTheme(new DarkTheme());

        fired.Should().BeTrue();
    }

    [Fact]
    public void SetTheme_SameTheme_DoesNotFire()
    {
        var service = new ThemeService();
        var fireCount = 0;
        service.OnThemeChanged += () => fireCount++;

        service.SetTheme(new LightTheme());

        fireCount.Should().Be(0);
    }
}

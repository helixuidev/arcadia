using FluentAssertions;
using Arcadia.Theme;
using Xunit;

namespace Arcadia.Tests.Unit.Theme;

public class HelixThemeBaseTests
{
    [Fact]
    public void LightTheme_HasCorrectName()
    {
        var theme = new LightTheme();

        theme.Name.Should().Be("light");
    }

    [Fact]
    public void DarkTheme_HasCorrectName()
    {
        var theme = new DarkTheme();

        theme.Name.Should().Be("dark");
    }

    [Fact]
    public void LightTheme_HasPrimaryColor()
    {
        var theme = new LightTheme();

        theme.GetProperty("--arcadia-color-primary").Should().Be("#2563eb");
    }

    [Fact]
    public void DarkTheme_HasPrimaryColor()
    {
        var theme = new DarkTheme();

        theme.GetProperty("--arcadia-color-primary").Should().Be("#60a5fa");
    }

    [Fact]
    public void GetProperty_UnknownKey_ReturnsNull()
    {
        var theme = new LightTheme();

        theme.GetProperty("--arcadia-nonexistent").Should().BeNull();
    }

    [Fact]
    public void Properties_ReturnsAllTokens()
    {
        var theme = new LightTheme();

        theme.Properties.Should().NotBeEmpty();
        theme.Properties.Should().ContainKey("--arcadia-color-primary");
        theme.Properties.Should().ContainKey("--arcadia-color-surface");
        theme.Properties.Should().ContainKey("--arcadia-color-text");
        theme.Properties.Should().ContainKey("--arcadia-color-danger");
    }

    [Fact]
    public void LightAndDark_HaveDifferentSurfaceColors()
    {
        var light = new LightTheme();
        var dark = new DarkTheme();

        light.GetProperty("--arcadia-color-surface")
            .Should().NotBe(dark.GetProperty("--arcadia-color-surface"));
    }
}

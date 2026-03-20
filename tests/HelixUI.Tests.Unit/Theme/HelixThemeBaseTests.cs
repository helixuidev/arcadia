using FluentAssertions;
using HelixUI.Theme;
using Xunit;

namespace HelixUI.Tests.Unit.Theme;

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

        theme.GetProperty("--helix-color-primary").Should().Be("#2563eb");
    }

    [Fact]
    public void DarkTheme_HasPrimaryColor()
    {
        var theme = new DarkTheme();

        theme.GetProperty("--helix-color-primary").Should().Be("#60a5fa");
    }

    [Fact]
    public void GetProperty_UnknownKey_ReturnsNull()
    {
        var theme = new LightTheme();

        theme.GetProperty("--helix-nonexistent").Should().BeNull();
    }

    [Fact]
    public void Properties_ReturnsAllTokens()
    {
        var theme = new LightTheme();

        theme.Properties.Should().NotBeEmpty();
        theme.Properties.Should().ContainKey("--helix-color-primary");
        theme.Properties.Should().ContainKey("--helix-color-surface");
        theme.Properties.Should().ContainKey("--helix-color-text");
        theme.Properties.Should().ContainKey("--helix-color-danger");
    }

    [Fact]
    public void LightAndDark_HaveDifferentSurfaceColors()
    {
        var light = new LightTheme();
        var dark = new DarkTheme();

        light.GetProperty("--helix-color-surface")
            .Should().NotBe(dark.GetProperty("--helix-color-surface"));
    }
}

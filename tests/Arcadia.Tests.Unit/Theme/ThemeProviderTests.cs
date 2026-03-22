using Bunit;
using FluentAssertions;
using Arcadia.Theme;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Arcadia.Tests.Unit.Theme;

public class ThemeProviderTests : BunitContext
{
    public ThemeProviderTests()
    {
        Services.AddScoped<ThemeService>();
    }

    [Fact]
    public void Renders_WithLightThemeByDefault()
    {
        var cut = Render<ThemeProvider>(parameters =>
            parameters.AddChildContent("<p>Hello</p>"));

        cut.Find("div").GetAttribute("data-arcadia-theme").Should().Be("light");
    }

    [Fact]
    public void Renders_WithDensityAttribute()
    {
        var cut = Render<ThemeProvider>(parameters =>
            parameters.Add(p => p.Density, "compact")
                      .AddChildContent("<p>Hello</p>"));

        cut.Find("div").GetAttribute("data-arcadia-density").Should().Be("compact");
    }

    [Fact]
    public void Renders_DefaultDensity()
    {
        var cut = Render<ThemeProvider>(parameters =>
            parameters.AddChildContent("<p>Hello</p>"));

        cut.Find("div").GetAttribute("data-arcadia-density").Should().Be("default");
    }

    [Fact]
    public void Renders_ChildContent()
    {
        var cut = Render<ThemeProvider>(parameters =>
            parameters.AddChildContent("<p>Content here</p>"));

        cut.Find("p").TextContent.Should().Be("Content here");
    }

    [Fact]
    public void Renders_WithHelixThemeProviderClass()
    {
        var cut = Render<ThemeProvider>(parameters =>
            parameters.AddChildContent("<p>Hello</p>"));

        cut.Find("div").ClassList.Should().Contain("arcadia-theme-provider");
    }

    [Fact]
    public void Renders_WithCustomClass()
    {
        var cut = Render<ThemeProvider>(parameters =>
            parameters.Add(p => p.Class, "my-app")
                      .AddChildContent("<p>Hello</p>"));

        var classes = cut.Find("div").GetAttribute("class");
        classes.Should().Contain("arcadia-theme-provider");
        classes.Should().Contain("my-app");
    }

    [Fact]
    public void Renders_WithBaseStyles()
    {
        var cut = Render<ThemeProvider>(parameters =>
            parameters.AddChildContent("<p>Hello</p>"));

        var style = cut.Find("div").GetAttribute("style");
        style.Should().Contain("font-family:");
        style.Should().Contain("color:");
        style.Should().Contain("background-color:");
    }

    [Fact]
    public void Updates_WhenThemeChanges()
    {
        var themeService = Services.GetRequiredService<ThemeService>();

        var cut = Render<ThemeProvider>(parameters =>
            parameters.AddChildContent("<p>Hello</p>"));

        cut.Find("div").GetAttribute("data-arcadia-theme").Should().Be("light");

        themeService.SetTheme(new DarkTheme());

        cut.Find("div").GetAttribute("data-arcadia-theme").Should().Be("dark");
    }

    [Fact]
    public void Accepts_ExplicitThemeService()
    {
        var customService = new ThemeService(new DarkTheme());

        var cut = Render<ThemeProvider>(parameters =>
            parameters.Add(p => p.Theme, customService)
                      .AddChildContent("<p>Hello</p>"));

        cut.Find("div").GetAttribute("data-arcadia-theme").Should().Be("dark");
    }
}

using Bunit;
using FluentAssertions;
using HelixUI.Core.Accessibility;
using Xunit;

namespace HelixUI.Tests.Unit.Accessibility;

public class LiveRegionTests : BunitContext
{
    [Fact]
    public void Renders_WithPoliteByDefault()
    {
        var cut = Render<LiveRegion>(parameters =>
            parameters.Add(p => p.Message, "Item saved"));

        var div = cut.Find("div");
        div.GetAttribute("role").Should().Be("status");
        div.GetAttribute("aria-live").Should().Be("polite");
    }

    [Fact]
    public void Renders_WithAssertive_WhenSet()
    {
        var cut = Render<LiveRegion>(parameters =>
            parameters.Add(p => p.Message, "Error occurred")
                      .Add(p => p.Assertive, true));

        var div = cut.Find("div");
        div.GetAttribute("role").Should().Be("alert");
        div.GetAttribute("aria-live").Should().Be("assertive");
    }

    [Fact]
    public void Renders_WithAriaAtomic()
    {
        var cut = Render<LiveRegion>(parameters =>
            parameters.Add(p => p.Message, "test"));

        cut.Find("div").GetAttribute("aria-atomic").Should().Be("true");
    }

    [Fact]
    public void Renders_Message()
    {
        var cut = Render<LiveRegion>(parameters =>
            parameters.Add(p => p.Message, "3 items selected"));

        cut.Find("div").TextContent.Trim().Should().Be("3 items selected");
    }

    [Fact]
    public void IsVisuallyHidden_ByDefault()
    {
        var cut = Render<LiveRegion>(parameters =>
            parameters.Add(p => p.Message, "test"));

        var style = cut.Find("div").GetAttribute("style");
        style.Should().Contain("position:absolute");
        style.Should().Contain("width:1px");
        style.Should().Contain("overflow:hidden");
    }

    [Fact]
    public void IsVisible_WhenVisuallyHiddenIsFalse()
    {
        var cut = Render<LiveRegion>(parameters =>
            parameters.Add(p => p.Message, "test")
                      .Add(p => p.VisuallyHidden, false));

        var style = cut.Find("div").GetAttribute("style");
        (style is null || !style.Contains("position:absolute")).Should().BeTrue();
    }

    [Fact]
    public void Renders_WithCustomClass()
    {
        var cut = Render<LiveRegion>(parameters =>
            parameters.Add(p => p.Message, "test")
                      .Add(p => p.Class, "custom-class"));

        cut.Find("div").GetAttribute("class").Should().Contain("helix-live-region");
        cut.Find("div").GetAttribute("class").Should().Contain("custom-class");
    }

    [Fact]
    public void Updates_Message_Dynamically()
    {
        var cut = Render<LiveRegion>(parameters =>
            parameters.Add(p => p.Message, "first"));

        cut.Find("div").TextContent.Trim().Should().Be("first");

        cut.Render(parameters =>
            parameters.Add(p => p.Message, "second"));

        cut.Find("div").TextContent.Trim().Should().Be("second");
    }
}

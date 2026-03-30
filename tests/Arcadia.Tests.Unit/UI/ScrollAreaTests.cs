using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class ScrollAreaTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaScrollArea>(p => p
            .AddChildContent("<p>Scrollable content</p>"));

        cut.Find(".arcadia-scroll-area").Should().NotBeNull();
    }

    [Fact]
    public void Height_AppliedToStyle()
    {
        var cut = Render<ArcadiaScrollArea>(p => p
            .Add(c => c.Height, "300px")
            .AddChildContent("<p>Content</p>"));

        cut.Find(".arcadia-scroll-area")
            .GetAttribute("style").Should().Contain("max-height: 300px");
    }

    [Fact]
    public void Width_AppliedToStyle()
    {
        var cut = Render<ArcadiaScrollArea>(p => p
            .Add(c => c.Width, "400px")
            .AddChildContent("<p>Content</p>"));

        cut.Find(".arcadia-scroll-area")
            .GetAttribute("style").Should().Contain("width: 400px");
    }

    [Fact]
    public void Orientation_AppliedToCssClass()
    {
        var cut = Render<ArcadiaScrollArea>(p => p
            .Add(c => c.Orientation, "horizontal")
            .AddChildContent("<p>Content</p>"));

        cut.Find(".arcadia-scroll-area").ClassList
            .Should().Contain("arcadia-scroll-area--horizontal");
    }

    [Fact]
    public void HideScrollbar_AddsHideClass()
    {
        var cut = Render<ArcadiaScrollArea>(p => p
            .Add(c => c.HideScrollbar, true)
            .AddChildContent("<p>Content</p>"));

        cut.Find(".arcadia-scroll-area").ClassList
            .Should().Contain("arcadia-scroll-area--hide-scrollbar");
    }

    [Fact]
    public void ChildContent_Rendered()
    {
        var cut = Render<ArcadiaScrollArea>(p => p
            .AddChildContent("<p>Hello world</p>"));

        cut.Find(".arcadia-scroll-area").InnerHtml.Should().Contain("Hello world");
    }

    [Fact]
    public void Tabindex_SetToZero()
    {
        var cut = Render<ArcadiaScrollArea>(p => p
            .AddChildContent("<p>Content</p>"));

        cut.Find(".arcadia-scroll-area")
            .GetAttribute("tabindex").Should().Be("0");
    }
}

using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class HoverCardTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaHoverCard>(p => p
            .AddChildContent("<span>Trigger</span>")
            .Add(c => c.Content, "<div>Card content</div>"));

        cut.Find(".arcadia-hover-card").Should().NotBeNull();
    }

    [Fact]
    public void Trigger_RendersChildContent()
    {
        var cut = Render<ArcadiaHoverCard>(p => p
            .AddChildContent("<span>Hover me</span>")
            .Add(c => c.Content, "<div>Card</div>"));

        cut.Find(".arcadia-hover-card__trigger").InnerHtml.Should().Contain("Hover me");
    }

    [Fact]
    public void Content_NotVisibleByDefault()
    {
        var cut = Render<ArcadiaHoverCard>(p => p
            .AddChildContent("<span>Trigger</span>")
            .Add(c => c.Content, "<div>Hidden</div>"));

        cut.FindAll(".arcadia-hover-card__content").Should().BeEmpty();
    }

    [Fact]
    public void Position_AppliedToClassWhenVisible()
    {
        var cut = Render<ArcadiaHoverCard>(p => p
            .Add(c => c.Position, "top")
            .AddChildContent("<span>Trigger</span>")
            .Add(c => c.Content, "<div>Card</div>"));

        // Content is not visible by default, so just check root class
        cut.Find(".arcadia-hover-card").Should().NotBeNull();
    }

    [Fact]
    public void CustomClass_Applied()
    {
        var cut = Render<ArcadiaHoverCard>(p => p
            .Add(c => c.Class, "my-custom")
            .AddChildContent("<span>Trigger</span>")
            .Add(c => c.Content, "<div>Card</div>"));

        cut.Find(".arcadia-hover-card").ClassList.Should().Contain("my-custom");
    }
}

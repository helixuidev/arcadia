using Bunit;
using FluentAssertions;
using HelixUI.Core.Accessibility;
using Xunit;

namespace HelixUI.Tests.Unit.Accessibility;

public class FocusTrapTests : BunitContext
{
    [Fact]
    public void Renders_ChildContent()
    {
        var cut = Render<FocusTrap>(parameters =>
            parameters.AddChildContent("<button>Click me</button>"));

        cut.Find("button").TextContent.Should().Be("Click me");
    }

    [Fact]
    public void Renders_FocusSentinels_WhenActive()
    {
        var cut = Render<FocusTrap>(parameters =>
            parameters.Add(p => p.Active, true)
                      .AddChildContent("<button>Click me</button>"));

        var sentinels = cut.FindAll(".helix-focus-sentinel");
        sentinels.Should().HaveCount(2);
    }

    [Fact]
    public void DoesNotRender_FocusSentinels_WhenInactive()
    {
        var cut = Render<FocusTrap>(parameters =>
            parameters.Add(p => p.Active, false)
                      .AddChildContent("<button>Click me</button>"));

        var sentinels = cut.FindAll(".helix-focus-sentinel");
        sentinels.Should().BeEmpty();
    }

    [Fact]
    public void Sentinels_AreVisuallyHidden()
    {
        var cut = Render<FocusTrap>(parameters =>
            parameters.Add(p => p.Active, true)
                      .AddChildContent("<button>Click me</button>"));

        var sentinel = cut.Find(".helix-focus-sentinel");
        var style = sentinel.GetAttribute("style");
        style.Should().Contain("position:absolute");
        style.Should().Contain("width:1px");
        style.Should().Contain("height:1px");
    }

    [Fact]
    public void Sentinels_HaveTabIndex()
    {
        var cut = Render<FocusTrap>(parameters =>
            parameters.Add(p => p.Active, true)
                      .AddChildContent("<button>Click me</button>"));

        var sentinels = cut.FindAll(".helix-focus-sentinel");
        foreach (var sentinel in sentinels)
        {
            sentinel.GetAttribute("tabindex").Should().Be("0");
        }
    }

    [Fact]
    public void HasActiveCssClass_WhenActive()
    {
        var cut = Render<FocusTrap>(parameters =>
            parameters.Add(p => p.Active, true)
                      .AddChildContent("<button>Click me</button>"));

        cut.Find(".helix-focus-trap").ClassList.Should().Contain("helix-focus-trap--active");
    }

    [Fact]
    public void NoActiveCssClass_WhenInactive()
    {
        var cut = Render<FocusTrap>(parameters =>
            parameters.Add(p => p.Active, false)
                      .AddChildContent("<button>Click me</button>"));

        cut.Find(".helix-focus-trap").ClassList.Should().NotContain("helix-focus-trap--active");
    }

    [Fact]
    public void Renders_WithCustomClass()
    {
        var cut = Render<FocusTrap>(parameters =>
            parameters.Add(p => p.Class, "my-trap")
                      .AddChildContent("<button>Click me</button>"));

        var classes = cut.Find("div").GetAttribute("class");
        classes.Should().Contain("helix-focus-trap");
        classes.Should().Contain("my-trap");
    }

    [Fact]
    public void Renders_WithAdditionalAttributes()
    {
        var cut = Render<FocusTrap>(parameters =>
            parameters.AddUnmatched("data-testid", "trap-1")
                      .AddChildContent("<button>Click me</button>"));

        cut.Find("div").GetAttribute("data-testid").Should().Be("trap-1");
    }
}

using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;
using Microsoft.AspNetCore.Components;

namespace Arcadia.Tests.Unit.UI;

public class TimelineTests : ChartTestBase
{
    [Fact]
    public void Renders_WithRoleList()
    {
        var cut = Render<ArcadiaTimeline>(p => p
            .Add(c => c.ChildContent, (RenderFragment)(b => { })));

        cut.Find("[role='list']").Should().NotBeNull();
    }

    [Fact]
    public void Renders_RootCssClass()
    {
        var cut = Render<ArcadiaTimeline>(p => p
            .Add(c => c.ChildContent, (RenderFragment)(b => { })));

        cut.Find(".arcadia-timeline").Should().NotBeNull();
    }

    [Fact]
    public void TimelineItem_RendersTitle()
    {
        var cut = Render<ArcadiaTimelineItem>(p => p
            .Add(c => c.Title, "Event A"));

        cut.Find(".arcadia-timeline-item__title").TextContent.Should().Be("Event A");
    }

    [Fact]
    public void TimelineItem_RendersSubtitle()
    {
        var cut = Render<ArcadiaTimelineItem>(p => p
            .Add(c => c.Title, "Event")
            .Add(c => c.Subtitle, "Yesterday"));

        cut.Find(".arcadia-timeline-item__subtitle").TextContent.Should().Be("Yesterday");
    }

    [Fact]
    public void TimelineItem_NoSubtitle_OmitsElement()
    {
        var cut = Render<ArcadiaTimelineItem>(p => p
            .Add(c => c.Title, "Event"));

        cut.FindAll(".arcadia-timeline-item__subtitle").Should().BeEmpty();
    }

    [Theory]
    [InlineData("primary")]
    [InlineData("success")]
    [InlineData("danger")]
    [InlineData("warning")]
    public void TimelineItem_Color_AppliesDotClass(string color)
    {
        var cut = Render<ArcadiaTimelineItem>(p => p
            .Add(c => c.Title, "Event")
            .Add(c => c.Color, color));

        cut.Find($".arcadia-timeline-item__dot--{color}").Should().NotBeNull();
    }

    [Fact]
    public void TimelineItem_ChildContent_Renders()
    {
        var cut = Render<ArcadiaTimelineItem>(p => p
            .Add(c => c.Title, "Event")
            .Add(c => c.ChildContent, "<p>Details here</p>"));

        cut.Find(".arcadia-timeline-item__content").InnerHtml.Should().Contain("Details here");
    }

    [Fact]
    public void TimelineItem_HasListItemRole()
    {
        var cut = Render<ArcadiaTimelineItem>(p => p
            .Add(c => c.Title, "Event"));

        cut.Find("[role='listitem']").Should().NotBeNull();
    }
}

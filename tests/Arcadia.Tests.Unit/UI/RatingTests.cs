using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Arcadia.Tests.Unit.UI;

public class RatingTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaRating>();

        cut.Find(".arcadia-rating").Should().NotBeNull();
    }

    [Fact]
    public void Default_RendersFiveStars()
    {
        var cut = Render<ArcadiaRating>();

        cut.FindAll(".arcadia-rating__star").Should().HaveCount(5);
    }

    [Fact]
    public void Max_CustomValue_RendersCorrectStars()
    {
        var cut = Render<ArcadiaRating>(p => p
            .Add(c => c.Max, 10));

        cut.FindAll(".arcadia-rating__star").Should().HaveCount(10);
    }

    [Fact]
    public void StarClick_UpdatesValue()
    {
        var newValue = 0;
        var cut = Render<ArcadiaRating>(p => p
            .Add(c => c.Value, 0)
            .Add(c => c.ValueChanged, v => newValue = v));

        cut.FindAll(".arcadia-rating__star")[2].Click();

        newValue.Should().Be(3);
    }

    [Fact]
    public void ReadOnly_PreventsValueChange()
    {
        var changed = false;
        var cut = Render<ArcadiaRating>(p => p
            .Add(c => c.Value, 3)
            .Add(c => c.ReadOnly, true)
            .Add(c => c.ValueChanged, _ => changed = true));

        cut.FindAll(".arcadia-rating__star")[4].Click();

        changed.Should().BeFalse();
    }

    [Fact]
    public void Aria_RoleSlider()
    {
        var cut = Render<ArcadiaRating>(p => p
            .Add(c => c.Value, 3)
            .Add(c => c.Max, 5));

        var el = cut.Find("[role='slider']");
        el.GetAttribute("aria-valuenow").Should().Be("3");
        el.GetAttribute("aria-valuemax").Should().Be("5");
    }

    [Fact]
    public void ReadOnly_HasReadOnlyClass()
    {
        var cut = Render<ArcadiaRating>(p => p
            .Add(c => c.ReadOnly, true));

        cut.Find(".arcadia-rating").ClassList.Should().Contain("arcadia-rating--readonly");
    }
}

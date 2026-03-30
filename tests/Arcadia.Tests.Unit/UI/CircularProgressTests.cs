using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class CircularProgressTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaCircularProgress>();

        cut.Find(".arcadia-circular-progress").Should().NotBeNull();
    }

    [Fact]
    public void SVG_RendersTrackAndArc()
    {
        var cut = Render<ArcadiaCircularProgress>(p => p
            .Add(c => c.Value, 50));

        cut.Find(".arcadia-circular-progress__track").Should().NotBeNull();
        cut.Find(".arcadia-circular-progress__arc").Should().NotBeNull();
    }

    [Fact]
    public void ShowLabel_DisplaysPercentage()
    {
        var cut = Render<ArcadiaCircularProgress>(p => p
            .Add(c => c.Value, 80)
            .Add(c => c.ShowLabel, true));

        cut.Find(".arcadia-circular-progress__label").TextContent.Should().Be("80%");
    }

    [Fact]
    public void Indeterminate_AddsIndeterminateClass()
    {
        var cut = Render<ArcadiaCircularProgress>(p => p
            .Add(c => c.Indeterminate, true));

        cut.Find(".arcadia-circular-progress").ClassList
            .Should().Contain("arcadia-circular-progress--indeterminate");
    }

    [Fact]
    public void Aria_Progressbar_Role()
    {
        var cut = Render<ArcadiaCircularProgress>(p => p
            .Add(c => c.Value, 40));

        var el = cut.Find("[role='progressbar']");
        el.Should().NotBeNull();
        el.GetAttribute("aria-valuenow").Should().Be("40");
        el.GetAttribute("aria-valuemin").Should().Be("0");
        el.GetAttribute("aria-valuemax").Should().Be("100");
    }

    [Fact]
    public void Color_AppliedToCssClass()
    {
        var cut = Render<ArcadiaCircularProgress>(p => p
            .Add(c => c.Color, "danger"));

        cut.Find(".arcadia-circular-progress").ClassList
            .Should().Contain("arcadia-circular-progress--danger");
    }

    [Fact]
    public void Size_AppliedToCssClass()
    {
        var cut = Render<ArcadiaCircularProgress>(p => p
            .Add(c => c.Size, "lg"));

        cut.Find(".arcadia-circular-progress").ClassList
            .Should().Contain("arcadia-circular-progress--lg");
    }
}

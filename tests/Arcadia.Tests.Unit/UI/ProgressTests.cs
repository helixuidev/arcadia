using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class ProgressTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaProgress>();

        cut.Find(".arcadia-progress").Should().NotBeNull();
    }

    [Fact]
    public void Value_SetsBarWidth()
    {
        var cut = Render<ArcadiaProgress>(p => p
            .Add(c => c.Value, 60));

        cut.Find(".arcadia-progress__bar")
            .GetAttribute("style").Should().Contain("width: 60%");
    }

    [Fact]
    public void ShowLabel_DisplaysPercentage()
    {
        var cut = Render<ArcadiaProgress>(p => p
            .Add(c => c.Value, 75)
            .Add(c => c.ShowLabel, true));

        cut.Find(".arcadia-progress__label").TextContent.Should().Be("75%");
    }

    [Fact]
    public void Indeterminate_AddsIndeterminateClass()
    {
        var cut = Render<ArcadiaProgress>(p => p
            .Add(c => c.Indeterminate, true));

        cut.Find(".arcadia-progress").ClassList
            .Should().Contain("arcadia-progress--indeterminate");
    }

    [Fact]
    public void Striped_AddsStripedClassToBar()
    {
        var cut = Render<ArcadiaProgress>(p => p
            .Add(c => c.Striped, true));

        cut.Find(".arcadia-progress__bar").ClassList
            .Should().Contain("arcadia-progress__bar--striped");
    }

    [Fact]
    public void Aria_Progressbar_Role()
    {
        var cut = Render<ArcadiaProgress>(p => p
            .Add(c => c.Value, 50));

        var el = cut.Find("[role='progressbar']");
        el.Should().NotBeNull();
        el.GetAttribute("aria-valuenow").Should().Be("50");
        el.GetAttribute("aria-valuemin").Should().Be("0");
        el.GetAttribute("aria-valuemax").Should().Be("100");
    }

    [Fact]
    public void Color_AppliedToCssClass()
    {
        var cut = Render<ArcadiaProgress>(p => p
            .Add(c => c.Color, "success"));

        cut.Find(".arcadia-progress").ClassList
            .Should().Contain("arcadia-progress--success");
    }
}

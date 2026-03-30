using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class SpinnerTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaSpinner>();

        cut.Find(".arcadia-spinner").Should().NotBeNull();
    }

    [Fact]
    public void Default_RendersSVG()
    {
        var cut = Render<ArcadiaSpinner>();

        cut.Find(".arcadia-spinner__svg").Should().NotBeNull();
    }

    [Fact]
    public void Label_RendersLabelText()
    {
        var cut = Render<ArcadiaSpinner>(p => p
            .Add(c => c.Label, "Loading data..."));

        cut.Find(".arcadia-spinner__label").TextContent.Should().Be("Loading data...");
    }

    [Fact]
    public void Size_AppliedToCssClass()
    {
        var cut = Render<ArcadiaSpinner>(p => p
            .Add(c => c.Size, "lg"));

        cut.Find(".arcadia-spinner").ClassList.Should().Contain("arcadia-spinner--lg");
    }

    [Fact]
    public void Color_AppliedToCssClass()
    {
        var cut = Render<ArcadiaSpinner>(p => p
            .Add(c => c.Color, "danger"));

        cut.Find(".arcadia-spinner").ClassList.Should().Contain("arcadia-spinner--danger");
    }

    [Fact]
    public void Aria_RoleStatus()
    {
        var cut = Render<ArcadiaSpinner>();

        cut.Find("[role='status']").Should().NotBeNull();
    }

    [Fact]
    public void Aria_LabelDefault()
    {
        var cut = Render<ArcadiaSpinner>();

        cut.Find("[role='status']").GetAttribute("aria-label").Should().Be("Loading");
    }

    [Fact]
    public void Aria_LabelCustom()
    {
        var cut = Render<ArcadiaSpinner>(p => p
            .Add(c => c.Label, "Saving"));

        cut.Find("[role='status']").GetAttribute("aria-label").Should().Be("Saving");
    }
}

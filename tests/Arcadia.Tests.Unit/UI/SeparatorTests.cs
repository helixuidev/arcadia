using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class SeparatorTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaSeparator>();

        cut.Find(".arcadia-separator").Should().NotBeNull();
    }

    [Fact]
    public void Orientation_Horizontal_AppliedToClass()
    {
        var cut = Render<ArcadiaSeparator>(p => p
            .Add(c => c.Orientation, "horizontal"));

        cut.Find(".arcadia-separator").ClassList
            .Should().Contain("arcadia-separator--horizontal");
    }

    [Fact]
    public void Orientation_Vertical_AppliedToClass()
    {
        var cut = Render<ArcadiaSeparator>(p => p
            .Add(c => c.Orientation, "vertical"));

        cut.Find(".arcadia-separator").ClassList
            .Should().Contain("arcadia-separator--vertical");
    }

    [Fact]
    public void Label_RendersLabelText()
    {
        var cut = Render<ArcadiaSeparator>(p => p
            .Add(c => c.Label, "OR"));

        cut.Find(".arcadia-separator__label").TextContent.Should().Be("OR");
    }

    [Fact]
    public void Label_AddsHasLabelClass()
    {
        var cut = Render<ArcadiaSeparator>(p => p
            .Add(c => c.Label, "OR"));

        cut.Find(".arcadia-separator").ClassList
            .Should().Contain("arcadia-separator--has-label");
    }

    [Fact]
    public void Variant_AppliedToClass()
    {
        var cut = Render<ArcadiaSeparator>(p => p
            .Add(c => c.Variant, "dashed"));

        cut.Find(".arcadia-separator").ClassList
            .Should().Contain("arcadia-separator--dashed");
    }

    [Fact]
    public void Aria_RoleSeparator()
    {
        var cut = Render<ArcadiaSeparator>();

        cut.Find("[role='separator']").Should().NotBeNull();
    }

    [Fact]
    public void Aria_Orientation()
    {
        var cut = Render<ArcadiaSeparator>(p => p
            .Add(c => c.Orientation, "vertical"));

        cut.Find("[role='separator']")
            .GetAttribute("aria-orientation").Should().Be("vertical");
    }
}

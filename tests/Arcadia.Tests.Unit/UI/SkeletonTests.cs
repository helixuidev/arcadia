using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class SkeletonTests : ChartTestBase
{
    [Fact]
    public void Default_RendersOneElement()
    {
        var cut = Render<ArcadiaSkeleton>();

        cut.FindAll(".arcadia-skeleton").Should().HaveCount(1);
    }

    [Theory]
    [InlineData("text", "arcadia-skeleton--text")]
    [InlineData("circular", "arcadia-skeleton--circular")]
    [InlineData("rectangular", "arcadia-skeleton--rectangular")]
    public void Variant_AppliesCorrectClass(string variant, string expectedClass)
    {
        var cut = Render<ArcadiaSkeleton>(p => p
            .Add(c => c.Variant, variant));

        cut.Find($".{expectedClass}").Should().NotBeNull();
    }

    [Fact]
    public void Count_RendersMultipleElements()
    {
        var cut = Render<ArcadiaSkeleton>(p => p
            .Add(c => c.Count, 4));

        cut.FindAll(".arcadia-skeleton").Should().HaveCount(4);
    }

    [Fact]
    public void CustomWidth_SetsStyleVariable()
    {
        var cut = Render<ArcadiaSkeleton>(p => p
            .Add(c => c.Width, "200px"));

        var el = cut.Find(".arcadia-skeleton");
        el.GetAttribute("style").Should().Contain("--arcadia-skeleton-width");
        el.GetAttribute("style").Should().Contain("200px");
    }

    [Fact]
    public void CustomHeight_SetsStyleVariable()
    {
        var cut = Render<ArcadiaSkeleton>(p => p
            .Add(c => c.Height, "50px"));

        var el = cut.Find(".arcadia-skeleton");
        el.GetAttribute("style").Should().Contain("--arcadia-skeleton-height");
        el.GetAttribute("style").Should().Contain("50px");
    }

    [Fact]
    public void AriaHidden_IsTrue()
    {
        var cut = Render<ArcadiaSkeleton>();

        var el = cut.Find(".arcadia-skeleton");
        el.GetAttribute("aria-hidden").Should().Be("true");
    }

    [Fact]
    public void DefaultVariant_IsText()
    {
        var cut = Render<ArcadiaSkeleton>();

        cut.Find(".arcadia-skeleton--text").Should().NotBeNull();
    }
}

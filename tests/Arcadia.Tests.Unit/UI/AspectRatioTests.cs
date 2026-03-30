using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class AspectRatioTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaAspectRatio>(p => p
            .AddChildContent("<img src='test.jpg' />"));

        cut.Find(".arcadia-aspect-ratio").Should().NotBeNull();
    }

    [Fact]
    public void DefaultRatio_Is16By9()
    {
        var cut = Render<ArcadiaAspectRatio>(p => p
            .AddChildContent("<img src='test.jpg' />"));

        cut.Find(".arcadia-aspect-ratio")
            .GetAttribute("style").Should().Contain("aspect-ratio: 16/9");
    }

    [Fact]
    public void CustomRatio_AppliedToStyle()
    {
        var cut = Render<ArcadiaAspectRatio>(p => p
            .Add(c => c.Ratio, "4/3")
            .AddChildContent("<img src='test.jpg' />"));

        cut.Find(".arcadia-aspect-ratio")
            .GetAttribute("style").Should().Contain("aspect-ratio: 4/3");
    }

    [Fact]
    public void ChildContent_Rendered()
    {
        var cut = Render<ArcadiaAspectRatio>(p => p
            .AddChildContent("<img src='photo.jpg' alt='Test' />"));

        cut.Find(".arcadia-aspect-ratio").InnerHtml.Should().Contain("photo.jpg");
    }

    [Fact]
    public void CustomClass_Applied()
    {
        var cut = Render<ArcadiaAspectRatio>(p => p
            .Add(c => c.Class, "my-ratio")
            .AddChildContent("<div>Content</div>"));

        cut.Find(".arcadia-aspect-ratio").ClassList.Should().Contain("my-ratio");
    }
}

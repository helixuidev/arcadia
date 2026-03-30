using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class CarouselTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaCarousel>(p => p
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 1"))
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 2")));

        cut.Find(".arcadia-carousel").Should().NotBeNull();
    }

    [Fact]
    public void Slides_RenderedInTrack()
    {
        var cut = Render<ArcadiaCarousel>(p => p
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 1"))
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 2"))
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 3")));

        cut.FindAll(".arcadia-carousel__slide").Should().HaveCount(3);
    }

    [Fact]
    public void ShowArrows_RendersNavigationButtons()
    {
        var cut = Render<ArcadiaCarousel>(p => p
            .Add(c => c.ShowArrows, true)
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 1"))
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 2")));

        cut.Find(".arcadia-carousel__arrow--prev").Should().NotBeNull();
        cut.Find(".arcadia-carousel__arrow--next").Should().NotBeNull();
    }

    [Fact]
    public void ShowDots_RendersDotIndicators()
    {
        var cut = Render<ArcadiaCarousel>(p => p
            .Add(c => c.ShowDots, true)
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 1"))
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 2")));

        cut.FindAll(".arcadia-carousel__dot").Should().HaveCount(2);
    }

    [Fact]
    public void NextButton_AdvancesSlide()
    {
        var newIndex = 0;
        var cut = Render<ArcadiaCarousel>(p => p
            .Add(c => c.ActiveIndex, 0)
            .Add(c => c.ActiveIndexChanged, i => newIndex = i)
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 1"))
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 2")));

        cut.Find(".arcadia-carousel__arrow--next").Click();

        newIndex.Should().Be(1);
    }

    [Fact]
    public void Aria_RegionRole()
    {
        var cut = Render<ArcadiaCarousel>(p => p
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 1"))
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 2")));

        var el = cut.Find("[role='region']");
        el.Should().NotBeNull();
        el.GetAttribute("aria-roledescription").Should().Be("carousel");
    }

    [Fact]
    public void Dots_HaveTabRole()
    {
        var cut = Render<ArcadiaCarousel>(p => p
            .Add(c => c.ShowDots, true)
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 1"))
            .AddChildContent<ArcadiaCarouselSlide>(s => s.AddChildContent("Slide 2")));

        cut.FindAll("[role='tab']").Should().HaveCount(2);
    }
}

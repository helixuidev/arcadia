using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class PaginationTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.CurrentPage, 1));

        cut.Find(".arcadia-pagination").Should().NotBeNull();
    }

    [Fact]
    public void PageButtons_RenderedForTotalPages()
    {
        var cut = Render<ArcadiaPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.CurrentPage, 1));

        cut.FindAll(".arcadia-pagination__btn--page").Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void CurrentPage_HasActiveClass()
    {
        var cut = Render<ArcadiaPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.CurrentPage, 1));

        cut.Find(".arcadia-pagination__btn--active").Should().NotBeNull();
    }

    [Fact]
    public void PageClick_FiresCurrentPageChanged()
    {
        var newPage = 0;
        var cut = Render<ArcadiaPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.CurrentPage, 1)
            .Add(c => c.CurrentPageChanged, p => newPage = p));

        // Click page 2
        var pageButtons = cut.FindAll(".arcadia-pagination__btn--page");
        pageButtons[1].Click();

        newPage.Should().Be(2);
    }

    [Fact]
    public void PrevButton_DisabledOnFirstPage()
    {
        var cut = Render<ArcadiaPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.CurrentPage, 1));

        cut.Find(".arcadia-pagination__btn--prev")
            .HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void NextButton_DisabledOnLastPage()
    {
        var cut = Render<ArcadiaPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.CurrentPage, 5));

        cut.Find(".arcadia-pagination__btn--next")
            .HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Aria_NavigationRole()
    {
        var cut = Render<ArcadiaPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.CurrentPage, 1));

        var nav = cut.Find("[role='navigation']");
        nav.Should().NotBeNull();
        nav.GetAttribute("aria-label").Should().Be("Pagination");
    }

    [Fact]
    public void CurrentPage_HasAriaCurrent()
    {
        var cut = Render<ArcadiaPagination>(p => p
            .Add(c => c.TotalPages, 5)
            .Add(c => c.CurrentPage, 1));

        cut.Find("[aria-current='page']").Should().NotBeNull();
    }
}

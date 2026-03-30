using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class EmptyStateTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaEmptyState>();

        cut.Find(".arcadia-empty-state").Should().NotBeNull();
    }

    [Fact]
    public void Default_RendersDefaultTitle()
    {
        var cut = Render<ArcadiaEmptyState>();

        cut.Find(".arcadia-empty-state__title").TextContent.Should().Be("No data");
    }

    [Fact]
    public void Title_RendersCustomTitle()
    {
        var cut = Render<ArcadiaEmptyState>(p => p
            .Add(c => c.Title, "Nothing here"));

        cut.Find(".arcadia-empty-state__title").TextContent.Should().Be("Nothing here");
    }

    [Fact]
    public void Description_RendersDescriptionText()
    {
        var cut = Render<ArcadiaEmptyState>(p => p
            .Add(c => c.Description, "Try creating a new item"));

        cut.Find(".arcadia-empty-state__description").TextContent
            .Should().Be("Try creating a new item");
    }

    [Fact]
    public void Description_NotRenderedWhenNull()
    {
        var cut = Render<ArcadiaEmptyState>();

        cut.FindAll(".arcadia-empty-state__description").Should().BeEmpty();
    }

    [Fact]
    public void ActionTemplate_RendersActions()
    {
        var cut = Render<ArcadiaEmptyState>(p => p
            .Add(c => c.ActionTemplate, "<button>Create</button>"));

        cut.Find(".arcadia-empty-state__actions").InnerHtml.Should().Contain("Create");
    }

    [Fact]
    public void Size_AppliedToCssClass()
    {
        var cut = Render<ArcadiaEmptyState>(p => p
            .Add(c => c.Size, "lg"));

        cut.Find(".arcadia-empty-state").ClassList
            .Should().Contain("arcadia-empty-state--lg");
    }

    [Fact]
    public void DefaultIcon_Rendered()
    {
        var cut = Render<ArcadiaEmptyState>();

        cut.Find(".arcadia-empty-state__icon").Should().NotBeNull();
    }
}

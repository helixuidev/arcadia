using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class CardFeatureTests : ChartTestBase
{
    [Fact]
    public void Card_VariantGlass_AddsGlassClass()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.Variant, "glass")
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card").ClassList.Should().Contain("arcadia-card--glass");
    }

    [Fact]
    public void Card_VariantOutlined_AddsOutlinedClass()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.Variant, "outlined")
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card").ClassList.Should().Contain("arcadia-card--outlined");
    }

    [Fact]
    public void Card_VariantGhost_AddsGhostClass()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.Variant, "ghost")
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card").ClassList.Should().Contain("arcadia-card--ghost");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Card_Elevation_AddsElevationClass(int elevation)
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.Elevation, elevation)
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card").ClassList
            .Should().Contain($"arcadia-card--elevation-{elevation}");
    }

    [Fact]
    public void Card_GradientBorder_AddsGradientClass()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.GradientBorder, true)
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card").ClassList
            .Should().Contain("arcadia-card--gradient-border");
    }

    [Fact]
    public void Card_AccentColor_AddsAccentClass()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.AccentColor, "#ff5722")
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card").ClassList.Should().Contain("arcadia-card--accent");
    }

    [Fact]
    public void Card_ImageUrl_RendersImageDiv()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.ImageUrl, "https://example.com/image.jpg")
            .AddChildContent("<p>Body</p>"));

        var imageDiv = cut.Find(".arcadia-card__image");
        imageDiv.Should().NotBeNull();
        imageDiv.GetAttribute("style").Should().Contain("background-image:url('https://example.com/image.jpg')");
    }

    [Fact]
    public void Card_ImageOverlay_RendersOverlay()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.ImageUrl, "https://example.com/image.jpg")
            .Add(c => c.ImageOverlay, true)
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card__image-overlay").Should().NotBeNull();
    }

    [Fact]
    public void Card_Badge_RendersBadgeSpan()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.Title, "Test Card")
            .Add(c => c.Badge, "New")
            .AddChildContent("<p>Body</p>"));

        var badge = cut.Find(".arcadia-card__badge");
        badge.TextContent.Should().Be("New");
    }

    [Fact]
    public void Card_Loading_RendersSkeletonNotContent()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.Loading, true)
            .Add(c => c.Title, "Should Not Appear")
            .AddChildContent("<p>Body should be hidden</p>"));

        cut.Find(".arcadia-card__skeleton").Should().NotBeNull();
        cut.FindAll(".arcadia-card__body").Should().BeEmpty();
    }

    [Fact]
    public void Card_Collapsible_RendersCollapseButton()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.Collapsible, true)
            .Add(c => c.Title, "Collapsible Card")
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card__collapse-toggle").Should().NotBeNull();
        cut.Find(".arcadia-card__collapse-toggle")
            .GetAttribute("aria-label").Should().Be("Toggle card body");
    }

    [Theory]
    [InlineData("info")]
    [InlineData("warning")]
    [InlineData("error")]
    [InlineData("success")]
    public void Card_Status_AddsStatusClass(string status)
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.Status, status)
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card").ClassList
            .Should().Contain($"arcadia-card--status-{status}");
    }

    [Fact]
    public void Card_Horizontal_AddsHorizontalClass()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.Orientation, "horizontal")
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card").ClassList
            .Should().Contain("arcadia-card--horizontal");
    }

    [Fact]
    public void Card_Selected_AddsSelectedClass()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.Selected, true)
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card").ClassList.Should().Contain("arcadia-card--selected");
    }

    [Fact]
    public void Card_Disabled_AddsDisabledClass_And_AriaDisabled()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("<p>Body</p>"));

        var card = cut.Find(".arcadia-card");
        card.ClassList.Should().Contain("arcadia-card--disabled");
        card.GetAttribute("aria-disabled").Should().Be("true");
    }

    [Fact]
    public void Card_MenuTemplate_RendersMenuButton()
    {
        var cut = Render<ArcadiaCard>(p => p
            .Add(c => c.MenuTemplate, "<span>Edit</span><span>Delete</span>")
            .AddChildContent("<p>Body</p>"));

        cut.Find(".arcadia-card__menu-trigger").Should().NotBeNull();
        cut.Find(".arcadia-card__menu-trigger")
            .GetAttribute("aria-label").Should().Be("Card menu");
    }
}

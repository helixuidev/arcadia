using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class ChipTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaChip>(p => p
            .Add(c => c.Label, "Tag"));

        cut.Find(".arcadia-chip").Should().NotBeNull();
    }

    [Fact]
    public void Label_RendersInChipLabel()
    {
        var cut = Render<ArcadiaChip>(p => p
            .Add(c => c.Label, "Status"));

        cut.Find(".arcadia-chip__label").TextContent.Should().Be("Status");
    }

    [Fact]
    public void Clickable_RendersAsButton()
    {
        var cut = Render<ArcadiaChip>(p => p
            .Add(c => c.Label, "Click me")
            .Add(c => c.Clickable, true));

        cut.Find("button.arcadia-chip").Should().NotBeNull();
    }

    [Fact]
    public void NotClickable_RendersAsSpan()
    {
        var cut = Render<ArcadiaChip>(p => p
            .Add(c => c.Label, "Static"));

        cut.Find("span.arcadia-chip").Should().NotBeNull();
    }

    [Fact]
    public void OnClick_Fires_WhenClickable()
    {
        var clicked = false;
        var cut = Render<ArcadiaChip>(p => p
            .Add(c => c.Label, "Click")
            .Add(c => c.Clickable, true)
            .Add(c => c.OnClick, () => clicked = true));

        cut.Find("button.arcadia-chip").Click();

        clicked.Should().BeTrue();
    }

    [Fact]
    public void Deletable_ShowsDeleteButton()
    {
        var cut = Render<ArcadiaChip>(p => p
            .Add(c => c.Label, "Remove me")
            .Add(c => c.Deletable, true));

        cut.Find(".arcadia-chip__delete").Should().NotBeNull();
    }

    [Fact]
    public void OnDelete_Fires_WhenDeleteClicked()
    {
        var deleted = false;
        var cut = Render<ArcadiaChip>(p => p
            .Add(c => c.Label, "Remove")
            .Add(c => c.Deletable, true)
            .Add(c => c.OnDelete, () => deleted = true));

        cut.Find(".arcadia-chip__delete").Click();

        deleted.Should().BeTrue();
    }

    [Fact]
    public void Color_AppliedToCssClass()
    {
        var cut = Render<ArcadiaChip>(p => p
            .Add(c => c.Label, "Success")
            .Add(c => c.Color, "success"));

        cut.Find(".arcadia-chip").ClassList.Should().Contain("arcadia-chip--success");
    }

    [Fact]
    public void DeleteButton_HasAriaLabel()
    {
        var cut = Render<ArcadiaChip>(p => p
            .Add(c => c.Label, "Tag")
            .Add(c => c.Deletable, true));

        cut.Find(".arcadia-chip__delete")
            .GetAttribute("aria-label").Should().Be("Remove");
    }
}

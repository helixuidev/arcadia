using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class CommandItemTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaCommandItem>(p => p
            .Add(c => c.Label, "Open File"));

        cut.Find(".arcadia-command-item").Should().NotBeNull();
    }

    [Fact]
    public void Label_RendersInContent()
    {
        var cut = Render<ArcadiaCommandItem>(p => p
            .Add(c => c.Label, "New File"));

        cut.Find(".arcadia-command-item__label").TextContent.Should().Be("New File");
    }

    [Fact]
    public void Shortcut_RendersKbd()
    {
        var cut = Render<ArcadiaCommandItem>(p => p
            .Add(c => c.Label, "Save")
            .Add(c => c.Shortcut, "Ctrl+S"));

        cut.Find(".arcadia-command-item__shortcut").TextContent.Should().Be("Ctrl+S");
    }

    [Fact]
    public void OnSelect_Fires_WhenClicked()
    {
        var selected = false;
        var cut = Render<ArcadiaCommandItem>(p => p
            .Add(c => c.Label, "Run")
            .Add(c => c.OnSelect, () => selected = true));

        cut.Find(".arcadia-command-item").Click();

        selected.Should().BeTrue();
    }

    [Fact]
    public void Disabled_PreventsOnSelect()
    {
        var selected = false;
        var cut = Render<ArcadiaCommandItem>(p => p
            .Add(c => c.Label, "Run")
            .Add(c => c.Disabled, true)
            .Add(c => c.OnSelect, () => selected = true));

        cut.Find(".arcadia-command-item").Click();

        selected.Should().BeFalse();
    }

    [Fact]
    public void Disabled_HasAriaDisabled()
    {
        var cut = Render<ArcadiaCommandItem>(p => p
            .Add(c => c.Label, "Disabled")
            .Add(c => c.Disabled, true));

        cut.Find(".arcadia-command-item").GetAttribute("aria-disabled").Should().Be("true");
    }

    [Fact]
    public void Aria_RoleOption()
    {
        var cut = Render<ArcadiaCommandItem>(p => p
            .Add(c => c.Label, "Item"));

        cut.Find("[role='option']").Should().NotBeNull();
    }
}

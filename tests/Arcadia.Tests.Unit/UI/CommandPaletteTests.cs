using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Arcadia.Tests.Unit.UI;

public class CommandPaletteTests : ChartTestBase
{
    [Fact]
    public void Open_False_RendersNothing()
    {
        var cut = Render<ArcadiaCommandPalette>(p => p
            .Add(c => c.Open, false));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Open_True_RendersOverlayAndPalette()
    {
        var cut = Render<ArcadiaCommandPalette>(p => p
            .Add(c => c.Open, true));

        cut.Find(".arcadia-command-palette-overlay").Should().NotBeNull();
        cut.Find(".arcadia-command-palette").Should().NotBeNull();
    }

    [Fact]
    public void Placeholder_RendersInInput()
    {
        var cut = Render<ArcadiaCommandPalette>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Placeholder, "Type a command..."));

        cut.Find(".arcadia-command-palette__input")
            .GetAttribute("placeholder").Should().Be("Type a command...");
    }

    [Fact]
    public void Escape_ClosesThePalette()
    {
        var closed = false;
        var cut = Render<ArcadiaCommandPalette>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, v => closed = true));

        cut.Find(".arcadia-command-palette-overlay")
            .KeyDown(new KeyboardEventArgs { Key = "Escape" });

        closed.Should().BeTrue();
    }

    [Fact]
    public void Aria_RoleDialog_AndAriaModal()
    {
        var cut = Render<ArcadiaCommandPalette>(p => p
            .Add(c => c.Open, true));

        var dialog = cut.Find("[role='dialog']");
        dialog.Should().NotBeNull();
        dialog.GetAttribute("aria-modal").Should().Be("true");
    }

    [Fact]
    public void SearchInput_HasComboboxRole()
    {
        var cut = Render<ArcadiaCommandPalette>(p => p
            .Add(c => c.Open, true));

        var input = cut.Find("[role='combobox']");
        input.Should().NotBeNull();
        input.GetAttribute("aria-autocomplete").Should().Be("list");
    }

    [Fact]
    public void Listbox_RolePresent()
    {
        var cut = Render<ArcadiaCommandPalette>(p => p
            .Add(c => c.Open, true));

        cut.Find("[role='listbox']").Should().NotBeNull();
    }
}

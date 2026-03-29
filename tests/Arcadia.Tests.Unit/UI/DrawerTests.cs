using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Arcadia.Tests.Unit.UI;

public class DrawerTests : ChartTestBase
{
    [Fact]
    public void Visible_False_RendersNothing()
    {
        var cut = Render<ArcadiaDrawer>(p => p
            .Add(c => c.Visible, false)
            .Add(c => c.Title, "Test"));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Visible_True_ShowsDrawer()
    {
        var cut = Render<ArcadiaDrawer>(p => p
            .Add(c => c.Visible, true)
            .Add(c => c.Title, "Test"));

        cut.Find(".arcadia-drawer").Should().NotBeNull();
    }

    [Fact]
    public void Title_RendersInHeader()
    {
        var cut = Render<ArcadiaDrawer>(p => p
            .Add(c => c.Visible, true)
            .Add(c => c.Title, "My Drawer"));

        cut.Find(".arcadia-drawer__title").TextContent.Should().Be("My Drawer");
    }

    [Theory]
    [InlineData("left", "arcadia-drawer--left")]
    [InlineData("right", "arcadia-drawer--right")]
    [InlineData("bottom", "arcadia-drawer--bottom")]
    public void Position_AppliesCorrectClass(string position, string expectedClass)
    {
        var cut = Render<ArcadiaDrawer>(p => p
            .Add(c => c.Visible, true)
            .Add(c => c.Position, position));

        cut.Find($".{expectedClass}").Should().NotBeNull();
    }

    [Fact]
    public void ShowOverlay_RendersOverlay()
    {
        var cut = Render<ArcadiaDrawer>(p => p
            .Add(c => c.Visible, true)
            .Add(c => c.ShowOverlay, true));

        cut.Find(".arcadia-drawer-overlay").Should().NotBeNull();
    }

    [Fact]
    public void ShowOverlay_False_NoOverlay()
    {
        var cut = Render<ArcadiaDrawer>(p => p
            .Add(c => c.Visible, true)
            .Add(c => c.ShowOverlay, false));

        cut.FindAll(".arcadia-drawer-overlay").Should().BeEmpty();
    }

    [Fact]
    public void Escape_ClosesDrawer()
    {
        var closed = false;
        var cut = Render<ArcadiaDrawer>(p => p
            .Add(c => c.Visible, true)
            .Add(c => c.VisibleChanged, v => closed = !v));

        cut.Find(".arcadia-drawer").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        closed.Should().BeTrue();
    }

    [Fact]
    public void CloseButton_FiresVisibleChanged()
    {
        var closed = false;
        var cut = Render<ArcadiaDrawer>(p => p
            .Add(c => c.Visible, true)
            .Add(c => c.VisibleChanged, v => closed = !v));

        cut.Find(".arcadia-drawer__close").Click();

        closed.Should().BeTrue();
    }

    [Fact]
    public void Aria_RoleDialogAndModal()
    {
        var cut = Render<ArcadiaDrawer>(p => p
            .Add(c => c.Visible, true)
            .Add(c => c.Title, "Accessible"));

        var drawer = cut.Find("[role='dialog']");
        drawer.Should().NotBeNull();
        drawer.GetAttribute("aria-modal").Should().Be("true");
    }
}

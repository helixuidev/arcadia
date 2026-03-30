using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Arcadia.Tests.Unit.UI;

public class ContextMenuTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaContextMenu>(p => p
            .AddChildContent("<div>Right-click me</div>")
            .Add(c => c.MenuContent, "<div>Menu</div>"));

        cut.Find(".arcadia-context-menu").Should().NotBeNull();
    }

    [Fact]
    public void MenuNotVisibleByDefault()
    {
        var cut = Render<ArcadiaContextMenu>(p => p
            .AddChildContent("<div>Right-click me</div>")
            .Add(c => c.MenuContent, "<div>Menu</div>"));

        cut.FindAll(".arcadia-context-menu__menu").Should().BeEmpty();
    }

    [Fact]
    public void RightClick_ShowsMenu()
    {
        var cut = Render<ArcadiaContextMenu>(p => p
            .AddChildContent("<div>Right-click me</div>")
            .Add(c => c.MenuContent, "<div>Menu items</div>"));

        cut.Find(".arcadia-context-menu").TriggerEvent("oncontextmenu",
            new MouseEventArgs { ClientX = 100, ClientY = 200 });

        cut.Find(".arcadia-context-menu__menu").Should().NotBeNull();
    }

    [Fact]
    public void Disabled_PreventsMenuOpening()
    {
        var cut = Render<ArcadiaContextMenu>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("<div>Right-click me</div>")
            .Add(c => c.MenuContent, "<div>Menu</div>"));

        cut.Find(".arcadia-context-menu").TriggerEvent("oncontextmenu",
            new MouseEventArgs { ClientX = 100, ClientY = 200 });

        cut.FindAll(".arcadia-context-menu__menu").Should().BeEmpty();
    }

    [Fact]
    public void Disabled_AddsDisabledClass()
    {
        var cut = Render<ArcadiaContextMenu>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("<div>Content</div>")
            .Add(c => c.MenuContent, "<div>Menu</div>"));

        cut.Find(".arcadia-context-menu").ClassList
            .Should().Contain("arcadia-context-menu--disabled");
    }

    [Fact]
    public void Aria_RoleMenu_WhenOpen()
    {
        var cut = Render<ArcadiaContextMenu>(p => p
            .AddChildContent("<div>Content</div>")
            .Add(c => c.MenuContent, "<div>Menu</div>"));

        cut.Find(".arcadia-context-menu").TriggerEvent("oncontextmenu",
            new MouseEventArgs { ClientX = 50, ClientY = 50 });

        cut.Find("[role='menu']").Should().NotBeNull();
    }
}

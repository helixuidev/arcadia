using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Arcadia.Tests.Unit.UI;

public class MenuTests : ChartTestBase
{
    [Fact]
    public void Closed_DoesNotRenderDropdown()
    {
        var cut = Render<ArcadiaMenu>(p => p
            .Add(c => c.Open, false)
            .Add(c => c.Trigger, "<button>Open</button>"));

        cut.FindAll("[role='menu']").Should().BeEmpty();
    }

    [Fact]
    public void Open_RendersDropdownWithRoleMenu()
    {
        var cut = Render<ArcadiaMenu>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Trigger, "<button>Open</button>"));

        cut.Find("[role='menu']").Should().NotBeNull();
    }

    [Fact]
    public void TriggerClick_TogglesOpen()
    {
        bool? openValue = null;
        var cut = Render<ArcadiaMenu>(p => p
            .Add(c => c.Open, false)
            .Add(c => c.OpenChanged, v => openValue = v)
            .Add(c => c.Trigger, "<button>Open</button>"));

        cut.Find(".arcadia-menu__trigger").Click();

        openValue.Should().BeTrue();
    }

    [Fact]
    public void Escape_ClosesMenu()
    {
        bool? openValue = null;
        var cut = Render<ArcadiaMenu>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, v => openValue = v)
            .Add(c => c.Trigger, "<button>Open</button>"));

        cut.Find("[role='menu']").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        openValue.Should().BeFalse();
    }

    [Fact]
    public void BackdropClick_ClosesMenu()
    {
        bool? openValue = null;
        var cut = Render<ArcadiaMenu>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, v => openValue = v)
            .Add(c => c.Trigger, "<button>Open</button>"));

        cut.Find(".arcadia-menu__backdrop").Click();

        openValue.Should().BeFalse();
    }

    [Fact]
    public void Open_AddsOpenCssClass()
    {
        var cut = Render<ArcadiaMenu>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Trigger, "<button>Open</button>"));

        cut.Find(".arcadia-menu--open").Should().NotBeNull();
    }

    [Fact]
    public void MenuItem_Click_FiresCallback()
    {
        var clicked = false;
        var cut = Render<ArcadiaMenuItem>(p => p
            .Add(c => c.Text, "Edit")
            .Add(c => c.OnClick, () => clicked = true));

        cut.Find("[role='menuitem']").Click();

        clicked.Should().BeTrue();
    }

    [Fact]
    public void MenuItem_Disabled_HasDisabledAttribute()
    {
        var cut = Render<ArcadiaMenuItem>(p => p
            .Add(c => c.Text, "Disabled Item")
            .Add(c => c.Disabled, true));

        var button = cut.Find("[role='menuitem']");
        button.GetAttribute("disabled").Should().NotBeNull();
        button.ClassList.Should().Contain("arcadia-menu-item--disabled");
    }

    [Fact]
    public void MenuItem_Divider_RendersSeparator()
    {
        var cut = Render<ArcadiaMenuItem>(p => p
            .Add(c => c.Divider, true));

        cut.Find("[role='separator']").Should().NotBeNull();
        cut.FindAll("[role='menuitem']").Should().BeEmpty();
    }
}

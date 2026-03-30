using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Arcadia.Tests.Unit.UI;

public class PopoverTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaPopover>(p => p
            .AddChildContent("<button>Click me</button>")
            .Add(c => c.Content, "<div>Popover</div>"));

        cut.Find(".arcadia-popover").Should().NotBeNull();
    }

    [Fact]
    public void Open_False_ContentHidden()
    {
        var cut = Render<ArcadiaPopover>(p => p
            .Add(c => c.Open, false)
            .AddChildContent("<button>Click</button>")
            .Add(c => c.Content, "<div>Hidden</div>"));

        cut.FindAll(".arcadia-popover__content").Should().BeEmpty();
    }

    [Fact]
    public void Open_True_ContentVisible()
    {
        var cut = Render<ArcadiaPopover>(p => p
            .Add(c => c.Open, true)
            .AddChildContent("<button>Click</button>")
            .Add(c => c.Content, "<div>Visible</div>"));

        cut.Find(".arcadia-popover__content").Should().NotBeNull();
        cut.Find(".arcadia-popover").ClassList.Should().Contain("arcadia-popover--open");
    }

    [Fact]
    public void TriggerClick_TogglesOpen()
    {
        var openState = false;
        var cut = Render<ArcadiaPopover>(p => p
            .Add(c => c.Open, false)
            .Add(c => c.OpenChanged, v => openState = v)
            .AddChildContent("<button>Toggle</button>")
            .Add(c => c.Content, "<div>Content</div>"));

        cut.Find(".arcadia-popover__trigger").Click();

        openState.Should().BeTrue();
    }

    [Fact]
    public void Escape_ClosePopover()
    {
        var openState = true;
        var cut = Render<ArcadiaPopover>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, v => openState = v)
            .AddChildContent("<button>Toggle</button>")
            .Add(c => c.Content, "<div>Content</div>"));

        cut.Find(".arcadia-popover__content")
            .KeyDown(new KeyboardEventArgs { Key = "Escape" });

        openState.Should().BeFalse();
    }

    [Fact]
    public void Aria_RoleDialog()
    {
        var cut = Render<ArcadiaPopover>(p => p
            .Add(c => c.Open, true)
            .AddChildContent("<button>Toggle</button>")
            .Add(c => c.Content, "<div>Content</div>"));

        cut.Find("[role='dialog']").Should().NotBeNull();
    }

    [Fact]
    public void Position_AppliedToContentClass()
    {
        var cut = Render<ArcadiaPopover>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Position, "top")
            .AddChildContent("<button>Toggle</button>")
            .Add(c => c.Content, "<div>Content</div>"));

        cut.Find(".arcadia-popover__content--top").Should().NotBeNull();
    }
}

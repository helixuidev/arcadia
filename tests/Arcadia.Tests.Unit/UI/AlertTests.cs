using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class AlertTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaAlert>(p => p
            .AddChildContent("Alert message"));

        cut.Find(".arcadia-alert").Should().NotBeNull();
    }

    [Fact]
    public void Severity_AppliedToClass()
    {
        var cut = Render<ArcadiaAlert>(p => p
            .Add(c => c.Severity, "error")
            .AddChildContent("Error!"));

        cut.Find(".arcadia-alert").ClassList.Should().Contain("arcadia-alert--error");
    }

    [Fact]
    public void Title_RendersInTitleElement()
    {
        var cut = Render<ArcadiaAlert>(p => p
            .Add(c => c.Title, "Warning")
            .AddChildContent("Something happened"));

        cut.Find(".arcadia-alert__title").TextContent.Should().Be("Warning");
    }

    [Fact]
    public void Dismissible_ShowsDismissButton()
    {
        var cut = Render<ArcadiaAlert>(p => p
            .Add(c => c.Dismissible, true)
            .AddChildContent("Dismissible alert"));

        cut.Find(".arcadia-alert__dismiss").Should().NotBeNull();
    }

    [Fact]
    public void DismissButton_FiresOnDismiss()
    {
        var dismissed = false;
        var cut = Render<ArcadiaAlert>(p => p
            .Add(c => c.Dismissible, true)
            .Add(c => c.OnDismiss, () => dismissed = true)
            .AddChildContent("Alert"));

        cut.Find(".arcadia-alert__dismiss").Click();

        dismissed.Should().BeTrue();
    }

    [Fact]
    public void Visible_False_RendersNothing()
    {
        var cut = Render<ArcadiaAlert>(p => p
            .Add(c => c.Visible, false)
            .AddChildContent("Hidden"));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Aria_RoleAlert()
    {
        var cut = Render<ArcadiaAlert>(p => p
            .AddChildContent("Alert"));

        cut.Find("[role='alert']").Should().NotBeNull();
    }

    [Fact]
    public void DismissButton_HasAriaLabel()
    {
        var cut = Render<ArcadiaAlert>(p => p
            .Add(c => c.Dismissible, true)
            .AddChildContent("Alert"));

        cut.Find(".arcadia-alert__dismiss")
            .GetAttribute("aria-label").Should().Be("Dismiss alert");
    }
}

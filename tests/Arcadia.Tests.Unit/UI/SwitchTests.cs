using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class SwitchTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaSwitch>();

        cut.Find(".arcadia-switch").Should().NotBeNull();
    }

    [Fact]
    public void Value_True_AddsCheckedClass()
    {
        var cut = Render<ArcadiaSwitch>(p => p
            .Add(c => c.Value, true));

        cut.Find(".arcadia-switch").ClassList.Should().Contain("arcadia-switch--checked");
    }

    [Fact]
    public void ValueChanged_Fires_OnToggle()
    {
        var newValue = false;
        var cut = Render<ArcadiaSwitch>(p => p
            .Add(c => c.Value, false)
            .Add(c => c.ValueChanged, v => newValue = v));

        cut.Find(".arcadia-switch__input").Change(true);

        newValue.Should().BeTrue();
    }

    [Fact]
    public void Disabled_AddsDisabledClass()
    {
        var cut = Render<ArcadiaSwitch>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".arcadia-switch").ClassList.Should().Contain("arcadia-switch--disabled");
    }

    [Fact]
    public void Disabled_InputHasDisabledAttribute()
    {
        var cut = Render<ArcadiaSwitch>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".arcadia-switch__input").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Label_RendersLabelText()
    {
        var cut = Render<ArcadiaSwitch>(p => p
            .Add(c => c.Label, "Dark Mode"));

        cut.Find(".arcadia-switch__label").TextContent.Should().Be("Dark Mode");
    }

    [Fact]
    public void Aria_RoleSwitch()
    {
        var cut = Render<ArcadiaSwitch>();

        var input = cut.Find("[role='switch']");
        input.Should().NotBeNull();
    }

    [Fact]
    public void Aria_AriaChecked_ReflectsValue()
    {
        var cut = Render<ArcadiaSwitch>(p => p
            .Add(c => c.Value, true));

        cut.Find("[role='switch']").GetAttribute("aria-checked").Should().Be("true");
    }
}

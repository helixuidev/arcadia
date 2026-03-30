using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class ColorPickerTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaColorPicker>();

        cut.Find(".arcadia-color-picker").Should().NotBeNull();
    }

    [Fact]
    public void Default_RendersSwatches()
    {
        var cut = Render<ArcadiaColorPicker>();

        cut.FindAll(".arcadia-color-picker__swatch").Count.Should().Be(12);
    }

    [Fact]
    public void CustomPresets_RendersCorrectCount()
    {
        var cut = Render<ArcadiaColorPicker>(p => p
            .Add(c => c.Presets, new[] { "#ff0000", "#00ff00", "#0000ff" }));

        cut.FindAll(".arcadia-color-picker__swatch").Count.Should().Be(3);
    }

    [Fact]
    public void SwatchClick_UpdatesValue()
    {
        string? selected = null;
        var cut = Render<ArcadiaColorPicker>(p => p
            .Add(c => c.ValueChanged, v => selected = v));

        cut.FindAll(".arcadia-color-picker__swatch")[0].Click();

        selected.Should().Be("#ef4444");
    }

    [Fact]
    public void Disabled_AddsDisabledClass()
    {
        var cut = Render<ArcadiaColorPicker>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".arcadia-color-picker").ClassList
            .Should().Contain("arcadia-color-picker--disabled");
    }

    [Fact]
    public void ShowInput_True_RendersTextInput()
    {
        var cut = Render<ArcadiaColorPicker>(p => p
            .Add(c => c.ShowInput, true));

        cut.Find(".arcadia-color-picker__input").Should().NotBeNull();
    }

    [Fact]
    public void Aria_RadioGroup()
    {
        var cut = Render<ArcadiaColorPicker>();

        cut.Find("[role='radiogroup']").Should().NotBeNull();
    }

    [Fact]
    public void Aria_SwatchAriaChecked()
    {
        var cut = Render<ArcadiaColorPicker>(p => p
            .Add(c => c.Value, "#ef4444"));

        var swatch = cut.FindAll("[role='radio']")[0];
        swatch.GetAttribute("aria-checked").Should().Be("true");
    }
}

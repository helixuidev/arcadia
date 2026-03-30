using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class SliderTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaSlider>();

        cut.Find(".arcadia-slider").Should().NotBeNull();
    }

    [Fact]
    public void Value_DisplayedWhenShowValueTrue()
    {
        var cut = Render<ArcadiaSlider>(p => p
            .Add(c => c.Value, 50)
            .Add(c => c.ShowValue, true));

        cut.Find(".arcadia-slider__value").TextContent.Should().Be("50");
    }

    [Fact]
    public void Label_RendersAboveSlider()
    {
        var cut = Render<ArcadiaSlider>(p => p
            .Add(c => c.Label, "Volume"));

        cut.Find(".arcadia-slider__label").TextContent.Should().Be("Volume");
    }

    [Fact]
    public void Disabled_AddsDisabledClass()
    {
        var cut = Render<ArcadiaSlider>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".arcadia-slider").ClassList.Should().Contain("arcadia-slider--disabled");
    }

    [Fact]
    public void Disabled_InputHasDisabledAttribute()
    {
        var cut = Render<ArcadiaSlider>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".arcadia-slider__input").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Aria_ValueAttributes()
    {
        var cut = Render<ArcadiaSlider>(p => p
            .Add(c => c.Value, 30)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100));

        var input = cut.Find(".arcadia-slider__input");
        input.GetAttribute("aria-valuenow").Should().Be("30");
        input.GetAttribute("aria-valuemin").Should().Be("0");
        input.GetAttribute("aria-valuemax").Should().Be("100");
    }

    [Fact]
    public void Color_AppliedToCssClass()
    {
        var cut = Render<ArcadiaSlider>(p => p
            .Add(c => c.Color, "success"));

        cut.Find(".arcadia-slider").ClassList.Should().Contain("arcadia-slider--success");
    }
}

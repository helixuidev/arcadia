using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class DatePickerTests : ChartTestBase
{
    [Fact]
    public void Renders_InputTypeDate()
    {
        var cut = Render<ArcadiaDatePicker>();

        var input = cut.Find("input[type='date']");
        input.Should().NotBeNull();
    }

    [Fact]
    public void Label_RendersLabelElement()
    {
        var cut = Render<ArcadiaDatePicker>(p => p
            .Add(c => c.Label, "Start Date"));

        var label = cut.Find(".arcadia-datepicker__label");
        label.TextContent.Should().Be("Start Date");
    }

    [Fact]
    public void NoLabel_DoesNotRenderLabelElement()
    {
        var cut = Render<ArcadiaDatePicker>();

        cut.FindAll(".arcadia-datepicker__label").Should().BeEmpty();
    }

    [Fact]
    public void Disabled_SetsDisabledAttribute()
    {
        var cut = Render<ArcadiaDatePicker>(p => p
            .Add(c => c.Disabled, true));

        var input = cut.Find("input[type='date']");
        input.GetAttribute("disabled").Should().NotBeNull();
    }

    [Fact]
    public void Disabled_AddsCssClass()
    {
        var cut = Render<ArcadiaDatePicker>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".arcadia-datepicker--disabled").Should().NotBeNull();
    }

    [Fact]
    public void Min_SetsMinAttribute()
    {
        var minDate = new DateTime(2025, 1, 1);
        var cut = Render<ArcadiaDatePicker>(p => p
            .Add(c => c.Min, minDate));

        var input = cut.Find("input[type='date']");
        input.GetAttribute("min").Should().Be("2025-01-01");
    }

    [Fact]
    public void Max_SetsMaxAttribute()
    {
        var maxDate = new DateTime(2025, 12, 31);
        var cut = Render<ArcadiaDatePicker>(p => p
            .Add(c => c.Max, maxDate));

        var input = cut.Find("input[type='date']");
        input.GetAttribute("max").Should().Be("2025-12-31");
    }

    [Fact]
    public void Value_SetsInputValue()
    {
        var date = new DateTime(2025, 6, 15);
        var cut = Render<ArcadiaDatePicker>(p => p
            .Add(c => c.Value, date));

        var input = cut.Find("input[type='date']");
        input.GetAttribute("value").Should().Be("2025-06-15");
    }
}

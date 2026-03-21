using Bunit;
using FluentAssertions;
using HelixUI.FormBuilder.Components.Fields;
using HelixUI.FormBuilder.Schema;
using Xunit;

namespace HelixUI.Tests.Unit.FormBuilder;

public class NewFieldTests : BunitContext
{
    [Fact]
    public void PasswordField_Renders_HiddenByDefault()
    {
        var cut = Render<PasswordField>(p =>
            p.Add(c => c.Label, "Password")
             .Add(c => c.Value, "secret"));

        cut.Find("input").GetAttribute("type").Should().Be("password");
    }

    [Fact]
    public void PasswordField_Toggle_ShowsPassword()
    {
        var cut = Render<PasswordField>(p =>
            p.Add(c => c.Label, "Password")
             .Add(c => c.Value, "secret"));

        cut.Find(".helix-field__password-toggle").Click();

        cut.Find("input").GetAttribute("type").Should().Be("text");
    }

    [Fact]
    public void PasswordField_StrengthMeter_Shows()
    {
        var cut = Render<PasswordField>(p =>
            p.Add(c => c.Label, "Password")
             .Add(c => c.Value, "Str0ng!Pass")
             .Add(c => c.ShowStrength, true));

        cut.FindAll(".helix-field__password-strength").Should().HaveCount(1);
    }

    [Fact]
    public void RatingField_Renders_Stars()
    {
        var cut = Render<RatingField>(p =>
            p.Add(c => c.Label, "Rating")
             .Add(c => c.Value, 3)
             .Add(c => c.MaxRating, 5));

        var stars = cut.FindAll(".helix-field__rating-star");
        stars.Should().HaveCount(5);
        stars.Count(s => s.ClassList.Contains("helix-field__rating-star--active")).Should().Be(3);
    }

    [Fact]
    public void RatingField_Click_SetsValue()
    {
        var value = 0;
        var cut = Render<RatingField>(p =>
            p.Add(c => c.Label, "Rating")
             .Add(c => c.Value, 0)
             .Add(c => c.ValueChanged, (int v) => value = v));

        cut.FindAll(".helix-field__rating-star")[3].Click();

        value.Should().Be(4);
    }

    [Fact]
    public void SliderField_Renders_WithMinMax()
    {
        var cut = Render<SliderField>(p =>
            p.Add(c => c.Label, "Volume")
             .Add(c => c.Value, 50)
             .Add(c => c.Min, 0)
             .Add(c => c.Max, 100));

        var slider = cut.Find("input[type='range']");
        slider.GetAttribute("min").Should().Be("0");
        slider.GetAttribute("max").Should().Be("100");
    }

    [Fact]
    public void ColorField_Renders_WithColorAndText()
    {
        var cut = Render<ColorField>(p =>
            p.Add(c => c.Label, "Color")
             .Add(c => c.Value, "#ff0000"));

        cut.FindAll("input").Should().HaveCount(2); // color + text
        cut.Find("input[type='color']").GetAttribute("value").Should().Be("#ff0000");
    }

    [Fact]
    public void TimeField_Renders()
    {
        var cut = Render<TimeField>(p =>
            p.Add(c => c.Label, "Start Time")
             .Add(c => c.Value, new TimeSpan(14, 30, 0)));

        cut.Find("input[type='time']").GetAttribute("value").Should().Be("14:30");
    }

    [Fact]
    public void DateRangeField_Renders_TwoInputs()
    {
        var cut = Render<DateRangeField>(p =>
            p.Add(c => c.Label, "Date Range")
             .Add(c => c.StartDate, new DateTime(2026, 1, 1))
             .Add(c => c.EndDate, new DateTime(2026, 12, 31)));

        var inputs = cut.FindAll("input[type='date']");
        inputs.Should().HaveCount(2);
    }

    [Fact]
    public void CheckboxGroupField_Renders_Options()
    {
        var options = new List<FieldOption>
        {
            new() { Label = "Red", Value = "red" },
            new() { Label = "Blue", Value = "blue" },
            new() { Label = "Green", Value = "green" }
        };

        var cut = Render<CheckboxGroupField>(p =>
            p.Add(c => c.Label, "Colors")
             .Add(c => c.Values, new List<string> { "red" })
             .Add(c => c.Options, options));

        var checkboxes = cut.FindAll("input[type='checkbox']");
        checkboxes.Should().HaveCount(3);
    }

    [Fact]
    public void HiddenField_Renders_Hidden()
    {
        var cut = Render<HiddenField>(p =>
            p.Add(c => c.Name, "token")
             .Add(c => c.Value, "abc123"));

        var input = cut.Find("input[type='hidden']");
        input.GetAttribute("value").Should().Be("abc123");
        input.GetAttribute("name").Should().Be("token");
    }

    [Fact]
    public void MultiSelectField_Renders_WithTags()
    {
        var options = new List<FieldOption>
        {
            new() { Label = "A", Value = "a" },
            new() { Label = "B", Value = "b" },
            new() { Label = "C", Value = "c" }
        };

        var cut = Render<MultiSelectField>(p =>
            p.Add(c => c.Label, "Items")
             .Add(c => c.Values, new List<string> { "a", "b" })
             .Add(c => c.Options, options));

        cut.FindAll(".helix-field__multiselect-tag").Should().HaveCount(2);
    }
}

using Bunit;
using FluentAssertions;
using Arcadia.FormBuilder.Components.Fields;
using Xunit;

namespace Arcadia.Tests.Unit.FormBuilder;

public class TextFieldTests : BunitContext
{
    [Fact]
    public void Renders_WithLabel()
    {
        var cut = Render<TextField>(p =>
            p.Add(c => c.Label, "Full Name")
             .Add(c => c.Value, ""));

        cut.Find("label").TextContent.Should().Contain("Full Name");
    }

    [Fact]
    public void Renders_WithValue()
    {
        var cut = Render<TextField>(p =>
            p.Add(c => c.Label, "Name")
             .Add(c => c.Value, "John"));

        cut.Find("input").GetAttribute("value").Should().Be("John");
    }

    [Fact]
    public void Renders_RequiredIndicator()
    {
        var cut = Render<TextField>(p =>
            p.Add(c => c.Label, "Name")
             .Add(c => c.Required, true)
             .Add(c => c.Value, ""));

        cut.Find(".arcadia-field__required").TextContent.Should().Be("*");
        cut.Find("input").GetAttribute("aria-required").Should().Be("true");
    }

    [Fact]
    public void Renders_Placeholder()
    {
        var cut = Render<TextField>(p =>
            p.Add(c => c.Label, "Name")
             .Add(c => c.Placeholder, "Enter name")
             .Add(c => c.Value, ""));

        cut.Find("input").GetAttribute("placeholder").Should().Be("Enter name");
    }

    [Fact]
    public void Renders_HelperText()
    {
        var cut = Render<TextField>(p =>
            p.Add(c => c.Label, "Name")
             .Add(c => c.HelperText, "Enter your full name")
             .Add(c => c.Value, ""));

        cut.Find(".arcadia-field__helper").TextContent.Should().Be("Enter your full name");
    }

    [Fact]
    public void Renders_Errors()
    {
        var errors = new List<string> { "Name is required" } as IReadOnlyList<string>;

        var cut = Render<TextField>(p =>
            p.Add(c => c.Label, "Name")
             .Add(c => c.Errors, errors)
             .Add(c => c.Value, ""));

        cut.Find(".arcadia-field__error").TextContent.Should().Be("Name is required");
        cut.Find(".arcadia-field--error").Should().NotBeNull();
        cut.Find("input").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Fact]
    public void NoErrors_NoAriaInvalid()
    {
        var cut = Render<TextField>(p =>
            p.Add(c => c.Label, "Name")
             .Add(c => c.Value, "John"));

        cut.Find("input").GetAttribute("aria-invalid").Should().BeNull();
    }

    [Fact]
    public void Renders_Disabled()
    {
        var cut = Render<TextField>(p =>
            p.Add(c => c.Label, "Name")
             .Add(c => c.Disabled, true)
             .Add(c => c.Value, ""));

        cut.Find("input").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Label_LinkedToInput_ViaFor()
    {
        var cut = Render<TextField>(p =>
            p.Add(c => c.Label, "Name")
             .Add(c => c.Value, ""));

        var inputId = cut.Find("input").GetAttribute("id");
        var labelFor = cut.Find("label").GetAttribute("for");
        labelFor.Should().Be(inputId);
    }

    [Fact]
    public void ValueChanged_FiresOnInput()
    {
        string? newValue = null;

        var cut = Render<TextField>(p =>
            p.Add(c => c.Label, "Name")
             .Add(c => c.Value, "")
             .Add(c => c.ValueChanged, (string? v) => newValue = v));

        cut.Find("input").Input("Hello");

        newValue.Should().Be("Hello");
    }
}

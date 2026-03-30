using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class TextAreaTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaTextArea>();

        cut.Find(".arcadia-textarea").Should().NotBeNull();
    }

    [Fact]
    public void Label_RendersAboveTextarea()
    {
        var cut = Render<ArcadiaTextArea>(p => p
            .Add(c => c.Label, "Description"));

        cut.Find(".arcadia-textarea__label").TextContent.Should().Be("Description");
    }

    [Fact]
    public void Placeholder_RendersOnTextarea()
    {
        var cut = Render<ArcadiaTextArea>(p => p
            .Add(c => c.Placeholder, "Enter text..."));

        cut.Find(".arcadia-textarea__input")
            .GetAttribute("placeholder").Should().Be("Enter text...");
    }

    [Fact]
    public void Disabled_AddsDisabledClass()
    {
        var cut = Render<ArcadiaTextArea>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".arcadia-textarea").ClassList.Should().Contain("arcadia-textarea--disabled");
    }

    [Fact]
    public void Disabled_TextareaHasDisabledAttribute()
    {
        var cut = Render<ArcadiaTextArea>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".arcadia-textarea__input").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void ErrorText_RendersErrorMessage()
    {
        var cut = Render<ArcadiaTextArea>(p => p
            .Add(c => c.ErrorText, "Field is required"));

        cut.Find(".arcadia-textarea__error").TextContent.Should().Be("Field is required");
    }

    [Fact]
    public void ErrorText_AddsErrorClass()
    {
        var cut = Render<ArcadiaTextArea>(p => p
            .Add(c => c.ErrorText, "Error"));

        cut.Find(".arcadia-textarea").ClassList.Should().Contain("arcadia-textarea--error");
    }

    [Fact]
    public void Aria_InvalidWhenError()
    {
        var cut = Render<ArcadiaTextArea>(p => p
            .Add(c => c.ErrorText, "Invalid"));

        cut.Find(".arcadia-textarea__input")
            .GetAttribute("aria-invalid").Should().Be("true");
    }

    [Fact]
    public void HelperText_RendersHelperMessage()
    {
        var cut = Render<ArcadiaTextArea>(p => p
            .Add(c => c.HelperText, "Max 500 chars"));

        cut.Find(".arcadia-textarea__helper").TextContent.Should().Be("Max 500 chars");
    }
}

using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Arcadia.Tests.Unit.UI;

public class TagInputTests : ChartTestBase
{
    [Fact]
    public void Default_RendersWithCorrectCssClass()
    {
        var cut = Render<ArcadiaTagInput>();

        cut.Find(".arcadia-tag-input").Should().NotBeNull();
    }

    [Fact]
    public void Tags_RendersTagElements()
    {
        var cut = Render<ArcadiaTagInput>(p => p
            .Add(c => c.Tags, new List<string> { "C#", "Blazor", "Razor" }));

        cut.FindAll(".arcadia-tag-input__tag").Should().HaveCount(3);
    }

    [Fact]
    public void Tags_DisplayTagText()
    {
        var cut = Render<ArcadiaTagInput>(p => p
            .Add(c => c.Tags, new List<string> { "Blazor" }));

        cut.Find(".arcadia-tag-input__tag-text").TextContent.Should().Be("Blazor");
    }

    [Fact]
    public void RemoveButton_HasAriaLabel()
    {
        var cut = Render<ArcadiaTagInput>(p => p
            .Add(c => c.Tags, new List<string> { "Tag1" }));

        cut.Find(".arcadia-tag-input__tag-remove")
            .GetAttribute("aria-label").Should().Be("Remove Tag1");
    }

    [Fact]
    public void Disabled_AddsDisabledClass()
    {
        var cut = Render<ArcadiaTagInput>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".arcadia-tag-input").ClassList.Should().Contain("arcadia-tag-input--disabled");
    }

    [Fact]
    public void Disabled_InputHasDisabledAttribute()
    {
        var cut = Render<ArcadiaTagInput>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".arcadia-tag-input__input").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Disabled_NoRemoveButtons()
    {
        var cut = Render<ArcadiaTagInput>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.Tags, new List<string> { "Tag1" }));

        cut.FindAll(".arcadia-tag-input__tag-remove").Should().BeEmpty();
    }

    [Fact]
    public void Placeholder_RendersInInput()
    {
        var cut = Render<ArcadiaTagInput>(p => p
            .Add(c => c.Placeholder, "Add skill..."));

        cut.Find(".arcadia-tag-input__input")
            .GetAttribute("placeholder").Should().Be("Add skill...");
    }
}

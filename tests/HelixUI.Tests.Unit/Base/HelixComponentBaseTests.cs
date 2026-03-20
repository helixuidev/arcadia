using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;

namespace HelixUI.Tests.Unit.Base;

/// <summary>
/// A minimal concrete component for testing HelixComponentBase.
/// </summary>
public class TestComponent : HelixUI.Core.Base.HelixComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", ElementId);
        builder.AddAttribute(2, "class", Class);
        builder.AddAttribute(3, "style", Style);
        builder.AddMultipleAttributes(4, AdditionalAttributes!);
        builder.AddContent(5, ChildContent);
        builder.CloseElement();
    }
}

public class HelixComponentBaseTests : BunitContext
{
    [Fact]
    public void Renders_WithClassParameter()
    {
        var cut = Render<TestComponent>(parameters =>
            parameters.Add(p => p.Class, "custom-class"));

        cut.Find("div").GetAttribute("class").Should().Be("custom-class");
    }

    [Fact]
    public void Renders_WithStyleParameter()
    {
        var cut = Render<TestComponent>(parameters =>
            parameters.Add(p => p.Style, "color: red;"));

        cut.Find("div").GetAttribute("style").Should().Be("color: red;");
    }

    [Fact]
    public void Renders_WithAdditionalAttributes()
    {
        var cut = Render<TestComponent>(parameters =>
            parameters.AddUnmatched("data-testid", "my-component")
                      .AddUnmatched("aria-label", "test label"));

        var div = cut.Find("div");
        div.GetAttribute("data-testid").Should().Be("my-component");
        div.GetAttribute("aria-label").Should().Be("test label");
    }

    [Fact]
    public void ElementId_IsGenerated()
    {
        var cut = Render<TestComponent>();

        var id = cut.Find("div").GetAttribute("id");
        id.Should().NotBeNullOrEmpty();
        id.Should().StartWith("helix-");
    }

    [Fact]
    public void ElementId_IsStableAcrossRenders()
    {
        var cut = Render<TestComponent>();

        var id1 = cut.Find("div").GetAttribute("id");
        cut.Render();
        var id2 = cut.Find("div").GetAttribute("id");

        id1.Should().Be(id2);
    }

    [Fact]
    public void Renders_WithChildContent()
    {
        var cut = Render<TestComponent>(parameters =>
            parameters.AddChildContent("<span>Hello</span>"));

        cut.Find("span").TextContent.Should().Be("Hello");
    }
}

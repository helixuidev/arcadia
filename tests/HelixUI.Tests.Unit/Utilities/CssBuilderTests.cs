using FluentAssertions;
using HelixUI.Core.Utilities;
using Xunit;

namespace HelixUI.Tests.Unit.Utilities;

public class CssBuilderTests
{
    [Fact]
    public void Default_WithNoClass_ReturnsNull()
    {
        var result = CssBuilder.Default().Build();

        result.Should().BeNull();
    }

    [Fact]
    public void Default_WithInitialClass_ReturnsClass()
    {
        var result = CssBuilder.Default("btn").Build();

        result.Should().Be("btn");
    }

    [Fact]
    public void AddClass_AppendsWithSpace()
    {
        var result = CssBuilder.Default("btn")
            .AddClass("btn-primary")
            .Build();

        result.Should().Be("btn btn-primary");
    }

    [Fact]
    public void AddClass_SkipsNullAndEmpty()
    {
        var result = CssBuilder.Default("btn")
            .AddClass(null)
            .AddClass("")
            .AddClass("   ")
            .AddClass("active")
            .Build();

        result.Should().Be("btn active");
    }

    [Fact]
    public void AddClass_WithCondition_True_AddsClass()
    {
        var result = CssBuilder.Default("btn")
            .AddClass("disabled", true)
            .Build();

        result.Should().Be("btn disabled");
    }

    [Fact]
    public void AddClass_WithCondition_False_SkipsClass()
    {
        var result = CssBuilder.Default("btn")
            .AddClass("disabled", false)
            .Build();

        result.Should().Be("btn");
    }

    [Fact]
    public void AddClass_WithFuncCondition_EvaluatesLazily()
    {
        var isActive = true;

        var result = CssBuilder.Default("btn")
            .AddClass("active", () => isActive)
            .Build();

        result.Should().Be("btn active");
    }

    [Fact]
    public void AddClass_WithFactory_WhenTrue_InvokesFactory()
    {
        var result = CssBuilder.Default("btn")
            .AddClass(() => "computed-class", true)
            .Build();

        result.Should().Be("btn computed-class");
    }

    [Fact]
    public void AddClass_WithFactory_WhenFalse_SkipsFactory()
    {
        var invoked = false;

        var result = CssBuilder.Default("btn")
            .AddClass(() => { invoked = true; return "computed"; }, false)
            .Build();

        result.Should().Be("btn");
        invoked.Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsBuildResult()
    {
        var builder = CssBuilder.Default("btn").AddClass("primary");

        builder.ToString().Should().Be("btn primary");
    }

    [Fact]
    public void ToString_WhenEmpty_ReturnsEmptyString()
    {
        var builder = CssBuilder.Default();

        builder.ToString().Should().BeEmpty();
    }

    [Fact]
    public void MultipleClasses_BuildsCorrectly()
    {
        var result = CssBuilder.Default("helix-input")
            .AddClass("helix-input--disabled", true)
            .AddClass("helix-input--readonly", false)
            .AddClass("helix-input--error", true)
            .AddClass("custom-class")
            .Build();

        result.Should().Be("helix-input helix-input--disabled helix-input--error custom-class");
    }
}

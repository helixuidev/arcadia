using FluentAssertions;
using Arcadia.Core.Utilities;
using Xunit;

namespace Arcadia.Tests.Unit.Utilities;

public class StyleBuilderTests
{
    [Fact]
    public void Default_WithNoStyle_ReturnsNull()
    {
        var result = StyleBuilder.Default().Build();

        result.Should().BeNull();
    }

    [Fact]
    public void Default_WithInitialStyle_ReturnsStyle()
    {
        var result = StyleBuilder.Default("color: red;").Build();

        result.Should().Be("color: red;");
    }

    [Fact]
    public void Default_WithInitialStyle_AppendsSemicolonIfMissing()
    {
        var result = StyleBuilder.Default("color: red").Build();

        result.Should().Be("color: red;");
    }

    [Fact]
    public void AddStyle_AppendsPropertyValue()
    {
        var result = StyleBuilder.Default()
            .AddStyle("color", "red")
            .Build();

        result.Should().Be("color: red;");
    }

    [Fact]
    public void AddStyle_MultipleProperties_SeparatesWithSpace()
    {
        var result = StyleBuilder.Default()
            .AddStyle("color", "red")
            .AddStyle("font-size", "14px")
            .Build();

        result.Should().Be("color: red; font-size: 14px;");
    }

    [Fact]
    public void AddStyle_SkipsNullValue()
    {
        var result = StyleBuilder.Default()
            .AddStyle("color", "red")
            .AddStyle("font-size", null)
            .AddStyle("margin", "0")
            .Build();

        result.Should().Be("color: red; margin: 0;");
    }

    [Fact]
    public void AddStyle_WithCondition_True_AddsStyle()
    {
        var result = StyleBuilder.Default()
            .AddStyle("display", "none", true)
            .Build();

        result.Should().Be("display: none;");
    }

    [Fact]
    public void AddStyle_WithCondition_False_SkipsStyle()
    {
        var result = StyleBuilder.Default()
            .AddStyle("display", "none", false)
            .Build();

        result.Should().BeNull();
    }

    [Fact]
    public void AddStyle_WithFuncCondition_EvaluatesLazily()
    {
        var isHidden = true;

        var result = StyleBuilder.Default()
            .AddStyle("display", "none", () => isHidden)
            .Build();

        result.Should().Be("display: none;");
    }

    [Fact]
    public void AddRaw_AppendsRawStyle()
    {
        var result = StyleBuilder.Default()
            .AddStyle("color", "red")
            .AddRaw("font-size: 14px; margin: 0")
            .Build();

        result.Should().Be("color: red; font-size: 14px; margin: 0;");
    }

    [Fact]
    public void AddRaw_SkipsNullAndEmpty()
    {
        var result = StyleBuilder.Default()
            .AddStyle("color", "red")
            .AddRaw(null)
            .AddRaw("")
            .Build();

        result.Should().Be("color: red;");
    }

    [Fact]
    public void ToString_ReturnsBuildResult()
    {
        var builder = StyleBuilder.Default().AddStyle("color", "red");

        builder.ToString().Should().Be("color: red;");
    }

    [Fact]
    public void ToString_WhenEmpty_ReturnsEmptyString()
    {
        var builder = StyleBuilder.Default();

        builder.ToString().Should().BeEmpty();
    }
}

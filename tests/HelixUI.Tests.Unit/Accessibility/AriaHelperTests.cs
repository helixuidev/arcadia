using FluentAssertions;
using HelixUI.Core.Accessibility;
using Xunit;

namespace HelixUI.Tests.Unit.Accessibility;

public class AriaHelperTests
{
    [Fact]
    public void Join_WithMultipleIds_ReturnsSpaceSeparated()
    {
        var result = AriaHelper.Join("id1", "id2", "id3");

        result.Should().Be("id1 id2 id3");
    }

    [Fact]
    public void Join_FiltersNullAndEmpty()
    {
        var result = AriaHelper.Join("id1", null, "", "  ", "id2");

        result.Should().Be("id1 id2");
    }

    [Fact]
    public void Join_AllEmpty_ReturnsNull()
    {
        var result = AriaHelper.Join(null, "", "  ");

        result.Should().BeNull();
    }

    [Fact]
    public void Join_NoArgs_ReturnsNull()
    {
        var result = AriaHelper.Join();

        result.Should().BeNull();
    }

    [Fact]
    public void Join_SingleId_ReturnsThatId()
    {
        var result = AriaHelper.Join("only-one");

        result.Should().Be("only-one");
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Expanded_ReturnsCorrectValue(bool expanded, string expected)
    {
        AriaHelper.Expanded(expanded).Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Selected_ReturnsCorrectValue(bool selected, string expected)
    {
        AriaHelper.Selected(selected).Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    [InlineData(null, "mixed")]
    public void Checked_ReturnsCorrectValue(bool? isChecked, string expected)
    {
        AriaHelper.Checked(isChecked).Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Hidden_ReturnsCorrectValue(bool hidden, string expected)
    {
        AriaHelper.Hidden(hidden).Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Disabled_ReturnsCorrectValue(bool disabled, string expected)
    {
        AriaHelper.Disabled(disabled).Should().Be(expected);
    }

    [Fact]
    public void Current_WithValue_ReturnsValue()
    {
        AriaHelper.Current("page").Should().Be("page");
    }

    [Fact]
    public void Current_WithNull_ReturnsNull()
    {
        AriaHelper.Current(null).Should().BeNull();
    }

    [Fact]
    public void Current_WithEmpty_ReturnsNull()
    {
        AriaHelper.Current("").Should().BeNull();
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, null)]
    public void TrueOrNull_ReturnsCorrectValue(bool value, string? expected)
    {
        AriaHelper.TrueOrNull(value).Should().Be(expected);
    }
}

using FluentAssertions;
using HelixUI.Core.Accessibility;
using Xunit;

namespace HelixUI.Tests.Unit.Accessibility;

public class KeyboardNavigationTests
{
    [Fact]
    public void GetNextIndex_Next_AdvancesForward()
    {
        var result = KeyboardNavigation.GetNextIndex(2, 5, NavigationDirection.Next);

        result.Should().Be(3);
    }

    [Fact]
    public void GetNextIndex_Next_WrapsToBeginning()
    {
        var result = KeyboardNavigation.GetNextIndex(4, 5, NavigationDirection.Next);

        result.Should().Be(0);
    }

    [Fact]
    public void GetNextIndex_Previous_MovesBackward()
    {
        var result = KeyboardNavigation.GetNextIndex(2, 5, NavigationDirection.Previous);

        result.Should().Be(1);
    }

    [Fact]
    public void GetNextIndex_Previous_WrapsToEnd()
    {
        var result = KeyboardNavigation.GetNextIndex(0, 5, NavigationDirection.Previous);

        result.Should().Be(4);
    }

    [Fact]
    public void GetNextIndex_First_ReturnsZero()
    {
        var result = KeyboardNavigation.GetNextIndex(3, 5, NavigationDirection.First);

        result.Should().Be(0);
    }

    [Fact]
    public void GetNextIndex_Last_ReturnsLastIndex()
    {
        var result = KeyboardNavigation.GetNextIndex(0, 5, NavigationDirection.Last);

        result.Should().Be(4);
    }

    [Fact]
    public void GetNextIndex_EmptyList_ReturnsZero()
    {
        var result = KeyboardNavigation.GetNextIndex(0, 0, NavigationDirection.Next);

        result.Should().Be(0);
    }

    [Fact]
    public void GetNextEnabledIndex_SkipsDisabledItems()
    {
        // Items: [enabled, disabled, disabled, enabled, enabled]
        bool IsDisabled(int i) => i == 1 || i == 2;

        var result = KeyboardNavigation.GetNextEnabledIndex(0, 5, NavigationDirection.Next, IsDisabled);

        result.Should().Be(3);
    }

    [Fact]
    public void GetNextEnabledIndex_AllDisabled_ReturnsCurrent()
    {
        var result = KeyboardNavigation.GetNextEnabledIndex(2, 5, NavigationDirection.Next, _ => true);

        result.Should().Be(2);
    }

    [Fact]
    public void GetNextEnabledIndex_WrapsAroundToFindEnabled()
    {
        // Items: [disabled, disabled, disabled, enabled, disabled]
        bool IsDisabled(int i) => i != 3;

        var result = KeyboardNavigation.GetNextEnabledIndex(4, 5, NavigationDirection.Next, IsDisabled);

        result.Should().Be(3);
    }

    [Fact]
    public void GetNextEnabledIndex_Previous_SkipsDisabled()
    {
        // Items: [enabled, disabled, disabled, enabled, enabled]
        bool IsDisabled(int i) => i == 1 || i == 2;

        var result = KeyboardNavigation.GetNextEnabledIndex(3, 5, NavigationDirection.Previous, IsDisabled);

        result.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, -1)]
    [InlineData(2, 0, -1)]
    public void GetTabIndex_ActiveItemGetsZero_OthersGetNegativeOne(int itemIndex, int activeIndex, int expected)
    {
        var result = KeyboardNavigation.GetTabIndex(itemIndex, activeIndex);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetTabIndex_ActiveItem_ReturnsZero()
    {
        var result = KeyboardNavigation.GetTabIndex(2, 2);

        result.Should().Be(0);
    }

    [Fact]
    public void GetTabIndex_InactiveItem_ReturnsNegativeOne()
    {
        var result = KeyboardNavigation.GetTabIndex(1, 2);

        result.Should().Be(-1);
    }

    [Theory]
    [InlineData("ArrowDown", NavigationDirection.Next)]
    [InlineData("ArrowUp", NavigationDirection.Previous)]
    [InlineData("Home", NavigationDirection.First)]
    [InlineData("End", NavigationDirection.Last)]
    public void MapVerticalKey_MapsCorrectly(string key, NavigationDirection expected)
    {
        KeyboardNavigation.MapVerticalKey(key).Should().Be(expected);
    }

    [Theory]
    [InlineData("ArrowLeft")]
    [InlineData("ArrowRight")]
    [InlineData("Tab")]
    [InlineData("Enter")]
    [InlineData(null)]
    public void MapVerticalKey_UnrecognizedKey_ReturnsNull(string? key)
    {
        KeyboardNavigation.MapVerticalKey(key).Should().BeNull();
    }

    [Theory]
    [InlineData("ArrowRight", NavigationDirection.Next)]
    [InlineData("ArrowLeft", NavigationDirection.Previous)]
    [InlineData("Home", NavigationDirection.First)]
    [InlineData("End", NavigationDirection.Last)]
    public void MapHorizontalKey_MapsCorrectly(string key, NavigationDirection expected)
    {
        KeyboardNavigation.MapHorizontalKey(key).Should().Be(expected);
    }

    [Theory]
    [InlineData("ArrowUp")]
    [InlineData("ArrowDown")]
    [InlineData("Tab")]
    [InlineData(null)]
    public void MapHorizontalKey_UnrecognizedKey_ReturnsNull(string? key)
    {
        KeyboardNavigation.MapHorizontalKey(key).Should().BeNull();
    }
}

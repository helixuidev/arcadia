namespace HelixUI.Core.Accessibility;

/// <summary>
/// Provides utilities for common keyboard navigation patterns used in
/// accessible composite widgets (menus, listboxes, tab lists, toolbars).
/// </summary>
public static class KeyboardNavigation
{
    /// <summary>
    /// Computes the next index using roving tabindex navigation.
    /// Wraps around at the boundaries.
    /// </summary>
    /// <param name="currentIndex">The currently focused item index.</param>
    /// <param name="itemCount">The total number of navigable items.</param>
    /// <param name="direction">The navigation direction.</param>
    /// <returns>The new focused index after navigation.</returns>
    public static int GetNextIndex(int currentIndex, int itemCount, NavigationDirection direction)
    {
        if (itemCount <= 0)
            return 0;

        return direction switch
        {
            NavigationDirection.Next => (currentIndex + 1) % itemCount,
            NavigationDirection.Previous => (currentIndex - 1 + itemCount) % itemCount,
            NavigationDirection.First => 0,
            NavigationDirection.Last => itemCount - 1,
            _ => currentIndex
        };
    }

    /// <summary>
    /// Computes the next index, skipping disabled items. Wraps around at the boundaries.
    /// Returns the current index if all items are disabled.
    /// </summary>
    /// <param name="currentIndex">The currently focused item index.</param>
    /// <param name="itemCount">The total number of navigable items.</param>
    /// <param name="direction">The navigation direction.</param>
    /// <param name="isDisabled">A function that returns true if the item at the given index is disabled.</param>
    /// <returns>The new focused index, or the current index if no enabled item is found.</returns>
    public static int GetNextEnabledIndex(int currentIndex, int itemCount, NavigationDirection direction, Func<int, bool> isDisabled)
    {
        if (itemCount <= 0)
            return 0;

        var nextIndex = GetNextIndex(currentIndex, itemCount, direction);
        var visited = 0;

        while (isDisabled(nextIndex) && visited < itemCount)
        {
            var stepDirection = direction is NavigationDirection.Previous or NavigationDirection.Last
                ? NavigationDirection.Previous
                : NavigationDirection.Next;

            nextIndex = GetNextIndex(nextIndex, itemCount, stepDirection);
            visited++;
        }

        return visited >= itemCount ? currentIndex : nextIndex;
    }

    /// <summary>
    /// Returns the <c>tabindex</c> value for roving tabindex pattern.
    /// The active item gets <c>0</c>, all others get <c>-1</c>.
    /// </summary>
    /// <param name="itemIndex">The index of the item.</param>
    /// <param name="activeIndex">The index of the currently active (focused) item.</param>
    public static int GetTabIndex(int itemIndex, int activeIndex) =>
        itemIndex == activeIndex ? 0 : -1;

    /// <summary>
    /// Maps a keyboard key string to a navigation direction for vertical layouts
    /// (e.g., menus, listboxes). Returns null for unrecognized keys.
    /// </summary>
    /// <param name="key">The keyboard key value (e.g., "ArrowDown", "Home").</param>
    public static NavigationDirection? MapVerticalKey(string? key) => key switch
    {
        "ArrowDown" => NavigationDirection.Next,
        "ArrowUp" => NavigationDirection.Previous,
        "Home" => NavigationDirection.First,
        "End" => NavigationDirection.Last,
        _ => null
    };

    /// <summary>
    /// Maps a keyboard key string to a navigation direction for horizontal layouts
    /// (e.g., tab lists, toolbars). Returns null for unrecognized keys.
    /// </summary>
    /// <param name="key">The keyboard key value (e.g., "ArrowRight", "Home").</param>
    public static NavigationDirection? MapHorizontalKey(string? key) => key switch
    {
        "ArrowRight" => NavigationDirection.Next,
        "ArrowLeft" => NavigationDirection.Previous,
        "Home" => NavigationDirection.First,
        "End" => NavigationDirection.Last,
        _ => null
    };
}

/// <summary>
/// Represents the direction of keyboard navigation within a composite widget.
/// </summary>
public enum NavigationDirection
{
    /// <summary>Move to the next item (down or right).</summary>
    Next,

    /// <summary>Move to the previous item (up or left).</summary>
    Previous,

    /// <summary>Move to the first item.</summary>
    First,

    /// <summary>Move to the last item.</summary>
    Last
}

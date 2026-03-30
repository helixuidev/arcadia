namespace Arcadia.DashboardKit.Models;

/// <summary>
/// Provides contextual information about a <see cref="Components.ArcadiaDragGridItem"/>
/// to content rendered via <see cref="Components.ArcadiaDragGridItem.ItemTemplate"/>.
/// This enables panel content to adapt its rendering based on the current size and state.
/// </summary>
/// <param name="ColSpan">The current number of columns the item spans.</param>
/// <param name="RowSpan">The current number of rows the item spans.</param>
/// <param name="IsEditing">Whether the parent grid is currently in edit mode.</param>
/// <param name="IsLocked">Whether this item is locked and cannot be moved or resized.</param>
public record DragGridItemContext(int ColSpan, int RowSpan, bool IsEditing, bool IsLocked);

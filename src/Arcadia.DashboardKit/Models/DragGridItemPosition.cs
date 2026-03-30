namespace Arcadia.DashboardKit.Models;

/// <summary>
/// Represents the position and size of a single item within a <see cref="DragGridLayout"/>.
/// </summary>
public class DragGridItemPosition
{
    /// <summary>
    /// Gets or sets the unique identifier for this item.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Gets or sets the display order of this item in the grid.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the number of columns this item spans.
    /// </summary>
    public int ColSpan { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of rows this item spans.
    /// </summary>
    public int RowSpan { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether this item is locked and cannot be dragged or resized.
    /// </summary>
    public bool Locked { get; set; }
}

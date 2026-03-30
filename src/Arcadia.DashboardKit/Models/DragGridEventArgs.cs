namespace Arcadia.DashboardKit.Models;

/// <summary>
/// Event arguments raised when a grid item is reordered via drag-and-drop.
/// </summary>
public class DragGridReorderEventArgs
{
    /// <summary>
    /// Gets or sets the unique identifier of the item that was moved.
    /// </summary>
    public string ItemId { get; set; } = "";

    /// <summary>
    /// Gets or sets the item's previous index in the grid.
    /// </summary>
    public int OldIndex { get; set; }

    /// <summary>
    /// Gets or sets the item's new index in the grid.
    /// </summary>
    public int NewIndex { get; set; }
}

/// <summary>
/// Event arguments raised when a grid item is resized.
/// </summary>
public class DragGridResizeEventArgs
{
    /// <summary>
    /// Gets or sets the unique identifier of the item that was resized.
    /// </summary>
    public string ItemId { get; set; } = "";

    /// <summary>
    /// Gets or sets the previous column span.
    /// </summary>
    public int OldColSpan { get; set; }

    /// <summary>
    /// Gets or sets the previous row span.
    /// </summary>
    public int OldRowSpan { get; set; }

    /// <summary>
    /// Gets or sets the new column span.
    /// </summary>
    public int NewColSpan { get; set; }

    /// <summary>
    /// Gets or sets the new row span.
    /// </summary>
    public int NewRowSpan { get; set; }
}

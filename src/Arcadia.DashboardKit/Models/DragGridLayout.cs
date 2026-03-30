namespace Arcadia.DashboardKit.Models;

/// <summary>
/// Represents the complete layout state of an <see cref="Components.ArcadiaDragGrid"/>,
/// including the position and size of every item.
/// </summary>
public class DragGridLayout
{
    /// <summary>
    /// Gets or sets the collection of item positions in the grid.
    /// </summary>
    public List<DragGridItemPosition> Items { get; set; } = new();
}

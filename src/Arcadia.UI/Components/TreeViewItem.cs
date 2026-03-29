namespace Arcadia.UI.Components;

/// <summary>
/// Represents a single item in an <see cref="ArcadiaTreeView"/> hierarchy.
/// </summary>
public class TreeViewItem
{
    /// <summary>
    /// Gets or sets the unique identifier for this tree item.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display text for this tree item.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional icon CSS class or name.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the child items of this tree node.
    /// </summary>
    public List<TreeViewItem> Children { get; set; } = new();

    /// <summary>
    /// Gets or sets whether this node is expanded.
    /// </summary>
    public bool Expanded { get; set; }

    /// <summary>
    /// Gets or sets whether this node is selected.
    /// </summary>
    public bool Selected { get; set; }
}

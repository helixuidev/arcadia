using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Arcadia.Core.Base;
using Arcadia.Core.Utilities;
using Arcadia.DashboardKit.Models;

namespace Arcadia.DashboardKit.Components;

/// <summary>
/// Represents a single item within an <see cref="ArcadiaDragGrid"/>.
/// Supports configurable column/row spans, locking, custom header content,
/// keyboard drag reordering, floating mode, and adaptive content via <see cref="ItemTemplate"/>.
/// </summary>
public partial class ArcadiaDragGridItem : ArcadiaComponentBase, IDisposable
{
    /// <summary>
    /// Gets the parent <see cref="ArcadiaDragGrid"/> via cascading parameter.
    /// </summary>
    [CascadingParameter]
    public ArcadiaDragGrid? Parent { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this grid item.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the child content rendered inside the item.
    /// If <see cref="ItemTemplate"/> is also set, ItemTemplate takes precedence.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets an alternative content template that receives a <see cref="DragGridItemContext"/>
    /// with the current size and state of this panel. When set, this takes precedence over <see cref="ChildContent"/>.
    /// This enables panel content to adapt its rendering based on the current column/row span.
    /// </summary>
    [Parameter]
    public RenderFragment<DragGridItemContext>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the number of columns this item spans. Default is 1.
    /// </summary>
    [Parameter]
    public int ColSpan { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of rows this item spans. Default is 1.
    /// </summary>
    [Parameter]
    public int RowSpan { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether this item is locked (cannot be dragged or resized).
    /// </summary>
    [Parameter]
    public bool Locked { get; set; }

    /// <summary>
    /// Gets or sets the minimum column span allowed during resize.
    /// </summary>
    [Parameter]
    public int MinColSpan { get; set; } = 1;

    /// <summary>
    /// Gets or sets the minimum row span allowed during resize.
    /// </summary>
    [Parameter]
    public int MinRowSpan { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum column span allowed during resize.
    /// </summary>
    [Parameter]
    public int MaxColSpan { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets the maximum row span allowed during resize.
    /// </summary>
    [Parameter]
    public int MaxRowSpan { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets optional header content displayed in the drag handle area.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// Gets or sets whether this item uses absolute positioning instead of CSS Grid placement.
    /// Floating items can overlap other items and are excluded from the occupancy grid.
    /// They can be dragged freely anywhere within the grid container.
    /// </summary>
    [Parameter]
    public bool Floating { get; set; }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Parent?.RegisterItem(this);
    }

    // Runtime overrides — set by resize, take precedence over [Parameter] values
    internal int? RuntimeColSpan { get; set; }
    internal int? RuntimeRowSpan { get; set; }

    internal int EffectiveColSpan => RuntimeColSpan ?? ColSpan;
    internal int EffectiveRowSpan => RuntimeRowSpan ?? RowSpan;

    internal void SetSpans(int colSpan, int rowSpan)
    {
        RuntimeColSpan = Math.Clamp(colSpan, MinColSpan, MaxColSpan);
        RuntimeRowSpan = Math.Clamp(rowSpan, MinRowSpan, MaxRowSpan);
    }

    /// <summary>
    /// Gets the current context for use with <see cref="ItemTemplate"/>.
    /// </summary>
    private DragGridItemContext ItemContext => new(
        EffectiveColSpan,
        EffectiveRowSpan,
        Parent?.EditMode ?? false,
        Locked
    );

    /// <summary>
    /// Whether the keydown event should call preventDefault (for arrow keys and space during drag).
    /// </summary>
    private bool ShouldPreventDefault => Parent?.IsKeyboardDragging(Id) == true;

    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (Parent is not null && Id is not null)
        {
            await Parent.HandleItemKeyDown(Id, e);
        }
    }

    private async Task HandleRemove()
    {
        if (Parent is not null && Id is not null)
        {
            await Parent.HandleRemovePanel(Id);
        }
    }

    private string ItemStyle
    {
        get
        {
            if (Floating)
            {
                // Floating items use absolute positioning
                var s = "position:absolute;z-index:50;";
                var cs = EffectiveColSpan;
                var rs = EffectiveRowSpan;
                // Use grid-aware sizing but absolute positioning
                s += $"width:calc({cs} * (100% / {Parent?.Columns ?? 4}) - {(Parent?.Gap ?? 16) * (cs - 1) / cs}px);";
                s += $"height:calc({rs} * {Parent?.RowHeight ?? 120}px + {(rs - 1) * (Parent?.Gap ?? 16)}px);";

                if (!string.IsNullOrEmpty(Style))
                {
                    s += Style;
                }

                return s;
            }

            var col = Parent?.GetGridCol(Id) ?? 0;
            var row = Parent?.GetGridRow(Id) ?? 0;

            string result;
            var colSpan = EffectiveColSpan;
            var rowSpan = EffectiveRowSpan;

            if (col > 0 && row > 0)
            {
                result = $"grid-column:{col} / span {colSpan};grid-row:{row} / span {rowSpan};";
            }
            else
            {
                result = $"grid-column:span {colSpan};grid-row:span {rowSpan};";
            }

            if (!string.IsNullOrEmpty(Style))
            {
                result += Style;
            }

            return result;
        }
    }

    private string? CssClass => CssBuilder.Default("arcadia-draggrid__item")
        .AddClass("arcadia-draggrid__item--locked", Locked)
        .AddClass("arcadia-draggrid__item--keyboard-dragging", Parent?.IsKeyboardDragging(Id) == true)
        .AddClass("arcadia-draggrid__item--floating", Floating)
        .AddClass(Class)
        .Build();

    /// <inheritdoc />
    public void Dispose()
    {
        Parent?.UnregisterItem(this);
    }
}

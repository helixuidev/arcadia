using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.DashboardKit.Components;

namespace Arcadia.Tests.Unit.DashboardKit;

public class DragGridItemTests : ChartTestBase
{
    /// <summary>
    /// Helper: renders a DragGridItem inside a parent DragGrid so the CascadingValue is available.
    /// </summary>
    private IRenderedComponent<ArcadiaDragGrid> RenderItemInGrid(
        Action<ComponentParameterCollectionBuilder<ArcadiaDragGridItem>> itemConfig,
        Action<ComponentParameterCollectionBuilder<ArcadiaDragGrid>>? gridConfig = null)
    {
        return Render<ArcadiaDragGrid>(p =>
        {
            p.Add(g => g.Columns, 4);
            p.Add(g => g.EditMode, true);
            p.Add(g => g.AllowResize, true);
            gridConfig?.Invoke(p);
            p.AddChildContent<ArcadiaDragGridItem>(itemConfig);
        });
    }

    [Fact]
    public void DragGridItem_Renders_WithItemClass()
    {
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "item-1")
            .AddChildContent("<p>Content</p>"));

        cut.Find(".arcadia-draggrid__item").Should().NotBeNull();
    }

    [Fact]
    public void DragGridItem_Id_SetsDataAttribute()
    {
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "panel-42")
            .AddChildContent("<p>Content</p>"));

        cut.Find("[data-draggrid-id='panel-42']").Should().NotBeNull();
    }

    [Fact]
    public void DragGridItem_ColSpan_SetsGridColumn()
    {
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "wide")
            .Add(c => c.ColSpan, 2)
            .AddChildContent("<p>Content</p>"));

        // After first render, grid positions are computed on OnAfterRenderAsync.
        // In bUnit, the style should contain span 2 at minimum.
        var style = cut.Find(".arcadia-draggrid__item").GetAttribute("style");
        style.Should().Contain("span 2");
    }

    [Fact]
    public void DragGridItem_RowSpan_SetsGridRow()
    {
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "tall")
            .Add(c => c.RowSpan, 3)
            .AddChildContent("<p>Content</p>"));

        var style = cut.Find(".arcadia-draggrid__item").GetAttribute("style");
        style.Should().Contain("span 3");
    }

    [Fact]
    public void DragGridItem_Locked_AddsLockedClass()
    {
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "locked-1")
            .Add(c => c.Locked, true)
            .AddChildContent("<p>Content</p>"));

        cut.Find(".arcadia-draggrid__item").ClassList
            .Should().Contain("arcadia-draggrid__item--locked");
    }

    [Fact]
    public void DragGridItem_Locked_SetsDataAttribute()
    {
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "locked-2")
            .Add(c => c.Locked, true)
            .AddChildContent("<p>Content</p>"));

        cut.Find("[data-draggrid-locked='true']").Should().NotBeNull();
    }

    [Fact]
    public void DragGridItem_HeaderContent_RendersCustomHeader()
    {
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "custom-header")
            .Add(c => c.HeaderContent, "<span class='my-header'>Custom</span>")
            .AddChildContent("<p>Content</p>"));

        cut.Find(".arcadia-draggrid__handle .my-header").TextContent.Should().Be("Custom");
    }

    [Fact]
    public void DragGridItem_DefaultHeader_RendersGripIcon()
    {
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "default-header")
            .AddChildContent("<p>Content</p>"));

        cut.Find(".arcadia-draggrid__grip-icon").Should().NotBeNull();
    }

    [Fact]
    public void DragGridItem_ResizeHandle_RendersWhenEditMode()
    {
        // The helper already sets EditMode=true and AllowResize=true on the parent grid.
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "resizable")
            .Add(c => c.Locked, false)
            .AddChildContent("<p>Content</p>"));

        cut.Find(".arcadia-draggrid__resize-handle").Should().NotBeNull();
    }

    [Fact]
    public void DragGridItem_Floating_AddsFloatingClass()
    {
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "float-1")
            .Add(c => c.Floating, true)
            .AddChildContent("<p>Content</p>"));

        cut.Find(".arcadia-draggrid__item").ClassList
            .Should().Contain("arcadia-draggrid__item--floating");
    }

    [Fact]
    public void DragGridItem_Accessibility_HasTabindex()
    {
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "a11y-tab")
            .AddChildContent("<p>Content</p>"));

        var el = cut.Find(".arcadia-draggrid__item");
        el.GetAttribute("tabindex").Should().Be("0");
    }

    [Fact]
    public void DragGridItem_Accessibility_HasRole()
    {
        var cut = RenderItemInGrid(p => p
            .Add(c => c.Id, "a11y-role")
            .AddChildContent("<p>Content</p>"));

        var el = cut.Find(".arcadia-draggrid__item");
        el.GetAttribute("role").Should().Be("gridcell");
    }
}

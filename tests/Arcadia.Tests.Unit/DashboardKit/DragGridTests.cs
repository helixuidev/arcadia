using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.DashboardKit.Components;

namespace Arcadia.Tests.Unit.DashboardKit;

public class DragGridTests : ChartTestBase
{
    [Fact]
    public void DragGrid_Renders_WithGridClass()
    {
        var cut = Render<ArcadiaDragGrid>(p => p
            .Add(c => c.ChildContent, "<div>item</div>"));

        cut.Find(".arcadia-draggrid").Should().NotBeNull();
    }

    [Fact]
    public void DragGrid_Columns_SetsGridTemplateColumns()
    {
        var cut = Render<ArcadiaDragGrid>(p => p
            .Add(c => c.Columns, 6)
            .Add(c => c.ChildContent, "<div>item</div>"));

        var style = cut.Find(".arcadia-draggrid").GetAttribute("style");
        style.Should().Contain("grid-template-columns:repeat(6,1fr)");
    }

    [Fact]
    public void DragGrid_RowHeight_SetsGridAutoRows()
    {
        var cut = Render<ArcadiaDragGrid>(p => p
            .Add(c => c.RowHeight, 200)
            .Add(c => c.ChildContent, "<div>item</div>"));

        var style = cut.Find(".arcadia-draggrid").GetAttribute("style");
        style.Should().Contain("grid-auto-rows:200px");
    }

    [Fact]
    public void DragGrid_Gap_SetsGap()
    {
        var cut = Render<ArcadiaDragGrid>(p => p
            .Add(c => c.Gap, 24)
            .Add(c => c.ChildContent, "<div>item</div>"));

        var style = cut.Find(".arcadia-draggrid").GetAttribute("style");
        style.Should().Contain("gap:24px");
    }

    [Fact]
    public void DragGrid_EditMode_AddsEditClass()
    {
        var cut = Render<ArcadiaDragGrid>(p => p
            .Add(c => c.EditMode, true)
            .Add(c => c.DragMode, "direct")
            .Add(c => c.ChildContent, "<div>item</div>"));

        cut.Find(".arcadia-draggrid").ClassList.Should().Contain("arcadia-draggrid--edit");
    }

    [Fact]
    public void DragGrid_EditModeFalse_NoEditClass()
    {
        var cut = Render<ArcadiaDragGrid>(p => p
            .Add(c => c.EditMode, false)
            .Add(c => c.DragMode, "direct")
            .Add(c => c.ChildContent, "<div>item</div>"));

        cut.Find(".arcadia-draggrid").ClassList.Should().NotContain("arcadia-draggrid--edit");
    }

    [Fact]
    public void DragGrid_DragModeLongpress_AddsLongpressClass()
    {
        var cut = Render<ArcadiaDragGrid>(p => p
            .Add(c => c.DragMode, "longpress")
            .Add(c => c.ChildContent, "<div>item</div>"));

        cut.Find(".arcadia-draggrid").ClassList.Should().Contain("arcadia-draggrid--mode-longpress");
    }

    [Fact]
    public void DragGrid_ChildContent_RendersItems()
    {
        var cut = Render<ArcadiaDragGrid>(p => p
            .Add(c => c.ChildContent, "<span class='test-item'>Hello</span>"));

        cut.Find(".test-item").TextContent.Should().Be("Hello");
    }

    [Fact]
    public void DragGrid_ShowAddButton_RendersAddBtn()
    {
        var cut = Render<ArcadiaDragGrid>(p => p
            .Add(c => c.ShowAddButton, true)
            .Add(c => c.ChildContent, "<div>item</div>"));

        cut.Find(".arcadia-draggrid__add-btn").Should().NotBeNull();
        cut.Find(".arcadia-draggrid__add-btn").TextContent.Should().Contain("Add Panel");
    }

    [Fact]
    public void DragGrid_ShowAddButton_False_NoAddBtn()
    {
        var cut = Render<ArcadiaDragGrid>(p => p
            .Add(c => c.ShowAddButton, false)
            .Add(c => c.ChildContent, "<div>item</div>"));

        cut.FindAll(".arcadia-draggrid__add-btn").Should().BeEmpty();
    }
}

using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;

namespace Arcadia.Tests.Unit.UI;

public class TreeViewTests : ChartTestBase
{
    [Fact]
    public void Renders_RoleTree()
    {
        var cut = Render<ArcadiaTreeView>(p => p
            .Add(c => c.Items, new List<TreeViewItem>()));

        cut.Find("[role='tree']").Should().NotBeNull();
    }

    [Fact]
    public void Renders_Items_Text()
    {
        var items = new List<TreeViewItem>
        {
            new() { Id = "1", Text = "Node A" },
            new() { Id = "2", Text = "Node B" }
        };

        var cut = Render<ArcadiaTreeView>(p => p
            .Add(c => c.Items, items));

        var texts = cut.FindAll(".arcadia-treeview__text");
        texts.Should().HaveCount(2);
        texts[0].TextContent.Should().Be("Node A");
        texts[1].TextContent.Should().Be("Node B");
    }

    [Fact]
    public void NestedChildren_RenderGroupRole()
    {
        var items = new List<TreeViewItem>
        {
            new()
            {
                Id = "1", Text = "Parent", Expanded = true,
                Children = new() { new() { Id = "1.1", Text = "Child" } }
            }
        };

        var cut = Render<ArcadiaTreeView>(p => p
            .Add(c => c.Items, items));

        cut.Find("[role='group']").Should().NotBeNull();
        cut.FindAll(".arcadia-treeview__text").Should().HaveCount(2);
    }

    [Fact]
    public void CollapsedNode_DoesNotRenderChildren()
    {
        var items = new List<TreeViewItem>
        {
            new()
            {
                Id = "1", Text = "Parent", Expanded = false,
                Children = new() { new() { Id = "1.1", Text = "Child" } }
            }
        };

        var cut = Render<ArcadiaTreeView>(p => p
            .Add(c => c.Items, items));

        cut.FindAll("[role='group']").Should().BeEmpty();
        cut.FindAll(".arcadia-treeview__text").Should().HaveCount(1);
    }

    [Fact]
    public void ExpandAll_ExpandsCollapsedNodes()
    {
        var items = new List<TreeViewItem>
        {
            new()
            {
                Id = "1", Text = "Parent", Expanded = false,
                Children = new() { new() { Id = "1.1", Text = "Child" } }
            }
        };

        var cut = Render<ArcadiaTreeView>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.ExpandAll, true));

        cut.Find("[role='group']").Should().NotBeNull();
    }

    [Fact]
    public void Click_FiresOnItemClick()
    {
        TreeViewItem? clicked = null;
        var items = new List<TreeViewItem>
        {
            new() { Id = "1", Text = "Clickable" }
        };

        var cut = Render<ArcadiaTreeView>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.OnItemClick, item => clicked = item));

        cut.Find(".arcadia-treeview__item-content").Click();

        clicked.Should().NotBeNull();
        clicked!.Text.Should().Be("Clickable");
    }

    [Fact]
    public void TreeItem_HasTreeItemRole()
    {
        var items = new List<TreeViewItem>
        {
            new() { Id = "1", Text = "Item" }
        };

        var cut = Render<ArcadiaTreeView>(p => p
            .Add(c => c.Items, items));

        cut.Find("[role='treeitem']").Should().NotBeNull();
    }

    [Fact]
    public void RootElement_HasCorrectCssClass()
    {
        var cut = Render<ArcadiaTreeView>(p => p
            .Add(c => c.Items, new List<TreeViewItem>()));

        cut.Find(".arcadia-treeview").Should().NotBeNull();
    }
}

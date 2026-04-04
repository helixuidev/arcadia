using Bunit;
using FluentAssertions;
using Arcadia.Charts.Components.Charts;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public record TreemapItem(string Name, double Value);

public class TreemapChartTests : ChartTestBase
{
    private static readonly List<TreemapItem> BasicData = new()
    {
        new("Alpha", 500),
        new("Beta", 300),
        new("Gamma", 200),
        new("Delta", 150),
        new("Epsilon", 100),
    };

    [Fact]
    public void Renders_SvgWithFigureRole()
    {
        var cut = Render<ArcadiaTreemapChart<TreemapItem>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<TreemapItem, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<TreemapItem, double>)(d => d.Value))
            .Add(c => c.Width, 600));

        cut.Find("svg[role='figure']").Should().NotBeNull();
    }

    [Fact]
    public void Renders_OneRectPerDataItem()
    {
        var cut = Render<ArcadiaTreemapChart<TreemapItem>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<TreemapItem, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<TreemapItem, double>)(d => d.Value))
            .Add(c => c.Width, 600));

        // 5 data items => 5 cell rects
        cut.FindAll("rect").Count.Should().BeGreaterOrEqualTo(5);
    }

    [Fact]
    public void Renders_CellNames_InMarkup()
    {
        var cut = Render<ArcadiaTreemapChart<TreemapItem>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<TreemapItem, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<TreemapItem, double>)(d => d.Value)));

        cut.Markup.Should().Contain("Alpha");
        cut.Markup.Should().Contain("Epsilon");
    }

    [Fact]
    public void Renders_ScreenReaderTable()
    {
        var cut = Render<ArcadiaTreemapChart<TreemapItem>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<TreemapItem, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<TreemapItem, double>)(d => d.Value)));

        cut.Find("table.arcadia-sr-only").Should().NotBeNull();
    }

    [Fact]
    public void Renders_NoData_WhenDataIsNull()
    {
        var cut = Render<ArcadiaTreemapChart<TreemapItem>>(p => p
            .Add(c => c.Data, (IReadOnlyList<TreemapItem>?)null));

        cut.Markup.Should().Contain("arcadia-chart__no-data");
    }

    [Fact]
    public void Renders_NoData_WhenDataIsEmpty()
    {
        var cut = Render<ArcadiaTreemapChart<TreemapItem>>(p => p
            .Add(c => c.Data, new List<TreemapItem>()));

        cut.Markup.Should().Contain("arcadia-chart__no-data");
    }

    [Fact]
    public void Renders_Title()
    {
        var cut = Render<ArcadiaTreemapChart<TreemapItem>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<TreemapItem, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<TreemapItem, double>)(d => d.Value))
            .Add(c => c.Title, "Revenue by Product"));

        cut.Find(".arcadia-chart__title").TextContent.Should().Be("Revenue by Product");
    }

    [Fact]
    public void CustomCellStrokeColor_AppliedToStroke()
    {
        var cut = Render<ArcadiaTreemapChart<TreemapItem>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<TreemapItem, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<TreemapItem, double>)(d => d.Value))
            .Add(c => c.CellStrokeColor, "#112233"));

        cut.Markup.Should().Contain("#112233");
    }

    [Fact]
    public void AllZeroValues_DoesNotCrash()
    {
        var zeros = new List<TreemapItem> { new("A", 0), new("B", 0) };
        var render = () => Render<ArcadiaTreemapChart<TreemapItem>>(p => p
            .Add(c => c.Data, zeros)
            .Add(c => c.NameField, (Func<TreemapItem, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<TreemapItem, double>)(d => d.Value)));

        render.Should().NotThrow();
    }

    [Fact]
    public void SingleItem_RendersSingleCell()
    {
        var single = new List<TreemapItem> { new("Solo", 100) };
        var cut = Render<ArcadiaTreemapChart<TreemapItem>>(p => p
            .Add(c => c.Data, single)
            .Add(c => c.NameField, (Func<TreemapItem, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<TreemapItem, double>)(d => d.Value)));

        cut.Markup.Should().Contain("Solo");
    }
}

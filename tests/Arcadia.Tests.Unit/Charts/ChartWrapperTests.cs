using Bunit;
using FluentAssertions;
using Arcadia.Charts.Components.Charts;
using Arcadia.Charts.Core;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

/// <summary>
/// Smoke tests for chart components that are thin inheriting wrappers over another chart type.
/// These just verify the wrapper resolves its base correctly and renders a chart SVG.
/// </summary>
public record WrapperPoint(string Label, double Value, double Size);

public class ChartWrapperTests : ChartTestBase
{
    private static readonly List<WrapperPoint> Data = new()
    {
        new("Jan", 100, 10),
        new("Feb", 120, 14),
        new("Mar", 130, 12),
    };

    private static readonly List<SeriesConfig<WrapperPoint>> Series = new()
    {
        new() { Name = "Values", Field = d => d.Value },
    };

    [Fact]
    public void AreaChart_InheritsLineChart_AndRendersSvg()
    {
        var cut = Render<ArcadiaAreaChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.XField, (Func<WrapperPoint, object>)(d => d.Label))
            .Add(c => c.Series, Series)
            .Add(c => c.Width, 600)
            .Add(c => c.AnimateOnLoad, false));

        cut.Find("svg[data-chart]").Should().NotBeNull();
        // Inherits from LineChart, so it renders line paths
        cut.FindAll("path.arcadia-chart__line").Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void AreaChart_HasAccessibilityTable()
    {
        var cut = Render<ArcadiaAreaChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.XField, (Func<WrapperPoint, object>)(d => d.Label))
            .Add(c => c.Series, Series)
            .Add(c => c.AnimateOnLoad, false));

        cut.Find("table.arcadia-sr-only").Should().NotBeNull();
    }

    [Fact]
    public void BubbleChart_InheritsScatterChart_AndRendersSvg()
    {
        var cut = Render<ArcadiaBubbleChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.XField, (Func<WrapperPoint, double>)(d => d.Value))
            .Add(c => c.YField, (Func<WrapperPoint, double>)(d => d.Size))
            .Add(c => c.SizeField, (Func<WrapperPoint, double>)(d => d.Size))
            .Add(c => c.Width, 600)
            .Add(c => c.AnimateOnLoad, false));

        cut.Find("svg[data-chart]").Should().NotBeNull();
    }

    [Fact]
    public void BubbleChart_HasAccessibilityTable()
    {
        var cut = Render<ArcadiaBubbleChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.XField, (Func<WrapperPoint, double>)(d => d.Value))
            .Add(c => c.YField, (Func<WrapperPoint, double>)(d => d.Size))
            .Add(c => c.AnimateOnLoad, false));

        cut.Find("table.arcadia-sr-only").Should().NotBeNull();
    }

    [Fact]
    public void DonutChart_InheritsPieChart_AndRendersSvg()
    {
        var cut = Render<ArcadiaDonutChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.NameField, (Func<WrapperPoint, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WrapperPoint, double>)(d => d.Value))
            .Add(c => c.Width, 400)
            .Add(c => c.AnimateOnLoad, false));

        cut.Find("svg[data-chart]").Should().NotBeNull();
        // Pie chart renders path slices
        cut.FindAll("path").Count.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void DonutChart_HasAccessibilityTable()
    {
        var cut = Render<ArcadiaDonutChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.NameField, (Func<WrapperPoint, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WrapperPoint, double>)(d => d.Value))
            .Add(c => c.AnimateOnLoad, false));

        cut.Find("table.arcadia-sr-only").Should().NotBeNull();
    }

    [Fact]
    public void StackedBarChart_InheritsBarChart_AndRendersSvg()
    {
        var cut = Render<ArcadiaStackedBarChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.XField, (Func<WrapperPoint, object>)(d => d.Label))
            .Add(c => c.Series, Series)
            .Add(c => c.Width, 600)
            .Add(c => c.AnimateOnLoad, false));

        cut.Find("svg[data-chart]").Should().NotBeNull();
        cut.FindAll("rect.arcadia-chart__bar").Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void StackedBarChart_HasAccessibilityTable()
    {
        var cut = Render<ArcadiaStackedBarChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.XField, (Func<WrapperPoint, object>)(d => d.Label))
            .Add(c => c.Series, Series)
            .Add(c => c.AnimateOnLoad, false));

        cut.Find("table.arcadia-sr-only").Should().NotBeNull();
    }

    [Fact]
    public void AllWrappers_RenderFigureRole()
    {
        var area = Render<ArcadiaAreaChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.XField, (Func<WrapperPoint, object>)(d => d.Label))
            .Add(c => c.Series, Series)
            .Add(c => c.AnimateOnLoad, false));
        area.Find("svg[role='figure']").Should().NotBeNull();

        var bubble = Render<ArcadiaBubbleChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.XField, (Func<WrapperPoint, double>)(d => d.Value))
            .Add(c => c.YField, (Func<WrapperPoint, double>)(d => d.Size))
            .Add(c => c.AnimateOnLoad, false));
        bubble.Find("svg[role='figure']").Should().NotBeNull();

        var donut = Render<ArcadiaDonutChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.NameField, (Func<WrapperPoint, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WrapperPoint, double>)(d => d.Value))
            .Add(c => c.AnimateOnLoad, false));
        donut.Find("svg[role='figure']").Should().NotBeNull();

        var stacked = Render<ArcadiaStackedBarChart<WrapperPoint>>(p => p
            .Add(c => c.Data, Data)
            .Add(c => c.XField, (Func<WrapperPoint, object>)(d => d.Label))
            .Add(c => c.Series, Series)
            .Add(c => c.AnimateOnLoad, false));
        stacked.Find("svg[role='figure']").Should().NotBeNull();
    }
}

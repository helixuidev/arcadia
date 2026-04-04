using Bunit;
using FluentAssertions;
using Arcadia.Charts.Components.Charts;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public record WaterfallStep(string Label, double Delta);

public class WaterfallChartTests : ChartTestBase
{
    private static readonly List<WaterfallStep> BasicData = new()
    {
        new("Start", 100),
        new("Revenue", 50),
        new("Costs", -30),
        new("Tax", -10),
        new("End", 0),
    };

    [Fact]
    public void Renders_SvgWithFigureRole()
    {
        var cut = Render<ArcadiaWaterfallChart<WaterfallStep>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<WaterfallStep, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WaterfallStep, double>)(d => d.Delta))
            .Add(c => c.Width, 600));

        cut.Find("svg[role='figure']").Should().NotBeNull();
    }

    [Fact]
    public void Renders_OneBarPerDataItem()
    {
        var cut = Render<ArcadiaWaterfallChart<WaterfallStep>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<WaterfallStep, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WaterfallStep, double>)(d => d.Delta))
            .Add(c => c.Width, 600));

        // 5 steps => at least 5 rect elements for bars
        cut.FindAll("rect").Count.Should().BeGreaterOrEqualTo(5);
    }

    [Fact]
    public void PositiveValues_UseCustomPositiveColor()
    {
        var cut = Render<ArcadiaWaterfallChart<WaterfallStep>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<WaterfallStep, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WaterfallStep, double>)(d => d.Delta))
            .Add(c => c.PositiveColor, "#00ff00"));

        cut.Markup.Should().Contain("#00ff00");
    }

    [Fact]
    public void NegativeValues_UseCustomNegativeColor()
    {
        var cut = Render<ArcadiaWaterfallChart<WaterfallStep>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<WaterfallStep, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WaterfallStep, double>)(d => d.Delta))
            .Add(c => c.NegativeColor, "#ff00ff"));

        cut.Markup.Should().Contain("#ff00ff");
    }

    [Fact]
    public void Renders_CategoryLabels()
    {
        var cut = Render<ArcadiaWaterfallChart<WaterfallStep>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<WaterfallStep, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WaterfallStep, double>)(d => d.Delta)));

        cut.Markup.Should().Contain("Start");
        cut.Markup.Should().Contain("Revenue");
        cut.Markup.Should().Contain("End");
    }

    [Fact]
    public void Renders_ScreenReaderTable()
    {
        var cut = Render<ArcadiaWaterfallChart<WaterfallStep>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<WaterfallStep, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WaterfallStep, double>)(d => d.Delta)));

        cut.Find("table.arcadia-sr-only").Should().NotBeNull();
    }

    [Fact]
    public void Renders_NoData_WhenDataIsNull()
    {
        var cut = Render<ArcadiaWaterfallChart<WaterfallStep>>(p => p
            .Add(c => c.Data, (IReadOnlyList<WaterfallStep>?)null));

        cut.Markup.Should().Contain("arcadia-chart__no-data");
    }

    [Fact]
    public void Renders_Title()
    {
        var cut = Render<ArcadiaWaterfallChart<WaterfallStep>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<WaterfallStep, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WaterfallStep, double>)(d => d.Delta))
            .Add(c => c.Title, "P&L Bridge"));

        cut.Find(".arcadia-chart__title").TextContent.Should().Be("P&L Bridge");
    }

    [Fact]
    public void AllPositive_RendersWithoutCrash()
    {
        var allPositive = new List<WaterfallStep>
        {
            new("A", 10), new("B", 20), new("C", 30),
        };
        var cut = Render<ArcadiaWaterfallChart<WaterfallStep>>(p => p
            .Add(c => c.Data, allPositive)
            .Add(c => c.CategoryField, (Func<WaterfallStep, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WaterfallStep, double>)(d => d.Delta)));

        cut.Find("svg[role='figure']").Should().NotBeNull();
    }

    [Fact]
    public void AllNegative_RendersWithoutCrash()
    {
        var allNegative = new List<WaterfallStep>
        {
            new("A", -10), new("B", -20), new("C", -30),
        };
        var cut = Render<ArcadiaWaterfallChart<WaterfallStep>>(p => p
            .Add(c => c.Data, allNegative)
            .Add(c => c.CategoryField, (Func<WaterfallStep, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WaterfallStep, double>)(d => d.Delta)));

        cut.Find("svg[role='figure']").Should().NotBeNull();
    }

    [Fact]
    public void ConnectorOpacity_AppliedToLineOpacity()
    {
        var cut = Render<ArcadiaWaterfallChart<WaterfallStep>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<WaterfallStep, string>)(d => d.Label))
            .Add(c => c.ValueField, (Func<WaterfallStep, double>)(d => d.Delta))
            .Add(c => c.ConnectorOpacity, 0.8));

        cut.Markup.Should().Contain("0.8");
    }
}

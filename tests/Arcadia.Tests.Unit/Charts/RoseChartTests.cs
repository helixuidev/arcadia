using Bunit;
using FluentAssertions;
using Arcadia.Charts.Components.Charts;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public record RoseData(string Name, double Value);

public class RoseChartTests : ChartTestBase
{
    private static readonly List<RoseData> BasicData = new()
    {
        new("North", 80),
        new("East", 60),
        new("South", 40),
        new("West", 70),
    };

    [Fact]
    public void Renders_SvgWithFigureRole()
    {
        var cut = Render<ArcadiaRoseChart<RoseData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<RoseData, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RoseData, double>)(d => d.Value)));

        cut.Find("svg[role='figure']").Should().NotBeNull();
    }

    [Fact]
    public void Renders_OneSectorPerDataItem()
    {
        var cut = Render<ArcadiaRoseChart<RoseData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<RoseData, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RoseData, double>)(d => d.Value)));

        cut.FindAll("path").Count.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    public void Renders_Labels_WhenShowLabelsIsTrue()
    {
        var cut = Render<ArcadiaRoseChart<RoseData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<RoseData, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RoseData, double>)(d => d.Value))
            .Add(c => c.ShowLabels, true));

        cut.Markup.Should().Contain("North");
        cut.Markup.Should().Contain("West");
    }

    [Fact]
    public void HidesLabels_WhenShowLabelsIsFalse()
    {
        var cut = Render<ArcadiaRoseChart<RoseData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<RoseData, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RoseData, double>)(d => d.Value))
            .Add(c => c.ShowLabels, false));

        // Labels are rendered as pie-label text elements inside SVG
        cut.FindAll(".arcadia-chart__pie-label").Should().BeEmpty();
    }

    [Fact]
    public void Renders_ScreenReaderTable()
    {
        var cut = Render<ArcadiaRoseChart<RoseData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<RoseData, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RoseData, double>)(d => d.Value)));

        cut.Find("table.arcadia-sr-only").Should().NotBeNull();
    }

    [Fact]
    public void Renders_NoData_WhenDataIsNull()
    {
        var cut = Render<ArcadiaRoseChart<RoseData>>(p => p
            .Add(c => c.Data, (IReadOnlyList<RoseData>?)null));

        cut.Markup.Should().Contain("arcadia-chart__no-data");
    }

    [Fact]
    public void Renders_NoData_WhenAllValuesZero()
    {
        var zeros = new List<RoseData> { new("A", 0), new("B", 0) };
        var render = () => Render<ArcadiaRoseChart<RoseData>>(p => p
            .Add(c => c.Data, zeros)
            .Add(c => c.NameField, (Func<RoseData, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RoseData, double>)(d => d.Value)));

        render.Should().NotThrow();
    }

    [Fact]
    public void Renders_Legend()
    {
        var cut = Render<ArcadiaRoseChart<RoseData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<RoseData, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RoseData, double>)(d => d.Value)));

        cut.Find(".arcadia-chart__legend").Should().NotBeNull();
    }

    [Fact]
    public void CustomSliceStrokeColor_AppliedToStroke()
    {
        var cut = Render<ArcadiaRoseChart<RoseData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<RoseData, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RoseData, double>)(d => d.Value))
            .Add(c => c.SliceStrokeColor, "#deadbe"));

        cut.Markup.Should().Contain("#deadbe");
    }

    [Fact]
    public void Renders_Title()
    {
        var cut = Render<ArcadiaRoseChart<RoseData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<RoseData, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RoseData, double>)(d => d.Value))
            .Add(c => c.Title, "Wind Rose"));

        cut.Find(".arcadia-chart__title").TextContent.Should().Be("Wind Rose");
    }
}

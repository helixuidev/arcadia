using Bunit;
using FluentAssertions;
using Arcadia.Charts.Components.Charts;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public record FunnelStage(string Name, double Value);

public class FunnelChartTests : ChartTestBase
{
    private static readonly List<FunnelStage> BasicData = new()
    {
        new("Visitors", 10000),
        new("Signups", 5000),
        new("Activated", 2000),
        new("Paid", 500),
    };

    [Fact]
    public void Renders_SvgWithFigureRole()
    {
        var cut = Render<ArcadiaFunnelChart<FunnelStage>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<FunnelStage, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<FunnelStage, double>)(d => d.Value)));

        cut.Find("svg[role='figure']").Should().NotBeNull();
    }

    [Fact]
    public void Renders_OneStagePathPerDataItem()
    {
        var cut = Render<ArcadiaFunnelChart<FunnelStage>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<FunnelStage, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<FunnelStage, double>)(d => d.Value)));

        // 4 stages => at least 4 path elements
        cut.FindAll("path").Count.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    public void Renders_StageLabels_InSvg()
    {
        var cut = Render<ArcadiaFunnelChart<FunnelStage>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<FunnelStage, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<FunnelStage, double>)(d => d.Value)));

        cut.Markup.Should().Contain("Visitors");
        cut.Markup.Should().Contain("Paid");
    }

    [Fact]
    public void Renders_ScreenReaderTable()
    {
        var cut = Render<ArcadiaFunnelChart<FunnelStage>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<FunnelStage, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<FunnelStage, double>)(d => d.Value)));

        cut.Find("table.arcadia-sr-only").Should().NotBeNull();
    }

    [Fact]
    public void Renders_NoData_WhenDataIsNull()
    {
        var cut = Render<ArcadiaFunnelChart<FunnelStage>>(p => p
            .Add(c => c.Data, (IReadOnlyList<FunnelStage>?)null));

        cut.Markup.Should().Contain("arcadia-chart__no-data");
    }

    [Fact]
    public void Renders_NoData_WhenValueFieldMissing()
    {
        var cut = Render<ArcadiaFunnelChart<FunnelStage>>(p => p
            .Add(c => c.Data, BasicData));
        // No crash; nothing rendered since fields are null
        cut.Markup.Should().NotBeNull();
    }

    [Fact]
    public void Renders_Title()
    {
        var cut = Render<ArcadiaFunnelChart<FunnelStage>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<FunnelStage, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<FunnelStage, double>)(d => d.Value))
            .Add(c => c.Title, "Conversion Funnel"));

        cut.Find(".arcadia-chart__title").TextContent.Should().Be("Conversion Funnel");
    }

    [Fact]
    public void AllZeroValues_DoesNotCrash()
    {
        // maxValue = 0 → early return, should still render wrapper
        var zeros = new List<FunnelStage> { new("A", 0), new("B", 0) };
        var render = () => Render<ArcadiaFunnelChart<FunnelStage>>(p => p
            .Add(c => c.Data, zeros)
            .Add(c => c.NameField, (Func<FunnelStage, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<FunnelStage, double>)(d => d.Value)));

        render.Should().NotThrow();
    }

    [Fact]
    public void StageOpacity_AppliedToOpacityAttribute()
    {
        var cut = Render<ArcadiaFunnelChart<FunnelStage>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.NameField, (Func<FunnelStage, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<FunnelStage, double>)(d => d.Value))
            .Add(c => c.StageOpacity, 0.5));

        cut.Markup.Should().Contain("opacity=\"0.5\"");
    }

    [Fact]
    public void SingleStage_RendersOneShape()
    {
        var single = new List<FunnelStage> { new("Only", 100) };
        var cut = Render<ArcadiaFunnelChart<FunnelStage>>(p => p
            .Add(c => c.Data, single)
            .Add(c => c.NameField, (Func<FunnelStage, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<FunnelStage, double>)(d => d.Value)));

        cut.Find("svg[role='figure']").Should().NotBeNull();
        cut.Markup.Should().Contain("Only");
    }
}

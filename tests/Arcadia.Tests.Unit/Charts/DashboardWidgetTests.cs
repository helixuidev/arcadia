using Bunit;
using FluentAssertions;
using Arcadia.Charts.Components.Dashboard;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public class SparklineTests : Arcadia.Tests.Unit.ChartTestBase
{
    [Fact]
    public void Renders_SvgWithPath()
    {
        var cut = Render<ArcadiaSparkline>(p =>
            p.Add(c => c.Data, new double[] { 1, 3, 2, 5, 4 })
             .Add(c => c.Width, 100)
             .Add(c => c.Height, 30));

        cut.Find("svg").Should().NotBeNull();
        cut.Find("path").Should().NotBeNull();
    }

    [Fact]
    public void Renders_WithArea()
    {
        var cut = Render<ArcadiaSparkline>(p =>
            p.Add(c => c.Data, new double[] { 1, 3, 2, 5, 4 })
             .Add(c => c.ShowArea, true));

        cut.FindAll("path").Count.Should().Be(2); // Line + area
    }

    [Fact]
    public void Renders_AriaLabel()
    {
        var cut = Render<ArcadiaSparkline>(p =>
            p.Add(c => c.Data, new double[] { 1, 2, 3 })
             .Add(c => c.AriaLabel, "Revenue trend"));

        cut.Find("svg").GetAttribute("aria-label").Should().Be("Revenue trend");
    }

    [Fact]
    public void NoData_NoPath()
    {
        var cut = Render<ArcadiaSparkline>(p =>
            p.Add(c => c.Data, Array.Empty<double>()));

        cut.FindAll("path").Should().BeEmpty();
    }
}

public class DeltaIndicatorTests : Arcadia.Tests.Unit.ChartTestBase
{
    [Fact]
    public void Increase_ShowsUpArrow()
    {
        var cut = Render<ArcadiaDeltaIndicator>(p =>
            p.Add(c => c.Value, "+12%")
             .Add(c => c.Type, DeltaType.Increase));

        cut.Find(".arcadia-delta__arrow").TextContent.Should().Contain("↑");
        cut.Find(".arcadia-delta").ClassList.Should().Contain("arcadia-delta--increase");
    }

    [Fact]
    public void Decrease_ShowsDownArrow()
    {
        var cut = Render<ArcadiaDeltaIndicator>(p =>
            p.Add(c => c.Value, "-5%")
             .Add(c => c.Type, DeltaType.Decrease));

        cut.Find(".arcadia-delta__arrow").TextContent.Should().Contain("↓");
        cut.Find(".arcadia-delta").ClassList.Should().Contain("arcadia-delta--decrease");
    }
}

public class ProgressBarTests : Arcadia.Tests.Unit.ChartTestBase
{
    [Fact]
    public void Renders_WithCorrectWidth()
    {
        var cut = Render<ArcadiaProgressBar>(p =>
            p.Add(c => c.Value, 75)
             .Add(c => c.Max, 100));

        cut.Find(".arcadia-progress__fill").GetAttribute("style").Should().Contain("width: 75%");
    }

    [Fact]
    public void Renders_AriaAttributes()
    {
        var cut = Render<ArcadiaProgressBar>(p =>
            p.Add(c => c.Value, 50)
             .Add(c => c.Max, 100)
             .Add(c => c.Label, "Loading"));

        var bar = cut.Find("[role='progressbar']");
        bar.GetAttribute("aria-valuenow").Should().Be("50");
        bar.GetAttribute("aria-valuemax").Should().Be("100");
    }

    [Fact]
    public void Renders_Label()
    {
        var cut = Render<ArcadiaProgressBar>(p =>
            p.Add(c => c.Value, 30)
             .Add(c => c.Label, "Storage"));

        cut.Find(".arcadia-progress__label").TextContent.Should().Be("Storage");
    }
}

public class KpiCardTests : Arcadia.Tests.Unit.ChartTestBase
{
    [Fact]
    public void Renders_TitleAndValue()
    {
        var cut = Render<ArcadiaKpiCard>(p =>
            p.Add(c => c.Title, "Revenue")
             .Add(c => c.Value, "$142K"));

        cut.Find(".arcadia-kpi__title").TextContent.Should().Be("Revenue");
        cut.Find(".arcadia-kpi__value").TextContent.Should().Be("$142K");
    }

    [Fact]
    public void Renders_Delta()
    {
        var cut = Render<ArcadiaKpiCard>(p =>
            p.Add(c => c.Title, "Revenue")
             .Add(c => c.Value, "$142K")
             .Add(c => c.Delta, "+12%")
             .Add(c => c.DeltaType, DeltaType.Increase));

        cut.FindAll(".arcadia-delta").Should().HaveCount(1);
    }

    [Fact]
    public void Renders_Sparkline()
    {
        var cut = Render<ArcadiaKpiCard>(p =>
            p.Add(c => c.Title, "Revenue")
             .Add(c => c.Value, "$142K")
             .Add(c => c.Sparkline, new double[] { 1, 3, 2, 5, 4, 6 }));

        cut.FindAll("svg").Should().HaveCount(1);
    }

    [Fact]
    public void Renders_Footer()
    {
        var cut = Render<ArcadiaKpiCard>(p =>
            p.Add(c => c.Title, "Revenue")
             .Add(c => c.Value, "$142K")
             .Add(c => c.Footer, "vs last month"));

        cut.Find(".arcadia-kpi__footer").TextContent.Should().Be("vs last month");
    }
}

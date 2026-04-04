using Bunit;
using FluentAssertions;
using Arcadia.Charts.Components.Charts;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public record RangePoint(string Label, double Upper, double Lower, double Middle);

public class RangeAreaChartTests : ChartTestBase
{
    private static readonly List<RangePoint> BasicData = new()
    {
        new("Mon", 85, 65, 75),
        new("Tue", 88, 68, 78),
        new("Wed", 82, 62, 72),
        new("Thu", 90, 70, 80),
        new("Fri", 92, 72, 82),
    };

    [Fact]
    public void Renders_SvgWithFigureRole()
    {
        var cut = Render<ArcadiaRangeAreaChart<RangePoint>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.XField, (Func<RangePoint, object>)(d => d.Label))
            .Add(c => c.UpperField, (Func<RangePoint, double>)(d => d.Upper))
            .Add(c => c.LowerField, (Func<RangePoint, double>)(d => d.Lower))
            .Add(c => c.Width, 600));

        cut.Find("svg[role='figure']").Should().NotBeNull();
    }

    [Fact]
    public void Renders_FillPath_WhenDataValid()
    {
        var cut = Render<ArcadiaRangeAreaChart<RangePoint>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.XField, (Func<RangePoint, object>)(d => d.Label))
            .Add(c => c.UpperField, (Func<RangePoint, double>)(d => d.Upper))
            .Add(c => c.LowerField, (Func<RangePoint, double>)(d => d.Lower))
            .Add(c => c.Width, 600));

        // At least 3 paths: fill, upper line, lower line
        cut.FindAll("path").Count.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void Renders_MiddleLine_WhenMiddleFieldProvided()
    {
        var cut = Render<ArcadiaRangeAreaChart<RangePoint>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.XField, (Func<RangePoint, object>)(d => d.Label))
            .Add(c => c.UpperField, (Func<RangePoint, double>)(d => d.Upper))
            .Add(c => c.LowerField, (Func<RangePoint, double>)(d => d.Lower))
            .Add(c => c.MiddleField, (Func<RangePoint, double>)(d => d.Middle))
            .Add(c => c.Width, 600));

        // With middle line: fill + upper + lower + middle = at least 4 paths
        cut.FindAll("path").Count.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    public void Renders_ScreenReaderTable_WithAllLabels()
    {
        var cut = Render<ArcadiaRangeAreaChart<RangePoint>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.XField, (Func<RangePoint, object>)(d => d.Label))
            .Add(c => c.UpperField, (Func<RangePoint, double>)(d => d.Upper))
            .Add(c => c.LowerField, (Func<RangePoint, double>)(d => d.Lower)));

        var table = cut.Find("table.arcadia-sr-only");
        table.Should().NotBeNull();
        var markup = table.InnerHtml;
        markup.Should().Contain("Mon");
        markup.Should().Contain("Fri");
    }

    [Fact]
    public void Renders_NoData_WhenDataIsNull()
    {
        var cut = Render<ArcadiaRangeAreaChart<RangePoint>>(p => p
            .Add(c => c.Data, (IReadOnlyList<RangePoint>?)null));

        cut.Markup.Should().Contain("arcadia-chart__no-data");
    }

    [Fact]
    public void Renders_NoData_WhenOnlyOneDataPoint()
    {
        // Component requires at least 2 points to draw a line/area
        var single = new List<RangePoint> { new("Solo", 10, 5, 7) };
        var cut = Render<ArcadiaRangeAreaChart<RangePoint>>(p => p
            .Add(c => c.Data, single)
            .Add(c => c.XField, (Func<RangePoint, object>)(d => d.Label))
            .Add(c => c.UpperField, (Func<RangePoint, double>)(d => d.Upper))
            .Add(c => c.LowerField, (Func<RangePoint, double>)(d => d.Lower)));

        cut.Markup.Should().Contain("arcadia-chart__no-data");
    }

    [Fact]
    public void Renders_Title_AndSubtitle()
    {
        var cut = Render<ArcadiaRangeAreaChart<RangePoint>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.XField, (Func<RangePoint, object>)(d => d.Label))
            .Add(c => c.UpperField, (Func<RangePoint, double>)(d => d.Upper))
            .Add(c => c.LowerField, (Func<RangePoint, double>)(d => d.Lower))
            .Add(c => c.Title, "Temperature Range")
            .Add(c => c.Subtitle, "This Week"));

        cut.Find(".arcadia-chart__title").TextContent.Should().Be("Temperature Range");
        cut.Find(".arcadia-chart__subtitle").Should().NotBeNull();
    }

    [Fact]
    public void CustomFillColor_AppliedToFillAttribute()
    {
        var cut = Render<ArcadiaRangeAreaChart<RangePoint>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.XField, (Func<RangePoint, object>)(d => d.Label))
            .Add(c => c.UpperField, (Func<RangePoint, double>)(d => d.Upper))
            .Add(c => c.LowerField, (Func<RangePoint, double>)(d => d.Lower))
            .Add(c => c.FillColor, "#aabbcc"));

        cut.Markup.Should().Contain("#aabbcc");
    }

    [Fact]
    public void SmoothCurve_RendersDifferentThanLinear()
    {
        var linear = Render<ArcadiaRangeAreaChart<RangePoint>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.XField, (Func<RangePoint, object>)(d => d.Label))
            .Add(c => c.UpperField, (Func<RangePoint, double>)(d => d.Upper))
            .Add(c => c.LowerField, (Func<RangePoint, double>)(d => d.Lower))
            .Add(c => c.CurveType, "linear"));

        var smooth = Render<ArcadiaRangeAreaChart<RangePoint>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.XField, (Func<RangePoint, object>)(d => d.Label))
            .Add(c => c.UpperField, (Func<RangePoint, double>)(d => d.Upper))
            .Add(c => c.LowerField, (Func<RangePoint, double>)(d => d.Lower))
            .Add(c => c.CurveType, "smooth"));

        linear.Markup.Should().NotBe(smooth.Markup);
    }

    [Fact]
    public void MiddleColor_OverridesStrokeColor_WhenMiddleLinePresent()
    {
        var cut = Render<ArcadiaRangeAreaChart<RangePoint>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.XField, (Func<RangePoint, object>)(d => d.Label))
            .Add(c => c.UpperField, (Func<RangePoint, double>)(d => d.Upper))
            .Add(c => c.LowerField, (Func<RangePoint, double>)(d => d.Lower))
            .Add(c => c.MiddleField, (Func<RangePoint, double>)(d => d.Middle))
            .Add(c => c.StrokeColor, "#111111")
            .Add(c => c.MiddleColor, "#ff5500"));

        cut.Markup.Should().Contain("#ff5500");
    }
}

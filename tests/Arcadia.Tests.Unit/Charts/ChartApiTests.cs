using Bunit;
using FluentAssertions;
using Arcadia.Charts.Core;
using Arcadia.Charts.Components.Charts;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

// Shared test data
public record SalesPoint(string Month, double Revenue, double Target);
public record PieSlice(string Name, double Value);
public record XYPoint(double X, double Y);

public class LineChartApiTests : BunitContext
{
    private static readonly List<SalesPoint> SampleData = new()
    {
        new("Jan", 100, 90), new("Feb", 120, 95), new("Mar", 110, 100),
        new("Apr", 140, 105), new("May", 130, 110), new("Jun", 150, 115),
    };

    private static readonly List<SeriesConfig<SalesPoint>> DefaultSeries = new()
    {
        new() { Name = "Revenue", Field = d => d.Revenue },
    };

    [Fact]
    public void Renders_Title()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries)
            .Add(c => c.Title, "Monthly Revenue"));

        cut.Find(".arcadia-chart__title").TextContent.Should().Be("Monthly Revenue");
    }

    [Fact]
    public void Renders_Subtitle()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries)
            .Add(c => c.Title, "Revenue")
            .Add(c => c.Subtitle, "FY2026"));

        cut.Markup.Should().Contain("FY2026");
    }

    [Fact]
    public void Renders_WithCustomDimensions()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries)
            .Add(c => c.Width, 800)
            .Add(c => c.Height, 400));

        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("800");
        svg.GetAttribute("height").Should().Be("400");
    }

    [Fact]
    public void Renders_AriaLabel()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries)
            .Add(c => c.AriaLabel, "Revenue over time"));

        cut.Find("svg").GetAttribute("aria-label").Should().Be("Revenue over time");
    }

    [Fact]
    public void Renders_HiddenDataTable()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries));

        var table = cut.Find("table.arcadia-sr-only");
        table.Should().NotBeNull();
        cut.FindAll("table.arcadia-sr-only tbody tr").Count.Should().Be(6);
    }

    [Fact]
    public void Renders_MultiSeries_Legend()
    {
        var multiSeries = new List<SeriesConfig<SalesPoint>>
        {
            new() { Name = "Revenue", Field = d => d.Revenue, Color = "primary" },
            new() { Name = "Target", Field = d => d.Target, Color = "secondary", Dashed = true },
        };

        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, multiSeries)
            .Add(c => c.ShowLegend, true));

        var legendItems = cut.FindAll(".arcadia-chart__legend-item");
        legendItems.Count.Should().Be(2);
        legendItems[0].TextContent.Should().Contain("Revenue");
        legendItems[1].TextContent.Should().Contain("Target");
    }

    [Fact]
    public void HidesLegend_WhenShowLegendFalse()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries)
            .Add(c => c.ShowLegend, false));

        cut.FindAll(".arcadia-chart__legend").Count.Should().Be(0);
    }

    [Fact]
    public void Renders_GridLines()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries)
            .Add(c => c.ShowGrid, true));

        cut.FindAll("line[stroke-dasharray]").Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void HidesGrid_WhenShowGridFalse()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries)
            .Add(c => c.ShowGrid, false));

        cut.FindAll("line[stroke-dasharray='4,4']").Count.Should().Be(0);
    }

    [Fact]
    public void Renders_DataPoints()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries)
            .Add(c => c.ShowPoints, true));

        cut.FindAll("circle.arcadia-chart__point").Count.Should().Be(6);
    }

    [Fact]
    public void HidesPoints_WhenShowPointsFalse()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries)
            .Add(c => c.ShowPoints, false));

        cut.FindAll("circle.arcadia-chart__point").Count.Should().Be(0);
    }

    [Fact]
    public void Renders_LinePath()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries));

        cut.FindAll("path.arcadia-chart__line").Count.Should().Be(1);
    }

    [Fact]
    public void Renders_DashedLine()
    {
        var dashedSeries = new List<SeriesConfig<SalesPoint>>
        {
            new() { Name = "Target", Field = d => d.Target, Dashed = true },
        };

        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, dashedSeries));

        var line = cut.Find("path.arcadia-chart__line");
        line.GetAttribute("stroke-dasharray").Should().Be("6,4");
    }

    [Fact]
    public void Renders_AreaFill()
    {
        var areaSeries = new List<SeriesConfig<SalesPoint>>
        {
            new() { Name = "Revenue", Field = d => d.Revenue, ShowArea = true, AreaOpacity = 0.2 },
        };

        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, areaSeries));

        // Area path + line path = 2 paths
        cut.FindAll("path").Count.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public void AnimationClasses_WhenAnimateOnLoad()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries)
            .Add(c => c.AnimateOnLoad, true));

        cut.FindAll(".arcadia-animate-line").Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void NoAnimationClasses_WhenAnimateOff()
    {
        var cut = Render<HelixLineChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, DefaultSeries)
            .Add(c => c.AnimateOnLoad, false));

        cut.FindAll(".arcadia-animate-line").Count.Should().Be(0);
        cut.FindAll(".arcadia-animate-point").Count.Should().Be(0);
    }
}

public class BarChartApiTests : BunitContext
{
    private static readonly List<SalesPoint> SampleData = new()
    {
        new("Q1", 100, 90), new("Q2", 150, 120), new("Q3", 130, 110), new("Q4", 180, 140),
    };

    [Fact]
    public void Renders_Bars()
    {
        var series = new List<SeriesConfig<SalesPoint>> { new() { Name = "Revenue", Field = d => d.Revenue } };

        var cut = Render<HelixBarChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, series));

        cut.FindAll("rect.arcadia-chart__bar").Count.Should().Be(4);
    }

    [Fact]
    public void Renders_GroupedBars_MultiSeries()
    {
        var series = new List<SeriesConfig<SalesPoint>>
        {
            new() { Name = "Revenue", Field = d => d.Revenue },
            new() { Name = "Target", Field = d => d.Target },
        };

        var cut = Render<HelixBarChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, series));

        cut.FindAll("rect.arcadia-chart__bar").Count.Should().Be(8); // 4 data points × 2 series
    }

    [Fact]
    public void Renders_StackedBars()
    {
        var series = new List<SeriesConfig<SalesPoint>>
        {
            new() { Name = "Revenue", Field = d => d.Revenue },
            new() { Name = "Target", Field = d => d.Target },
        };

        var cut = Render<HelixBarChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, series)
            .Add(c => c.Stacked, true));

        cut.FindAll("rect.arcadia-chart__bar").Count.Should().Be(8);
    }

    [Fact]
    public void BarAnimation_WhenEnabled()
    {
        var series = new List<SeriesConfig<SalesPoint>> { new() { Name = "Revenue", Field = d => d.Revenue } };

        var cut = Render<HelixBarChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, series)
            .Add(c => c.AnimateOnLoad, true));

        cut.FindAll(".arcadia-animate-bar").Count.Should().Be(4);
    }

    [Fact]
    public void Renders_RoundedCorners()
    {
        var series = new List<SeriesConfig<SalesPoint>> { new() { Name = "Revenue", Field = d => d.Revenue } };

        var cut = Render<HelixBarChart<SalesPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<SalesPoint, object>)(d => d.Month))
            .Add(c => c.Series, series)
            .Add(c => c.Rounded, true));

        var bar = cut.Find("rect.arcadia-chart__bar");
        bar.GetAttribute("rx").Should().NotBe("0");
    }
}

public class PieChartApiTests : BunitContext
{
    private static readonly List<PieSlice> SampleData = new()
    {
        new("Engineering", 45), new("Marketing", 25), new("Sales", 20), new("HR", 10),
    };

    [Fact]
    public void Renders_Slices()
    {
        var cut = Render<HelixPieChart<PieSlice>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.NameField, (Func<PieSlice, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<PieSlice, double>)(d => d.Value)));

        cut.FindAll("path.arcadia-chart__pie-slice").Count.Should().Be(4);
    }

    [Fact]
    public void Renders_PercentLabels_ByDefault()
    {
        var cut = Render<HelixPieChart<PieSlice>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.NameField, (Func<PieSlice, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<PieSlice, double>)(d => d.Value))
            .Add(c => c.ShowLabels, true)
            .Add(c => c.LabelFormat, PieLabelFormat.Percent));

        var labels = cut.FindAll(".arcadia-chart__pie-label");
        labels.Count.Should().BeGreaterThan(0);
        labels[0].TextContent.Should().Contain("%");
    }

    [Fact]
    public void Renders_NameLabels()
    {
        var cut = Render<HelixPieChart<PieSlice>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.NameField, (Func<PieSlice, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<PieSlice, double>)(d => d.Value))
            .Add(c => c.LabelFormat, PieLabelFormat.Name));

        var labels = cut.FindAll(".arcadia-chart__pie-label");
        labels.Count.Should().BeGreaterThan(0);
        labels[0].TextContent.Should().Contain("Engineering");
    }

    [Fact]
    public void HidesLabels_WhenFormatNone()
    {
        var cut = Render<HelixPieChart<PieSlice>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.NameField, (Func<PieSlice, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<PieSlice, double>)(d => d.Value))
            .Add(c => c.LabelFormat, PieLabelFormat.None));

        cut.FindAll(".arcadia-chart__pie-label").Count.Should().Be(0);
    }

    [Fact]
    public void Renders_Legend()
    {
        var cut = Render<HelixPieChart<PieSlice>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.NameField, (Func<PieSlice, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<PieSlice, double>)(d => d.Value))
            .Add(c => c.ShowLegend, true));

        cut.FindAll(".arcadia-chart__legend-item").Count.Should().Be(4);
    }

    [Fact]
    public void Renders_AsDonut_WithInnerRadius()
    {
        var cut = Render<HelixPieChart<PieSlice>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.NameField, (Func<PieSlice, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<PieSlice, double>)(d => d.Value))
            .Add(c => c.InnerRadius, 60));

        // Donut paths contain arc commands for inner radius
        var path = cut.Find("path.arcadia-chart__pie-slice");
        path.GetAttribute("d").Should().Contain("A"); // Arc commands
    }

    [Fact]
    public void SmallSlices_HideLabels_BelowThreshold()
    {
        var data = new List<PieSlice>
        {
            new("Big", 95), new("Tiny", 2), new("Mini", 3),
        };

        var cut = Render<HelixPieChart<PieSlice>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.NameField, (Func<PieSlice, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<PieSlice, double>)(d => d.Value))
            .Add(c => c.MinLabelPercent, 5));

        // Only "Big" (95%) should have a label
        cut.FindAll(".arcadia-chart__pie-label").Count.Should().Be(1);
    }

    [Fact]
    public void Renders_ScreenReaderTable()
    {
        var cut = Render<HelixPieChart<PieSlice>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.NameField, (Func<PieSlice, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<PieSlice, double>)(d => d.Value)));

        cut.FindAll("table.arcadia-sr-only tbody tr").Count.Should().Be(4);
    }

    [Fact]
    public void SliceAnimation_WhenEnabled()
    {
        var cut = Render<HelixPieChart<PieSlice>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.NameField, (Func<PieSlice, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<PieSlice, double>)(d => d.Value))
            .Add(c => c.AnimateOnLoad, true));

        cut.FindAll(".arcadia-animate-point").Count.Should().Be(4);
    }
}

public class ScatterChartApiTests : BunitContext
{
    private static readonly List<XYPoint> SampleData = Enumerable.Range(0, 20)
        .Select(i => new XYPoint(i * 5.0, 10 + i * 2.0 + (i % 3)))
        .ToList();

    [Fact]
    public void Renders_Points()
    {
        var cut = Render<HelixScatterChart<XYPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<XYPoint, double>)(d => d.X))
            .Add(c => c.YField, (Func<XYPoint, double>)(d => d.Y)));

        cut.FindAll("circle.arcadia-chart__point").Count.Should().Be(20);
    }

    [Fact]
    public void Renders_WithCustomPointSize()
    {
        var cut = Render<HelixScatterChart<XYPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<XYPoint, double>)(d => d.X))
            .Add(c => c.YField, (Func<XYPoint, double>)(d => d.Y))
            .Add(c => c.PointSize, 8));

        var point = cut.Find("circle.arcadia-chart__point");
        point.GetAttribute("r").Should().Be("8.0");
    }

    [Fact]
    public void PointAnimation_WithStagger()
    {
        var cut = Render<HelixScatterChart<XYPoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.XField, (Func<XYPoint, double>)(d => d.X))
            .Add(c => c.YField, (Func<XYPoint, double>)(d => d.Y))
            .Add(c => c.AnimateOnLoad, true));

        var points = cut.FindAll(".arcadia-animate-point");
        points.Count.Should().Be(20);
        // First point should have delay 0ms, later ones should have increasing delays
        points[0].GetAttribute("style").Should().Contain("animation-delay: 0ms");
        points[5].GetAttribute("style").Should().Contain("animation-delay: 100ms");
    }
}

public class CandlestickChartApiTests : BunitContext
{
    private static readonly List<CandlePoint> SampleData = new()
    {
        new("Mon", 100, 110, 95, 105),
        new("Tue", 105, 115, 100, 112),
        new("Wed", 112, 120, 108, 108),
        new("Thu", 108, 118, 105, 116),
        new("Fri", 116, 125, 112, 122),
    };

    public record CandlePoint(string Day, double Open, double High, double Low, double Close);

    [Fact]
    public void Renders_CandlestickBodies()
    {
        var cut = Render<HelixCandlestickChart<CandlePoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.LabelField, (Func<CandlePoint, string>)(d => d.Day))
            .Add(c => c.OpenField, (Func<CandlePoint, double>)(d => d.Open))
            .Add(c => c.HighField, (Func<CandlePoint, double>)(d => d.High))
            .Add(c => c.LowField, (Func<CandlePoint, double>)(d => d.Low))
            .Add(c => c.CloseField, (Func<CandlePoint, double>)(d => d.Close)));

        cut.FindAll(".arcadia-candle__body").Count.Should().Be(5);
        cut.FindAll(".arcadia-candle__wick").Count.Should().Be(5);
    }

    [Fact]
    public void Renders_UpAndDownColors()
    {
        var cut = Render<HelixCandlestickChart<CandlePoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.LabelField, (Func<CandlePoint, string>)(d => d.Day))
            .Add(c => c.OpenField, (Func<CandlePoint, double>)(d => d.Open))
            .Add(c => c.HighField, (Func<CandlePoint, double>)(d => d.High))
            .Add(c => c.LowField, (Func<CandlePoint, double>)(d => d.Low))
            .Add(c => c.CloseField, (Func<CandlePoint, double>)(d => d.Close))
            .Add(c => c.UpColor, "#00ff00")
            .Add(c => c.DownColor, "#ff0000"));

        var bodies = cut.FindAll(".arcadia-candle__body");
        // Wed is a down candle (close 108 < open 112)
        bodies[2].GetAttribute("fill").Should().Be("#ff0000");
        // Mon is an up candle (close 105 > open 100)
        bodies[0].GetAttribute("fill").Should().Be("#00ff00");
    }

    [Fact]
    public void CandleAnimation_WithStagger()
    {
        var cut = Render<HelixCandlestickChart<CandlePoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.LabelField, (Func<CandlePoint, string>)(d => d.Day))
            .Add(c => c.OpenField, (Func<CandlePoint, double>)(d => d.Open))
            .Add(c => c.HighField, (Func<CandlePoint, double>)(d => d.High))
            .Add(c => c.LowField, (Func<CandlePoint, double>)(d => d.Low))
            .Add(c => c.CloseField, (Func<CandlePoint, double>)(d => d.Close))
            .Add(c => c.AnimateOnLoad, true));

        cut.FindAll(".arcadia-animate-candle").Count.Should().Be(10); // 5 wicks + 5 bodies
    }

    [Fact]
    public void Renders_OverlayLineSeries()
    {
        var overlay = new List<SeriesConfig<CandlePoint>>
        {
            new() { Name = "MA", Field = d => (d.Open + d.Close) / 2, Color = "info" },
        };

        var cut = Render<HelixCandlestickChart<CandlePoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.LabelField, (Func<CandlePoint, string>)(d => d.Day))
            .Add(c => c.OpenField, (Func<CandlePoint, double>)(d => d.Open))
            .Add(c => c.HighField, (Func<CandlePoint, double>)(d => d.High))
            .Add(c => c.LowField, (Func<CandlePoint, double>)(d => d.Low))
            .Add(c => c.CloseField, (Func<CandlePoint, double>)(d => d.Close))
            .Add(c => c.OverlaySeries, overlay));

        cut.FindAll("path.arcadia-chart__line").Count.Should().Be(1);
    }

    [Fact]
    public void Renders_ScreenReaderTable()
    {
        var cut = Render<HelixCandlestickChart<CandlePoint>>(p => p
            .Add(c => c.Data, SampleData)
            .Add(c => c.LabelField, (Func<CandlePoint, string>)(d => d.Day))
            .Add(c => c.OpenField, (Func<CandlePoint, double>)(d => d.Open))
            .Add(c => c.HighField, (Func<CandlePoint, double>)(d => d.High))
            .Add(c => c.LowField, (Func<CandlePoint, double>)(d => d.Low))
            .Add(c => c.CloseField, (Func<CandlePoint, double>)(d => d.Close)));

        cut.FindAll("table.arcadia-sr-only tbody tr").Count.Should().Be(5);
    }
}

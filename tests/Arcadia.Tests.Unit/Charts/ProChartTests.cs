using Bunit;
using FluentAssertions;
using Arcadia.Charts.Components.Charts;
using Arcadia.Charts.Core;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public record RadarData(string Axis, double Speed, double Power, double Defense);
public record HeatmapData(string Day, string Hour, double Value);

public class RadarChartTests : BunitContext
{
    private static readonly List<RadarData> TestData = new()
    {
        new("Speed", 80, 60, 40),
        new("Strength", 70, 90, 50),
        new("Agility", 90, 50, 70),
        new("Stamina", 60, 70, 80),
        new("Defense", 50, 40, 95),
    };

    private static readonly List<SeriesConfig<RadarData>> TestSeries = new()
    {
        new() { Name = "Player A", Field = d => d.Speed },
        new() { Name = "Player B", Field = d => d.Power },
    };

    [Fact]
    public void Renders_SvgWithPolygons()
    {
        var cut = Render<HelixRadarChart<RadarData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.LabelField, d => d.Axis)
             .Add(c => c.Series, TestSeries));

        cut.Find("svg").Should().NotBeNull();
        // Grid rings (5 default) + 2 series polygons (fill) + 2 series polygons (stroke)
        cut.FindAll("polygon").Count.Should().BeGreaterOrEqualTo(5 + 2);
    }

    [Fact]
    public void Renders_AxisLabels()
    {
        var cut = Render<HelixRadarChart<RadarData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.LabelField, d => d.Axis)
             .Add(c => c.Series, TestSeries));

        var markup = cut.Markup;
        markup.Should().Contain("Speed");
        markup.Should().Contain("Agility");
    }

    [Fact]
    public void Renders_Legend_WhenMultipleSeries()
    {
        var cut = Render<HelixRadarChart<RadarData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.LabelField, d => d.Axis)
             .Add(c => c.Series, TestSeries));

        cut.FindAll(".arcadia-chart__legend-item").Count.Should().Be(2);
    }

    [Fact]
    public void Renders_AriaLabel()
    {
        var cut = Render<HelixRadarChart<RadarData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.LabelField, d => d.Axis)
             .Add(c => c.Series, TestSeries)
             .Add(c => c.AriaLabel, "Player stats"));

        cut.Find("svg[data-chart]").GetAttribute("aria-label").Should().Be("Player stats");
    }

    [Fact]
    public void Renders_DataPoints_WhenShowPointsTrue()
    {
        var cut = Render<HelixRadarChart<RadarData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.LabelField, d => d.Axis)
             .Add(c => c.Series, TestSeries)
             .Add(c => c.ShowPoints, true));

        // 5 data points per series * 2 series = 10
        cut.FindAll("circle").Count.Should().Be(10);
    }

    [Fact]
    public void HidesPoints_WhenShowPointsFalse()
    {
        var cut = Render<HelixRadarChart<RadarData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.LabelField, d => d.Axis)
             .Add(c => c.Series, TestSeries)
             .Add(c => c.ShowPoints, false));

        cut.FindAll("circle").Count.Should().Be(0);
    }

    [Fact]
    public void Renders_HiddenDataTable()
    {
        var cut = Render<HelixRadarChart<RadarData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.LabelField, d => d.Axis)
             .Add(c => c.Series, TestSeries));

        var table = cut.Find("table.arcadia-sr-only");
        table.Should().NotBeNull();
        // Header row + 5 data rows
        cut.FindAll("table.arcadia-sr-only tbody tr").Count.Should().Be(5);
    }

    [Fact]
    public void NoData_RendersNothing()
    {
        var cut = Render<HelixRadarChart<RadarData>>(p =>
            p.Add(c => c.Data, new List<RadarData>())
             .Add(c => c.LabelField, d => d.Axis)
             .Add(c => c.Series, TestSeries));

        cut.FindAll("svg").Count.Should().Be(0);
    }

    [Fact]
    public void Renders_Title()
    {
        var cut = Render<HelixRadarChart<RadarData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.LabelField, d => d.Axis)
             .Add(c => c.Series, TestSeries)
             .Add(c => c.Title, "Skills"));

        cut.Markup.Should().Contain("Skills");
    }
}

public class GaugeChartTests : BunitContext
{
    [Fact]
    public void Renders_SvgWithArc()
    {
        var cut = Render<HelixGaugeChart>(p =>
            p.Add(c => c.Value, 75)
             .Add(c => c.Min, 0)
             .Add(c => c.Max, 100));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("path").Count.Should().BeGreaterOrEqualTo(1); // At least track
    }

    [Fact]
    public void Renders_CenterValue()
    {
        var cut = Render<HelixGaugeChart>(p =>
            p.Add(c => c.Value, 75)
             .Add(c => c.Min, 0)
             .Add(c => c.Max, 100));

        cut.Markup.Should().Contain("75");
    }

    [Fact]
    public void Renders_Label()
    {
        var cut = Render<HelixGaugeChart>(p =>
            p.Add(c => c.Value, 42)
             .Add(c => c.Label, "Performance"));

        cut.Markup.Should().Contain("Performance");
    }

    [Fact]
    public void Renders_Title()
    {
        var cut = Render<HelixGaugeChart>(p =>
            p.Add(c => c.Value, 50)
             .Add(c => c.Title, "CPU Usage"));

        cut.Markup.Should().Contain("CPU Usage");
    }

    [Fact]
    public void Applies_ThresholdColor()
    {
        var thresholds = new List<GaugeThreshold>
        {
            new() { Value = 0, Color = "#22c55e" },
            new() { Value = 60, Color = "#f59e0b" },
            new() { Value = 80, Color = "#ef4444" },
        };

        var cut = Render<HelixGaugeChart>(p =>
            p.Add(c => c.Value, 85)
             .Add(c => c.Thresholds, thresholds));

        // Value is 85 which >= 80, so should use #ef4444
        var valuePath = cut.FindAll("path.arcadia-gauge__value");
        valuePath.Count.Should().BeGreaterOrEqualTo(1);
        valuePath[0].GetAttribute("stroke").Should().Be("#ef4444");
    }

    [Fact]
    public void Clamps_ValueToMinMax()
    {
        var cut = Render<HelixGaugeChart>(p =>
            p.Add(c => c.Value, 150)
             .Add(c => c.Min, 0)
             .Add(c => c.Max, 100));

        // Should display 100 (clamped)
        cut.Markup.Should().Contain("100");
    }

    [Fact]
    public void Renders_FullCircle()
    {
        var cut = Render<HelixGaugeChart>(p =>
            p.Add(c => c.Value, 50)
             .Add(c => c.FullCircle, true));

        cut.Find("svg").Should().NotBeNull();
        // Should have the track and value paths
        cut.FindAll("path").Count.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public void Renders_AriaLabel()
    {
        var cut = Render<HelixGaugeChart>(p =>
            p.Add(c => c.Value, 50)
             .Add(c => c.AriaLabel, "Server load"));

        cut.Find("svg[data-chart]").GetAttribute("aria-label").Should().Be("Server load");
    }

    [Fact]
    public void Renders_FormattedValue()
    {
        var cut = Render<HelixGaugeChart>(p =>
            p.Add(c => c.Value, 42.567)
             .Add(c => c.ValueFormatString, "F1"));

        cut.Markup.Should().Contain("42.6");
    }

    [Fact]
    public void Renders_Accessibility()
    {
        var cut = Render<HelixGaugeChart>(p =>
            p.Add(c => c.Value, 75)
             .Add(c => c.Max, 100));

        cut.Find(".arcadia-sr-only").Should().NotBeNull();
    }
}

public class HeatmapTests : BunitContext
{
    private static readonly List<HeatmapData> TestData = new()
    {
        new("Mon", "9AM", 10),
        new("Mon", "12PM", 25),
        new("Tue", "9AM", 30),
        new("Tue", "12PM", 45),
        new("Wed", "9AM", 15),
        new("Wed", "12PM", 60),
    };

    [Fact]
    public void Renders_SvgWithCells()
    {
        var cut = Render<HelixHeatmap<HeatmapData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.XField, d => d.Day)
             .Add(c => c.YField, d => d.Hour)
             .Add(c => c.ValueField, d => d.Value));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("rect.arcadia-heatmap__cell").Count.Should().Be(6);
    }

    [Fact]
    public void Renders_ColorGradient()
    {
        var cut = Render<HelixHeatmap<HeatmapData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.XField, d => d.Day)
             .Add(c => c.YField, d => d.Hour)
             .Add(c => c.ValueField, d => d.Value));

        // Check that cells have fill attributes
        var cells = cut.FindAll("rect.arcadia-heatmap__cell");
        foreach (var cell in cells)
        {
            cell.GetAttribute("fill").Should().StartWith("#");
        }
    }

    [Fact]
    public void Renders_Title()
    {
        var cut = Render<HelixHeatmap<HeatmapData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.XField, d => d.Day)
             .Add(c => c.YField, d => d.Hour)
             .Add(c => c.ValueField, d => d.Value)
             .Add(c => c.Title, "Activity"));

        cut.Markup.Should().Contain("Activity");
    }

    [Fact]
    public void Renders_HiddenDataTable()
    {
        var cut = Render<HelixHeatmap<HeatmapData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.XField, d => d.Day)
             .Add(c => c.YField, d => d.Hour)
             .Add(c => c.ValueField, d => d.Value));

        cut.Find("table.arcadia-sr-only").Should().NotBeNull();
        cut.FindAll("table.arcadia-sr-only tbody tr").Count.Should().Be(6);
    }

    [Fact]
    public void NoData_RendersNothing()
    {
        var cut = Render<HelixHeatmap<HeatmapData>>(p =>
            p.Add(c => c.Data, new List<HeatmapData>())
             .Add(c => c.XField, d => d.Day)
             .Add(c => c.YField, d => d.Hour)
             .Add(c => c.ValueField, d => d.Value));

        cut.FindAll("svg").Count.Should().Be(0);
    }

    [Fact]
    public void Renders_ScaleLegend()
    {
        var cut = Render<HelixHeatmap<HeatmapData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.XField, d => d.Day)
             .Add(c => c.YField, d => d.Hour)
             .Add(c => c.ValueField, d => d.Value));

        // Should have a linearGradient for the legend
        cut.Find("linearGradient").Should().NotBeNull();
    }

    [Fact]
    public void Custom_Colors()
    {
        var cut = Render<HelixHeatmap<HeatmapData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.XField, d => d.Day)
             .Add(c => c.YField, d => d.Hour)
             .Add(c => c.ValueField, d => d.Value)
             .Add(c => c.LowColor, "#ffffff")
             .Add(c => c.HighColor, "#ff0000"));

        var stops = cut.FindAll("stop");
        stops[0].GetAttribute("stop-color").Should().Be("#ffffff");
        stops[1].GetAttribute("stop-color").Should().Be("#ff0000");
    }

    [Fact]
    public void Renders_CellTooltips()
    {
        var cut = Render<HelixHeatmap<HeatmapData>>(p =>
            p.Add(c => c.Data, TestData)
             .Add(c => c.XField, d => d.Day)
             .Add(c => c.YField, d => d.Hour)
             .Add(c => c.ValueField, d => d.Value));

        // SVG <title> elements are inside cells for native tooltips
        cut.FindAll("rect.arcadia-heatmap__cell title").Count.Should().Be(6);
    }
}

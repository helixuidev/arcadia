using Bunit;
using FluentAssertions;
using HelixUI.Charts.Components.Charts;
using HelixUI.Charts.Core;
using HelixUI.Charts.Core.Data;
using Xunit;

namespace HelixUI.Tests.Unit.Charts;

public record SalesData(string Month, double Revenue, double Cost);
public record BarData(string Category, double A, double B, double C);

public class TrendlineCalculatorTests
{
    [Fact]
    public void LinearRegression_ProducesCorrectSlope()
    {
        // Perfect linear data: y = 2x + 1
        var values = new List<double> { 1, 3, 5, 7, 9 };
        var result = TrendlineCalculator.LinearRegression(values);

        result.Length.Should().Be(5);
        result[0].Should().BeApproximately(1, 0.01);
        result[4].Should().BeApproximately(9, 0.01);
    }

    [Fact]
    public void LinearRegression_SinglePoint_ReturnsSame()
    {
        var values = new List<double> { 42 };
        var result = TrendlineCalculator.LinearRegression(values);

        result.Should().ContainSingle().Which.Should().Be(42);
    }

    [Fact]
    public void MovingAverage_CalculatesCorrectly()
    {
        var values = new List<double> { 10, 20, 30, 40, 50 };
        var result = TrendlineCalculator.MovingAverage(values, 3);

        result.Length.Should().Be(5);
        result[0].Should().BeApproximately(10, 0.01);   // Only 1 value
        result[1].Should().BeApproximately(15, 0.01);   // avg(10,20)
        result[2].Should().BeApproximately(20, 0.01);   // avg(10,20,30)
        result[3].Should().BeApproximately(30, 0.01);   // avg(20,30,40)
        result[4].Should().BeApproximately(40, 0.01);   // avg(30,40,50)
    }

    [Fact]
    public void MovingAverage_EmptyInput()
    {
        var result = TrendlineCalculator.MovingAverage(new List<double>(), 3);
        result.Should().BeEmpty();
    }

    [Fact]
    public void MovingAverage_PeriodLargerThanData()
    {
        var values = new List<double> { 10, 20 };
        var result = TrendlineCalculator.MovingAverage(values, 10);

        result.Length.Should().Be(2);
        result[0].Should().BeApproximately(10, 0.01);
        result[1].Should().BeApproximately(15, 0.01);
    }
}

public class TrendlineRenderingTests : BunitContext
{
    [Fact]
    public void LineChart_Renders_LinearTrendline()
    {
        var data = new List<SalesData>
        {
            new("Jan", 10, 5), new("Feb", 20, 8),
            new("Mar", 30, 12), new("Apr", 40, 15),
        };
        var series = new List<SeriesConfig<SalesData>>
        {
            new()
            {
                Name = "Revenue",
                Field = d => d.Revenue,
                Trendline = new TrendlineConfig { Type = TrendlineType.Linear }
            }
        };

        var cut = Render<HelixLineChart<SalesData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, d => (object)d.Month)
             .Add(c => c.Series, series));

        cut.FindAll("path.helix-chart__trendline").Count.Should().Be(1);
    }

    [Fact]
    public void LineChart_Renders_MovingAverageTrendline()
    {
        var data = new List<SalesData>
        {
            new("Jan", 10, 5), new("Feb", 20, 8),
            new("Mar", 15, 12), new("Apr", 25, 15),
            new("May", 30, 18),
        };
        var series = new List<SeriesConfig<SalesData>>
        {
            new()
            {
                Name = "Revenue",
                Field = d => d.Revenue,
                Trendline = new TrendlineConfig
                {
                    Type = TrendlineType.MovingAverage,
                    MovingAveragePeriod = 3
                }
            }
        };

        var cut = Render<HelixLineChart<SalesData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, d => (object)d.Month)
             .Add(c => c.Series, series));

        cut.FindAll("path.helix-chart__trendline").Count.Should().Be(1);
    }

    [Fact]
    public void ScatterChart_Renders_Trendline()
    {
        var data = Enumerable.Range(1, 10)
            .Select(i => new SalesData($"P{i}", i * 2.0, i * 3.0))
            .ToList();

        var cut = Render<HelixScatterChart<SalesData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, (Func<SalesData, double>)(d => d.Revenue))
             .Add(c => c.YField, (Func<SalesData, double>)(d => d.Cost))
             .Add(c => c.Trendline, new TrendlineConfig { Type = TrendlineType.Linear }));

        cut.FindAll("path.helix-chart__trendline").Count.Should().Be(1);
    }
}

public class StackedBarTests : BunitContext
{
    [Fact]
    public void StackedBar_Renders_StackedBars()
    {
        var data = new List<BarData>
        {
            new("Q1", 10, 20, 30),
            new("Q2", 15, 25, 35),
        };
        var series = new List<SeriesConfig<BarData>>
        {
            new() { Name = "A", Field = d => d.A },
            new() { Name = "B", Field = d => d.B },
            new() { Name = "C", Field = d => d.C },
        };

        var cut = Render<HelixBarChart<BarData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, d => (object)d.Category)
             .Add(c => c.Series, series)
             .Add(c => c.Stacked, true));

        // 2 data points * 3 series = 6 bars
        cut.FindAll("rect.helix-chart__bar").Count.Should().Be(6);
    }

    [Fact]
    public void NonStacked_Renders_GroupedBars()
    {
        var data = new List<BarData>
        {
            new("Q1", 10, 20, 30),
        };
        var series = new List<SeriesConfig<BarData>>
        {
            new() { Name = "A", Field = d => d.A },
            new() { Name = "B", Field = d => d.B },
        };

        var cut = Render<HelixBarChart<BarData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, d => (object)d.Category)
             .Add(c => c.Series, series)
             .Add(c => c.Stacked, false));

        cut.FindAll("rect.helix-chart__bar").Count.Should().Be(2);
    }
}

public class DataLabelTests : BunitContext
{
    [Fact]
    public void BarChart_ShowsDataLabels()
    {
        var data = new List<SalesData> { new("Jan", 100, 50) };
        var series = new List<SeriesConfig<SalesData>>
        {
            new() { Name = "Revenue", Field = d => d.Revenue },
        };

        var cut = Render<HelixBarChart<SalesData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, d => (object)d.Month)
             .Add(c => c.Series, series)
             .Add(c => c.ShowDataLabels, true));

        cut.FindAll(".helix-chart__data-label").Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void LineChart_ShowsDataLabels()
    {
        var data = new List<SalesData>
        {
            new("Jan", 10, 5), new("Feb", 20, 8), new("Mar", 30, 12),
        };
        var series = new List<SeriesConfig<SalesData>>
        {
            new() { Name = "Revenue", Field = d => d.Revenue },
        };

        var cut = Render<HelixLineChart<SalesData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, d => (object)d.Month)
             .Add(c => c.Series, series)
             .Add(c => c.ShowDataLabels, true));

        cut.FindAll(".helix-chart__data-label").Count.Should().Be(3);
    }

    [Fact]
    public void BarChart_FormatsDataLabels()
    {
        var data = new List<SalesData> { new("Jan", 1234.5, 50) };
        var series = new List<SeriesConfig<SalesData>>
        {
            new() { Name = "Revenue", Field = d => d.Revenue },
        };

        var cut = Render<HelixBarChart<SalesData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, d => (object)d.Month)
             .Add(c => c.Series, series)
             .Add(c => c.ShowDataLabels, true)
             .Add(c => c.DataLabelFormatString, "F0"));

        cut.Markup.Should().Contain("1234"); // F0 format uses banker's rounding
    }
}

public class AxisFormattingTests : BunitContext
{
    [Fact]
    public void LineChart_FormatsYAxis()
    {
        var data = new List<SalesData>
        {
            new("Jan", 0.5, 5), new("Feb", 0.75, 8), new("Mar", 1.0, 12),
        };
        var series = new List<SeriesConfig<SalesData>>
        {
            new() { Name = "Revenue", Field = d => d.Revenue },
        };

        var cut = Render<HelixLineChart<SalesData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, d => (object)d.Month)
             .Add(c => c.Series, series)
             .Add(c => c.YAxisFormatString, "P0"));

        // P0 format should produce percentage strings
        cut.Markup.Should().Contain("%");
    }

    [Fact]
    public void BarChart_FormatsYAxis()
    {
        var data = new List<SalesData>
        {
            new("Jan", 1000, 5), new("Feb", 2000, 8),
        };
        var series = new List<SeriesConfig<SalesData>>
        {
            new() { Name = "Revenue", Field = d => d.Revenue },
        };

        var cut = Render<HelixBarChart<SalesData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, d => (object)d.Month)
             .Add(c => c.Series, series)
             .Add(c => c.YAxisFormatString, "N0"));

        // N0 format should add thousands separator
        cut.Markup.Should().ContainAny("1,000", "2,000", "1000", "2000");
    }
}

public class NullHandlingTests : BunitContext
{
    [Fact]
    public void LineChart_DefaultGap_RendersMultipleSegments()
    {
        // Use NaN to represent null in a non-nullable double field
        var data = new List<SalesData>
        {
            new("Jan", 10, 5),
            new("Feb", 20, 8),
            new("Mar", double.NaN, 12), // "null" value
            new("Apr", 40, 15),
            new("May", 50, 18),
        };
        var series = new List<SeriesConfig<SalesData>>
        {
            new() { Name = "Revenue", Field = d => d.Revenue },
        };

        var cut = Render<HelixLineChart<SalesData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, d => (object)d.Month)
             .Add(c => c.Series, series)
             .Add(c => c.NullHandling, NullHandling.Gap));

        // Should have a path with gap (two M commands)
        var linePath = cut.Find("path.helix-chart__line");
        linePath.Should().NotBeNull();
        var d = linePath.GetAttribute("d")!;
        // Two segments means two M commands
        d.Split('M', StringSplitOptions.RemoveEmptyEntries).Length.Should().Be(2);
    }

    [Fact]
    public void LineChart_Zero_TreatsNullAsZero()
    {
        var data = new List<SalesData>
        {
            new("Jan", 10, 5),
            new("Feb", double.NaN, 8),
            new("Mar", 30, 12),
        };
        var series = new List<SeriesConfig<SalesData>>
        {
            new() { Name = "Revenue", Field = d => d.Revenue },
        };

        var cut = Render<HelixLineChart<SalesData>>(p =>
            p.Add(c => c.Data, data)
             .Add(c => c.XField, d => (object)d.Month)
             .Add(c => c.Series, series)
             .Add(c => c.NullHandling, NullHandling.Zero));

        // Should have a single continuous path (one M)
        var linePath = cut.Find("path.helix-chart__line");
        var d = linePath.GetAttribute("d")!;
        d.Split('M', StringSplitOptions.RemoveEmptyEntries).Length.Should().Be(1);
    }
}

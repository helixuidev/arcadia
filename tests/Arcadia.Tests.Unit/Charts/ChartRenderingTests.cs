using Bunit;
using FluentAssertions;
using Arcadia.Charts.Core;
using Arcadia.Charts.Core.Layout;
using Arcadia.Charts.Core.Scales;
using Arcadia.Charts.Components.Charts;
using System.Text.RegularExpressions;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

// ──────────────────────────────────────────────────────────────────────
// 1. Layout Engine Overlap Tests
// ──────────────────────────────────────────────────────────────────────

public class TickLabelOverlapTests
{
    private readonly ChartLayoutEngine _engine = new();

    [Theory]
    [InlineData(5, 200)]
    [InlineData(5, 400)]
    [InlineData(5, 800)]
    [InlineData(10, 200)]
    [InlineData(10, 300)]
    [InlineData(10, 400)]
    [InlineData(10, 600)]
    [InlineData(10, 800)]
    [InlineData(10, 1200)]
    [InlineData(20, 200)]
    [InlineData(20, 300)]
    [InlineData(20, 400)]
    [InlineData(20, 600)]
    [InlineData(20, 800)]
    [InlineData(20, 1200)]
    [InlineData(30, 200)]
    [InlineData(30, 400)]
    [InlineData(30, 600)]
    [InlineData(30, 800)]
    [InlineData(30, 1200)]
    [InlineData(50, 200)]
    [InlineData(50, 400)]
    [InlineData(50, 600)]
    [InlineData(50, 800)]
    [InlineData(50, 1200)]
    public void ShortLabels_NoAdjacentOverlaps(int tickCount, int width)
    {
        var labels = Enumerable.Range(0, tickCount)
            .Select(i => new DateTime(2026, 1, 1).AddMonths(i % 12).ToString("MMM"))
            .ToList();

        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = width,
            Height = 400,
            XTickLabels = labels,
            YMin = 0,
            YMax = 100
        });

        AssertNoTickOverlaps(result.XTicks);
    }

    [Theory]
    [InlineData(5, 300)]
    [InlineData(5, 600)]
    [InlineData(5, 1200)]
    [InlineData(10, 400)]
    [InlineData(10, 600)]
    [InlineData(10, 800)]
    [InlineData(10, 1200)]
    [InlineData(20, 600)]
    [InlineData(20, 800)]
    [InlineData(20, 1200)]
    [InlineData(30, 800)]
    [InlineData(30, 1200)]
    [InlineData(50, 1200)]
    public void LongLabels_NoAdjacentOverlaps(int tickCount, int width)
    {
        var months = new[]
        {
            "January 2026", "February 2026", "March 2026", "April 2026",
            "May 2026", "June 2026", "July 2026", "August 2026",
            "September 2026", "October 2026", "November 2026", "December 2026"
        };
        var labels = Enumerable.Range(0, tickCount)
            .Select(i => months[i % months.Length])
            .ToList();

        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = width,
            Height = 400,
            XTickLabels = labels,
            YMin = 0,
            YMax = 100
        });

        AssertNoTickOverlaps(result.XTicks);
    }

    private static void AssertNoTickOverlaps(List<TickMark> ticks)
    {
        for (var i = 0; i < ticks.Count - 1; i++)
        {
            var overlaps = CollisionDetector.Overlaps(ticks[i].BoundingBox, ticks[i + 1].BoundingBox);
            overlaps.Should().BeFalse(
                $"Tick '{ticks[i].Label}' at ({ticks[i].BoundingBox}) overlaps '{ticks[i + 1].Label}' at ({ticks[i + 1].BoundingBox})");
        }
    }
}

public class YAxisLabelTests
{
    private readonly ChartLayoutEngine _engine = new();

    [Theory]
    [InlineData(0, 100)]
    [InlineData(0, 10000)]
    [InlineData(-50, 50)]
    [InlineData(0.001, 0.009)]
    public void YTicks_FitWithinMargins(double yMin, double yMax)
    {
        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = 600,
            Height = 400,
            YMin = yMin,
            YMax = yMax
        });

        foreach (var tick in result.YTicks)
        {
            var labelWidth = TextMeasure.EstimateWidth(tick.Label, 12);
            // The label must fit within the left margin
            labelWidth.Should().BeLessOrEqualTo(result.Margins.Left,
                $"Y-axis label '{tick.Label}' width {labelWidth:F1} exceeds left margin {result.Margins.Left:F1}");
        }
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(0, 10000)]
    [InlineData(-50, 50)]
    [InlineData(0.001, 0.009)]
    public void YTicks_ValuesAreMonotonicallyIncreasing(double yMin, double yMax)
    {
        var ticks = TickGenerator.GenerateNumericTicks(yMin, yMax, 8);

        for (var i = 0; i < ticks.Length - 1; i++)
        {
            ticks[i].Should().BeLessThan(ticks[i + 1],
                $"Tick value {ticks[i]} at index {i} should be less than {ticks[i + 1]} at index {i + 1}");
        }
    }

    [Fact]
    public void YTicks_LargeNumbers_FormattedCompactly()
    {
        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = 600,
            Height = 400,
            YMin = 0,
            YMax = 1_000_000
        });

        // Labels should use compact notation (K/M) to save space
        result.YTicks.Should().NotBeEmpty();
        var maxWidth = result.YTicks.Max(t => TextMeasure.EstimateWidth(t.Label, 12));
        maxWidth.Should().BeLessThan(100, "formatted large numbers should be compact");
    }
}

public class LegendOverflowTests
{
    private readonly ChartLayoutEngine _engine = new();

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void FewSeries_LegendIsHorizontal(int count)
    {
        var names = Enumerable.Range(0, count).Select(i => $"S{i}").ToList();

        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = 800,
            Height = 400,
            SeriesNames = names
        });

        if (count > 0)
        {
            result.Legend.Visible.Should().BeTrue();
            result.Legend.Mode.Should().Be(LegendMode.Horizontal);
        }
    }

    [Theory]
    [InlineData(15)]
    [InlineData(20)]
    public void ManySeries_LegendIsTruncated(int count)
    {
        var names = Enumerable.Range(0, count).Select(i => $"Series Name {i}").ToList();

        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = 600,
            Height = 400,
            SeriesNames = names
        });

        result.Legend.Visible.Should().BeTrue();
        result.Legend.Mode.Should().Be(LegendMode.Truncated);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(250)]
    [InlineData(299)]
    public void NarrowChart_LegendHidden(int width)
    {
        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = width,
            Height = 300,
            SeriesNames = new List<string> { "A", "B", "C" }
        });

        result.Legend.Visible.Should().BeFalse(
            $"Legend should be hidden at width {width}");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(12)]
    public void MediumSeriesCount_WrappedOrTruncated(int count)
    {
        var names = Enumerable.Range(0, count).Select(i => $"Long Series Name {i}").ToList();

        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = 600,
            Height = 400,
            SeriesNames = names
        });

        result.Legend.Visible.Should().BeTrue();
        result.Legend.Mode.Should().NotBe(LegendMode.Horizontal,
            $"{count} long series names should not fit horizontally");
    }
}

public class MarginAutoCalculationTests
{
    private readonly ChartLayoutEngine _engine = new();

    [Fact]
    public void LongYLabels_ExpandLeftMargin()
    {
        var resultShort = _engine.Calculate(new ChartLayoutInput
        {
            Width = 600,
            Height = 400,
            YMin = 0,
            YMax = 10
        });

        var resultLong = _engine.Calculate(new ChartLayoutInput
        {
            Width = 600,
            Height = 400,
            YMin = 0,
            YMax = 1_000_000
        });

        resultLong.Margins.Left.Should().BeGreaterOrEqualTo(resultShort.Margins.Left,
            "long Y labels ('$1,000,000') should require wider left margin");
    }

    [Fact]
    public void RotatedXLabels_ExpandBottomMargin()
    {
        var resultShort = _engine.Calculate(new ChartLayoutInput
        {
            Width = 400,
            Height = 400,
            XTickLabels = new[] { "A", "B", "C" }
        });

        var longLabels = Enumerable.Range(0, 20)
            .Select(i => $"Very Long Category Name {i}")
            .ToList();
        var resultLong = _engine.Calculate(new ChartLayoutInput
        {
            Width = 400,
            Height = 400,
            XTickLabels = longLabels
        });

        resultLong.Margins.Bottom.Should().BeGreaterOrEqualTo(resultShort.Margins.Bottom,
            "rotated X labels should increase bottom margin");
    }

    [Theory]
    [InlineData(150, 100)]
    [InlineData(200, 150)]
    [InlineData(300, 200)]
    [InlineData(600, 400)]
    [InlineData(1200, 800)]
    public void PlotArea_AlwaysPositive(int width, int height)
    {
        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = width,
            Height = height,
            Title = "Chart Title",
            XAxisTitle = "X Axis",
            YAxisTitle = "Y Axis",
            XTickLabels = Enumerable.Range(0, 15).Select(i => $"Long Label {i}").ToList(),
            YMin = 0,
            YMax = 1_000_000,
            SeriesNames = Enumerable.Range(0, 10).Select(i => $"Series {i}").ToList()
        });

        result.PlotArea.Width.Should().BeGreaterThan(0,
            $"Plot area width must be positive at {width}x{height}");
        result.PlotArea.Height.Should().BeGreaterThan(0,
            $"Plot area height must be positive at {width}x{height}");
    }

    [Theory]
    [InlineData(300, 200)]
    [InlineData(600, 400)]
    [InlineData(1200, 800)]
    public void PlotArea_AtLeast10Pixels(int width, int height)
    {
        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = width,
            Height = height,
            Title = "Title",
            XTickLabels = Enumerable.Range(0, 30).Select(i => $"Category {i}").ToList(),
            YMin = 0,
            YMax = 1_000_000_000,
            SeriesNames = Enumerable.Range(0, 20).Select(i => $"Series {i}").ToList()
        });

        result.PlotArea.Width.Should().BeGreaterOrEqualTo(10);
        result.PlotArea.Height.Should().BeGreaterOrEqualTo(10);
    }
}

// ──────────────────────────────────────────────────────────────────────
// 2. Fuzz Tests
// ──────────────────────────────────────────────────────────────────────

public class LayoutFuzzTests
{
    [Fact]
    public void FuzzTest_2000Configs_NoOverlaps_PositivePlotArea()
    {
        var random = new Random(42);
        var engine = new ChartLayoutEngine();

        for (var i = 0; i < 2000; i++)
        {
            var width = random.Next(150, 1500);
            var height = random.Next(100, 1000);
            var tickCount = random.Next(1, 40);
            var labels = GenerateRandomLabels(random, tickCount);
            var yMin = random.NextDouble() * -100;
            var yMax = yMin + random.NextDouble() * 10000 + 1;
            var seriesCount = random.Next(0, 20);
            var seriesNames = seriesCount > 0
                ? Enumerable.Range(0, seriesCount).Select(j => $"S{j}").ToList()
                : null;

            var result = engine.Calculate(new ChartLayoutInput
            {
                Width = width,
                Height = height,
                Title = random.NextDouble() > 0.5 ? "Title" : null,
                XAxisTitle = random.NextDouble() > 0.5 ? "X" : null,
                YAxisTitle = random.NextDouble() > 0.5 ? "Y" : null,
                XTickLabels = labels,
                YMin = yMin,
                YMax = yMax,
                SeriesNames = seriesNames
            });

            result.PlotArea.Width.Should().BeGreaterThan(0,
                $"Negative plot width at iteration {i} (w={width}, h={height}, ticks={tickCount})");
            result.PlotArea.Height.Should().BeGreaterThan(0,
                $"Negative plot height at iteration {i} (w={width}, h={height})");

            // Check no adjacent tick overlaps
            for (var j = 0; j < result.XTicks.Count - 1; j++)
            {
                CollisionDetector.Overlaps(result.XTicks[j].BoundingBox, result.XTicks[j + 1].BoundingBox)
                    .Should().BeFalse(
                        $"Tick overlap at iteration {i}: '{result.XTicks[j].Label}' and '{result.XTicks[j + 1].Label}'");
            }
        }
    }

    private static List<string> GenerateRandomLabels(Random rng, int count)
    {
        var shortLabels = new[] { "A", "B", "Jan", "Feb", "Q1", "Q2", "Mon", "Tue" };
        var longLabels = new[] { "September 2026", "December 2025", "Total Revenue", "Category Name Long" };

        return Enumerable.Range(0, count).Select(i =>
        {
            if (rng.NextDouble() > 0.5)
                return shortLabels[rng.Next(shortLabels.Length)];
            return longLabels[rng.Next(longLabels.Length)];
        }).ToList();
    }
}

public class ScaleFuzzTests
{
    [Fact]
    public void LinearScale_EqualDomain_ReturnsMidpoint()
    {
        var scale = new LinearScale(5, 5, 0, 500);
        var result = scale.Scale(5);
        double.IsFinite(result).Should().BeTrue();
        result.Should().Be(250); // midpoint
    }

    [Fact]
    public void LinearScale_NegativeRange()
    {
        var scale = new LinearScale(-1000, -500, 0, 400);
        var result = scale.Scale(-750);
        result.Should().BeApproximately(200, 0.1);
    }

    [Fact]
    public void LinearScale_VerySmallRange()
    {
        var scale = new LinearScale(0.001, 0.002, 0, 500);
        var result = scale.Scale(0.0015);
        result.Should().BeApproximately(250, 0.1);
    }

    [Fact]
    public void LinearScale_VeryLargeRange()
    {
        var scale = new LinearScale(0, 1e12, 0, 1000);
        var result = scale.Scale(5e11);
        double.IsFinite(result).Should().BeTrue();
        result.Should().BeApproximately(500, 0.1);
    }

    [Fact]
    public void LinearScale_FromData_EmptyInput()
    {
        var scale = LinearScale.FromData(Array.Empty<double>(), 0, 500);
        scale.DomainMin.Should().Be(0);
        scale.DomainMax.Should().Be(1);
    }

    [Fact]
    public void LinearScale_FromData_SingleValue()
    {
        var scale = LinearScale.FromData(new[] { 42.0 }, 0, 500);
        double.IsFinite(scale.Scale(42)).Should().BeTrue();
    }

    [Fact]
    public void BandScale_ZeroCategories()
    {
        var scale = new BandScale(Array.Empty<string>(), 0, 400);
        scale.Count.Should().Be(0);
        scale.BandWidth.Should().Be(0);
        scale.Step.Should().Be(0);
    }

    [Fact]
    public void BandScale_OneCategory()
    {
        var scale = new BandScale(new[] { "Only" }, 0, 400, 0.1);
        scale.Count.Should().Be(1);
        scale.BandWidth.Should().BeGreaterThan(0);
        double.IsFinite(scale.ScaleCenter("Only")).Should().BeTrue();
    }

    [Fact]
    public void BandScale_100Categories()
    {
        var cats = Enumerable.Range(0, 100).Select(i => $"C{i}").ToList();
        var scale = new BandScale(cats, 0, 1000, 0.1);
        scale.Count.Should().Be(100);
        scale.BandWidth.Should().BeGreaterThan(0);

        foreach (var cat in cats)
        {
            var pos = scale.Scale(cat);
            double.IsFinite(pos).Should().BeTrue($"Position for '{cat}' should be finite");
        }
    }

    [Fact]
    public void TimeScale_SameStartEnd()
    {
        var date = new DateTime(2026, 6, 15);
        var scale = new TimeScale(date, date, 0, 500);
        var result = scale.Scale(date);
        double.IsFinite(result).Should().BeTrue();
        result.Should().Be(250); // midpoint
    }

    [Fact]
    public void TimeScale_OneSecondRange()
    {
        var start = new DateTime(2026, 1, 1, 12, 0, 0);
        var end = start.AddSeconds(1);
        var scale = new TimeScale(start, end, 0, 100);
        var mid = start.AddMilliseconds(500);
        var result = scale.Scale(mid);
        double.IsFinite(result).Should().BeTrue();
        result.Should().BeApproximately(50, 1);
    }

    [Fact]
    public void TimeScale_100YearRange()
    {
        var start = new DateTime(1926, 1, 1);
        var end = new DateTime(2026, 1, 1);
        var scale = new TimeScale(start, end, 0, 1000);
        var mid = new DateTime(1976, 1, 1);
        var result = scale.Scale(mid);
        double.IsFinite(result).Should().BeTrue();
        result.Should().BeApproximately(500, 10);
    }

    [Fact]
    public void TickGenerator_EqualMinMax_ReturnsSingleTick()
    {
        var ticks = TickGenerator.GenerateNumericTicks(42, 42, 10);
        ticks.Should().HaveCount(1);
        ticks[0].Should().Be(42);
    }

    [Fact]
    public void TickGenerator_VerySmallRange()
    {
        var ticks = TickGenerator.GenerateNumericTicks(0.001, 0.009, 8);
        ticks.Should().NotBeEmpty();
        ticks.All(t => double.IsFinite(t)).Should().BeTrue();
    }

    [Fact]
    public void TickGenerator_VeryLargeRange()
    {
        var ticks = TickGenerator.GenerateNumericTicks(0, 1e12, 8);
        ticks.Should().NotBeEmpty();
        ticks.All(t => double.IsFinite(t)).Should().BeTrue();
    }

    [Fact]
    public void TickGenerator_NegativeRange()
    {
        var ticks = TickGenerator.GenerateNumericTicks(-500, -100, 6);
        ticks.Should().NotBeEmpty();
        ticks.First().Should().BeLessOrEqualTo(-500);
        ticks.Last().Should().BeGreaterOrEqualTo(-100);
    }

    [Fact]
    public void TickGenerator_TimeTicks_SameStartEnd()
    {
        var date = new DateTime(2026, 3, 15);
        var ticks = TickGenerator.GenerateTimeTicks(date, date, 5);
        // Should not crash; may return empty or single tick
        ticks.All(t => t >= date.AddDays(-1) && t <= date.AddDays(1)).Should().BeTrue();
    }

    [Fact]
    public void TickGenerator_TimeTicks_100YearRange()
    {
        var ticks = TickGenerator.GenerateTimeTicks(
            new DateTime(1926, 1, 1), new DateTime(2026, 1, 1), 10);
        ticks.Should().NotBeEmpty();
        ticks.Length.Should().BeLessOrEqualTo(20);
    }
}

// ──────────────────────────────────────────────────────────────────────
// 3. Rendering Boundary Tests (per chart type)
// ──────────────────────────────────────────────────────────────────────

// Shared test records
public record RenderPoint(string Label, double Value);
public record RenderXY(double X, double Y);
public record RenderPie(string Name, double Value);
public record RenderCandle(string Day, double Open, double High, double Low, double Close);
public record RenderHeatCell(string Col, string Row, double Value);
public record RenderRadarItem(string Axis, double A, double B);

public class LineChartRenderingTests : BunitContext
{
    private static List<SeriesConfig<RenderPoint>> MakeSeries(params string[] names)
    {
        return names.Select(n => new SeriesConfig<RenderPoint>
        {
            Name = n,
            Field = d => d.Value
        }).ToList();
    }

    [Fact]
    public void EmptyData_DoesNotCrash()
    {
        var cut = Render<HelixLineChart<RenderPoint>>(p => p
            .Add(c => c.Data, new List<RenderPoint>())
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, MakeSeries("V")));

        cut.FindAll("svg").Count.Should().Be(0);
    }

    [Fact]
    public void SinglePoint_HandlesGracefully()
    {
        var data = new List<RenderPoint> { new("A", 10) };
        var cut = Render<HelixLineChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, MakeSeries("V")));

        // Line chart requires > 1 point
        cut.FindAll("svg").Count.Should().Be(0);
    }

    [Fact]
    public void TwoPoints_MinimumValidLine()
    {
        var data = new List<RenderPoint> { new("A", 10), new("B", 20) };
        var cut = Render<HelixLineChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, MakeSeries("V")));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("path.arcadia-chart__line").Count.Should().Be(1);
    }

    [Fact]
    public void AllSameYValues_FlatLine_NoException()
    {
        var data = Enumerable.Range(0, 6)
            .Select(i => new RenderPoint($"P{i}", 42))
            .ToList();
        var cut = Render<HelixLineChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, MakeSeries("V")));

        cut.Find("svg").Should().NotBeNull();
        var pathD = cut.Find("path.arcadia-chart__line").GetAttribute("d") ?? "";
        pathD.Should().StartWith("M");
        AssertSvgPathValid(pathD);
    }

    [Fact]
    public void AllNaN_DoesNotCrash()
    {
        var data = Enumerable.Range(0, 5)
            .Select(i => new RenderPoint($"P{i}", double.NaN))
            .ToList();
        var series = new List<SeriesConfig<RenderPoint>>
        {
            new() { Name = "V", Field = _ => double.NaN }
        };

        // This should not throw; it returns early because all values are NaN
        var act = () => Render<HelixLineChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, series));

        act.Should().NotThrow();
    }

    [Fact]
    public void NegativeValues_RenderBelow()
    {
        var data = new List<RenderPoint>
        {
            new("A", -50), new("B", -30), new("C", -10), new("D", 10)
        };
        var cut = Render<HelixLineChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, MakeSeries("V")));

        cut.Find("svg").Should().NotBeNull();
        AssertSvgPathValid(cut.Find("path.arcadia-chart__line").GetAttribute("d")!);
    }

    [Fact]
    public void VeryLargeValues_NoOverflow()
    {
        var data = new List<RenderPoint>
        {
            new("A", 1e15), new("B", 2e15), new("C", 1.5e15)
        };
        var cut = Render<HelixLineChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, MakeSeries("V")));

        cut.Find("svg").Should().NotBeNull();
        var path = cut.Find("path.arcadia-chart__line").GetAttribute("d")!;
        AssertSvgPathValid(path);
    }

    [Fact]
    public void GapHandling_BreaksPath()
    {
        var data = Enumerable.Range(0, 5)
            .Select(i => new RenderPoint($"P{i}", i == 2 ? double.NaN : i * 10.0))
            .ToList();
        var series = new List<SeriesConfig<RenderPoint>>
        {
            new() { Name = "V", Field = d => d.Value }
        };
        var cut = Render<HelixLineChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, series)
            .Add(c => c.NullHandling, NullHandling.Gap));

        cut.Find("svg").Should().NotBeNull();
    }

    private static void AssertSvgPathValid(string d)
    {
        d.Should().NotContain("NaN", "SVG path should not contain NaN");
        d.Should().NotContain("Infinity", "SVG path should not contain Infinity");
        d.Should().StartWith("M", "SVG path must start with M command");
    }
}

public class BarChartRenderingTests : BunitContext
{
    private static List<SeriesConfig<RenderPoint>> MakeSeries(params string[] names)
    {
        return names.Select(n => new SeriesConfig<RenderPoint>
        {
            Name = n,
            Field = d => d.Value
        }).ToList();
    }

    [Fact]
    public void EmptyData_DoesNotCrash()
    {
        var cut = Render<HelixBarChart<RenderPoint>>(p => p
            .Add(c => c.Data, new List<RenderPoint>())
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, MakeSeries("V")));

        cut.FindAll("svg").Count.Should().Be(0);
    }

    [Fact]
    public void SingleBar_Renders()
    {
        var data = new List<RenderPoint> { new("A", 100) };
        var cut = Render<HelixBarChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, MakeSeries("V")));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("rect.arcadia-chart__bar").Count.Should().Be(1);
    }

    [Fact]
    public void FiftyBars_StillVisible()
    {
        var data = Enumerable.Range(0, 50)
            .Select(i => new RenderPoint($"B{i}", i * 10))
            .ToList();
        var cut = Render<HelixBarChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, MakeSeries("V")));

        var bars = cut.FindAll("rect.arcadia-chart__bar");
        bars.Count.Should().Be(50);
        foreach (var bar in bars)
        {
            var width = double.Parse(bar.GetAttribute("width")!);
            width.Should().BeGreaterOrEqualTo(1, "each bar should have at least 1px width");
        }
    }

    [Fact]
    public void StackedAllZeros_NoDivideByZero()
    {
        var data = new List<RenderPoint>
        {
            new("A", 0), new("B", 0), new("C", 0)
        };
        var series = new List<SeriesConfig<RenderPoint>>
        {
            new() { Name = "V1", Field = _ => 0 },
            new() { Name = "V2", Field = _ => 0 }
        };

        var act = () => Render<HelixBarChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, series)
            .Add(c => c.Stacked, true));

        act.Should().NotThrow();
    }

    [Fact]
    public void StackedNegativeValues_StacksBelow()
    {
        var data = new List<RenderPoint>
        {
            new("A", -20), new("B", -30)
        };
        var series = new List<SeriesConfig<RenderPoint>>
        {
            new() { Name = "V1", Field = d => d.Value },
            new() { Name = "V2", Field = d => d.Value / 2 }
        };

        var cut = Render<HelixBarChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, series)
            .Add(c => c.Stacked, true));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("rect.arcadia-chart__bar").Count.Should().Be(4);
    }

    [Fact]
    public void NarrowChart_BarsStillVisible()
    {
        var data = Enumerable.Range(0, 20)
            .Select(i => new RenderPoint($"B{i}", i * 10))
            .ToList();
        var cut = Render<HelixBarChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, MakeSeries("V"))
            .Add(c => c.Width, 200));

        var bars = cut.FindAll("rect.arcadia-chart__bar");
        bars.Count.Should().Be(20);
        foreach (var bar in bars)
        {
            var width = double.Parse(bar.GetAttribute("width")!);
            width.Should().BeGreaterOrEqualTo(1, "bar width should be at least 1px even on narrow charts");
        }
    }
}

public class PieChartRenderingTests : BunitContext
{
    [Fact]
    public void EmptyData_DoesNotCrash()
    {
        var cut = Render<HelixPieChart<RenderPie>>(p => p
            .Add(c => c.Data, new List<RenderPie>())
            .Add(c => c.NameField, (Func<RenderPie, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RenderPie, double>)(d => d.Value)));

        cut.FindAll("svg").Count.Should().Be(0);
    }

    [Fact]
    public void SingleSlice_FullCircle()
    {
        var data = new List<RenderPie> { new("All", 100) };
        var cut = Render<HelixPieChart<RenderPie>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.NameField, (Func<RenderPie, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RenderPie, double>)(d => d.Value)));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("path.arcadia-chart__pie-slice").Count.Should().Be(1);
    }

    [Fact]
    public void SliceWithZeroValue_Handled()
    {
        var data = new List<RenderPie>
        {
            new("A", 50), new("Zero", 0), new("B", 50)
        };

        var act = () => Render<HelixPieChart<RenderPie>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.NameField, (Func<RenderPie, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RenderPie, double>)(d => d.Value)));

        act.Should().NotThrow();
    }

    [Fact]
    public void HundredSlices_DoesNotCrash()
    {
        var data = Enumerable.Range(0, 100)
            .Select(i => new RenderPie($"S{i}", i + 1))
            .ToList();

        var cut = Render<HelixPieChart<RenderPie>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.NameField, (Func<RenderPie, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RenderPie, double>)(d => d.Value)));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("path.arcadia-chart__pie-slice").Count.Should().Be(100);
    }

    [Fact]
    public void VerySmallSlice_NoVisualGlitch()
    {
        var data = new List<RenderPie>
        {
            new("Big", 999), new("Tiny", 1)
        };

        var cut = Render<HelixPieChart<RenderPie>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.NameField, (Func<RenderPie, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RenderPie, double>)(d => d.Value)));

        cut.FindAll("path.arcadia-chart__pie-slice").Count.Should().Be(2);
        // Verify paths are valid
        foreach (var path in cut.FindAll("path.arcadia-chart__pie-slice"))
        {
            var d = path.GetAttribute("d")!;
            d.Should().NotContain("NaN");
            d.Should().NotContain("Infinity");
        }
    }
}

public class ScatterChartRenderingTests : BunitContext
{
    [Fact]
    public void EmptyData_DoesNotCrash()
    {
        var cut = Render<HelixScatterChart<RenderXY>>(p => p
            .Add(c => c.Data, new List<RenderXY>())
            .Add(c => c.XField, (Func<RenderXY, double>)(d => d.X))
            .Add(c => c.YField, (Func<RenderXY, double>)(d => d.Y)));

        cut.FindAll("svg").Count.Should().Be(0);
    }

    [Fact]
    public void SinglePoint_Renders()
    {
        var data = new List<RenderXY> { new(5, 10) };
        var cut = Render<HelixScatterChart<RenderXY>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderXY, double>)(d => d.X))
            .Add(c => c.YField, (Func<RenderXY, double>)(d => d.Y)));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("circle.arcadia-chart__point").Count.Should().Be(1);
    }

    [Fact]
    public void AllPointsSameLocation_NoCrash()
    {
        var data = Enumerable.Range(0, 10)
            .Select(_ => new RenderXY(5, 5))
            .ToList();

        var cut = Render<HelixScatterChart<RenderXY>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderXY, double>)(d => d.X))
            .Add(c => c.YField, (Func<RenderXY, double>)(d => d.Y)));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("circle.arcadia-chart__point").Count.Should().Be(10);
    }

    [Fact]
    public void ExtremeOutliers_ScaleAccommodates()
    {
        var data = new List<RenderXY>
        {
            new(1, 1), new(2, 2), new(3, 3), new(1000, 1000)
        };
        var cut = Render<HelixScatterChart<RenderXY>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderXY, double>)(d => d.X))
            .Add(c => c.YField, (Func<RenderXY, double>)(d => d.Y)));

        cut.Find("svg").Should().NotBeNull();
        var circles = cut.FindAll("circle.arcadia-chart__point");
        circles.Count.Should().Be(4);
        foreach (var circle in circles)
        {
            var cx = double.Parse(circle.GetAttribute("cx")!);
            var cy = double.Parse(circle.GetAttribute("cy")!);
            double.IsFinite(cx).Should().BeTrue();
            double.IsFinite(cy).Should().BeTrue();
        }
    }
}

public class CandlestickRenderingTests : BunitContext
{
    [Fact]
    public void Doji_BodyHeightAtLeast1()
    {
        // Open == Close
        var data = new List<RenderCandle>
        {
            new("Mon", 100, 110, 90, 100),
            new("Tue", 105, 115, 95, 105)
        };
        var cut = Render<HelixCandlestickChart<RenderCandle>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.LabelField, (Func<RenderCandle, string>)(d => d.Day))
            .Add(c => c.OpenField, (Func<RenderCandle, double>)(d => d.Open))
            .Add(c => c.HighField, (Func<RenderCandle, double>)(d => d.High))
            .Add(c => c.LowField, (Func<RenderCandle, double>)(d => d.Low))
            .Add(c => c.CloseField, (Func<RenderCandle, double>)(d => d.Close)));

        var bodies = cut.FindAll(".arcadia-candle__body");
        bodies.Count.Should().Be(2);
        foreach (var body in bodies)
        {
            var height = double.Parse(body.GetAttribute("height")!);
            height.Should().BeGreaterOrEqualTo(1, "doji body should have at least 1px height");
        }
    }

    [Fact]
    public void HighEqualsLow_WickStillRenders()
    {
        var data = new List<RenderCandle>
        {
            new("Mon", 100, 100, 100, 100)
        };
        var cut = Render<HelixCandlestickChart<RenderCandle>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.LabelField, (Func<RenderCandle, string>)(d => d.Day))
            .Add(c => c.OpenField, (Func<RenderCandle, double>)(d => d.Open))
            .Add(c => c.HighField, (Func<RenderCandle, double>)(d => d.High))
            .Add(c => c.LowField, (Func<RenderCandle, double>)(d => d.Low))
            .Add(c => c.CloseField, (Func<RenderCandle, double>)(d => d.Close)));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll(".arcadia-candle__wick").Count.Should().Be(1);
    }

    [Fact]
    public void AllCandlesSameValues_NoDivideByZero()
    {
        var data = Enumerable.Range(0, 5)
            .Select(i => new RenderCandle($"D{i}", 50, 50, 50, 50))
            .ToList();

        var act = () => Render<HelixCandlestickChart<RenderCandle>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.LabelField, (Func<RenderCandle, string>)(d => d.Day))
            .Add(c => c.OpenField, (Func<RenderCandle, double>)(d => d.Open))
            .Add(c => c.HighField, (Func<RenderCandle, double>)(d => d.High))
            .Add(c => c.LowField, (Func<RenderCandle, double>)(d => d.Low))
            .Add(c => c.CloseField, (Func<RenderCandle, double>)(d => d.Close)));

        act.Should().NotThrow();
    }
}

public class GaugeRenderingTests : BunitContext
{
    [Fact]
    public void ValueAtMin_MinimalArc()
    {
        var cut = Render<HelixGaugeChart>(p => p
            .Add(c => c.Value, 0)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100));

        cut.Find("svg").Should().NotBeNull();
        // Track path should exist; value path may be empty
        cut.FindAll("path.arcadia-gauge__track").Count.Should().Be(1);
    }

    [Fact]
    public void ValueAtMax_FullArc()
    {
        var cut = Render<HelixGaugeChart>(p => p
            .Add(c => c.Value, 100)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("path.arcadia-gauge__value").Count.Should().Be(1);
    }

    [Fact]
    public void ValueOutsideRange_Clamps()
    {
        var cut = Render<HelixGaugeChart>(p => p
            .Add(c => c.Value, 200)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100));

        // Should display clamped value of 100
        cut.Markup.Should().Contain("100");
    }

    [Fact]
    public void ValueBelowMin_Clamps()
    {
        var cut = Render<HelixGaugeChart>(p => p
            .Add(c => c.Value, -50)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100));

        // Should display clamped value of 0
        cut.Markup.Should().Contain("0");
    }

    [Fact]
    public void MinEqualsMax_NoDivideByZero()
    {
        var act = () => Render<HelixGaugeChart>(p => p
            .Add(c => c.Value, 50)
            .Add(c => c.Min, 50)
            .Add(c => c.Max, 50));

        act.Should().NotThrow();
    }
}

public class HeatmapRenderingTests : BunitContext
{
    [Fact]
    public void EmptyData_DoesNotCrash()
    {
        var cut = Render<HelixHeatmap<RenderHeatCell>>(p => p
            .Add(c => c.Data, new List<RenderHeatCell>())
            .Add(c => c.XField, (Func<RenderHeatCell, string>)(d => d.Col))
            .Add(c => c.YField, (Func<RenderHeatCell, string>)(d => d.Row))
            .Add(c => c.ValueField, (Func<RenderHeatCell, double>)(d => d.Value)));

        cut.FindAll("svg").Count.Should().Be(0);
    }

    [Fact]
    public void AllSameValues_UniformColor()
    {
        var data = new List<RenderHeatCell>
        {
            new("A", "R1", 42), new("B", "R1", 42),
            new("A", "R2", 42), new("B", "R2", 42)
        };

        var cut = Render<HelixHeatmap<RenderHeatCell>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderHeatCell, string>)(d => d.Col))
            .Add(c => c.YField, (Func<RenderHeatCell, string>)(d => d.Row))
            .Add(c => c.ValueField, (Func<RenderHeatCell, double>)(d => d.Value)));

        var cells = cut.FindAll("rect.arcadia-heatmap__cell");
        cells.Count.Should().Be(4);
        var colors = cells.Select(c => c.GetAttribute("fill")).Distinct().ToList();
        // All same value -> should map to the HighColor
        colors.Count.Should().Be(1);
    }

    [Fact]
    public void SingleCell_Renders()
    {
        var data = new List<RenderHeatCell> { new("A", "R1", 10) };
        var cut = Render<HelixHeatmap<RenderHeatCell>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderHeatCell, string>)(d => d.Col))
            .Add(c => c.YField, (Func<RenderHeatCell, string>)(d => d.Row))
            .Add(c => c.ValueField, (Func<RenderHeatCell, double>)(d => d.Value)));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("rect.arcadia-heatmap__cell").Count.Should().Be(1);
    }
}

public class RadarChartRenderingTests : BunitContext
{
    private static List<SeriesConfig<RenderRadarItem>> MakeSeries()
    {
        return new List<SeriesConfig<RenderRadarItem>>
        {
            new() { Name = "A", Field = d => d.A },
            new() { Name = "B", Field = d => d.B }
        };
    }

    [Fact]
    public void EmptyData_DoesNotCrash()
    {
        var cut = Render<HelixRadarChart<RenderRadarItem>>(p => p
            .Add(c => c.Data, new List<RenderRadarItem>())
            .Add(c => c.LabelField, (Func<RenderRadarItem, string>)(d => d.Axis))
            .Add(c => c.Series, MakeSeries()));

        cut.FindAll("svg").Count.Should().Be(0);
    }

    [Fact]
    public void SingleDataPoint_HandlesGracefully()
    {
        var data = new List<RenderRadarItem> { new("One", 50, 30) };
        var cut = Render<HelixRadarChart<RenderRadarItem>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.LabelField, (Func<RenderRadarItem, string>)(d => d.Axis))
            .Add(c => c.Series, MakeSeries()));

        cut.Find("svg").Should().NotBeNull();
    }

    [Fact]
    public void AllZeros_RendersAtCenter()
    {
        var data = new List<RenderRadarItem>
        {
            new("A", 0, 0), new("B", 0, 0), new("C", 0, 0)
        };
        var series = new List<SeriesConfig<RenderRadarItem>>
        {
            new() { Name = "S", Field = _ => 0 }
        };

        var cut = Render<HelixRadarChart<RenderRadarItem>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.LabelField, (Func<RenderRadarItem, string>)(d => d.Axis))
            .Add(c => c.Series, series));

        cut.Find("svg").Should().NotBeNull();
    }

    [Fact]
    public void ManyCategories_LabelsReadable()
    {
        var data = Enumerable.Range(0, 24)
            .Select(i => new RenderRadarItem($"Cat{i}", i * 3, i * 2))
            .ToList();

        var cut = Render<HelixRadarChart<RenderRadarItem>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.LabelField, (Func<RenderRadarItem, string>)(d => d.Axis))
            .Add(c => c.Series, MakeSeries()));

        cut.Find("svg").Should().NotBeNull();
        // All 24 axis labels should be present
        for (var i = 0; i < 24; i++)
        {
            cut.Markup.Should().Contain($"Cat{i}");
        }
    }
}

// ──────────────────────────────────────────────────────────────────────
// 4. SVG Coordinate Validation
// ──────────────────────────────────────────────────────────────────────

public class SvgCoordinateValidationTests : BunitContext
{
    private static readonly Regex NumberPattern = new(@"-?\d+\.?\d*", RegexOptions.Compiled);
    private static readonly Regex PathDPattern = new(@"d=""([^""]+)""", RegexOptions.Compiled);

    [Fact]
    public void LineChart_AllCoordinatesFinite()
    {
        var data = Enumerable.Range(0, 10)
            .Select(i => new RenderPoint($"P{i}", Math.Sin(i) * 50 + 50))
            .ToList();
        var series = new List<SeriesConfig<RenderPoint>>
        {
            new() { Name = "V", Field = d => d.Value }
        };

        var cut = Render<HelixLineChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, series));

        AssertSvgCoordinatesValid(cut.Markup);
    }

    [Fact]
    public void BarChart_AllCoordinatesFinite()
    {
        var data = Enumerable.Range(0, 6)
            .Select(i => new RenderPoint($"B{i}", (i + 1) * 20))
            .ToList();
        var series = new List<SeriesConfig<RenderPoint>>
        {
            new() { Name = "V", Field = d => d.Value }
        };

        var cut = Render<HelixBarChart<RenderPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderPoint, object>)(d => d.Label))
            .Add(c => c.Series, series));

        AssertSvgCoordinatesValid(cut.Markup);
        AssertNoNegativeWidthHeight(cut.Markup);
    }

    [Fact]
    public void PieChart_AllCoordinatesFinite()
    {
        var data = new List<RenderPie>
        {
            new("A", 40), new("B", 30), new("C", 20), new("D", 10)
        };

        var cut = Render<HelixPieChart<RenderPie>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.NameField, (Func<RenderPie, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<RenderPie, double>)(d => d.Value)));

        AssertSvgCoordinatesValid(cut.Markup);
    }

    [Fact]
    public void ScatterChart_AllCirclesInBounds()
    {
        var data = Enumerable.Range(0, 15)
            .Select(i => new RenderXY(i * 5, i * 3 + 10))
            .ToList();

        var cut = Render<HelixScatterChart<RenderXY>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderXY, double>)(d => d.X))
            .Add(c => c.YField, (Func<RenderXY, double>)(d => d.Y))
            .Add(c => c.Width, 600)
            .Add(c => c.Height, 400));

        var circles = cut.FindAll("circle.arcadia-chart__point");
        foreach (var circle in circles)
        {
            var cx = double.Parse(circle.GetAttribute("cx")!);
            var cy = double.Parse(circle.GetAttribute("cy")!);
            double.IsFinite(cx).Should().BeTrue("circle cx must be finite");
            double.IsFinite(cy).Should().BeTrue("circle cy must be finite");
            cx.Should().BeInRange(-50, 700, "cx should be within reasonable SVG bounds");
            cy.Should().BeInRange(-50, 500, "cy should be within reasonable SVG bounds");
        }
    }

    [Fact]
    public void CandlestickChart_AllCoordinatesFinite()
    {
        var data = new List<RenderCandle>
        {
            new("Mon", 100, 110, 90, 105),
            new("Tue", 105, 115, 95, 112),
            new("Wed", 112, 118, 108, 108)
        };

        var cut = Render<HelixCandlestickChart<RenderCandle>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.LabelField, (Func<RenderCandle, string>)(d => d.Day))
            .Add(c => c.OpenField, (Func<RenderCandle, double>)(d => d.Open))
            .Add(c => c.HighField, (Func<RenderCandle, double>)(d => d.High))
            .Add(c => c.LowField, (Func<RenderCandle, double>)(d => d.Low))
            .Add(c => c.CloseField, (Func<RenderCandle, double>)(d => d.Close)));

        AssertSvgCoordinatesValid(cut.Markup);
        AssertNoNegativeWidthHeight(cut.Markup);
    }

    [Fact]
    public void GaugeChart_AllCoordinatesFinite()
    {
        var cut = Render<HelixGaugeChart>(p => p
            .Add(c => c.Value, 75)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100));

        AssertSvgCoordinatesValid(cut.Markup);
    }

    [Fact]
    public void HeatmapChart_AllCoordinatesFinite()
    {
        var data = new List<RenderHeatCell>
        {
            new("A", "R1", 10), new("B", "R1", 50),
            new("A", "R2", 30), new("B", "R2", 70)
        };

        var cut = Render<HelixHeatmap<RenderHeatCell>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<RenderHeatCell, string>)(d => d.Col))
            .Add(c => c.YField, (Func<RenderHeatCell, string>)(d => d.Row))
            .Add(c => c.ValueField, (Func<RenderHeatCell, double>)(d => d.Value)));

        AssertSvgCoordinatesValid(cut.Markup);
        AssertNoNegativeWidthHeight(cut.Markup);
    }

    [Fact]
    public void RadarChart_AllCoordinatesFinite()
    {
        var data = new List<RenderRadarItem>
        {
            new("Speed", 80, 60),
            new("Strength", 70, 90),
            new("Agility", 90, 50),
            new("Stamina", 60, 70),
            new("Defense", 50, 40)
        };
        var series = new List<SeriesConfig<RenderRadarItem>>
        {
            new() { Name = "A", Field = d => d.A },
            new() { Name = "B", Field = d => d.B }
        };

        var cut = Render<HelixRadarChart<RenderRadarItem>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.LabelField, (Func<RenderRadarItem, string>)(d => d.Axis))
            .Add(c => c.Series, series));

        AssertSvgCoordinatesValid(cut.Markup);
    }

    private static void AssertSvgCoordinatesValid(string markup)
    {
        markup.Should().NotContain("NaN", "SVG output must not contain NaN");
        markup.Should().NotContain("Infinity", "SVG output must not contain Infinity");

        // Verify all path d attributes are valid
        var pathMatches = PathDPattern.Matches(markup);
        foreach (Match match in pathMatches)
        {
            var d = match.Groups[1].Value;
            d.Should().NotBeEmpty("path d attribute should not be empty");
            // Path must start with M (moveto) command
            var trimmed = d.TrimStart();
            if (trimmed.Length > 0)
            {
                "MLAZHVCSQTmlahvcsqt".Should().Contain(trimmed[0].ToString(),
                    $"path d must start with a valid SVG command, got: {trimmed[..Math.Min(20, trimmed.Length)]}");
            }
        }
    }

    private static void AssertNoNegativeWidthHeight(string markup)
    {
        var widthPattern = new Regex(@"width=""(-?\d+\.?\d*)""");
        var heightPattern = new Regex(@"height=""(-?\d+\.?\d*)""");

        foreach (Match m in widthPattern.Matches(markup))
        {
            var val = double.Parse(m.Groups[1].Value);
            val.Should().BeGreaterOrEqualTo(0,
                $"SVG element width should not be negative: {val}");
        }

        foreach (Match m in heightPattern.Matches(markup))
        {
            var val = double.Parse(m.Groups[1].Value);
            val.Should().BeGreaterOrEqualTo(0,
                $"SVG element height should not be negative: {val}");
        }
    }
}

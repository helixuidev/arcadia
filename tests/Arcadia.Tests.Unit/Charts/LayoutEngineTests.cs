using FluentAssertions;
using Arcadia.Charts.Core.Layout;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public class TickGeneratorTests
{
    [Fact]
    public void GenerateNumericTicks_ProducesNiceNumbers()
    {
        var ticks = TickGenerator.GenerateNumericTicks(0, 100, 6);

        ticks.Should().Contain(0);
        ticks.Should().Contain(100);
        ticks.All(t => t % 10 == 0 || t % 20 == 0 || t % 25 == 0 || t % 50 == 0).Should().BeTrue();
    }

    [Fact]
    public void GenerateNumericTicks_RespectsMaxTicks()
    {
        var ticks = TickGenerator.GenerateNumericTicks(0, 1000, 5);

        ticks.Length.Should().BeLessOrEqualTo(7); // May slightly exceed due to rounding
    }

    [Fact]
    public void GenerateTimeTicks_Years()
    {
        var ticks = TickGenerator.GenerateTimeTicks(
            new DateTime(2020, 1, 1), new DateTime(2030, 1, 1), 6);

        ticks.Length.Should().BeInRange(3, 12);
        ticks.All(t => t.Month == 1 && t.Day == 1).Should().BeTrue();
    }

    [Fact]
    public void GenerateTimeTicks_Days()
    {
        var ticks = TickGenerator.GenerateTimeTicks(
            new DateTime(2026, 3, 1), new DateTime(2026, 3, 14), 7);

        ticks.Length.Should().BeInRange(3, 15);
    }
}

public class CollisionDetectorTests
{
    [Fact]
    public void Overlaps_OverlappingBoxes_ReturnsTrue()
    {
        var a = new LabelBox(0, 0, 50, 20);
        var b = new LabelBox(40, 0, 50, 20);

        CollisionDetector.Overlaps(a, b).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_NonOverlapping_ReturnsFalse()
    {
        var a = new LabelBox(0, 0, 50, 20);
        var b = new LabelBox(60, 0, 50, 20);

        CollisionDetector.Overlaps(a, b).Should().BeFalse();
    }

    [Fact]
    public void HasOverlaps_ListWithOverlap_ReturnsTrue()
    {
        var boxes = new List<LabelBox>
        {
            new(0, 0, 50, 20),
            new(40, 0, 50, 20),
            new(100, 0, 50, 20)
        };

        CollisionDetector.HasOverlaps(boxes).Should().BeTrue();
    }

    [Fact]
    public void HasOverlaps_NoOverlap_ReturnsFalse()
    {
        var boxes = new List<LabelBox>
        {
            new(0, 0, 30, 20),
            new(40, 0, 30, 20),
            new(80, 0, 30, 20)
        };

        CollisionDetector.HasOverlaps(boxes).Should().BeFalse();
    }
}

public class TextMeasureTests
{
    [Fact]
    public void EstimateWidth_LongerText_WiderResult()
    {
        var short_ = TextMeasure.EstimateWidth("Hi", 12);
        var long_ = TextMeasure.EstimateWidth("Hello World", 12);

        long_.Should().BeGreaterThan(short_);
    }

    [Fact]
    public void EstimateWidth_Null_ReturnsZero()
    {
        TextMeasure.EstimateWidth(null).Should().Be(0);
    }

    [Fact]
    public void EstimateRotated_90Degrees_SwapsDimensions()
    {
        var (w0, h0) = TextMeasure.EstimateRotated("Test", 12, 0);
        var (w90, h90) = TextMeasure.EstimateRotated("Test", 12, 90);

        w90.Should().BeLessThan(w0); // Rotated text is narrower
        h90.Should().BeGreaterThan(h0); // But taller
    }
}

public class ChartLayoutEngineTests
{
    private readonly ChartLayoutEngine _engine = new();

    [Fact]
    public void Calculate_ReturnsPositivePlotArea()
    {
        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = 600,
            Height = 400,
            XTickLabels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
            YMin = 0,
            YMax = 100
        });

        result.PlotArea.Width.Should().BeGreaterThan(0);
        result.PlotArea.Height.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(800, ResponsiveTier.Wide)]
    [InlineData(500, ResponsiveTier.Medium)]
    [InlineData(350, ResponsiveTier.Narrow)]
    [InlineData(200, ResponsiveTier.Compact)]
    public void Calculate_ResponsiveTier(double width, ResponsiveTier expected)
    {
        var result = _engine.Calculate(new ChartLayoutInput { Width = width, Height = 300 });

        result.Tier.Should().Be(expected);
    }

    [Fact]
    public void Calculate_NarrowChart_ReducesOrRotatesLabels()
    {
        var allLabels = Enumerable.Range(0, 12).Select(i => new DateTime(2026, i + 1, 1).ToString("MMMM")).ToList();
        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = 300,
            Height = 300,
            XTickLabels = allLabels
        });

        // At narrow width, engine should either rotate or reduce labels
        var reducedLabels = result.XTicks.Count < allLabels.Count;
        var rotated = result.TickLabelRotation > 0;
        (reducedLabels || rotated).Should().BeTrue("narrow chart should reduce or rotate labels");
    }

    [Fact]
    public void Calculate_WideChart_NoRotation()
    {
        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = 1200,
            Height = 400,
            XTickLabels = new[] { "A", "B", "C", "D" }
        });

        result.TickLabelRotation.Should().Be(0);
    }

    [Fact]
    public void Calculate_ManySeries_TruncatesLegend()
    {
        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = 600,
            Height = 400,
            SeriesNames = Enumerable.Range(0, 20).Select(i => $"Series {i}").ToList()
        });

        result.Legend.Visible.Should().BeTrue();
        result.Legend.Mode.Should().Be(LegendMode.Truncated);
    }

    [Fact]
    public void Calculate_NarrowChart_HidesLegend()
    {
        var result = _engine.Calculate(new ChartLayoutInput
        {
            Width = 250,
            Height = 300,
            SeriesNames = new[] { "A", "B" }
        });

        result.Legend.Visible.Should().BeFalse();
    }

    [Fact]
    public void FuzzTest_NoNegativePlotArea()
    {
        var random = new Random(42);
        for (var i = 0; i < 500; i++)
        {
            var width = random.Next(150, 1200);
            var height = random.Next(100, 800);
            var tickCount = random.Next(2, 30);
            var labels = Enumerable.Range(0, tickCount).Select(j => $"Label {j}").ToList();

            var result = _engine.Calculate(new ChartLayoutInput
            {
                Width = width,
                Height = height,
                XTickLabels = labels,
                YMin = 0,
                YMax = random.Next(10, 10000),
                SeriesNames = Enumerable.Range(0, random.Next(1, 10)).Select(j => $"S{j}").ToList()
            });

            result.PlotArea.Width.Should().BeGreaterThan(0, $"width={width}, ticks={tickCount}");
            result.PlotArea.Height.Should().BeGreaterThan(0, $"height={height}");
        }
    }
}

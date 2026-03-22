using FluentAssertions;
using Arcadia.Charts.Core.Scales;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public class LinearScaleTests
{
    [Fact]
    public void Scale_MapsToRange()
    {
        var scale = new LinearScale(0, 100, 0, 500);

        scale.Scale(0).Should().Be(0);
        scale.Scale(50).Should().Be(250);
        scale.Scale(100).Should().Be(500);
    }

    [Fact]
    public void Invert_MapsBack()
    {
        var scale = new LinearScale(0, 100, 0, 500);

        scale.Invert(250).Should().Be(50);
    }

    [Fact]
    public void Scale_HandlesNegativeRange()
    {
        var scale = new LinearScale(-50, 50, 0, 200);

        scale.Scale(0).Should().Be(100);
    }

    [Fact]
    public void FromData_IncludesZero()
    {
        var scale = LinearScale.FromData(new[] { 10.0, 20.0, 30.0 }, 0, 100, includeZero: true);

        scale.DomainMin.Should().Be(0);
    }

    [Fact]
    public void FromData_AddsPadding()
    {
        var scale = LinearScale.FromData(new[] { 10.0, 20.0 }, 0, 100);

        scale.DomainMin.Should().BeLessThan(10);
        scale.DomainMax.Should().BeGreaterThan(20);
    }
}

public class BandScaleTests
{
    [Fact]
    public void BandWidth_CalculatesCorrectly()
    {
        var scale = new BandScale(new[] { "A", "B", "C" }, 0, 300, padding: 0.1);

        scale.BandWidth.Should().BeApproximately(90, 1); // 300/3 * 0.9
    }

    [Fact]
    public void ScaleCenter_MiddleOfBand()
    {
        var scale = new BandScale(new[] { "A", "B" }, 0, 200, padding: 0);

        scale.ScaleCenter("A").Should().BeApproximately(50, 1);
        scale.ScaleCenter("B").Should().BeApproximately(150, 1);
    }

    [Fact]
    public void Count_ReturnsCategories()
    {
        var scale = new BandScale(new[] { "X", "Y", "Z" }, 0, 100);

        scale.Count.Should().Be(3);
    }
}

public class TimeScaleTests
{
    [Fact]
    public void Scale_MapsTimeToPixels()
    {
        var min = new DateTime(2026, 1, 1);
        var max = new DateTime(2026, 12, 31);
        var scale = new TimeScale(min, max, 0, 365);

        var midPoint = scale.Scale(new DateTime(2026, 7, 1));
        midPoint.Should().BeInRange(170, 195); // Roughly half
    }

    [Fact]
    public void Invert_MapsBackToTime()
    {
        var min = new DateTime(2026, 1, 1);
        var max = new DateTime(2026, 12, 31);
        var scale = new TimeScale(min, max, 0, 100);

        var result = scale.Invert(50);
        result.Should().BeAfter(new DateTime(2026, 6, 1));
        result.Should().BeBefore(new DateTime(2026, 8, 1));
    }
}

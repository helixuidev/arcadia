using FluentAssertions;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public class LogScaleTests
{
    [Fact]
    public void Scale_MapsMinToRangeMin()
    {
        var scale = new Arcadia.Charts.Core.Scales.LogarithmicScale(1, 1000, 0, 300);
        scale.Scale(1).Should().BeApproximately(0, 0.01);
    }

    [Fact]
    public void Scale_MapsMaxToRangeMax()
    {
        var scale = new Arcadia.Charts.Core.Scales.LogarithmicScale(1, 1000, 0, 300);
        scale.Scale(1000).Should().BeApproximately(300, 0.01);
    }

    [Fact]
    public void Scale_MapsMidpointCorrectly()
    {
        // log10(1)=0, log10(1000)=3, log10(~31.6)=1.5 => midpoint
        var scale = new Arcadia.Charts.Core.Scales.LogarithmicScale(1, 1000, 0, 300);
        var mid = scale.Scale(Math.Sqrt(1000)); // ~31.6
        mid.Should().BeApproximately(150, 1);
    }

    [Fact]
    public void Invert_RoundTrips()
    {
        var scale = new Arcadia.Charts.Core.Scales.LogarithmicScale(10, 10000, 0, 400);
        var pixel = scale.Scale(500);
        var value = scale.Invert(pixel);
        value.Should().BeApproximately(500, 1);
    }

    [Fact]
    public void FromData_CreatesNiceBounds()
    {
        var data = new[] { 3.0, 50, 800 };
        var scale = Arcadia.Charts.Core.Scales.LogarithmicScale.FromData(data, 0, 300);
        scale.DomainMin.Should().Be(1);     // floor(log10(3)) = 0 => 10^0 = 1
        scale.DomainMax.Should().Be(1000);  // ceil(log10(800)) = 3 => 10^3 = 1000
    }

    [Fact]
    public void FromData_HandlesEmptyList()
    {
        var scale = Arcadia.Charts.Core.Scales.LogarithmicScale.FromData(Array.Empty<double>(), 0, 300);
        scale.DomainMin.Should().Be(1);
        scale.DomainMax.Should().Be(1000);
    }

    [Fact]
    public void GenerateTicks_ReturnsNicePowerOf10Values()
    {
        var ticks = Arcadia.Charts.Core.Scales.LogarithmicScale.GenerateTicks(1, 10000, 20);
        ticks.Should().Contain(1);
        ticks.Should().Contain(10);
        ticks.Should().Contain(100);
        ticks.Should().Contain(1000);
        ticks.Should().Contain(10000);
    }

    [Fact]
    public void GenerateTicks_IncludesSubdivisions()
    {
        var ticks = Arcadia.Charts.Core.Scales.LogarithmicScale.GenerateTicks(1, 100, 20);
        ticks.Should().Contain(2);
        ticks.Should().Contain(5);
        ticks.Should().Contain(10);
        ticks.Should().Contain(20);
        ticks.Should().Contain(50);
    }

    [Fact]
    public void Scale_ClampsNegativeValues()
    {
        var scale = new Arcadia.Charts.Core.Scales.LogarithmicScale(1, 1000, 0, 300);
        // Negative values should clamp to DomainMin (1), so scale to 0
        scale.Scale(-5).Should().BeApproximately(0, 0.01);
    }

    [Fact]
    public void Scale_ClampsZeroValues()
    {
        var scale = new Arcadia.Charts.Core.Scales.LogarithmicScale(1, 1000, 0, 300);
        scale.Scale(0).Should().BeApproximately(0, 0.01);
    }
}

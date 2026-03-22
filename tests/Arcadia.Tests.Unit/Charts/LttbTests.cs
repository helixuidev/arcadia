using FluentAssertions;
using Arcadia.Charts.Core.Data;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public record LttbPoint(double X, double Y);

public class LttbTests
{
    [Fact]
    public void Downsample_FewerThanTarget_ReturnsAll()
    {
        var data = new[] { new LttbPoint(0, 1), new LttbPoint(1, 2), new LttbPoint(2, 3) };
        var result = LttbDownsampler.Downsample(data, 10, d => d.X, d => d.Y);
        result.Should().HaveCount(3);
    }

    [Fact]
    public void Downsample_ReducesToTarget()
    {
        var data = Enumerable.Range(0, 1000).Select(i => new LttbPoint(i, Math.Sin(i * 0.1))).ToList();
        var result = LttbDownsampler.Downsample(data, 100, d => d.X, d => d.Y);
        result.Should().HaveCount(100);
    }

    [Fact]
    public void Downsample_KeepsFirstAndLast()
    {
        var data = Enumerable.Range(0, 100).Select(i => new LttbPoint(i, i * 2.0)).ToList();
        var result = LttbDownsampler.Downsample(data, 10, d => d.X, d => d.Y);
        result[0].X.Should().Be(0);
        result[^1].X.Should().Be(99);
    }

    [Fact]
    public void Downsample_PreservesPeaks()
    {
        var data = Enumerable.Range(0, 100).Select(i => new LttbPoint(i, i == 50 ? 100.0 : 10.0)).ToList();
        var result = LttbDownsampler.Downsample(data, 20, d => d.X, d => d.Y);
        result.Should().Contain(d => d.Y == 100.0);
    }

    [Fact]
    public void Downsample_TargetTwo_ReturnsFirstAndLast()
    {
        var data = new[] { new LttbPoint(0, 1), new LttbPoint(1, 2), new LttbPoint(2, 3), new LttbPoint(3, 4), new LttbPoint(4, 5) };
        var result = LttbDownsampler.Downsample(data, 2, d => d.X, d => d.Y);
        result.Should().HaveCount(2);
        result[0].Should().Be(data[0]);
        result[1].Should().Be(data[^1]);
    }
}

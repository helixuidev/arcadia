namespace Arcadia.Charts.Core.Scales;

/// <summary>
/// Maps a DateTime range to a pixel range.
/// </summary>
public class TimeScale
{
    private readonly LinearScale _inner;
    public DateTime DomainMin { get; }
    public DateTime DomainMax { get; }

    public TimeScale(DateTime domainMin, DateTime domainMax, double rangeMin, double rangeMax)
    {
        DomainMin = domainMin;
        DomainMax = domainMax;
        _inner = new LinearScale(domainMin.Ticks, domainMax.Ticks, rangeMin, rangeMax);
    }

    /// <summary>
    /// Maps a DateTime to a pixel position.
    /// </summary>
    public double Scale(DateTime value) => _inner.Scale(value.Ticks);

    /// <summary>
    /// Maps a pixel position back to a DateTime.
    /// </summary>
    public DateTime Invert(double pixel) => new((long)_inner.Invert(pixel));

    /// <summary>
    /// Creates a time scale from data with padding.
    /// </summary>
    public static TimeScale FromData(IEnumerable<DateTime> values, double rangeMin, double rangeMax)
    {
        var list = values.ToList();
        if (list.Count == 0)
            return new TimeScale(DateTime.Now.AddDays(-30), DateTime.Now, rangeMin, rangeMax);

        var min = list.Min();
        var max = list.Max();
        var padding = TimeSpan.FromTicks((long)((max - min).Ticks * 0.02));

        return new TimeScale(min - padding, max + padding, rangeMin, rangeMax);
    }
}

namespace Arcadia.Charts.Core.Scales;

/// <summary>
/// Maps a continuous numeric data range to a pixel range.
/// </summary>
public class LinearScale
{
    public double DomainMin { get; }
    public double DomainMax { get; }
    public double RangeMin { get; }
    public double RangeMax { get; }

    public LinearScale(double domainMin, double domainMax, double rangeMin, double rangeMax)
    {
        DomainMin = domainMin;
        DomainMax = domainMax;
        RangeMin = rangeMin;
        RangeMax = rangeMax;
    }

    /// <summary>
    /// Maps a data value to a pixel position.
    /// </summary>
    public double Scale(double value)
    {
        if (Math.Abs(DomainMax - DomainMin) < double.Epsilon)
            return (RangeMin + RangeMax) / 2;

        var normalized = (value - DomainMin) / (DomainMax - DomainMin);
        return RangeMin + normalized * (RangeMax - RangeMin);
    }

    /// <summary>
    /// Maps a pixel position back to a data value.
    /// </summary>
    public double Invert(double pixel)
    {
        if (Math.Abs(RangeMax - RangeMin) < double.Epsilon)
            return (DomainMin + DomainMax) / 2;

        var normalized = (pixel - RangeMin) / (RangeMax - RangeMin);
        return DomainMin + normalized * (DomainMax - DomainMin);
    }

    /// <summary>
    /// Creates a scale with a nice padded domain (adds ~5% padding on each side).
    /// </summary>
    public static LinearScale FromData(IEnumerable<double> values, double rangeMin, double rangeMax, bool includeZero = false)
    {
        var list = values.ToList();
        if (list.Count == 0)
            return new LinearScale(0, 1, rangeMin, rangeMax);

        var min = list.Min();
        var max = list.Max();

        if (includeZero)
        {
            if (min > 0) min = 0;
            if (max < 0) max = 0;
        }

        var padding = (max - min) * 0.05;
        if (Math.Abs(padding) < double.Epsilon) padding = 1;

        return new LinearScale(
            includeZero && min >= 0 ? 0 : min - padding,
            max + padding,
            rangeMin, rangeMax);
    }
}

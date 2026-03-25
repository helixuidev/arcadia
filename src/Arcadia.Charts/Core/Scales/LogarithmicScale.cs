namespace Arcadia.Charts.Core.Scales;

/// <summary>
/// Maps a continuous numeric data range to a pixel range using a logarithmic scale.
/// Values must be positive (log of zero/negative is undefined).
/// </summary>
internal class LogarithmicScale
{
    public double DomainMin { get; }
    public double DomainMax { get; }
    public double RangeMin { get; }
    public double RangeMax { get; }

    private readonly double _logMin;
    private readonly double _logMax;

    public LogarithmicScale(double domainMin, double domainMax, double rangeMin, double rangeMax)
    {
        // Clamp to positive values for log safety
        DomainMin = domainMin > 0 ? domainMin : 1;
        DomainMax = domainMax > DomainMin ? domainMax : DomainMin * 10;
        RangeMin = rangeMin;
        RangeMax = rangeMax;
        _logMin = Math.Log10(DomainMin);
        _logMax = Math.Log10(DomainMax);
    }

    /// <summary>
    /// Maps a data value to a pixel position using log10 scale.
    /// </summary>
    public double Scale(double value)
    {
        if (value <= 0) value = DomainMin;
        if (Math.Abs(_logMax - _logMin) < double.Epsilon)
            return (RangeMin + RangeMax) / 2;

        var logValue = Math.Log10(value);
        var normalized = (logValue - _logMin) / (_logMax - _logMin);
        return RangeMin + normalized * (RangeMax - RangeMin);
    }

    /// <summary>
    /// Maps a pixel position back to a data value.
    /// </summary>
    public double Invert(double pixel)
    {
        if (Math.Abs(RangeMax - RangeMin) < double.Epsilon)
            return Math.Pow(10, (_logMin + _logMax) / 2);

        var normalized = (pixel - RangeMin) / (RangeMax - RangeMin);
        var logValue = _logMin + normalized * (_logMax - _logMin);
        return Math.Pow(10, logValue);
    }

    /// <summary>
    /// Creates a log scale from data values with nice power-of-10 bounds.
    /// </summary>
    public static LogarithmicScale FromData(IEnumerable<double> values, double rangeMin, double rangeMax)
    {
        var list = values.Where(v => v > 0).ToList();
        if (list.Count == 0)
            return new LogarithmicScale(1, 1000, rangeMin, rangeMax);

        var min = list.Min();
        var max = list.Max();

        // Round to nice power-of-10 bounds
        var logMin = Math.Floor(Math.Log10(min));
        var logMax = Math.Ceiling(Math.Log10(max));
        if (logMin == logMax) logMax = logMin + 1;

        return new LogarithmicScale(
            Math.Pow(10, logMin),
            Math.Pow(10, logMax),
            rangeMin, rangeMax);
    }

    /// <summary>
    /// Generates nice log-scale tick values (powers of 10 and subdivisions).
    /// </summary>
    public static List<double> GenerateTicks(double domainMin, double domainMax, int maxTicks = 10)
    {
        var ticks = new List<double>();
        if (domainMin <= 0) domainMin = 1;
        if (domainMax <= domainMin) domainMax = domainMin * 10;

        var logMin = Math.Floor(Math.Log10(domainMin));
        var logMax = Math.Ceiling(Math.Log10(domainMax));

        for (var power = (int)logMin; power <= (int)logMax && ticks.Count < maxTicks; power++)
        {
            var value = Math.Pow(10, power);
            if (value >= domainMin && value <= domainMax)
                ticks.Add(value);

            // Add subdivisions (2, 5) if there's room
            if (ticks.Count < maxTicks - 2)
            {
                var sub2 = 2 * value;
                var sub5 = 5 * value;
                if (sub2 >= domainMin && sub2 <= domainMax) ticks.Add(sub2);
                if (sub5 >= domainMin && sub5 <= domainMax) ticks.Add(sub5);
            }
        }

        ticks.Sort();
        return ticks;
    }
}

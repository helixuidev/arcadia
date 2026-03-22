namespace Arcadia.Charts.Core.Layout;

/// <summary>
/// Generates "nice" tick values for axes using an extended Wilkinson algorithm.
/// Produces round numbers (1, 2, 5, 10, 20, 50, 100, etc.) that look natural.
/// </summary>
public static class TickGenerator
{
    private static readonly double[] NiceNumbers = { 1, 2, 2.5, 5, 10 };

    /// <summary>
    /// Generates nice tick values for a numeric range.
    /// </summary>
    /// <param name="min">The data minimum.</param>
    /// <param name="max">The data maximum.</param>
    /// <param name="maxTicks">The maximum number of ticks to generate.</param>
    /// <returns>An array of tick values.</returns>
    public static double[] GenerateNumericTicks(double min, double max, int maxTicks = 10)
    {
        if (maxTicks < 2) maxTicks = 2;
        if (Math.Abs(max - min) < double.Epsilon)
            return new[] { min };

        var range = max - min;
        var roughStep = range / (maxTicks - 1);
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughStep)));

        // Find the nicest step size
        var bestStep = magnitude;
        foreach (var nice in NiceNumbers)
        {
            var candidate = nice * magnitude;
            if (range / candidate <= maxTicks)
            {
                bestStep = candidate;
                break;
            }
        }

        var niceMin = Math.Floor(min / bestStep) * bestStep;
        var niceMax = Math.Ceiling(max / bestStep) * bestStep;

        var ticks = new List<double>();
        for (var v = niceMin; v <= niceMax + bestStep * 0.001; v += bestStep)
        {
            ticks.Add(Math.Round(v, 10)); // Avoid floating-point drift
        }

        return ticks.ToArray();
    }

    /// <summary>
    /// Generates tick values for a time range, choosing appropriate intervals
    /// (hours, days, weeks, months, years) based on the span.
    /// </summary>
    public static DateTime[] GenerateTimeTicks(DateTime min, DateTime max, int maxTicks = 10)
    {
        var span = max - min;
        var ticks = new List<DateTime>();

        if (span.TotalDays > 365 * 5)
        {
            // Years
            var startYear = min.Year;
            var endYear = max.Year;
            var yearStep = Math.Max(1, (endYear - startYear) / maxTicks);
            for (var y = startYear; y <= endYear; y += yearStep)
                ticks.Add(new DateTime(y, 1, 1));
        }
        else if (span.TotalDays > 60)
        {
            // Months
            var current = new DateTime(min.Year, min.Month, 1);
            var monthStep = Math.Max(1, (int)(span.TotalDays / 30 / maxTicks));
            while (current <= max)
            {
                ticks.Add(current);
                current = current.AddMonths(monthStep);
            }
        }
        else if (span.TotalDays > 2)
        {
            // Days
            var dayStep = Math.Max(1, (int)(span.TotalDays / maxTicks));
            var current = min.Date;
            while (current <= max)
            {
                ticks.Add(current);
                current = current.AddDays(dayStep);
            }
        }
        else
        {
            // Hours
            var hourStep = Math.Max(1, (int)(span.TotalHours / maxTicks));
            var current = new DateTime(min.Year, min.Month, min.Day, min.Hour, 0, 0);
            while (current <= max)
            {
                ticks.Add(current);
                current = current.AddHours(hourStep);
            }
        }

        return ticks.ToArray();
    }
}

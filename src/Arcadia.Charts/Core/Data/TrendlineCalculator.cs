namespace Arcadia.Charts.Core.Data;

/// <summary>
/// Calculates trendline data points for chart overlays.
/// </summary>
public static class TrendlineCalculator
{
    /// <summary>
    /// Calculates linear regression (least squares) for a set of y-values.
    /// Returns predicted y-values for each x position.
    /// </summary>
    /// <param name="yValues">The data values.</param>
    /// <returns>Predicted y-values from the linear regression line.</returns>
    public static double[] LinearRegression(IReadOnlyList<double> yValues)
    {
        if (yValues.Count < 2)
            return yValues.ToArray();

        var n = yValues.Count;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

        for (var i = 0; i < n; i++)
        {
            sumX += i;
            sumY += yValues[i];
            sumXY += i * yValues[i];
            sumX2 += (double)i * i;
        }

        var denominator = n * sumX2 - sumX * sumX;
        if (Math.Abs(denominator) < double.Epsilon)
            return yValues.ToArray();

        var slope = (n * sumXY - sumX * sumY) / denominator;
        var intercept = (sumY - slope * sumX) / n;

        var result = new double[n];
        for (var i = 0; i < n; i++)
        {
            result[i] = slope * i + intercept;
        }

        return result;
    }

    /// <summary>
    /// Calculates a simple moving average over the given window period.
    /// Points with insufficient history use whatever data is available.
    /// </summary>
    /// <param name="yValues">The data values.</param>
    /// <param name="period">The moving average window size.</param>
    /// <returns>Moving average values, one per input data point.</returns>
    public static double[] MovingAverage(IReadOnlyList<double> yValues, int period)
    {
        if (yValues.Count == 0)
            return Array.Empty<double>();

        period = Math.Max(1, Math.Min(period, yValues.Count));
        var result = new double[yValues.Count];

        for (var i = 0; i < yValues.Count; i++)
        {
            var start = Math.Max(0, i - period + 1);
            var count = i - start + 1;
            double sum = 0;
            for (var j = start; j <= i; j++)
                sum += yValues[j];
            result[i] = sum / count;
        }

        return result;
    }
}

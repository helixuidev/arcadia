namespace Arcadia.Charts.Core.Indicators;

/// <summary>
/// Base class for financial technical indicators.
/// </summary>
public abstract class TechnicalIndicator
{
    /// <summary>Calculate indicator values from close prices.</summary>
    public abstract double[] Calculate(double[] closePrices);
}

/// <summary>
/// Simple Moving Average (SMA).
/// </summary>
public class SimpleMovingAverage : TechnicalIndicator
{
    /// <summary>Number of periods.</summary>
    public int Period { get; set; } = 20;

    /// <inheritdoc />
    public override double[] Calculate(double[] closePrices)
    {
        var result = new double[closePrices.Length];
        for (var i = 0; i < closePrices.Length; i++)
        {
            if (i < Period - 1) { result[i] = double.NaN; continue; }
            var sum = 0.0;
            for (var j = i - Period + 1; j <= i; j++) sum += closePrices[j];
            result[i] = sum / Period;
        }
        return result;
    }
}

/// <summary>
/// Exponential Moving Average (EMA).
/// </summary>
public class ExponentialMovingAverage : TechnicalIndicator
{
    /// <summary>Number of periods.</summary>
    public int Period { get; set; } = 20;

    /// <inheritdoc />
    public override double[] Calculate(double[] closePrices)
    {
        var result = new double[closePrices.Length];
        var multiplier = 2.0 / (Period + 1);

        // First EMA value is SMA
        var sum = 0.0;
        for (var i = 0; i < Math.Min(Period, closePrices.Length); i++) sum += closePrices[i];

        for (var i = 0; i < closePrices.Length; i++)
        {
            if (i < Period - 1) { result[i] = double.NaN; continue; }
            if (i == Period - 1) { result[i] = sum / Period; continue; }
            result[i] = (closePrices[i] - result[i - 1]) * multiplier + result[i - 1];
        }
        return result;
    }
}

/// <summary>
/// Bollinger Bands (upper, middle, lower).
/// </summary>
public class BollingerBands : TechnicalIndicator
{
    /// <summary>SMA period.</summary>
    public int Period { get; set; } = 20;

    /// <summary>Standard deviation multiplier.</summary>
    public double StdDevMultiplier { get; set; } = 2.0;

    /// <summary>Upper band values.</summary>
    public double[] Upper { get; private set; } = Array.Empty<double>();

    /// <summary>Lower band values.</summary>
    public double[] Lower { get; private set; } = Array.Empty<double>();

    /// <inheritdoc />
    public override double[] Calculate(double[] closePrices)
    {
        var middle = new SimpleMovingAverage { Period = Period }.Calculate(closePrices);
        Upper = new double[closePrices.Length];
        Lower = new double[closePrices.Length];

        for (var i = 0; i < closePrices.Length; i++)
        {
            if (double.IsNaN(middle[i])) { Upper[i] = Lower[i] = double.NaN; continue; }

            var sumSqDiff = 0.0;
            for (var j = i - Period + 1; j <= i; j++)
            {
                var diff = closePrices[j] - middle[i];
                sumSqDiff += diff * diff;
            }
            var stdDev = Math.Sqrt(sumSqDiff / Period);
            Upper[i] = middle[i] + StdDevMultiplier * stdDev;
            Lower[i] = middle[i] - StdDevMultiplier * stdDev;
        }

        return middle; // middle band
    }
}

/// <summary>
/// Relative Strength Index (RSI).
/// </summary>
public class RelativeStrengthIndex : TechnicalIndicator
{
    /// <summary>Number of periods.</summary>
    public int Period { get; set; } = 14;

    /// <inheritdoc />
    public override double[] Calculate(double[] closePrices)
    {
        var result = new double[closePrices.Length];
        if (closePrices.Length < Period + 1)
        {
            Array.Fill(result, double.NaN);
            return result;
        }

        var gains = 0.0;
        var losses = 0.0;

        for (var i = 1; i <= Period; i++)
        {
            var change = closePrices[i] - closePrices[i - 1];
            if (change >= 0) gains += change; else losses -= change;
        }

        var avgGain = gains / Period;
        var avgLoss = losses / Period;

        for (var i = 0; i < Period; i++) result[i] = double.NaN;
        result[Period] = avgLoss == 0 ? 100 : 100 - 100 / (1 + avgGain / avgLoss);

        for (var i = Period + 1; i < closePrices.Length; i++)
        {
            var change = closePrices[i] - closePrices[i - 1];
            var gain = change >= 0 ? change : 0;
            var loss = change < 0 ? -change : 0;

            avgGain = (avgGain * (Period - 1) + gain) / Period;
            avgLoss = (avgLoss * (Period - 1) + loss) / Period;

            result[i] = avgLoss == 0 ? 100 : 100 - 100 / (1 + avgGain / avgLoss);
        }

        return result;
    }
}

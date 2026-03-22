namespace Arcadia.Charts.Core.Data;

/// <summary>
/// Largest Triangle Three Buckets (LTTB) downsampling algorithm.
/// Reduces data point count while preserving visual shape — keeps peaks and valleys.
/// </summary>
public static class LttbDownsampler
{
    /// <summary>
    /// Downsamples a data series to the target number of points.
    /// If the data already has fewer points, returns it unchanged.
    /// </summary>
    /// <param name="data">The source data points.</param>
    /// <param name="targetCount">The desired number of output points.</param>
    /// <param name="xSelector">Function to extract X value (index/time).</param>
    /// <param name="ySelector">Function to extract Y value.</param>
    /// <returns>A downsampled subset of the original data.</returns>
    public static List<T> Downsample<T>(IReadOnlyList<T> data, int targetCount, Func<T, double> xSelector, Func<T, double> ySelector)
    {
        if (data.Count <= targetCount)
            return data.ToList();

        if (targetCount < 3)
            return new List<T> { data[0], data[^1] };

        var result = new List<T>(targetCount) { data[0] }; // Always keep first

        var bucketSize = (double)(data.Count - 2) / (targetCount - 2);

        var prevIndex = 0;
        for (var i = 0; i < targetCount - 2; i++)
        {
            var bucketStart = (int)Math.Floor((i + 0) * bucketSize) + 1;
            var bucketEnd = (int)Math.Floor((i + 1) * bucketSize) + 1;
            bucketEnd = Math.Min(bucketEnd, data.Count - 1);

            // Calculate average point of next bucket (for triangle area calculation)
            var nextBucketStart = (int)Math.Floor((i + 1) * bucketSize) + 1;
            var nextBucketEnd = (int)Math.Floor((i + 2) * bucketSize) + 1;
            nextBucketEnd = Math.Min(nextBucketEnd, data.Count - 1);

            var avgX = 0.0;
            var avgY = 0.0;
            var nextCount = nextBucketEnd - nextBucketStart;
            if (nextCount > 0)
            {
                for (var j = nextBucketStart; j < nextBucketEnd; j++)
                {
                    avgX += xSelector(data[j]);
                    avgY += ySelector(data[j]);
                }
                avgX /= nextCount;
                avgY /= nextCount;
            }
            else
            {
                avgX = xSelector(data[^1]);
                avgY = ySelector(data[^1]);
            }

            // Find the point in this bucket that forms the largest triangle
            var maxArea = -1.0;
            var selectedIndex = bucketStart;
            var prevX = xSelector(data[prevIndex]);
            var prevY = ySelector(data[prevIndex]);

            for (var j = bucketStart; j < bucketEnd; j++)
            {
                var area = Math.Abs(
                    (prevX - avgX) * (ySelector(data[j]) - prevY) -
                    (prevX - xSelector(data[j])) * (avgY - prevY)
                ) * 0.5;

                if (area > maxArea)
                {
                    maxArea = area;
                    selectedIndex = j;
                }
            }

            result.Add(data[selectedIndex]);
            prevIndex = selectedIndex;
        }

        result.Add(data[^1]); // Always keep last
        return result;
    }

    /// <summary>
    /// Downsamples using array index as X value.
    /// </summary>
    public static List<T> Downsample<T>(IReadOnlyList<T> data, int targetCount, Func<T, double> ySelector)
    {
        return Downsample(data, targetCount, (_, i) => i, ySelector);
    }

    private static List<T> Downsample<T>(IReadOnlyList<T> data, int targetCount, Func<T, int, double> xSelector, Func<T, double> ySelector)
    {
        var indexed = data.Select((item, i) => (Item: item, X: (double)i)).ToList();
        var result = Downsample(indexed, targetCount, d => d.X, d => ySelector(d.Item));
        return result.Select(r => r.Item).ToList();
    }
}

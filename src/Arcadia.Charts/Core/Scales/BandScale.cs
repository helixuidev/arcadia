namespace Arcadia.Charts.Core.Scales;

/// <summary>
/// Maps discrete categories to equal-width bands within a pixel range.
/// Used for bar chart category axes.
/// </summary>
public class BandScale
{
    private readonly List<string> _categories;
    public double RangeMin { get; }
    public double RangeMax { get; }
    public double Padding { get; }

    public BandScale(IEnumerable<string> categories, double rangeMin, double rangeMax, double padding = 0.1)
    {
        _categories = categories.ToList();
        RangeMin = rangeMin;
        RangeMax = rangeMax;
        Padding = padding;
    }

    /// <summary>
    /// Gets the number of categories.
    /// </summary>
    public int Count => _categories.Count;

    /// <summary>
    /// Gets the width of each band (excluding padding).
    /// </summary>
    public double BandWidth
    {
        get
        {
            if (_categories.Count == 0) return 0;
            var totalRange = RangeMax - RangeMin;
            var step = totalRange / _categories.Count;
            return step * (1 - Padding);
        }
    }

    /// <summary>
    /// Gets the step size (band width + padding).
    /// </summary>
    public double Step
    {
        get
        {
            if (_categories.Count == 0) return 0;
            return (RangeMax - RangeMin) / _categories.Count;
        }
    }

    /// <summary>
    /// Maps a category to its band start position.
    /// </summary>
    public double Scale(string category)
    {
        var index = _categories.IndexOf(category);
        if (index < 0) return RangeMin;
        return RangeMin + index * Step + Step * Padding / 2;
    }

    /// <summary>
    /// Maps a category to its band center position.
    /// </summary>
    public double ScaleCenter(string category)
    {
        return Scale(category) + BandWidth / 2;
    }

    /// <summary>
    /// Gets all categories in order.
    /// </summary>
    public IReadOnlyList<string> Categories => _categories;
}

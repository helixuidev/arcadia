namespace Arcadia.Charts.Core.Layout;

/// <summary>
/// AABB collision detection for label bounding boxes.
/// </summary>
public static class CollisionDetector
{
    /// <summary>
    /// Checks if two bounding boxes overlap.
    /// </summary>
    /// <param name="a">First box.</param>
    /// <param name="b">Second box.</param>
    /// <param name="padding">Minimum spacing between boxes.</param>
    public static bool Overlaps(LabelBox a, LabelBox b, double padding = 2)
    {
        return a.X < b.X + b.Width + padding
            && a.X + a.Width + padding > b.X
            && a.Y < b.Y + b.Height + padding
            && a.Y + a.Height + padding > b.Y;
    }

    /// <summary>
    /// Checks if any adjacent labels in a sorted list overlap.
    /// </summary>
    public static bool HasOverlaps(IReadOnlyList<LabelBox> boxes, double padding = 2)
    {
        for (var i = 0; i < boxes.Count - 1; i++)
        {
            if (Overlaps(boxes[i], boxes[i + 1], padding))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns indices of all overlapping pairs.
    /// </summary>
    public static List<(int A, int B)> FindOverlaps(IReadOnlyList<LabelBox> boxes, double padding = 2)
    {
        var overlaps = new List<(int, int)>();
        for (var i = 0; i < boxes.Count; i++)
        {
            for (var j = i + 1; j < boxes.Count; j++)
            {
                if (Overlaps(boxes[i], boxes[j], padding))
                    overlaps.Add((i, j));
            }
        }
        return overlaps;
    }
}

/// <summary>
/// Axis-aligned bounding box for a label.
/// </summary>
public readonly struct LabelBox
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }

    public LabelBox(double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override string ToString() => $"({X:F1}, {Y:F1}, {Width:F1}×{Height:F1})";
}

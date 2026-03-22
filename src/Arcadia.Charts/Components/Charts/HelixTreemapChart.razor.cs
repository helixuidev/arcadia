using Microsoft.AspNetCore.Components;
using Arcadia.Charts.Core;

namespace Arcadia.Charts.Components.Charts;

public partial class HelixTreemapChart<T> : ChartBase<T>
{
    [Parameter] public Func<T, string>? NameField { get; set; }
    [Parameter] public Func<T, double>? ValueField { get; set; }

    private List<TreemapCell> _cells = new();

    protected override void OnParametersSet()
    {
        if (!HasData || NameField is null || ValueField is null) return;

        _cells.Clear();
        var total = Data!.Sum(ValueField);
        if (total <= 0) return;

        var topPad = string.IsNullOrEmpty(Title) ? 8 : 32;
        var sorted = Data!.Select((item, i) => (item, i))
            .OrderByDescending(x => ValueField(x.item))
            .ToList();

        // Squarified treemap — simplified slice-and-dice
        var rects = Squarify(sorted.Select(x => ValueField(x.item) / total).ToList(),
            8, topPad, Width - 16, Height - topPad - 8);

        for (var i = 0; i < sorted.Count && i < rects.Count; i++)
        {
            var (item, origIndex) = sorted[i];
            var r = rects[i];
            _cells.Add(new TreemapCell
            {
                X = r.X, Y = r.Y, W = r.W, H = r.H,
                Name = NameField(item),
                ValueDisplay = ValueField(item).ToString("N0"),
                Color = EffectivePalette.GetColor(origIndex)
            });
        }
    }

    private static List<(double X, double Y, double W, double H)> Squarify(
        List<double> ratios, double x, double y, double w, double h)
    {
        var result = new List<(double, double, double, double)>();
        var remaining = ratios.ToList();
        var rx = x; var ry = y; var rw = w; var rh = h;

        while (remaining.Count > 0)
        {
            var isWide = rw >= rh;
            var sum = remaining.Sum();

            if (remaining.Count == 1)
            {
                result.Add((rx, ry, rw, rh));
                break;
            }

            // Take items for this row/column
            var rowSum = 0.0;
            var rowCount = 0;
            var bestRatio = double.MaxValue;

            for (var i = 0; i < remaining.Count; i++)
            {
                rowSum += remaining[i];
                rowCount = i + 1;
                var rowFraction = rowSum / sum;
                var sliceSize = isWide ? rw * rowFraction : rh * rowFraction;
                var maxItemRatio = 0.0;

                for (var j = 0; j <= i; j++)
                {
                    var itemFraction = remaining[j] / rowSum;
                    var itemSize = isWide ? rh * itemFraction : rw * itemFraction;
                    var ratio = Math.Max(sliceSize / itemSize, itemSize / sliceSize);
                    maxItemRatio = Math.Max(maxItemRatio, ratio);
                }

                if (maxItemRatio > bestRatio && i > 0)
                {
                    rowCount = i;
                    rowSum -= remaining[i];
                    break;
                }
                bestRatio = maxItemRatio;
            }

            // Layout this row
            var fraction = rowSum / sum;
            var sliceW = isWide ? rw * fraction : rw;
            var sliceH = isWide ? rh : rh * fraction;
            var offset = 0.0;

            for (var j = 0; j < rowCount; j++)
            {
                var itemFrac = remaining[j] / rowSum;
                if (isWide)
                {
                    var ih = rh * itemFrac;
                    result.Add((rx, ry + offset, sliceW, ih));
                    offset += ih;
                }
                else
                {
                    var iw = rw * itemFrac;
                    result.Add((rx + offset, ry, iw, sliceH));
                    offset += iw;
                }
            }

            remaining.RemoveRange(0, rowCount);
            if (isWide) { rx += sliceW; rw -= sliceW; }
            else { ry += sliceH; rh -= sliceH; }
        }

        return result;
    }

    private static string F(double v) => v.ToString("F1");

    private class TreemapCell
    {
        public double X, Y, W, H;
        public string Name = "", ValueDisplay = "", Color = "";
    }
}

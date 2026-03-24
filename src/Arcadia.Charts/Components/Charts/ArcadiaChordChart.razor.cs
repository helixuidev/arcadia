using Microsoft.AspNetCore.Components;
using Arcadia.Charts.Core;

namespace Arcadia.Charts.Components.Charts;

/// <summary>
/// Chord diagram component that visualizes relationships between entities
/// arranged in a circular layout. Nodes form an outer ring; chord ribbons
/// connect them with width proportional to the flow magnitude.
/// </summary>
public partial class ArcadiaChordChart : ChartBase<ChordNode>
{
    /// <summary>The links (chords) between nodes.</summary>
    [Parameter] public IReadOnlyList<ChordLink>? Links { get; set; }

    /// <summary>Inner radius of the outer ring. Null = auto (70% of outer radius).</summary>
    [Parameter] public double? InnerRadius { get; set; }

    /// <summary>Outer radius of the ring. Null = auto from chart dimensions.</summary>
    [Parameter] public double? OuterRadius { get; set; }

    /// <summary>Gap between arc segments in degrees.</summary>
    [Parameter] public double PadAngle { get; set; } = 2.0;

    /// <summary>Whether to show node labels on the outer ring.</summary>
    [Parameter] public bool ShowLabels { get; set; } = true;

    /// <summary>Default fill opacity for chord ribbons.</summary>
    [Parameter] public double ChordOpacity { get; set; } = 0.75;

    /// <summary>Fill opacity for chord ribbons on hover.</summary>
    [Parameter] public double ChordHoverOpacity { get; set; } = 1.0;

    /// <summary>Minimum arc angle in degrees for a label to be shown. Arcs smaller than this are label-free.</summary>
    [Parameter] public double MinLabelAngle { get; set; } = 8;

    /// <summary>Fired when a chord ribbon is clicked.</summary>
    [Parameter] public EventCallback<ChordClickEventArgs> OnChordClick { get; set; }

    private new bool HasData => Data is not null && Data.Count > 0 && Links is not null && Links.Count > 0;

    private List<LayoutArc> _arcs = new();
    private List<LayoutChord> _chords = new();
    private double _cx, _cy;

    protected override void OnParametersSet()
    {
        if (!HasData) return;
        ComputeLayout();
    }

    private void ComputeLayout()
    {
        _arcs.Clear();
        _chords.Clear();

        var nodes = Data!;
        var links = Links!;

        // Build node map — skip duplicates and empty IDs
        var nodeMap = new Dictionary<string, NodeInfo>();
        var nodeOrder = new List<string>();
        for (var i = 0; i < nodes.Count; i++)
        {
            var n = nodes[i];
            if (string.IsNullOrEmpty(n.Id) || nodeMap.ContainsKey(n.Id)) continue;
            nodeMap[n.Id] = new NodeInfo
            {
                Id = n.Id,
                Label = n.Label ?? n.Id,
                Color = n.Color ?? EffectivePalette.GetColor(i),
                Index = i
            };
            nodeOrder.Add(n.Id);
        }

        // Compute per-node total flow
        foreach (var link in links)
        {
            if (link.Value <= 0) continue;
            if (link.SourceId == link.TargetId) continue;
            if (!nodeMap.ContainsKey(link.SourceId) || !nodeMap.ContainsKey(link.TargetId)) continue;
            nodeMap[link.SourceId].TotalFlow += link.Value;
            nodeMap[link.TargetId].TotalFlow += link.Value;
        }

        var grandTotal = 0.0;
        foreach (var id in nodeOrder) grandTotal += nodeMap[id].TotalFlow;
        if (grandTotal <= 0) return;

        // Geometry constants
        var hasTitle = !string.IsNullOrEmpty(Title);
        var hasSubtitle = hasTitle && !string.IsNullOrEmpty(Subtitle);
        var titleOffset = hasSubtitle ? 50.0 : hasTitle ? 35.0 : 0.0;

        _cx = EffectiveWidth / 2;
        _cy = (Height + titleOffset) / 2;
        // Reserve ~25% of radius for labels (D3 standard: 20-30%)
        var maxR = Math.Min(EffectiveWidth, Height - titleOffset) / 2 - (ShowLabels ? 80 : 20);
        var outerR = OuterRadius ?? maxR;
        // Thin ring like D3 (~12% of outer radius)
        var innerR = InnerRadius ?? outerR * 0.88;
        if (innerR >= outerR) innerR = outerR * 0.88;

        // Angular allocation
        var padRad = PadAngle * Math.PI / 180.0;
        var totalPad = padRad * nodeOrder.Count;
        var availableAngle = 2 * Math.PI - totalPad;
        if (availableAngle < 0.1) availableAngle = 0.1;

        // Assign arc angles
        var angle = -Math.PI / 2; // start at top
        foreach (var id in nodeOrder)
        {
            var node = nodeMap[id];
            var span = (node.TotalFlow / grandTotal) * availableAngle;
            node.StartAngle = angle;
            node.EndAngle = angle + span;
            node.ArcSpan = span;

            _arcs.Add(new LayoutArc
            {
                Id = node.Id,
                Label = node.Label,
                Color = node.Color,
                Index = node.Index,
                StartAngle = node.StartAngle,
                EndAngle = node.EndAngle,
                InnerR = innerR,
                OuterR = outerR
            });

            angle += span + padRad;
        }

        // Build chord ribbons
        // Track angular offset per node for stacking chords
        var nodeOffsets = new Dictionary<string, double>();
        foreach (var id in nodeOrder) nodeOffsets[id] = nodeMap[id].StartAngle;

        foreach (var link in links)
        {
            if (link.Value <= 0 || link.SourceId == link.TargetId) continue;
            if (!nodeMap.ContainsKey(link.SourceId) || !nodeMap.ContainsKey(link.TargetId)) continue;

            var src = nodeMap[link.SourceId];
            var tgt = nodeMap[link.TargetId];

            // Angular sub-span for this link on each arc
            var srcSpan = src.TotalFlow > 0 ? (link.Value / src.TotalFlow) * src.ArcSpan : 0;
            var tgtSpan = tgt.TotalFlow > 0 ? (link.Value / tgt.TotalFlow) * tgt.ArcSpan : 0;

            var srcStart = nodeOffsets[src.Id];
            var srcEnd = srcStart + srcSpan;
            var tgtStart = nodeOffsets[tgt.Id];
            var tgtEnd = tgtStart + tgtSpan;

            nodeOffsets[src.Id] = srcEnd;
            nodeOffsets[tgt.Id] = tgtEnd;

            var path = BuildRibbonPath(_cx, _cy, innerR, srcStart, srcEnd, tgtStart, tgtEnd);

            _chords.Add(new LayoutChord
            {
                SourceId = link.SourceId,
                TargetId = link.TargetId,
                SourceLabel = src.Label,
                TargetLabel = tgt.Label,
                Value = link.Value,
                Path = path,
                Color = link.Color ?? src.Color
            });
        }
    }

    /// <summary>Builds an annular arc segment path (outer ring).</summary>
    private static string BuildArcPath(double cx, double cy, double outerR, double innerR, double startAngle, double endAngle)
    {
        var largeArc = (endAngle - startAngle) > Math.PI ? 1 : 0;

        var x1 = cx + Math.Cos(startAngle) * outerR;
        var y1 = cy + Math.Sin(startAngle) * outerR;
        var x2 = cx + Math.Cos(endAngle) * outerR;
        var y2 = cy + Math.Sin(endAngle) * outerR;
        var x3 = cx + Math.Cos(endAngle) * innerR;
        var y3 = cy + Math.Sin(endAngle) * innerR;
        var x4 = cx + Math.Cos(startAngle) * innerR;
        var y4 = cy + Math.Sin(startAngle) * innerR;

        return $"M{F(x1)},{F(y1)} A{F(outerR)},{F(outerR)} 0 {largeArc},1 {F(x2)},{F(y2)} " +
               $"L{F(x3)},{F(y3)} A{F(innerR)},{F(innerR)} 0 {largeArc},0 {F(x4)},{F(y4)} Z";
    }

    /// <summary>Builds a chord ribbon path connecting two arc segments through the center.</summary>
    private static string BuildRibbonPath(double cx, double cy, double r,
        double srcStart, double srcEnd, double tgtStart, double tgtEnd)
    {
        var sx1 = cx + Math.Cos(srcStart) * r;
        var sy1 = cy + Math.Sin(srcStart) * r;
        var sx2 = cx + Math.Cos(srcEnd) * r;
        var sy2 = cy + Math.Sin(srcEnd) * r;
        var tx1 = cx + Math.Cos(tgtStart) * r;
        var ty1 = cy + Math.Sin(tgtStart) * r;
        var tx2 = cx + Math.Cos(tgtEnd) * r;
        var ty2 = cy + Math.Sin(tgtEnd) * r;

        var srcLargeArc = (srcEnd - srcStart) > Math.PI ? 1 : 0;
        var tgtLargeArc = (tgtEnd - tgtStart) > Math.PI ? 1 : 0;

        return $"M{F(sx1)},{F(sy1)} A{F(r)},{F(r)} 0 {srcLargeArc},1 {F(sx2)},{F(sy2)} " +
               $"Q{F(cx)},{F(cy)} {F(tx1)},{F(ty1)} " +
               $"A{F(r)},{F(r)} 0 {tgtLargeArc},1 {F(tx2)},{F(ty2)} " +
               $"Q{F(cx)},{F(cy)} {F(sx1)},{F(sy1)} Z";
    }

    /// <summary>Whether a given arc is large enough to show a label.</summary>
    private bool ShouldShowLabel(LayoutArc arc)
    {
        var spanDeg = (arc.EndAngle - arc.StartAngle) * 180 / Math.PI;
        return spanDeg >= MinLabelAngle;
    }

    /// <summary>Computes label position and rotation for an arc segment.</summary>
    private (double x, double y, double rotation, string anchor) GetLabelPosition(LayoutArc arc)
    {
        var midAngle = (arc.StartAngle + arc.EndAngle) / 2;
        var labelR = arc.OuterR + 20;
        var x = _cx + Math.Cos(midAngle) * labelR;
        var y = _cy + Math.Sin(midAngle) * labelR;
        var rotDeg = midAngle * 180 / Math.PI;

        // Flip text on left side so it reads left-to-right
        var isLeftSide = midAngle > Math.PI / 2 && midAngle < 3 * Math.PI / 2;
        // Also handle wrapping around -PI/2
        var normalizedAngle = ((midAngle % (2 * Math.PI)) + 2 * Math.PI) % (2 * Math.PI);
        isLeftSide = normalizedAngle > Math.PI / 2 && normalizedAngle < 3 * Math.PI / 2;

        if (isLeftSide)
        {
            rotDeg += 180;
            return (x, y, rotDeg, "end");
        }
        return (x, y, rotDeg, "start");
    }

    private async Task HandleChordClick(LayoutChord chord)
    {
        if (OnChordClick.HasDelegate)
        {
            await OnChordClick.InvokeAsync(new ChordClickEventArgs
            {
                SourceId = chord.SourceId,
                TargetId = chord.TargetId,
                SourceLabel = chord.SourceLabel,
                TargetLabel = chord.TargetLabel,
                Value = chord.Value
            });
        }
    }

    private static string F(double v) => v.ToString("F2");

    private class NodeInfo
    {
        public string Id = "", Label = "", Color = "";
        public int Index;
        public double TotalFlow;
        public double StartAngle, EndAngle, ArcSpan;
    }

    private class LayoutArc
    {
        public string Id = "", Label = "", Color = "";
        public int Index;
        public double StartAngle, EndAngle, InnerR, OuterR;
    }

    private class LayoutChord
    {
        public string SourceId = "", TargetId = "", Path = "", Color = "";
        public string SourceLabel = "", TargetLabel = "";
        public double Value;
    }
}

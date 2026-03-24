using Microsoft.AspNetCore.Components;
using Arcadia.Charts.Core;

namespace Arcadia.Charts.Components.Charts;

/// <summary>
/// Sankey diagram component that visualizes flows between nodes.
/// Nodes are connected by links whose width is proportional to the flow quantity.
/// Inherits from <see cref="ChartBase{T}"/> with <c>T = SankeyNode</c>,
/// where <see cref="ChartBase{T}.Data"/> provides the nodes.
/// </summary>
public partial class ArcadiaSankeyChart : ChartBase<SankeyNode>
{
    /// <summary>The links (flows) between nodes.</summary>
    [Parameter] public IReadOnlyList<SankeyLink>? Links { get; set; }

    /// <summary>Width of node rectangles in pixels.</summary>
    [Parameter] public double NodeWidth { get; set; } = 20;

    /// <summary>Vertical padding between nodes in the same column.</summary>
    [Parameter] public double NodePadding { get; set; } = 15;

    /// <summary>Whether to show node labels.</summary>
    [Parameter] public bool ShowLabels { get; set; } = true;

    /// <summary>Whether to show value labels on links.</summary>
    [Parameter] public bool ShowValues { get; set; } = true;

    /// <summary>Default fill opacity for link paths.</summary>
    [Parameter] public double LinkOpacity { get; set; } = 0.3;

    /// <summary>Fill opacity for link paths on hover.</summary>
    [Parameter] public double LinkHoverOpacity { get; set; } = 0.55;

    /// <summary>Fired when a link is clicked.</summary>
    [Parameter] public EventCallback<SankeyLinkClickEventArgs> OnLinkClick { get; set; }

    private new bool HasData => Data is not null && Data.Count > 0 && Links is not null && Links.Count > 0;

    private List<LayoutNode> _layoutNodes = new();
    private List<LayoutLink> _layoutLinks = new();
    private int _columnCount;

    protected override void OnParametersSet()
    {
        if (!HasData) return;
        ComputeLayout();
    }

    private void ComputeLayout()
    {
        _layoutNodes.Clear();
        _layoutLinks.Clear();

        var nodes = Data!;
        var links = Links!;

        // Build lookup — skip duplicate IDs
        var nodeMap = new Dictionary<string, LayoutNode>();
        for (var i = 0; i < nodes.Count; i++)
        {
            var n = nodes[i];
            if (string.IsNullOrEmpty(n.Id) || nodeMap.ContainsKey(n.Id)) continue;
            nodeMap[n.Id] = new LayoutNode
            {
                Id = n.Id,
                Label = n.Label ?? n.Id,
                Color = n.Color ?? EffectivePalette.GetColor(i),
                Index = i
            };
        }

        // Build adjacency — skip invalid or negative-value links
        foreach (var link in links)
        {
            if (link.Value <= 0) continue;
            if (!nodeMap.ContainsKey(link.SourceId) || !nodeMap.ContainsKey(link.TargetId)) continue;
            if (link.SourceId == link.TargetId) continue; // self-links

            var src = nodeMap[link.SourceId];
            var tgt = nodeMap[link.TargetId];
            src.OutgoingValue += link.Value;
            tgt.IncomingValue += link.Value;
            src.OutLinks.Add(link);
            tgt.InLinks.Add(link);
        }

        // Step 1: Assign columns via topological ordering
        var incomingSources = new Dictionary<string, HashSet<string>>();
        foreach (var n in nodeMap.Values) incomingSources[n.Id] = new HashSet<string>();
        foreach (var link in links)
        {
            if (link.Value <= 0) continue;
            if (link.SourceId == link.TargetId) continue;
            if (nodeMap.ContainsKey(link.SourceId) && nodeMap.ContainsKey(link.TargetId))
                incomingSources[link.TargetId].Add(link.SourceId);
        }

        foreach (var n in nodeMap.Values)
        {
            n.Column = incomingSources[n.Id].Count == 0 ? 0 : -1;
        }

        // Iterative column assignment with cycle-safe max iterations
        bool changed = true;
        int maxIter = nodes.Count * links.Count + 1;
        if (maxIter > 10000) maxIter = 10000; // safety cap
        while (changed && maxIter-- > 0)
        {
            changed = false;
            foreach (var link in links)
            {
                if (link.Value <= 0 || link.SourceId == link.TargetId) continue;
                if (!nodeMap.ContainsKey(link.SourceId) || !nodeMap.ContainsKey(link.TargetId)) continue;
                var src = nodeMap[link.SourceId];
                var tgt = nodeMap[link.TargetId];
                if (src.Column < 0) continue;
                var newCol = src.Column + 1;
                if (newCol > tgt.Column)
                {
                    tgt.Column = newCol;
                    changed = true;
                }
            }
        }

        // Nodes stuck at -1 are in cycles — place them after the last resolved column
        var maxResolved = 0;
        foreach (var n in nodeMap.Values)
        {
            if (n.Column > maxResolved) maxResolved = n.Column;
        }
        foreach (var n in nodeMap.Values)
        {
            if (n.Column < 0) n.Column = maxResolved + 1;
        }

        _columnCount = nodeMap.Values.Max(n => n.Column) + 1;
        if (_columnCount < 1) _columnCount = 1;

        // Step 2: Calculate positions
        var hasTitle = !string.IsNullOrEmpty(Title);
        var hasSubtitle = hasTitle && !string.IsNullOrEmpty(Subtitle);
        var topPad = hasSubtitle ? 60.0 : hasTitle ? 50.0 : 20.0;
        var bottomPad = 20.0;
        var leftPad = 60.0;
        var rightPad = 60.0;
        var availableWidth = EffectiveWidth - leftPad - rightPad;
        var availableHeight = Height - topPad - bottomPad;

        var columnSpacing = _columnCount > 1 ? availableWidth / (_columnCount - 1) : 0;

        var columns = new Dictionary<int, List<LayoutNode>>();
        foreach (var n in nodeMap.Values)
        {
            if (!columns.ContainsKey(n.Column)) columns[n.Column] = new();
            columns[n.Column].Add(n);
        }

        foreach (var (col, colNodes) in columns)
        {
            var totalNodeValue = colNodes.Sum(n => Math.Max(n.IncomingValue, n.OutgoingValue));
            var totalPadding = NodePadding * (colNodes.Count - 1);
            var scaleHeight = availableHeight - totalPadding;
            if (scaleHeight < 10) scaleHeight = 10;

            var y = topPad;
            foreach (var n in colNodes)
            {
                var nodeValue = Math.Max(n.IncomingValue, n.OutgoingValue);
                n.Height = totalNodeValue > 0 ? (nodeValue / totalNodeValue) * scaleHeight : scaleHeight / colNodes.Count;
                if (n.Height < 4) n.Height = 4;

                n.X = _columnCount > 1 ? leftPad + col * columnSpacing : leftPad;
                n.Y = y;
                n.Width = NodeWidth;

                y += n.Height + NodePadding;
            }
        }

        _layoutNodes = nodeMap.Values.ToList();

        // Step 3: Build links with bezier paths
        var sourceOffsets = nodeMap.Values.ToDictionary(n => n.Id, _ => 0.0);
        var targetOffsets = nodeMap.Values.ToDictionary(n => n.Id, _ => 0.0);

        foreach (var link in links)
        {
            if (link.Value <= 0 || link.SourceId == link.TargetId) continue;
            if (!nodeMap.ContainsKey(link.SourceId) || !nodeMap.ContainsKey(link.TargetId)) continue;

            var src = nodeMap[link.SourceId];
            var tgt = nodeMap[link.TargetId];

            var srcTotal = Math.Max(src.IncomingValue, src.OutgoingValue);
            var tgtTotal = Math.Max(tgt.IncomingValue, tgt.OutgoingValue);

            var linkHeightSrc = srcTotal > 0 ? (link.Value / srcTotal) * src.Height : 0;
            var linkHeightTgt = tgtTotal > 0 ? (link.Value / tgtTotal) * tgt.Height : 0;

            var srcY = src.Y + sourceOffsets[src.Id];
            var tgtY = tgt.Y + targetOffsets[tgt.Id];

            sourceOffsets[src.Id] += linkHeightSrc;
            targetOffsets[tgt.Id] += linkHeightTgt;

            var x0 = src.X + src.Width;
            var x1 = tgt.X;
            var midX = (x0 + x1) / 2;

            var path = $"M{F(x0)},{F(srcY)} " +
                       $"C{F(midX)},{F(srcY)} {F(midX)},{F(tgtY)} {F(x1)},{F(tgtY)} " +
                       $"L{F(x1)},{F(tgtY + linkHeightTgt)} " +
                       $"C{F(midX)},{F(tgtY + linkHeightTgt)} {F(midX)},{F(srcY + linkHeightSrc)} {F(x0)},{F(srcY + linkHeightSrc)} Z";

            _layoutLinks.Add(new LayoutLink
            {
                SourceId = link.SourceId,
                TargetId = link.TargetId,
                Value = link.Value,
                Path = path,
                Color = link.Color ?? src.Color,
                SourceLabel = src.Label,
                TargetLabel = tgt.Label
            });
        }
    }

    private async Task HandleLinkClick(LayoutLink link)
    {
        if (OnLinkClick.HasDelegate)
        {
            await OnLinkClick.InvokeAsync(new SankeyLinkClickEventArgs
            {
                SourceId = link.SourceId,
                TargetId = link.TargetId,
                SourceLabel = link.SourceLabel,
                TargetLabel = link.TargetLabel,
                Value = link.Value
            });
        }
    }

    private static string F(double v) => v.ToString("F1");

    private class LayoutNode
    {
        public string Id = "", Label = "", Color = "";
        public int Index, Column;
        public double X, Y, Width, Height;
        public double IncomingValue, OutgoingValue;
        public List<SankeyLink> InLinks = new();
        public List<SankeyLink> OutLinks = new();
    }

    private class LayoutLink
    {
        public string SourceId = "", TargetId = "", Path = "", Color = "";
        public string SourceLabel = "", TargetLabel = "";
        public double Value;
    }
}

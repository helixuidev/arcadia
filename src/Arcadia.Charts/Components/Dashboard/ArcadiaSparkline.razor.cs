using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.Charts.Core.Scales;

namespace Arcadia.Charts.Components.Dashboard;

/// <summary>
/// Inline mini chart with no axes, labels, or chrome.
/// Embeddable in text, table cells, or KPI cards.
/// </summary>
public partial class ArcadiaSparkline : Arcadia.Core.Base.ArcadiaComponentBase
{
    /// <summary>Data values to plot.</summary>
    [Parameter] public IReadOnlyList<double>? Data { get; set; }

    /// <summary>Width in pixels.</summary>
    [Parameter] public double Width { get; set; } = 120;

    /// <summary>Height in pixels.</summary>
    [Parameter] public double Height { get; set; } = 32;

    /// <summary>Stroke width of the line.</summary>
    [Parameter] public double StrokeWidth { get; set; } = 1.5;

    /// <summary>Whether to show a filled area below the line.</summary>
    [Parameter] public bool ShowArea { get; set; }

    /// <summary>Color name: "primary", "success", "danger", etc. or a CSS color.</summary>
    [Parameter] public string Color { get; set; } = "primary";

    /// <summary>Accessible label for the sparkline.</summary>
    [Parameter] public string? AriaLabel { get; set; }

    private string DefaultAriaLabel => Data is { Count: > 0 }
        ? $"Sparkline chart with {Data.Count} data points, range {Data.Min():G4} to {Data.Max():G4}"
        : "Sparkline chart";

    private string? _pathData;
    private string? _areaPath;

    private string ResolvedColor => Color switch
    {
        "primary" => "var(--arcadia-color-primary, #2563eb)",
        "secondary" => "var(--arcadia-color-secondary, #7c3aed)",
        "success" => "var(--arcadia-color-success, #16a34a)",
        "danger" => "var(--arcadia-color-danger, #dc2626)",
        "warning" => "var(--arcadia-color-warning, #d97706)",
        "info" => "var(--arcadia-color-info, #0284c7)",
        _ => Color
    };

    private string? CssClass => CssBuilder.Default("arcadia-sparkline")
        .AddClass(Class)
        .Build();

    protected override void OnParametersSet()
    {
        BuildPaths();
    }

    private void BuildPaths()
    {
        if (Data is null || Data.Count < 2)
        {
            _pathData = null;
            _areaPath = null;
            return;
        }

        var padding = StrokeWidth;
        var xScale = new LinearScale(0, Data.Count - 1, padding, Width - padding);
        var yScale = new LinearScale(Data.Min(), Data.Max(), Height - padding, padding); // Inverted Y

        var points = new List<string>();
        for (var i = 0; i < Data.Count; i++)
        {
            var x = xScale.Scale(i);
            var y = yScale.Scale(Data[i]);
            points.Add($"{x:F1},{y:F1}");
        }

        _pathData = "M" + string.Join(" L", points);

        if (ShowArea)
        {
            var firstX = xScale.Scale(0);
            var lastX = xScale.Scale(Data.Count - 1);
            _areaPath = _pathData + $" L{lastX:F1},{Height - padding:F1} L{firstX:F1},{Height - padding:F1} Z";
        }
    }
}

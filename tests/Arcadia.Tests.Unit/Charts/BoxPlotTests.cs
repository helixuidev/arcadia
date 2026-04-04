using Bunit;
using FluentAssertions;
using Arcadia.Charts.Components.Charts;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public record BoxData(string Category, double Min, double Q1, double Median, double Q3, double Max);

public class BoxPlotTests : ChartTestBase
{
    private static readonly List<BoxData> BasicData = new()
    {
        new("Group A", 10, 20, 30, 40, 50),
        new("Group B", 15, 25, 35, 45, 55),
        new("Group C", 5, 15, 25, 35, 45),
    };

    [Fact]
    public void Renders_SvgWithFigureRole()
    {
        var cut = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<BoxData, string>)(d => d.Category))
            .Add(c => c.MinField, (Func<BoxData, double>)(d => d.Min))
            .Add(c => c.Q1Field, (Func<BoxData, double>)(d => d.Q1))
            .Add(c => c.MedianField, (Func<BoxData, double>)(d => d.Median))
            .Add(c => c.Q3Field, (Func<BoxData, double>)(d => d.Q3))
            .Add(c => c.MaxField, (Func<BoxData, double>)(d => d.Max))
            .Add(c => c.Width, 600));

        var svg = cut.Find("svg[role='figure']");
        svg.Should().NotBeNull();
        svg.GetAttribute("aria-label").Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Renders_OneBoxRectPerCategory()
    {
        var cut = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<BoxData, string>)(d => d.Category))
            .Add(c => c.MinField, (Func<BoxData, double>)(d => d.Min))
            .Add(c => c.Q1Field, (Func<BoxData, double>)(d => d.Q1))
            .Add(c => c.MedianField, (Func<BoxData, double>)(d => d.Median))
            .Add(c => c.Q3Field, (Func<BoxData, double>)(d => d.Q3))
            .Add(c => c.MaxField, (Func<BoxData, double>)(d => d.Max))
            .Add(c => c.Width, 600));

        // 3 categories => at least 3 rect elements (box bodies) plus any axis/background rects
        cut.FindAll("rect").Count.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void Renders_ScreenReaderTable_WithCategoryData()
    {
        var cut = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<BoxData, string>)(d => d.Category))
            .Add(c => c.MinField, (Func<BoxData, double>)(d => d.Min))
            .Add(c => c.Q1Field, (Func<BoxData, double>)(d => d.Q1))
            .Add(c => c.MedianField, (Func<BoxData, double>)(d => d.Median))
            .Add(c => c.Q3Field, (Func<BoxData, double>)(d => d.Q3))
            .Add(c => c.MaxField, (Func<BoxData, double>)(d => d.Max)));

        var table = cut.Find("table.arcadia-sr-only");
        table.Should().NotBeNull();
        var markup = table.InnerHtml;
        markup.Should().Contain("Group A");
        markup.Should().Contain("Group B");
        markup.Should().Contain("Group C");
    }

    [Fact]
    public void Renders_NoData_WhenDataIsNull()
    {
        var cut = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, (IReadOnlyList<BoxData>?)null));

        cut.Markup.Should().Contain("arcadia-chart__no-data");
    }

    [Fact]
    public void Renders_NoData_WhenDataIsEmpty()
    {
        var cut = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, new List<BoxData>()));

        cut.Markup.Should().Contain("arcadia-chart__no-data");
    }

    [Fact]
    public void Renders_Title()
    {
        var cut = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<BoxData, string>)(d => d.Category))
            .Add(c => c.MinField, (Func<BoxData, double>)(d => d.Min))
            .Add(c => c.Q1Field, (Func<BoxData, double>)(d => d.Q1))
            .Add(c => c.MedianField, (Func<BoxData, double>)(d => d.Median))
            .Add(c => c.Q3Field, (Func<BoxData, double>)(d => d.Q3))
            .Add(c => c.MaxField, (Func<BoxData, double>)(d => d.Max))
            .Add(c => c.Title, "Distribution"));

        cut.Find(".arcadia-chart__title").TextContent.Should().Be("Distribution");
    }

    [Fact]
    public void CustomBoxColor_AppliedToFillAttribute()
    {
        var cut = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<BoxData, string>)(d => d.Category))
            .Add(c => c.MinField, (Func<BoxData, double>)(d => d.Min))
            .Add(c => c.Q1Field, (Func<BoxData, double>)(d => d.Q1))
            .Add(c => c.MedianField, (Func<BoxData, double>)(d => d.Median))
            .Add(c => c.Q3Field, (Func<BoxData, double>)(d => d.Q3))
            .Add(c => c.MaxField, (Func<BoxData, double>)(d => d.Max))
            .Add(c => c.BoxColor, "#ff00aa"));

        cut.Markup.Should().Contain("#ff00aa");
    }

    [Fact]
    public void MissingRequiredField_DoesNotCrash()
    {
        // Median accessor is null — component should degrade gracefully, not throw
        var render = () => Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<BoxData, string>)(d => d.Category))
            .Add(c => c.MinField, (Func<BoxData, double>)(d => d.Min))
            .Add(c => c.Q1Field, (Func<BoxData, double>)(d => d.Q1))
            // MedianField intentionally missing
            .Add(c => c.Q3Field, (Func<BoxData, double>)(d => d.Q3))
            .Add(c => c.MaxField, (Func<BoxData, double>)(d => d.Max)));

        render.Should().NotThrow();
    }

    [Fact]
    public void SingleCategory_RendersSingleBox()
    {
        var single = new List<BoxData> { new("Only", 1, 2, 3, 4, 5) };
        var cut = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, single)
            .Add(c => c.CategoryField, (Func<BoxData, string>)(d => d.Category))
            .Add(c => c.MinField, (Func<BoxData, double>)(d => d.Min))
            .Add(c => c.Q1Field, (Func<BoxData, double>)(d => d.Q1))
            .Add(c => c.MedianField, (Func<BoxData, double>)(d => d.Median))
            .Add(c => c.Q3Field, (Func<BoxData, double>)(d => d.Q3))
            .Add(c => c.MaxField, (Func<BoxData, double>)(d => d.Max)));

        cut.Find("svg[role='figure']").Should().NotBeNull();
        cut.Markup.Should().Contain("Only");
    }

    [Fact]
    public void CustomMedianLineColor_AppliedToLine()
    {
        var cut = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<BoxData, string>)(d => d.Category))
            .Add(c => c.MinField, (Func<BoxData, double>)(d => d.Min))
            .Add(c => c.Q1Field, (Func<BoxData, double>)(d => d.Q1))
            .Add(c => c.MedianField, (Func<BoxData, double>)(d => d.Median))
            .Add(c => c.Q3Field, (Func<BoxData, double>)(d => d.Q3))
            .Add(c => c.MaxField, (Func<BoxData, double>)(d => d.Max))
            .Add(c => c.MedianLineColor, "#abcdef"));

        cut.Markup.Should().Contain("#abcdef");
    }

    [Fact]
    public void BoxWidthFraction_AffectsRenderedWidth()
    {
        var cutNarrow = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<BoxData, string>)(d => d.Category))
            .Add(c => c.MinField, (Func<BoxData, double>)(d => d.Min))
            .Add(c => c.Q1Field, (Func<BoxData, double>)(d => d.Q1))
            .Add(c => c.MedianField, (Func<BoxData, double>)(d => d.Median))
            .Add(c => c.Q3Field, (Func<BoxData, double>)(d => d.Q3))
            .Add(c => c.MaxField, (Func<BoxData, double>)(d => d.Max))
            .Add(c => c.Width, 600)
            .Add(c => c.BoxWidth, 0.3));

        var cutWide = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<BoxData, string>)(d => d.Category))
            .Add(c => c.MinField, (Func<BoxData, double>)(d => d.Min))
            .Add(c => c.Q1Field, (Func<BoxData, double>)(d => d.Q1))
            .Add(c => c.MedianField, (Func<BoxData, double>)(d => d.Median))
            .Add(c => c.Q3Field, (Func<BoxData, double>)(d => d.Q3))
            .Add(c => c.MaxField, (Func<BoxData, double>)(d => d.Max))
            .Add(c => c.Width, 600)
            .Add(c => c.BoxWidth, 0.9));

        cutNarrow.Markup.Should().NotBe(cutWide.Markup);
    }

    [Fact]
    public void CssClass_AppendedToRoot()
    {
        var cut = Render<ArcadiaBoxPlot<BoxData>>(p => p
            .Add(c => c.Data, BasicData)
            .Add(c => c.CategoryField, (Func<BoxData, string>)(d => d.Category))
            .Add(c => c.MinField, (Func<BoxData, double>)(d => d.Min))
            .Add(c => c.Q1Field, (Func<BoxData, double>)(d => d.Q1))
            .Add(c => c.MedianField, (Func<BoxData, double>)(d => d.Median))
            .Add(c => c.Q3Field, (Func<BoxData, double>)(d => d.Q3))
            .Add(c => c.MaxField, (Func<BoxData, double>)(d => d.Max))
            .Add(c => c.Class, "my-custom-box"));

        cut.Markup.Should().Contain("my-custom-box");
    }
}

using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.Gauge.Components;

namespace Arcadia.Tests.Unit.Gauge;

public class ArcadiaRadialGaugeTests : ChartTestBase
{
    [Fact]
    public void RendersWithDefaultParameters()
    {
        var cut = Render<ArcadiaRadialGauge>();

        var div = cut.Find(".arcadia-gauge");
        div.Should().NotBeNull();

        var svg = cut.Find("svg");
        svg.Should().NotBeNull();
        svg.GetAttribute("width").Should().Be("220");
        svg.GetAttribute("height").Should().Be("220");
    }

    [Fact]
    public void RendersCorrectValueDisplay()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 73)
            .Add(c => c.ShowValue, true));

        // The value text should contain "73"
        var markup = cut.Markup;
        markup.Should().Contain("73");
    }

    [Fact]
    public void HidesValueWhenShowValueIsFalse()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 73)
            .Add(c => c.ShowValue, false));

        // The screen-reader table still contains the value, but no visible text element
        var texts = cut.FindAll("text");
        texts.Should().BeEmpty();
    }

    [Fact]
    public void RendersLabelText()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 50)
            .Add(c => c.Label, "CPU Usage"));

        var markup = cut.Markup;
        markup.Should().Contain("CPU Usage");
    }

    [Fact]
    public void RespectsMinMaxBounds_ClampsAboveMax()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 150)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100));

        // Value above max should still render — the arc fraction is clamped to 1.0
        // The display should show the raw value (150)
        var markup = cut.Markup;
        markup.Should().Contain("150");
    }

    [Fact]
    public void RespectsMinMaxBounds_ClampsBelowMin()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, -20)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100));

        // Value below min should still render — the arc fraction is clamped to 0.0
        var markup = cut.Markup;
        markup.Should().Contain("-20");
    }

    [Fact]
    public void RespectsCustomMinMax()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 500)
            .Add(c => c.Min, 200)
            .Add(c => c.Max, 800));

        // Screen reader table should reflect the custom min/max
        var markup = cut.Markup;
        markup.Should().Contain("200");
        markup.Should().Contain("800");
        markup.Should().Contain("500");
    }

    [Fact]
    public void CustomColor_AppliedToValueArc()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 50)
            .Add(c => c.Color, "#ff0000"));

        // The value arc path should use the custom color as stroke
        var paths = cut.FindAll("path");
        var valueArcExists = false;
        foreach (var path in paths)
        {
            if (path.GetAttribute("stroke") == "#ff0000")
            {
                valueArcExists = true;
                break;
            }
        }
        valueArcExists.Should().BeTrue("the value arc should use the custom color #ff0000");
    }

    [Fact]
    public void TrackColor_AppliedToTrackArc()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 50)
            .Add(c => c.TrackColor, "#333333"));

        var paths = cut.FindAll("path");
        var trackFound = false;
        foreach (var path in paths)
        {
            if (path.GetAttribute("stroke") == "#333333")
            {
                trackFound = true;
                break;
            }
        }
        trackFound.Should().BeTrue("the track arc should use the custom TrackColor");
    }

    [Fact]
    public void ThresholdColors_ChangeBasedOnValue()
    {
        var thresholds = new List<GaugeThreshold>
        {
            new() { Value = 0, Color = "#22c55e" },
            new() { Value = 50, Color = "#eab308" },
            new() { Value = 80, Color = "#ef4444" }
        };

        // Value 90 should use the red threshold (#ef4444)
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 90)
            .Add(c => c.Thresholds, thresholds));

        var markup = cut.Markup;
        markup.Should().Contain("#ef4444");
    }

    [Fact]
    public void AriaLabel_DefaultUsesLabelAndValue()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 73)
            .Add(c => c.Label, "Speed"));

        var svg = cut.Find("svg");
        svg.GetAttribute("aria-label").Should().Be("Speed: 73");
    }

    [Fact]
    public void AriaLabel_CustomOverridesDefault()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 73)
            .Add(c => c.Label, "Speed")
            .Add(c => c.AriaLabel, "Current speed is 73 mph"));

        var svg = cut.Find("svg");
        svg.GetAttribute("aria-label").Should().Be("Current speed is 73 mph");
    }

    [Fact]
    public void Svg_HasFigureRole()
    {
        var cut = Render<ArcadiaRadialGauge>();

        var svg = cut.Find("svg");
        svg.GetAttribute("role").Should().Be("figure");
    }

    [Fact]
    public void ScreenReaderTable_RendersGaugeData()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 42)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100)
            .Add(c => c.Label, "Temperature"));

        var table = cut.Find("table.arcadia-sr-only");
        table.Should().NotBeNull();
        table.GetAttribute("aria-label").Should().Be("Gauge data");

        var markup = table.InnerHtml;
        markup.Should().Contain("Temperature");
        markup.Should().Contain("42");
        markup.Should().Contain("0");
        markup.Should().Contain("100");
    }

    [Fact]
    public void CssClass_AppendsToRootElement()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Class, "my-custom-class"));

        var div = cut.Find(".arcadia-gauge");
        div.ClassList.Should().Contain("my-custom-class");
    }

    [Fact]
    public void Style_AppliedToRootElement()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Style, "margin-top: 10px;"));

        var div = cut.Find(".arcadia-gauge");
        div.GetAttribute("style").Should().Contain("margin-top: 10px;");
    }

    [Fact]
    public void Editable_AddsCssModifier()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Editable, true));

        var div = cut.Find(".arcadia-gauge");
        div.ClassList.Should().Contain("arcadia-gauge--editable");
    }

    [Fact]
    public void NotEditable_NoCssModifier()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Editable, false));

        var div = cut.Find(".arcadia-gauge");
        div.ClassList.Should().NotContain("arcadia-gauge--editable");
    }

    [Fact]
    public void FullCircle_RendersCorrectly()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 75)
            .Add(c => c.FullCircle, true));

        // Should render without error — the arc paths should exist
        var paths = cut.FindAll("path");
        paths.Count.Should().BeGreaterThanOrEqualTo(2, "track and value arcs should be present");
    }

    [Fact]
    public void CustomDimensions_AppliedToSvg()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Width, 300)
            .Add(c => c.Height, 250));

        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("300");
        svg.GetAttribute("height").Should().Be("250");
    }

    [Fact]
    public void ShowNeedle_RendersNeedleElements()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 50)
            .Add(c => c.ShowNeedle, true));

        // Needle is a polygon + circle
        var polygons = cut.FindAll("polygon");
        polygons.Count.Should().BeGreaterThanOrEqualTo(1);

        var circles = cut.FindAll("circle");
        circles.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void ShowTicks_RendersTickLines()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 50)
            .Add(c => c.ShowTicks, true));

        var lines = cut.FindAll("line");
        lines.Count.Should().BeGreaterThan(0, "tick marks should be rendered as line elements");
    }

    [Fact]
    public void GradientColors_RendersLinearGradient()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 50)
            .Add(c => c.GradientColors, new List<string> { "#22c55e", "#eab308", "#ef4444" }));

        var markup = cut.Markup;
        markup.Should().Contain("linearGradient");
        markup.Should().Contain("#22c55e");
        markup.Should().Contain("#ef4444");
    }

    [Fact]
    public void ValueFormatString_FormatsDisplay()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 0.75)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 1)
            .Add(c => c.ValueFormatString, "P0"));

        // P0 format should render as "75 %" (or "75%")
        var markup = cut.Markup;
        markup.Should().Contain("75");
        markup.Should().Contain("%");
    }

    [Fact]
    public void Ranges_RendersColoredBands()
    {
        var ranges = new List<GaugeRange>
        {
            new() { Start = 0, End = 33, Color = "#22c55e" },
            new() { Start = 33, End = 66, Color = "#eab308" },
            new() { Start = 66, End = 100, Color = "#ef4444" }
        };

        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 50)
            .Add(c => c.Ranges, ranges));

        var markup = cut.Markup;
        markup.Should().Contain("#22c55e");
        markup.Should().Contain("#eab308");
        markup.Should().Contain("#ef4444");
    }

    [Fact]
    public void AdditionalAttributes_PassedThrough()
    {
        var cut = Render<ArcadiaRadialGauge>(p => p
            .Add(c => c.Value, 50)
            .AddUnmatched("data-testid", "my-gauge"));

        var div = cut.Find(".arcadia-gauge");
        div.GetAttribute("data-testid").Should().Be("my-gauge");
    }
}

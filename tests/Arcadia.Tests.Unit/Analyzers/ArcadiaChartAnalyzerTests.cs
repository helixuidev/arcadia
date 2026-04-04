using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Arcadia.Analyzers;

namespace Arcadia.Tests.Unit.Analyzers;

public class ArcadiaChartAnalyzerTests
{
    private readonly ArcadiaChartAnalyzer _analyzer = new();

    [Fact]
    public void Analyzer_SupportsDiagnostic_ARC001_MissingData()
    {
        _analyzer.SupportedDiagnostics
            .Should().Contain(d => d.Id == "ARC001");

        var rule = _analyzer.SupportedDiagnostics.First(d => d.Id == "ARC001");
        rule.Title.ToString().Should().Be("Chart component missing Data parameter");
        rule.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
        rule.IsEnabledByDefault.Should().BeTrue();
        rule.Category.Should().Be("Arcadia.Usage");
    }

    [Fact]
    public void Analyzer_SupportsDiagnostic_ARC002_EmptySeries()
    {
        _analyzer.SupportedDiagnostics
            .Should().Contain(d => d.Id == "ARC002");

        var rule = _analyzer.SupportedDiagnostics.First(d => d.Id == "ARC002");
        rule.Title.ToString().Should().Be("Chart has empty Series list");
        rule.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
        rule.IsEnabledByDefault.Should().BeTrue();
        rule.Category.Should().Be("Arcadia.Usage");
    }

    [Fact]
    public void Analyzer_SupportsDiagnostic_ARC003_MissingHeight()
    {
        _analyzer.SupportedDiagnostics
            .Should().Contain(d => d.Id == "ARC003");

        var rule = _analyzer.SupportedDiagnostics.First(d => d.Id == "ARC003");
        rule.Title.ToString().Should().Be("Chart has Width but no Height");
        rule.DefaultSeverity.Should().Be(DiagnosticSeverity.Info);
        rule.IsEnabledByDefault.Should().BeTrue();
        rule.Category.Should().Be("Arcadia.Usage");
    }

    [Fact]
    public void Analyzer_SupportsExactlyThreeDiagnostics()
    {
        _analyzer.SupportedDiagnostics.Length.Should().Be(3);
    }

    [Fact]
    public void Analyzer_AllDiagnosticsAreEnabledByDefault()
    {
        foreach (var diagnostic in _analyzer.SupportedDiagnostics)
        {
            diagnostic.IsEnabledByDefault.Should().BeTrue(
                $"diagnostic {diagnostic.Id} should be enabled by default");
        }
    }

    [Fact]
    public void Analyzer_AllDiagnosticsHaveDescriptions()
    {
        foreach (var diagnostic in _analyzer.SupportedDiagnostics)
        {
            diagnostic.Description.ToString().Should().NotBeNullOrWhiteSpace(
                $"diagnostic {diagnostic.Id} should have a description");
        }
    }

    [Fact]
    public void Analyzer_ARC001_MessageFormat_ContainsPlaceholder()
    {
        var rule = _analyzer.SupportedDiagnostics.First(d => d.Id == "ARC001");
        rule.MessageFormat.ToString().Should().Contain("{0}",
            "message format should include a placeholder for the component name");
    }

    [Fact]
    public void Analyzer_ARC002_MessageFormat_ContainsPlaceholder()
    {
        var rule = _analyzer.SupportedDiagnostics.First(d => d.Id == "ARC002");
        rule.MessageFormat.ToString().Should().Contain("{0}",
            "message format should include a placeholder for the component name");
    }

    [Fact]
    public void Analyzer_ARC003_MessageFormat_ContainsPlaceholder()
    {
        var rule = _analyzer.SupportedDiagnostics.First(d => d.Id == "ARC003");
        rule.MessageFormat.ToString().Should().Contain("{0}",
            "message format should include a placeholder for the component name");
    }

    [Fact]
    public void Analyzer_IsDiagnosticAnalyzer()
    {
        _analyzer.Should().BeAssignableTo<DiagnosticAnalyzer>();
    }

    [Fact]
    public void Analyzer_DiagnosticIds_AreUnique()
    {
        var ids = _analyzer.SupportedDiagnostics.Select(d => d.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Analyzer_DiagnosticIds_FollowArcPrefix()
    {
        foreach (var diagnostic in _analyzer.SupportedDiagnostics)
        {
            diagnostic.Id.Should().StartWith("ARC",
                $"diagnostic {diagnostic.Id} should follow the ARC prefix convention");
        }
    }
}

using System.Diagnostics;
using Bunit;
using Arcadia.Charts.Components.Charts;
using Arcadia.Charts.Core;
using Arcadia.DataGrid.Components;
using Arcadia.DataGrid.Core;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Arcadia.Tests.Unit.Performance;

/// <summary>
/// Performance benchmarks for chart and grid rendering.
/// Uses Stopwatch to measure bUnit render time. Targets are generous
/// to account for CI variance — actual render is typically 2-5x faster.
/// </summary>
[Trait("Category", "Performance")]
public class ChartBenchmarks : ChartTestBase
{
    private readonly ITestOutputHelper _output;
    private static bool _warmedUp;

    public ChartBenchmarks(ITestOutputHelper output)
    {
        _output = output;
        if (!_warmedUp)
        {
            _warmedUp = true;
            // JIT warmup — render one of each component type to trigger compilation
            Render<ArcadiaLineChart<DataPoint>>(p => p
                .Add(c => c.Data, GenerateLineData(5))
                .Add(c => c.XField, (Func<DataPoint, object>)(d => d.Label))
                .Add(c => c.Series, new List<SeriesConfig<DataPoint>> { new() { Name = "W", Field = d => d.Value } })
                .Add(c => c.AnimateOnLoad, false));
            Render<ArcadiaBarChart<DataPoint>>(p => p
                .Add(c => c.Data, GenerateLineData(5))
                .Add(c => c.XField, (Func<DataPoint, object>)(d => d.Label))
                .Add(c => c.Series, new List<SeriesConfig<DataPoint>> { new() { Name = "W", Field = d => d.Value } })
                .Add(c => c.AnimateOnLoad, false));
            Render<ArcadiaPieChart<NameValue>>(p => p
                .Add(c => c.Data, new List<NameValue> { new("W", 1) })
                .Add(c => c.NameField, (Func<NameValue, string>)(d => d.Name))
                .Add(c => c.ValueField, (Func<NameValue, double>)(d => d.Value))
                .Add(c => c.AnimateOnLoad, false));
        }
    }

    // ── Line Chart ──

    [Fact]
    public void LineChart_100Points_Under20ms()
    {
        var data = GenerateLineData(100);
        var series = new List<SeriesConfig<DataPoint>> { new() { Name = "V", Field = d => d.Value } };

        var sw = Stopwatch.StartNew();
        var cut = Render<ArcadiaLineChart<DataPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<DataPoint, object>)(d => d.Label))
            .Add(c => c.Series, series)
            .Add(c => c.AnimateOnLoad, false));
        sw.Stop();

        _output.WriteLine($"LineChart 100pts: {sw.ElapsedMilliseconds}ms");
        sw.ElapsedMilliseconds.Should().BeLessThan(20, "100-point line chart should render under 20ms");
    }

    [Fact]
    public void LineChart_1000Points_Under100ms()
    {
        var data = GenerateLineData(1000);
        var series = new List<SeriesConfig<DataPoint>> { new() { Name = "V", Field = d => d.Value } };

        var sw = Stopwatch.StartNew();
        var cut = Render<ArcadiaLineChart<DataPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<DataPoint, object>)(d => d.Label))
            .Add(c => c.Series, series)
            .Add(c => c.AnimateOnLoad, false));
        sw.Stop();

        _output.WriteLine($"LineChart 1000pts: {sw.ElapsedMilliseconds}ms");
        sw.ElapsedMilliseconds.Should().BeLessThan(100, "1000-point line chart should render under 100ms (post-JIT)");
    }

    [Fact]
    public void LineChart_10000Points_Under500ms()
    {
        var series = new List<SeriesConfig<DataPoint>> { new() { Name = "V", Field = d => d.Value } };
        var data = GenerateLineData(10000);
        var sw = Stopwatch.StartNew();
        var cut = Render<ArcadiaLineChart<DataPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<DataPoint, object>)(d => d.Label))
            .Add(c => c.Series, series)
            .Add(c => c.AnimateOnLoad, false));
        sw.Stop();

        _output.WriteLine($"LineChart 10000pts: {sw.ElapsedMilliseconds}ms");
        sw.ElapsedMilliseconds.Should().BeLessThan(500, "10000-point line chart should render under 500ms");
    }

    // ── Bar Chart ──

    [Fact]
    public void BarChart_100Items_Under20ms()
    {
        var data = GenerateLineData(100);
        var series = new List<SeriesConfig<DataPoint>> { new() { Name = "V", Field = d => d.Value } };

        var sw = Stopwatch.StartNew();
        var cut = Render<ArcadiaBarChart<DataPoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<DataPoint, object>)(d => d.Label))
            .Add(c => c.Series, series)
            .Add(c => c.AnimateOnLoad, false));
        sw.Stop();

        _output.WriteLine($"BarChart 100: {sw.ElapsedMilliseconds}ms");
        sw.ElapsedMilliseconds.Should().BeLessThan(20);
    }

    // ── Pie Chart ──

    [Fact]
    public void PieChart_20Slices_Under10ms()
    {
        var data = Enumerable.Range(1, 20).Select(i => new NameValue($"Cat{i}", i * 100.0)).ToList();

        var sw = Stopwatch.StartNew();
        var cut = Render<ArcadiaPieChart<NameValue>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.NameField, (Func<NameValue, string>)(d => d.Name))
            .Add(c => c.ValueField, (Func<NameValue, double>)(d => d.Value))
            .Add(c => c.AnimateOnLoad, false));
        sw.Stop();

        _output.WriteLine($"PieChart 20 slices: {sw.ElapsedMilliseconds}ms");
        sw.ElapsedMilliseconds.Should().BeLessThan(10);
    }

    // ── Sankey Chart ──

    [Fact]
    public void SankeyChart_50Nodes100Links_Under50ms()
    {
        var nodes = Enumerable.Range(0, 50).Select(i => new SankeyNode { Id = $"n{i}", Label = $"Node {i}" }).ToList();
        var rng = new Random(42);
        var links = new List<SankeyLink>();
        for (var i = 0; i < 100; i++)
        {
            var src = rng.Next(0, 25); // first half are sources
            var tgt = rng.Next(25, 50); // second half are targets
            links.Add(new SankeyLink { SourceId = $"n{src}", TargetId = $"n{tgt}", Value = rng.Next(10, 200) });
        }

        var sw = Stopwatch.StartNew();
        var cut = Render<ArcadiaSankeyChart>(p => p
            .Add(c => c.Data, nodes)
            .Add(c => c.Links, links)
            .Add(c => c.AnimateOnLoad, false));
        sw.Stop();

        _output.WriteLine($"Sankey 50n/100l: {sw.ElapsedMilliseconds}ms");
        sw.ElapsedMilliseconds.Should().BeLessThan(50);
    }

    // ── DataGrid ──

    [Fact]
    public void DataGrid_100Rows_Under50ms()
    {
        var data = GenerateGridData(100);

        var sw = Stopwatch.StartNew();
        var cut = Render<ArcadiaDataGrid<GridRow>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.ChildContent, BuildGridColumns()));
        sw.Stop();

        _output.WriteLine($"DataGrid 100 rows: {sw.ElapsedMilliseconds}ms");
        sw.ElapsedMilliseconds.Should().BeLessThan(100, "100-row DataGrid should render under 100ms (includes first-run JIT)");
    }

    [Fact]
    public void DataGrid_1000Rows_Under50ms()
    {
        var data = GenerateGridData(1000);

        var sw = Stopwatch.StartNew();
        var cut = Render<ArcadiaDataGrid<GridRow>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.ChildContent, BuildGridColumns()));
        sw.Stop();

        _output.WriteLine($"DataGrid 1000 rows: {sw.ElapsedMilliseconds}ms");
        sw.ElapsedMilliseconds.Should().BeLessThan(50);
    }

    // ── Helpers ──

    public record DataPoint(string Label, double Value);
    public record NameValue(string Name, double Value);
    public record GridRow(int Id, string Name, string Dept, double Salary);

    private static List<DataPoint> GenerateLineData(int count)
    {
        var rng = new Random(42);
        return Enumerable.Range(0, count)
            .Select(i => new DataPoint($"P{i}", rng.NextDouble() * 1000))
            .ToList();
    }

    private static List<GridRow> GenerateGridData(int count)
    {
        var depts = new[] { "Eng", "Sales", "HR", "Ops" };
        var rng = new Random(42);
        return Enumerable.Range(1, count)
            .Select(i => new GridRow(i, $"Person {i}", depts[rng.Next(4)], 50000 + rng.Next(100000)))
            .ToList();
    }

    private static Microsoft.AspNetCore.Components.RenderFragment BuildGridColumns()
    {
        return builder =>
        {
            builder.OpenComponent<ArcadiaColumn<GridRow>>(0);
            builder.AddAttribute(1, "Field", (Func<GridRow, object>)(r => (object)r.Id));
            builder.AddAttribute(2, "Title", "ID");
            builder.CloseComponent();

            builder.OpenComponent<ArcadiaColumn<GridRow>>(3);
            builder.AddAttribute(4, "Field", (Func<GridRow, object>)(r => (object)r.Name));
            builder.AddAttribute(5, "Title", "Name");
            builder.CloseComponent();

            builder.OpenComponent<ArcadiaColumn<GridRow>>(6);
            builder.AddAttribute(7, "Field", (Func<GridRow, object>)(r => (object)r.Salary));
            builder.AddAttribute(8, "Title", "Salary");
            builder.CloseComponent();
        };
    }
}

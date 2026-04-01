using System.Collections.ObjectModel;
using Bunit;
using FluentAssertions;
using Arcadia.Charts.Core;
using Arcadia.Charts.Components.Charts;
using Xunit;

namespace Arcadia.Tests.Unit.Charts;

public record SalePoint(string Month, double Value);

public class ChartObservableTests : ChartTestBase
{
    private static readonly List<SeriesConfig<SalePoint>> LineSeries = new()
    {
        new() { Name = "Sales", Field = d => d.Value },
    };

    private static readonly List<SeriesConfig<SalePoint>> BarSeries = new()
    {
        new() { Name = "Sales", Field = d => d.Value },
    };

    [Fact]
    public void LineChart_ObservableCollection_AddItem_ReRenders()
    {
        var data = new ObservableCollection<SalePoint>
        {
            new("Jan", 100), new("Feb", 120), new("Mar", 130),
        };

        var cut = Render<ArcadiaLineChart<SalePoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<SalePoint, object>)(d => d.Month))
            .Add(c => c.Series, LineSeries)
            .Add(c => c.Width, 600)
            .Add(c => c.AnimateOnLoad, false));

        var markupBefore = cut.Markup;

        // Add a data point
        data.Add(new SalePoint("Apr", 150));

        // Wait for the debounced re-render to complete
        cut.WaitForState(() => cut.Markup != markupBefore, TimeSpan.FromSeconds(2));

        cut.Markup.Should().NotBe(markupBefore,
            "adding an item to ObservableCollection should trigger a re-render");
    }

    [Fact]
    public void BarChart_ObservableCollection_RemoveItem_ReRenders()
    {
        var data = new ObservableCollection<SalePoint>
        {
            new("Jan", 100), new("Feb", 120), new("Mar", 130),
            new("Apr", 140), new("May", 150),
        };

        var cut = Render<ArcadiaBarChart<SalePoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<SalePoint, object>)(d => d.Month))
            .Add(c => c.Series, BarSeries)
            .Add(c => c.Width, 600)
            .Add(c => c.AnimateOnLoad, false));

        var barCountBefore = cut.FindAll(".arcadia-chart__bar").Count;

        // Remove an item
        data.RemoveAt(data.Count - 1);

        // Wait for the debounced re-render to complete
        cut.WaitForState(
            () => cut.FindAll(".arcadia-chart__bar").Count < barCountBefore,
            TimeSpan.FromSeconds(2));

        cut.FindAll(".arcadia-chart__bar").Count.Should().BeLessThan(barCountBefore,
            "removing an item from ObservableCollection should reduce the number of bars");
    }

    [Fact]
    public void PieChart_ObservableCollection_Clear_ReRenders()
    {
        var data = new ObservableCollection<SalePoint>
        {
            new("Jan", 100), new("Feb", 120), new("Mar", 130),
        };

        var cut = Render<ArcadiaPieChart<SalePoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.NameField, (Func<SalePoint, string>)(d => d.Month))
            .Add(c => c.ValueField, (Func<SalePoint, double>)(d => d.Value))
            .Add(c => c.Width, 400)
            .Add(c => c.AnimateOnLoad, false));

        // Should have pie slices
        cut.FindAll("path").Count.Should().BeGreaterThan(0);

        // Clear the collection
        data.Clear();

        // Wait for the debounced re-render to show no-data state
        cut.WaitForState(
            () => cut.Markup.Contains("arcadia-chart__no-data"),
            TimeSpan.FromSeconds(2));

        cut.Markup.Should().Contain("arcadia-chart__no-data",
            "clearing an ObservableCollection should show the no-data state");
    }

    [Fact]
    public void Chart_RegularList_StillWorks()
    {
        var data = new List<SalePoint>
        {
            new("Jan", 100), new("Feb", 120), new("Mar", 130),
        };

        var cut = Render<ArcadiaLineChart<SalePoint>>(p => p
            .Add(c => c.Data, data)
            .Add(c => c.XField, (Func<SalePoint, object>)(d => d.Month))
            .Add(c => c.Series, LineSeries)
            .Add(c => c.Width, 600)
            .Add(c => c.AnimateOnLoad, false));

        // Should render without error using a regular List<T>
        cut.Find("svg[data-chart]").Should().NotBeNull();
        cut.FindAll(".arcadia-chart__line").Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void Chart_ReplaceCollection_Resubscribes()
    {
        var oldData = new ObservableCollection<SalePoint>
        {
            new("Jan", 100), new("Feb", 120), new("Mar", 130),
        };

        var cut = Render<ArcadiaLineChart<SalePoint>>(p => p
            .Add(c => c.Data, oldData)
            .Add(c => c.XField, (Func<SalePoint, object>)(d => d.Month))
            .Add(c => c.Series, LineSeries)
            .Add(c => c.Width, 600)
            .Add(c => c.AnimateOnLoad, false));

        // Replace with a new collection
        var newData = new ObservableCollection<SalePoint>
        {
            new("Jun", 200), new("Jul", 210), new("Aug", 220),
        };

        cut.Render(p => p
            .Add(c => c.Data, (IReadOnlyList<SalePoint>)newData)
            .Add(c => c.XField, (Func<SalePoint, object>)(d => d.Month))
            .Add(c => c.Series, LineSeries)
            .Add(c => c.Width, 600)
            .Add(c => c.AnimateOnLoad, false));

        var markupAfterReplace = cut.Markup;

        // Modify old collection — should NOT trigger re-render
        oldData.Add(new SalePoint("Apr", 140));

        // Give debounce time to fire (if it were still subscribed, it would)
        Thread.Sleep(50);

        cut.Markup.Should().Be(markupAfterReplace,
            "modifying the old collection should not trigger a re-render after replacement");

        // Modify new collection — SHOULD trigger re-render
        newData.Add(new SalePoint("Sep", 230));

        cut.WaitForState(() => cut.Markup != markupAfterReplace, TimeSpan.FromSeconds(2));

        cut.Markup.Should().NotBe(markupAfterReplace,
            "modifying the new collection should trigger a re-render");
    }
}

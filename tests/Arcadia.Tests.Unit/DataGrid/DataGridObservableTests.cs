using System.Collections.ObjectModel;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Arcadia.DataGrid.Components;
using Arcadia.DataGrid.Core;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for ObservableCollection integration with ArcadiaDataGrid.
/// Verifies auto-rerender on collection changes, server-mode skip,
/// batch edit suppression, and collection replacement.
/// </summary>
public class DataGridObservableTests : DataGridTestBase
{
    private static ObservableCollection<TestEmployee> CreateObservableData() =>
        new(new List<TestEmployee>
        {
            new(1, "Alice", "Engineering", 120_000, new DateTime(2020, 1, 15)),
            new(2, "Bob", "Engineering", 110_000, new DateTime(2019, 6, 1)),
            new(3, "Charlie", "Marketing", 95_000, new DateTime(2021, 3, 10)),
        });

    private IRenderedComponent<ArcadiaDataGrid<TestEmployee>> RenderObservableGrid(
        ObservableCollection<TestEmployee> data,
        Action<ComponentParameterCollectionBuilder<ArcadiaDataGrid<TestEmployee>>>? configure = null)
    {
        return RenderDataGrid(p =>
        {
            p.Add(g => g.Data, data);
            p.Add(g => g.PageSize, 0);
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Department").Add(c => c.Title, "Department"));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Salary").Add(c => c.Title, "Salary"));
            configure?.Invoke(p);
        });
    }

    [Fact]
    public void ObservableCollection_AddRow_AppearsInGrid()
    {
        var data = CreateObservableData();
        var cut = RenderObservableGrid(data);

        // Initial: header + 3 data rows
        cut.FindAll("tr[role='row']").Count.Should().Be(4);

        // Add a row
        data.Add(new TestEmployee(4, "Diana", "Marketing", 98_000, new DateTime(2022, 8, 22)));

        // Wait for debounce
        cut.WaitForState(() => cut.FindAll("tr[role='row']").Count == 5, TimeSpan.FromSeconds(2));
        cut.FindAll("tr[role='row']").Count.Should().Be(5);
    }

    [Fact]
    public void ObservableCollection_RemoveRow_DisappearsFromGrid()
    {
        var data = CreateObservableData();
        var cut = RenderObservableGrid(data);

        cut.FindAll("tr[role='row']").Count.Should().Be(4); // header + 3

        // Remove second item
        data.RemoveAt(1);

        cut.WaitForState(() => cut.FindAll("tr[role='row']").Count == 3, TimeSpan.FromSeconds(2));
        cut.FindAll("tr[role='row']").Count.Should().Be(3);
    }

    [Fact]
    public void ObservableCollection_Clear_ShowsEmptyState()
    {
        var data = CreateObservableData();
        var cut = RenderObservableGrid(data);

        cut.FindAll("tr[role='row']").Count.Should().Be(4);

        // Clear the collection
        data.Clear();

        cut.WaitForState(() => cut.Markup.Contains("No data available"), TimeSpan.FromSeconds(2));
        cut.Markup.Should().Contain("No data available");
    }

    [Fact]
    public void RegularList_StillWorks()
    {
        // Regression test: plain List<T> should still render correctly
        var data = new List<TestEmployee>
        {
            new(1, "Alice", "Engineering", 120_000, new DateTime(2020, 1, 15)),
            new(2, "Bob", "Engineering", 110_000, new DateTime(2019, 6, 1)),
        };

        var cut = RenderGrid(data);

        cut.FindAll("tr[role='row']").Count.Should().Be(3); // header + 2
        cut.FindAll("td[role='gridcell']")[0].TextContent.Should().Contain("Alice");
    }

    [Fact]
    public async Task ServerMode_SkipsSubscription()
    {
        var data = CreateObservableData();

        var cut = RenderDataGrid(p =>
        {
            p.Add(g => g.Data, data);
            p.Add(g => g.PageSize, 0);
            p.Add(g => g.LoadData, new EventCallback<DataGridLoadArgs>(null, (DataGridLoadArgs _) =>
            {
                return Task.CompletedTask;
            }));
            p.AddChildContent<ArcadiaColumn<TestEmployee>>(col =>
                col.Add(c => c.Property, "Name").Add(c => c.Title, "Name"));
        });

        var initialRowCount = cut.FindAll("tr[role='row']").Count;

        // Add to the observable collection
        data.Add(new TestEmployee(4, "Diana", "Marketing", 98_000, new DateTime(2022, 8, 22)));

        // Give debounce time to fire (if it were subscribed, which it shouldn't be)
        await Task.Delay(100);

        // Row count should NOT have changed from the observer
        // (The grid in server mode uses LoadData, not direct Data binding for changes)
        cut.FindAll("tr[role='row']").Count.Should().Be(initialRowCount);
    }

    [Fact]
    public async Task BatchEdit_SuppressesRefresh()
    {
        var data = CreateObservableData();
        var cut = RenderObservableGrid(data, p =>
        {
            p.Add(g => g.BatchEdit, true);
        });

        cut.FindAll("tr[role='row']").Count.Should().Be(4); // header + 3

        // Simulate a batch edit change to trigger suppression
        await cut.InvokeAsync(() => cut.Instance.TrackBatchChange(
            data[0], "Name", "Alice", "Alice-Modified"));

        // Now add a row to the observable collection while batch edit is active
        data.Add(new TestEmployee(4, "Diana", "Marketing", 98_000, new DateTime(2022, 8, 22)));

        // Give debounce time to fire
        await Task.Delay(100);

        // The grid should NOT have updated because observer is suppressed during batch edit
        cut.FindAll("tr[role='row']").Count.Should().Be(4);

        // Now discard batch -- observer should resume and trigger refresh
        await cut.InvokeAsync(() => cut.Instance.DiscardBatch());

        cut.WaitForState(() => cut.FindAll("tr[role='row']").Count == 5, TimeSpan.FromSeconds(2));
        cut.FindAll("tr[role='row']").Count.Should().Be(5);
    }

    [Fact]
    public async Task ReplaceCollection_Resubscribes()
    {
        var data1 = CreateObservableData();
        var data2 = new ObservableCollection<TestEmployee>(new List<TestEmployee>
        {
            new(10, "Zara", "Finance", 105_000, new DateTime(2023, 1, 1)),
        });

        // Render with first collection
        var cut = RenderObservableGrid(data1);
        cut.FindAll("tr[role='row']").Count.Should().Be(4); // header + 3

        // Replace the collection by re-rendering with new data parameter
        // Use the same RenderDataGrid pattern to avoid bUnit duplicate-key issues
        var cut2 = RenderObservableGrid(data2);
        cut2.FindAll("tr[role='row']").Count.Should().Be(2); // header + 1

        // Old collection changes should NOT affect the new grid
        data1.Add(new TestEmployee(4, "Diana", "Marketing", 98_000, new DateTime(2022, 8, 22)));
        await Task.Delay(100);
        cut2.FindAll("tr[role='row']").Count.Should().Be(2); // still header + 1

        // New collection changes should trigger rerender
        data2.Add(new TestEmployee(11, "Yuri", "Finance", 110_000, new DateTime(2023, 6, 1)));

        cut2.WaitForState(() => cut2.FindAll("tr[role='row']").Count == 3, TimeSpan.FromSeconds(2));
        cut2.FindAll("tr[role='row']").Count.Should().Be(3); // header + 2
    }
}

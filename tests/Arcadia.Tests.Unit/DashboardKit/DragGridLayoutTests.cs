using System.Text.Json;
using FluentAssertions;
using Xunit;
using Arcadia.DashboardKit.Models;

namespace Arcadia.Tests.Unit.DashboardKit;

public class DragGridLayoutTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    [Fact]
    public void DragGridLayout_Serialization_RoundTrip()
    {
        var layout = new DragGridLayout
        {
            Items = new List<DragGridItemPosition>
            {
                new() { Id = "panel-1", Order = 0, ColSpan = 2, RowSpan = 1, Locked = false },
                new() { Id = "panel-2", Order = 1, ColSpan = 1, RowSpan = 3, Locked = true },
                new() { Id = "panel-3", Order = 2, ColSpan = 4, RowSpan = 2, Locked = false },
            }
        };

        var json = JsonSerializer.Serialize(layout, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<DragGridLayout>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Items.Should().HaveCount(3);

        deserialized.Items[0].Id.Should().Be("panel-1");
        deserialized.Items[0].ColSpan.Should().Be(2);
        deserialized.Items[0].Locked.Should().BeFalse();

        deserialized.Items[1].Id.Should().Be("panel-2");
        deserialized.Items[1].RowSpan.Should().Be(3);
        deserialized.Items[1].Locked.Should().BeTrue();

        deserialized.Items[2].Order.Should().Be(2);
        deserialized.Items[2].ColSpan.Should().Be(4);
    }

    [Fact]
    public void DragGridItemPosition_DefaultValues()
    {
        var pos = new DragGridItemPosition();

        pos.Id.Should().Be("");
        pos.Order.Should().Be(0);
        pos.ColSpan.Should().Be(1);
        pos.RowSpan.Should().Be(1);
        pos.Locked.Should().BeFalse();
    }

    [Fact]
    public void DragGridItemContext_RecordProperties()
    {
        var context = new DragGridItemContext(ColSpan: 3, RowSpan: 2, IsEditing: true, IsLocked: false);

        context.ColSpan.Should().Be(3);
        context.RowSpan.Should().Be(2);
        context.IsEditing.Should().BeTrue();
        context.IsLocked.Should().BeFalse();

        // Records support value equality
        var same = new DragGridItemContext(3, 2, true, false);
        context.Should().Be(same);

        var different = new DragGridItemContext(1, 1, false, true);
        context.Should().NotBe(different);
    }
}

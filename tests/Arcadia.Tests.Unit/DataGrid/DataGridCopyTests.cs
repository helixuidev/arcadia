using Bunit;
using FluentAssertions;
using Arcadia.DataGrid.Components;
using Xunit;

namespace Arcadia.Tests.Unit.DataGrid;

/// <summary>
/// Tests for copy-with-headers functionality.
/// </summary>
public class DataGridCopyTests : DataGridTestBase
{
    [Fact]
    public void CopyWithHeaders_Parameter_DefaultValue()
    {
        var cut = RenderGrid(SampleData);

        // The TextCopyWithHeaders parameter has a default value of "Copy with Headers"
        // which is used in the context menu / toolbar. Verify the grid renders
        // with the default localization value accessible via the component instance.
        var grid = cut.Instance;
        grid.TextCopyWithHeaders.Should().Be("Copy with Headers");
    }
}

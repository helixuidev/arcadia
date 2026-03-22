using Microsoft.AspNetCore.Components;

namespace Arcadia.FormBuilder.Components.Fields;

/// <summary>
/// Repeating section field that allows adding/removing rows of fields.
/// </summary>
public partial class RepeaterField : FieldBase
{
    [Parameter] public List<Dictionary<string, object?>> Rows { get; set; } = new();
    [Parameter] public EventCallback<List<Dictionary<string, object?>>> RowsChanged { get; set; }
    [Parameter] public RenderFragment<RepeaterRowContext>? RowTemplate { get; set; }
    [Parameter] public int MinRows { get; set; } = 0;
    [Parameter] public int? MaxRows { get; set; }
    [Parameter] public string AddText { get; set; } = "+ Add Row";

    private async Task AddRow()
    {
        Rows.Add(new Dictionary<string, object?>());
        await RowsChanged.InvokeAsync(Rows);
    }

    private async Task RemoveRow(int index)
    {
        if (Rows.Count > MinRows)
        {
            Rows.RemoveAt(index);
            await RowsChanged.InvokeAsync(Rows);
        }
    }
}

/// <summary>
/// Context passed to each repeater row template.
/// </summary>
public class RepeaterRowContext
{
    /// <summary>The zero-based row index.</summary>
    public int Index { get; }
    /// <summary>The row's field values.</summary>
    public Dictionary<string, object?> Values { get; }

    /// <summary>Creates a new repeater row context.</summary>
    public RepeaterRowContext(int index, Dictionary<string, object?> values)
    {
        Index = index;
        Values = values;
    }
}

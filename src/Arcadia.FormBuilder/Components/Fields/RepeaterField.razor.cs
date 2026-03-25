using Microsoft.AspNetCore.Components;

namespace Arcadia.FormBuilder.Components.Fields;

/// <summary>
/// Repeating section field that allows adding/removing rows of fields.
/// </summary>
public partial class RepeaterField : FieldBase
{
    /// <summary>
    /// Gets or sets the collection of row data, where each row is a dictionary of field names to values. Defaults to an empty list. Supports two-way binding.
    /// </summary>
    [Parameter] public List<Dictionary<string, object?>> Rows { get; set; } = new();

    /// <summary>
    /// Callback invoked whenever a row is added or removed, enabling two-way binding with the parent component.
    /// </summary>
    [Parameter] public EventCallback<List<Dictionary<string, object?>>> RowsChanged { get; set; }

    /// <summary>
    /// Gets or sets the template used to render each repeater row. Receives a <see cref="RepeaterRowContext"/> with the row index and values.
    /// This parameter is required for the repeater to display meaningful content.
    /// </summary>
    [Parameter] public RenderFragment<RepeaterRowContext>? RowTemplate { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of rows that must remain in the repeater. Defaults to 0.
    /// The remove button is disabled when the row count equals this value, preventing the user from deleting required entries.
    /// </summary>
    [Parameter] public int MinRows { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum number of rows allowed. Null means unlimited.
    /// When set, the add button is hidden once the row count reaches this limit.
    /// </summary>
    [Parameter] public int? MaxRows { get; set; }

    /// <summary>
    /// Gets or sets the label displayed on the button that adds a new row. Defaults to "+ Add Row".
    /// Customize this to match the domain context (e.g., "+ Add Line Item", "+ Add Contact").
    /// </summary>
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

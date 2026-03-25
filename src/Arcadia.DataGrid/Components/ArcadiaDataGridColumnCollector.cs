namespace Arcadia.DataGrid.Components;

/// <summary>
/// Internal helper that collects column definitions from child ArcadiaColumn components
/// during the first render pass.
/// </summary>
internal class ArcadiaDataGridColumnCollector<TItem>
{
    private readonly List<ArcadiaColumn<TItem>> _columns = new();

    public IReadOnlyList<ArcadiaColumn<TItem>> Columns => _columns;

    public void AddColumn(ArcadiaColumn<TItem> column)
    {
        if (!_columns.Contains(column))
            _columns.Add(column);
    }
}

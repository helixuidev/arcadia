namespace Arcadia.DataGrid.Components;

/// <summary>
/// Internal helper that collects a command column definition from a child ArcadiaCommandColumn component.
/// </summary>
internal class ArcadiaDataGridCommandCollector<TItem>
{
    /// <summary>The registered command column, if any.</summary>
    public ArcadiaCommandColumn<TItem>? CommandColumn { get; private set; }

    /// <summary>Callback invoked when a command column is registered, so the parent grid can re-render.</summary>
    public Action? OnCommandColumnChanged { get; set; }

    /// <summary>Whether a command column has been registered.</summary>
    public bool HasCommandColumn => CommandColumn is not null;

    public void Register(ArcadiaCommandColumn<TItem> commandColumn)
    {
        if (CommandColumn is null)
        {
            CommandColumn = commandColumn;
            OnCommandColumnChanged?.Invoke();
        }
    }
}

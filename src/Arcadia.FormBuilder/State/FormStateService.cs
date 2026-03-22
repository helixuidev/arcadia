namespace Arcadia.FormBuilder.State;

/// <summary>
/// Manages form state with undo/redo and optional auto-save to persistent storage.
/// </summary>
public class FormStateService : IDisposable
{
    private readonly FormState _state;
    private readonly IFormPersistence? _persistence;
    private readonly string? _formId;
    private readonly int _maxUndoDepth;

    private readonly List<Dictionary<string, object?>> _undoStack = new();
    private readonly List<Dictionary<string, object?>> _redoStack = new();
    private Timer? _autoSaveTimer;
    private bool _isDirty;

    /// <summary>
    /// Creates a new FormStateService.
    /// </summary>
    /// <param name="state">The form state to manage.</param>
    /// <param name="persistence">Optional persistence adapter for auto-save.</param>
    /// <param name="formId">Unique form identifier for persistence.</param>
    /// <param name="maxUndoDepth">Maximum undo history depth.</param>
    public FormStateService(FormState state, IFormPersistence? persistence = null, string? formId = null, int maxUndoDepth = 50)
    {
        _state = state;
        _persistence = persistence;
        _formId = formId;
        _maxUndoDepth = maxUndoDepth;

        _state.OnValuesChanged += HandleValuesChanged;
    }

    /// <summary>
    /// Gets the managed form state.
    /// </summary>
    public FormState State => _state;

    /// <summary>
    /// Gets whether there are undoable changes.
    /// </summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>
    /// Gets whether there are redoable changes.
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Gets whether the form has unsaved changes.
    /// </summary>
    public bool IsDirty => _isDirty;

    /// <summary>
    /// Raised when undo/redo availability changes.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Starts auto-saving at the specified interval.
    /// </summary>
    /// <param name="intervalMs">Auto-save interval in milliseconds.</param>
    public void EnableAutoSave(int intervalMs = 5000)
    {
        if (_persistence is null || _formId is null) return;

        _autoSaveTimer?.Dispose();
        _autoSaveTimer = new Timer(async _ =>
        {
            if (_isDirty)
            {
                await SaveAsync();
            }
        }, null, intervalMs, intervalMs);
    }

    /// <summary>
    /// Saves the current state to persistence immediately.
    /// </summary>
    public async Task SaveAsync()
    {
        if (_persistence is not null && _formId is not null)
        {
            await _persistence.SaveAsync(_formId, _state.Values);
            _isDirty = false;
        }
    }

    /// <summary>
    /// Loads state from persistence, restoring previously saved values.
    /// </summary>
    public async Task LoadAsync()
    {
        if (_persistence is not null && _formId is not null)
        {
            var values = await _persistence.LoadAsync(_formId);
            if (values is not null)
            {
                foreach (var (key, value) in values)
                {
                    _state.SetValue(key, value);
                }
                _isDirty = false;
            }
        }
    }

    /// <summary>
    /// Clears persisted state.
    /// </summary>
    public async Task ClearPersistedAsync()
    {
        if (_persistence is not null && _formId is not null)
        {
            await _persistence.ClearAsync(_formId);
        }
    }

    /// <summary>
    /// Undoes the last change.
    /// </summary>
    public void Undo()
    {
        if (_undoStack.Count == 0) return;

        // Save current state to redo
        _redoStack.Add(SnapshotValues());

        // Restore previous state
        var previous = _undoStack[^1];
        _undoStack.RemoveAt(_undoStack.Count - 1);
        RestoreValues(previous);

        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Redoes the last undone change.
    /// </summary>
    public void Redo()
    {
        if (_redoStack.Count == 0) return;

        // Save current state to undo
        _undoStack.Add(SnapshotValues());

        // Restore redo state
        var next = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);
        RestoreValues(next);

        OnStateChanged?.Invoke();
    }

    private void HandleValuesChanged()
    {
        // Push current state to undo stack before the change
        _undoStack.Add(SnapshotValues());
        _redoStack.Clear(); // New change invalidates redo history

        // Trim undo stack if too deep
        while (_undoStack.Count > _maxUndoDepth)
        {
            _undoStack.RemoveAt(0);
        }

        _isDirty = true;
        OnStateChanged?.Invoke();
    }

    private Dictionary<string, object?> SnapshotValues()
    {
        return new Dictionary<string, object?>(_state.Values);
    }

    private void RestoreValues(Dictionary<string, object?> values)
    {
        // Temporarily unhook to avoid re-pushing to undo stack
        _state.OnValuesChanged -= HandleValuesChanged;

        foreach (var (key, value) in values)
        {
            _state.SetValue(key, value);
        }

        _state.OnValuesChanged += HandleValuesChanged;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _autoSaveTimer?.Dispose();
        _state.OnValuesChanged -= HandleValuesChanged;
    }
}

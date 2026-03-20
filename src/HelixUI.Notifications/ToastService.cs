namespace HelixUI.Notifications;

/// <summary>
/// Service for imperatively showing and dismissing toast notifications.
/// Register as a scoped service in DI.
/// </summary>
public class ToastService
{
    private readonly List<ToastModel> _toasts = new();

    /// <summary>
    /// Gets the currently active toasts.
    /// </summary>
    public IReadOnlyList<ToastModel> Toasts => _toasts;

    /// <summary>
    /// Raised when the toast list changes (add or remove).
    /// </summary>
    public event Action? OnToastsChanged;

    /// <summary>
    /// Shows a toast with the specified parameters.
    /// </summary>
    /// <param name="message">The toast message.</param>
    /// <param name="level">The severity level.</param>
    /// <param name="title">Optional title.</param>
    /// <param name="duration">Auto-dismiss duration in ms. 0 = no auto-dismiss.</param>
    /// <returns>The ID of the created toast.</returns>
    public string Show(string message, ToastLevel level = ToastLevel.Info, string? title = null, int duration = 5000)
    {
        var toast = new ToastModel
        {
            Message = message,
            Level = level,
            Title = title,
            Duration = duration
        };

        _toasts.Add(toast);
        OnToastsChanged?.Invoke();
        return toast.Id;
    }

    /// <summary>
    /// Shows an info toast.
    /// </summary>
    public string ShowInfo(string message, string? title = null, int duration = 5000) =>
        Show(message, ToastLevel.Info, title, duration);

    /// <summary>
    /// Shows a success toast.
    /// </summary>
    public string ShowSuccess(string message, string? title = null, int duration = 5000) =>
        Show(message, ToastLevel.Success, title, duration);

    /// <summary>
    /// Shows a warning toast.
    /// </summary>
    public string ShowWarning(string message, string? title = null, int duration = 5000) =>
        Show(message, ToastLevel.Warning, title, duration);

    /// <summary>
    /// Shows an error toast. Defaults to no auto-dismiss since errors require attention.
    /// </summary>
    public string ShowError(string message, string? title = null, int duration = 0) =>
        Show(message, ToastLevel.Error, title, duration);

    /// <summary>
    /// Dismisses a specific toast by ID.
    /// </summary>
    /// <param name="toastId">The toast ID to dismiss.</param>
    public void Dismiss(string toastId)
    {
        var removed = _toasts.RemoveAll(t => t.Id == toastId);
        if (removed > 0)
        {
            OnToastsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Dismisses all active toasts.
    /// </summary>
    public void DismissAll()
    {
        if (_toasts.Count > 0)
        {
            _toasts.Clear();
            OnToastsChanged?.Invoke();
        }
    }
}

using Microsoft.AspNetCore.Components;
using HelixUI.Core.Utilities;

namespace HelixUI.Notifications;

/// <summary>
/// Container component that renders and positions all active toast notifications.
/// Place once in your app layout (typically inside ThemeProvider).
/// Auto-dismisses toasts based on their <see cref="ToastModel.Duration"/>.
/// </summary>
public partial class HelixToastContainer : Core.Base.HelixComponentBase, IDisposable
{
    [Inject]
    private ToastService ToastService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the screen position for toasts.
    /// </summary>
    [Parameter]
    public ToastPosition Position { get; set; } = ToastPosition.TopRight;

    /// <summary>
    /// Gets or sets the maximum number of visible toasts. Oldest are dismissed when exceeded.
    /// </summary>
    [Parameter]
    public int MaxVisible { get; set; } = 5;

    private string? _announcement;
    private readonly Dictionary<string, Timer> _timers = new();

    private string? CssClass => CssBuilder.Default("helix-toast-container")
        .AddClass($"helix-toast-container--{PositionClass}")
        .AddClass(Class)
        .Build();

    private string PositionClass => Position switch
    {
        ToastPosition.TopRight => "top-right",
        ToastPosition.TopLeft => "top-left",
        ToastPosition.TopCenter => "top-center",
        ToastPosition.BottomRight => "bottom-right",
        ToastPosition.BottomLeft => "bottom-left",
        ToastPosition.BottomCenter => "bottom-center",
        _ => "top-right"
    };

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        ToastService.OnToastsChanged += HandleToastsChanged;
    }

    private void HandleToastsChanged()
    {
        InvokeAsync(() =>
        {
            EnforceMaxVisible();
            ScheduleAutoDismiss();
            UpdateAnnouncement();
            StateHasChanged();
        });
    }

    private void EnforceMaxVisible()
    {
        while (ToastService.Toasts.Count > MaxVisible)
        {
            var oldest = ToastService.Toasts[0];
            CancelTimer(oldest.Id);
            ToastService.Dismiss(oldest.Id);
        }
    }

    private void ScheduleAutoDismiss()
    {
        foreach (var toast in ToastService.Toasts)
        {
            if (toast.Duration > 0 && !_timers.ContainsKey(toast.Id))
            {
                var id = toast.Id;
                var timer = new Timer(_ =>
                {
                    InvokeAsync(() =>
                    {
                        CancelTimer(id);
                        ToastService.Dismiss(id);
                    });
                }, null, toast.Duration, Timeout.Infinite);

                _timers[id] = timer;
            }
        }
    }

    private void UpdateAnnouncement()
    {
        var latest = ToastService.Toasts.LastOrDefault();
        if (latest is not null)
        {
            _announcement = string.IsNullOrEmpty(latest.Title)
                ? latest.Message
                : $"{latest.Title}: {latest.Message}";
        }
    }

    private void Dismiss(string toastId)
    {
        CancelTimer(toastId);
        ToastService.Dismiss(toastId);
    }

    private void CancelTimer(string toastId)
    {
        if (_timers.Remove(toastId, out var timer))
        {
            timer.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ToastService.OnToastsChanged -= HandleToastsChanged;

        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
        }
        _timers.Clear();
    }
}

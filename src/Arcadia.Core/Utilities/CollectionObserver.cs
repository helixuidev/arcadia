using System.Collections.Specialized;

namespace Arcadia.Core.Utilities;

/// <summary>
/// Observes an <see cref="IReadOnlyList{T}"/> for <see cref="INotifyCollectionChanged"/> and invokes a callback
/// with debouncing. Thread-safe for Blazor Server (SignalR callbacks) via caller-provided <c>InvokeAsync</c>.
/// </summary>
/// <typeparam name="T">The element type of the observed collection.</typeparam>
public sealed class CollectionObserver<T> : IDisposable
{
    private INotifyCollectionChanged? _observed;
    private readonly Func<Func<Task>, Task> _invokeAsync;
    private readonly Func<Task> _onChanged;
    private CancellationTokenSource? _debounceCts;
    private readonly int _debounceMs;
    private bool _disposed;
    private bool _suppressed;

    /// <summary>
    /// Creates a new <see cref="CollectionObserver{T}"/>.
    /// </summary>
    /// <param name="onChanged">Callback invoked when the collection changes (after debounce window).</param>
    /// <param name="invokeAsync">Blazor's <c>InvokeAsync</c> delegate for thread marshaling.</param>
    /// <param name="debounceMs">Debounce window in milliseconds (default 16 ≈ 1 frame at 60 fps).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onChanged"/> or <paramref name="invokeAsync"/> is null.</exception>
    public CollectionObserver(Func<Task> onChanged, Func<Func<Task>, Task> invokeAsync, int debounceMs = 16)
    {
        _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
        _invokeAsync = invokeAsync ?? throw new ArgumentNullException(nameof(invokeAsync));
        _debounceMs = debounceMs;
    }

    /// <summary>
    /// Attaches to a collection. Detaches from any previously observed collection first.
    /// If the collection implements <see cref="INotifyCollectionChanged"/>, subscribes to change notifications.
    /// </summary>
    /// <param name="data">The collection to observe. May be null.</param>
    public void Attach(IReadOnlyList<T>? data)
    {
        Detach();
        if (data is INotifyCollectionChanged observable)
        {
            _observed = observable;
            _observed.CollectionChanged += OnCollectionChanged;
        }
    }

    /// <summary>
    /// Detaches from the current collection and cancels any pending debounce timer.
    /// </summary>
    public void Detach()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;
        if (_observed is not null)
        {
            _observed.CollectionChanged -= OnCollectionChanged;
            _observed = null;
        }
    }

    /// <summary>
    /// Suppresses change notifications (e.g., during a batch edit operation).
    /// </summary>
    public void Suppress() => _suppressed = true;

    /// <summary>
    /// Resumes change notifications after suppression.
    /// </summary>
    /// <param name="triggerImmediately">If true, fires the callback immediately upon resuming.</param>
    public void Resume(bool triggerImmediately = false)
    {
        _suppressed = false;
        if (triggerImmediately)
        {
            _ = FireCallback();
        }
    }

    /// <summary>
    /// Gets a value indicating whether this observer is currently attached to an observable collection.
    /// </summary>
    public bool IsAttached => _observed is not null;

    private async void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_disposed || _suppressed) return;

        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        try
        {
            await Task.Delay(_debounceMs, token);
            if (!_disposed && !_suppressed)
            {
                await FireCallback();
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when debounce is cancelled by a subsequent change or disposal.
        }
        catch (ObjectDisposedException)
        {
            // Expected when CTS is disposed during shutdown.
        }
    }

    private Task FireCallback() => _invokeAsync(_onChanged);

    /// <summary>
    /// Disposes the observer, detaching from the collection and cancelling any pending debounce.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Detach();
    }
}

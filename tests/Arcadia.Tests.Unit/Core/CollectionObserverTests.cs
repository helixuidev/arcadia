using System.Collections.ObjectModel;
using Arcadia.Core.Utilities;
using FluentAssertions;
using Xunit;

namespace Arcadia.Tests.Unit.Core;

public class CollectionObserverTests : IDisposable
{
    private int _callbackCount;
    private readonly Func<Task> _callback;
    private readonly Func<Func<Task>, Task> _invokeAsync;

    public CollectionObserverTests()
    {
        _callbackCount = 0;
        _callback = () =>
        {
            Interlocked.Increment(ref _callbackCount);
            return Task.CompletedTask;
        };
        // In tests, just invoke directly (no Blazor dispatcher needed).
        _invokeAsync = fn => fn();
    }

    public void Dispose()
    {
        // No-op; individual tests dispose their observers.
    }

    /// <summary>
    /// Polls until <paramref name="predicate"/> is true or <paramref name="timeoutMs"/> elapses.
    /// Replaces bare Task.Delay waits which were flaky on loaded CI runners —
    /// the debounced observer fires via thread-pool continuation and scheduling
    /// can take 100ms+ under load, so fixed 50ms waits were inherently racy.
    /// </summary>
    private static async Task WaitFor(Func<bool> predicate, int timeoutMs = 2000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            if (predicate()) return;
            await Task.Delay(10);
        }
    }

    [Fact]
    public void Attach_NonObservable_NoSubscription()
    {
        // Arrange
        using var observer = new CollectionObserver<int>(_callback, _invokeAsync);
        var list = new List<int> { 1, 2, 3 };

        // Act
        observer.Attach(list);

        // Assert
        observer.IsAttached.Should().BeFalse();
    }

    [Fact]
    public void Attach_Observable_IsAttached()
    {
        // Arrange
        using var observer = new CollectionObserver<int>(_callback, _invokeAsync);
        var collection = new ObservableCollection<int> { 1, 2, 3 };

        // Act
        observer.Attach(collection);

        // Assert
        observer.IsAttached.Should().BeTrue();
    }

    [Fact]
    public async Task Add_TriggersCallback()
    {
        // Arrange
        using var observer = new CollectionObserver<int>(_callback, _invokeAsync, debounceMs: 5);
        var collection = new ObservableCollection<int>();
        observer.Attach(collection);

        // Act
        collection.Add(42);
        await WaitFor(() => _callbackCount >= 1);

        // Assert
        _callbackCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task Remove_TriggersCallback()
    {
        // Arrange
        using var observer = new CollectionObserver<int>(_callback, _invokeAsync, debounceMs: 5);
        var collection = new ObservableCollection<int> { 1, 2, 3 };
        observer.Attach(collection);

        // Act
        collection.Remove(2);
        await WaitFor(() => _callbackCount >= 1);

        // Assert
        _callbackCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task Debounce_BatchesRapidChanges()
    {
        // Arrange
        using var observer = new CollectionObserver<int>(_callback, _invokeAsync, debounceMs: 10);
        var collection = new ObservableCollection<int>();
        observer.Attach(collection);

        // Act — add 50 items rapidly
        for (int i = 0; i < 50; i++)
        {
            collection.Add(i);
        }

        await Task.Delay(100);

        // Assert — should batch into far fewer than 50 callbacks
        _callbackCount.Should().BeInRange(1, 5);
    }

    [Fact]
    public async Task Detach_StopsNotifications()
    {
        // Arrange
        using var observer = new CollectionObserver<int>(_callback, _invokeAsync, debounceMs: 5);
        var collection = new ObservableCollection<int>();
        observer.Attach(collection);

        // Act
        observer.Detach();
        collection.Add(42);
        await Task.Delay(50);

        // Assert
        _callbackCount.Should().Be(0);
        observer.IsAttached.Should().BeFalse();
    }

    [Fact]
    public async Task Dispose_StopsNotifications()
    {
        // Arrange
        var observer = new CollectionObserver<int>(_callback, _invokeAsync, debounceMs: 5);
        var collection = new ObservableCollection<int>();
        observer.Attach(collection);

        // Act
        observer.Dispose();
        collection.Add(42);
        await Task.Delay(50);

        // Assert
        _callbackCount.Should().Be(0);
    }

    [Fact]
    public async Task Reattach_NewCollection()
    {
        // Arrange
        using var observer = new CollectionObserver<int>(_callback, _invokeAsync, debounceMs: 5);
        var collectionA = new ObservableCollection<int>();
        var collectionB = new ObservableCollection<int>();

        // Act — attach to A, then reattach to B
        observer.Attach(collectionA);
        observer.Attach(collectionB);

        // Modify B → should fire callback
        collectionB.Add(1);
        await WaitFor(() => _callbackCount >= 1);
        var countAfterB = _callbackCount;
        countAfterB.Should().BeGreaterOrEqualTo(1);

        // Modify A → should NOT fire callback
        collectionA.Add(99);
        await Task.Delay(50);

        // Assert
        _callbackCount.Should().Be(countAfterB);
    }

    [Fact]
    public async Task Suppress_BlocksCallback()
    {
        // Arrange
        using var observer = new CollectionObserver<int>(_callback, _invokeAsync, debounceMs: 5);
        var collection = new ObservableCollection<int>();
        observer.Attach(collection);

        // Act — suppress, add item, verify no callback
        observer.Suppress();
        collection.Add(1);
        await Task.Delay(50);
        _callbackCount.Should().Be(0);

        // Resume, add another item, verify callback fires
        observer.Resume();
        collection.Add(2);
        await WaitFor(() => _callbackCount >= 1);
        _callbackCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task Dispose_DuringPending_NoCrash()
    {
        // Arrange
        var observer = new CollectionObserver<int>(_callback, _invokeAsync, debounceMs: 50);
        var collection = new ObservableCollection<int>();
        observer.Attach(collection);

        // Act — trigger a debounce, then immediately dispose
        collection.Add(1);

        // Should not throw
        var act = () =>
        {
            observer.Dispose();
            return Task.CompletedTask;
        };

        await act.Should().NotThrowAsync();

        // Give time for any pending tasks to complete/cancel
        await Task.Delay(100);

        // No crash = success
    }
}

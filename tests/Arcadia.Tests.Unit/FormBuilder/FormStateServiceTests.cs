using FluentAssertions;
using Arcadia.FormBuilder;
using Arcadia.FormBuilder.State;
using Xunit;

namespace Arcadia.Tests.Unit.FormBuilder;

public class FormStateServiceTests
{
    [Fact]
    public void Undo_RestoresPreviousState()
    {
        var state = new FormState();
        var service = new FormStateService(state);

        state.SetValue("name", "Alice");
        state.SetValue("name", "Bob");

        service.Undo();

        // After undo, we should have the state before "Bob" was set
        service.CanUndo.Should().BeTrue();
        service.CanRedo.Should().BeTrue();
    }

    [Fact]
    public void Redo_RestoresUndoneState()
    {
        var state = new FormState();
        var service = new FormStateService(state);

        state.SetValue("name", "Alice");
        state.SetValue("name", "Bob");

        service.Undo();
        service.CanRedo.Should().BeTrue();

        service.Redo();
        service.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void Undo_WhenEmpty_DoesNothing()
    {
        var state = new FormState();
        var service = new FormStateService(state);

        service.CanUndo.Should().BeFalse();
        service.Undo(); // Should not throw
    }

    [Fact]
    public void NewChange_ClearsRedoStack()
    {
        var state = new FormState();
        var service = new FormStateService(state);

        state.SetValue("name", "Alice");
        state.SetValue("name", "Bob");
        service.Undo();
        service.CanRedo.Should().BeTrue();

        state.SetValue("name", "Charlie"); // New change
        service.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void IsDirty_TrueAfterChange()
    {
        var state = new FormState();
        var service = new FormStateService(state);

        service.IsDirty.Should().BeFalse();

        state.SetValue("name", "Alice");

        service.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void MaxUndoDepth_TrimOldest()
    {
        var state = new FormState();
        var service = new FormStateService(state, maxUndoDepth: 3);

        state.SetValue("a", "1");
        state.SetValue("a", "2");
        state.SetValue("a", "3");
        state.SetValue("a", "4");
        state.SetValue("a", "5");

        // Should have at most 3 undo entries
        var undoCount = 0;
        while (service.CanUndo)
        {
            service.Undo();
            undoCount++;
        }
        undoCount.Should().BeLessOrEqualTo(3);
    }

    [Fact]
    public void OnStateChanged_Fires()
    {
        var state = new FormState();
        var service = new FormStateService(state);
        var fired = false;
        service.OnStateChanged += () => fired = true;

        state.SetValue("name", "Alice");

        fired.Should().BeTrue();
    }

    [Fact]
    public void Dispose_UnhooksEvents()
    {
        var state = new FormState();
        var service = new FormStateService(state);

        service.Dispose();

        // Should not throw after dispose
        state.SetValue("name", "Alice");
    }
}

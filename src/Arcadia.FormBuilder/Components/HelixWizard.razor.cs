using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;

namespace Arcadia.FormBuilder.Components;

/// <summary>
/// Multi-step wizard container. Manages step navigation, per-step validation,
/// progress tracking, and step status indicators.
/// </summary>
public partial class HelixWizard : Core.Base.HelixComponentBase
{
    private readonly List<HelixWizardStep> _steps = new();
    private int _highestReached;

    /// <summary>
    /// Gets or sets the child content (HelixWizardStep components).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the current (zero-based) step index.
    /// </summary>
    [Parameter]
    public int CurrentStep { get; set; }

    /// <summary>
    /// Gets or sets the callback fired when the current step changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> CurrentStepChanged { get; set; }

    /// <summary>
    /// Gets or sets whether steps must be completed in order.
    /// </summary>
    [Parameter]
    public bool Linear { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the progress bar.
    /// </summary>
    [Parameter]
    public bool ShowProgress { get; set; } = true;

    /// <summary>
    /// Gets or sets the callback fired on final step completion.
    /// </summary>
    [Parameter]
    public EventCallback OnComplete { get; set; }

    /// <summary>
    /// Gets or sets the callback fired before advancing to the next step.
    /// Return false to block navigation (e.g., validation failed).
    /// </summary>
    [Parameter]
    public Func<int, Task<bool>>? OnStepValidate { get; set; }

    [Parameter] public string PreviousText { get; set; } = "Previous";
    [Parameter] public string NextText { get; set; } = "Next";
    [Parameter] public string CompleteText { get; set; } = "Complete";

    /// <summary>
    /// Gets the current progress percentage.
    /// </summary>
    public int ProgressPercent => _steps.Count > 0
        ? (int)((CurrentStep + 1) / (double)_steps.Count * 100)
        : 0;

    private string? CssClass => CssBuilder.Default("arcadia-wizard")
        .AddClass(Class)
        .Build();

    internal void RegisterStep(HelixWizardStep step)
    {
        if (!_steps.Contains(step))
        {
            _steps.Add(step);
            StateHasChanged();
        }
    }

    internal void UnregisterStep(HelixWizardStep step)
    {
        _steps.Remove(step);
    }

    internal bool IsStepVisible(HelixWizardStep step)
    {
        var index = _steps.IndexOf(step);
        return index == CurrentStep;
    }

    private async Task Next()
    {
        if (OnStepValidate is not null)
        {
            var isValid = await OnStepValidate(CurrentStep);
            if (!isValid) return;
        }

        if (CurrentStep < _steps.Count - 1)
        {
            CurrentStep++;
            if (CurrentStep > _highestReached)
                _highestReached = CurrentStep;
            await CurrentStepChanged.InvokeAsync(CurrentStep);
        }
    }

    private async Task Previous()
    {
        if (CurrentStep > 0)
        {
            CurrentStep--;
            await CurrentStepChanged.InvokeAsync(CurrentStep);
        }
    }

    private async Task GoToStep(int index)
    {
        if (Linear && index > _highestReached) return;

        CurrentStep = index;
        await CurrentStepChanged.InvokeAsync(CurrentStep);
    }

    private async Task Complete()
    {
        if (OnStepValidate is not null)
        {
            var isValid = await OnStepValidate(CurrentStep);
            if (!isValid) return;
        }

        await OnComplete.InvokeAsync();
    }
}

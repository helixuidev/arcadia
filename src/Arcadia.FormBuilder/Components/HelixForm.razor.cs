using Microsoft.AspNetCore.Components;
using Arcadia.Core.Utilities;
using Arcadia.FormBuilder.Schema;
using Arcadia.FormBuilder.Validation;

namespace Arcadia.FormBuilder.Components;

/// <summary>
/// Main form wrapper that manages form state, validation, and submission.
/// Cascades <see cref="FormState"/> to all child field components.
/// </summary>
public partial class HelixForm : Core.Base.HelixComponentBase
{
    /// <summary>
    /// Gets or sets the child content (field components).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the form schema for validation.
    /// </summary>
    [Parameter]
    public FormSchema? Schema { get; set; }

    /// <summary>
    /// Gets or sets the form state. If not provided, one is created internally.
    /// </summary>
    [Parameter]
    public FormState? State { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked on valid form submission.
    /// </summary>
    [Parameter]
    public EventCallback<FormState> OnValidSubmit { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked on invalid form submission.
    /// </summary>
    [Parameter]
    public EventCallback<FormState> OnInvalidSubmit { get; set; }

    /// <summary>
    /// Gets or sets whether to show the error summary at the top.
    /// </summary>
    [Parameter]
    public bool ShowErrorSummary { get; set; } = true;

    /// <summary>
    /// Gets or sets the error summary heading text.
    /// </summary>
    [Parameter]
    public string ErrorSummaryTitle { get; set; } = "Please fix the following errors:";

    /// <summary>
    /// Gets or sets custom field validators.
    /// </summary>
    [Parameter]
    public IReadOnlyList<IFieldValidator>? Validators { get; set; }

    private bool HasErrors => State?.HasErrors ?? false;

    private string? CssClass => CssBuilder.Default("arcadia-form")
        .AddClass("arcadia-form--has-errors", HasErrors)
        .AddClass(Class)
        .Build();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        State ??= new FormState();

        if (Schema is not null)
        {
            State.InitializeDefaults(Schema);
        }
    }

    private async Task HandleSubmit()
    {
        if (State is null) return;

        State.MarkSubmitted();
        State.ClearErrors();

        // Run schema validation
        if (Schema is not null)
        {
            foreach (var field in Schema.AllFields)
            {
                var value = State.GetValue(field.Name);
                var errors = FieldValidator.Validate(field, value);

                // Run custom validators
                if (Validators is not null)
                {
                    foreach (var validator in Validators)
                    {
                        errors.AddRange(validator.Validate(field.Name, value, State.Values));
                    }
                }

                if (errors.Count > 0)
                {
                    State.SetErrors(field.Name, errors);
                }
            }
        }

        if (State.HasErrors)
        {
            await OnInvalidSubmit.InvokeAsync(State);
        }
        else
        {
            await OnValidSubmit.InvokeAsync(State);
        }
    }
}

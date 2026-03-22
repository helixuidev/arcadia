using Microsoft.AspNetCore.Components;
using Arcadia.FormBuilder.Schema;
using Arcadia.FormBuilder.Validation;

namespace Arcadia.FormBuilder.Components;

/// <summary>
/// Dynamically renders a complete form from a <see cref="FormSchema"/>.
/// Handles field rendering, conditional visibility, validation, and submission.
/// </summary>
public partial class HelixFormBuilder : Core.Base.HelixComponentBase
{
    /// <summary>
    /// Gets or sets the form schema that defines the form structure.
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
    /// Gets or sets the callback invoked when cancel is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// Gets or sets whether to show the error summary.
    /// </summary>
    [Parameter]
    public bool ShowErrorSummary { get; set; } = true;

    /// <summary>
    /// Gets or sets custom field validators.
    /// </summary>
    [Parameter]
    public IReadOnlyList<IFieldValidator>? Validators { get; set; }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        State ??= new FormState();

        if (Schema is not null)
        {
            State.InitializeDefaults(Schema);
        }
    }

    private bool IsFieldVisible(FieldSchema field)
    {
        if (field.Conditions is null || field.Conditions.Count == 0)
            return true;

        foreach (var condition in field.Conditions)
        {
            var result = condition.Evaluate(State!.Values);

            switch (condition.Action)
            {
                case ConditionalAction.Show:
                    if (!result) return false;
                    break;
                case ConditionalAction.Hide:
                    if (result) return false;
                    break;
            }
        }

        return true;
    }

    private void HandleFieldChange(string fieldName, object? value)
    {
        State!.SetValue(fieldName, value);
        StateHasChanged(); // Re-evaluate conditional visibility
    }

    private RenderFragment RenderField(FieldSchema field) => builder =>
    {
        if (!IsFieldVisible(field))
            return;

        var spanStyle = field.ColumnSpan < 12
            ? $"grid-column: span {field.ColumnSpan};"
            : null;

        if (spanStyle is not null)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "style", spanStyle);
        }

        var errors = State!.IsSubmitted ? State.GetErrors(field.Name) : Array.Empty<string>();
        var value = State.GetValue(field.Name);

        switch (field.Type)
        {
            case FieldType.Text:
                builder.OpenComponent<Fields.TextField>(10);
                builder.AddAttribute(11, "Schema", field);
                builder.AddAttribute(12, "Value", value?.ToString());
                builder.AddAttribute(13, "ValueChanged",
                    EventCallback.Factory.Create<string?>(this, v => HandleFieldChange(field.Name, v)));
                builder.AddAttribute(14, "Errors", errors);
                builder.CloseComponent();
                break;

            case FieldType.Number:
                builder.OpenComponent<Fields.NumberField>(10);
                builder.AddAttribute(11, "Schema", field);
                builder.AddAttribute(12, "Value", value is double d ? d : (double?)null);
                builder.AddAttribute(13, "ValueChanged",
                    EventCallback.Factory.Create<double?>(this, v => HandleFieldChange(field.Name, v)));
                builder.AddAttribute(14, "Errors", errors);
                if (field.Validation?.Min.HasValue == true)
                    builder.AddAttribute(15, "Min", field.Validation.Min);
                if (field.Validation?.Max.HasValue == true)
                    builder.AddAttribute(16, "Max", field.Validation.Max);
                builder.CloseComponent();
                break;

            case FieldType.TextArea:
                builder.OpenComponent<Fields.TextAreaField>(10);
                builder.AddAttribute(11, "Schema", field);
                builder.AddAttribute(12, "Value", value?.ToString());
                builder.AddAttribute(13, "ValueChanged",
                    EventCallback.Factory.Create<string?>(this, v => HandleFieldChange(field.Name, v)));
                builder.AddAttribute(14, "Errors", errors);
                builder.CloseComponent();
                break;

            case FieldType.Select:
                builder.OpenComponent<Fields.SelectField>(10);
                builder.AddAttribute(11, "Schema", field);
                builder.AddAttribute(12, "Value", value?.ToString());
                builder.AddAttribute(13, "ValueChanged",
                    EventCallback.Factory.Create<string?>(this, v => HandleFieldChange(field.Name, v)));
                builder.AddAttribute(14, "Errors", errors);
                builder.CloseComponent();
                break;

            case FieldType.Checkbox:
                builder.OpenComponent<Fields.CheckboxField>(10);
                builder.AddAttribute(11, "Schema", field);
                builder.AddAttribute(12, "Value", value is true);
                builder.AddAttribute(13, "ValueChanged",
                    EventCallback.Factory.Create<bool>(this, v => HandleFieldChange(field.Name, v)));
                builder.AddAttribute(14, "Errors", errors);
                builder.CloseComponent();
                break;

            case FieldType.RadioGroup:
                builder.OpenComponent<Fields.RadioGroupField>(10);
                builder.AddAttribute(11, "Schema", field);
                builder.AddAttribute(12, "Value", value?.ToString());
                builder.AddAttribute(13, "ValueChanged",
                    EventCallback.Factory.Create<string?>(this, v => HandleFieldChange(field.Name, v)));
                builder.AddAttribute(14, "Errors", errors);
                builder.CloseComponent();
                break;

            case FieldType.Date:
                builder.OpenComponent<Fields.DateField>(10);
                builder.AddAttribute(11, "Schema", field);
                builder.AddAttribute(12, "Value", value is DateTime dt ? dt : (DateTime?)null);
                builder.AddAttribute(13, "ValueChanged",
                    EventCallback.Factory.Create<DateTime?>(this, v => HandleFieldChange(field.Name, v)));
                builder.AddAttribute(14, "Errors", errors);
                builder.CloseComponent();
                break;

            case FieldType.Switch:
                builder.OpenComponent<Fields.SwitchField>(10);
                builder.AddAttribute(11, "Schema", field);
                builder.AddAttribute(12, "Value", value is true);
                builder.AddAttribute(13, "ValueChanged",
                    EventCallback.Factory.Create<bool>(this, v => HandleFieldChange(field.Name, v)));
                builder.AddAttribute(14, "Errors", errors);
                builder.CloseComponent();
                break;

            case FieldType.Autocomplete:
                builder.OpenComponent<Fields.AutocompleteField>(10);
                builder.AddAttribute(11, "Schema", field);
                builder.AddAttribute(12, "Value", value?.ToString());
                builder.AddAttribute(13, "ValueChanged",
                    EventCallback.Factory.Create<string?>(this, v => HandleFieldChange(field.Name, v)));
                builder.AddAttribute(14, "Errors", errors);
                builder.CloseComponent();
                break;
        }

        if (spanStyle is not null)
        {
            builder.CloseElement();
        }
    };

    private static string? GridStyle(int columns) => columns > 1
        ? $"display: grid; grid-template-columns: repeat({columns}, 1fr); gap: var(--arcadia-spacing-md);"
        : null;
}

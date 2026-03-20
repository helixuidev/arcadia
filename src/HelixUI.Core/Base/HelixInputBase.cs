using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace HelixUI.Core.Base;

/// <summary>
/// Base class for HelixUI form input components. Provides two-way binding support
/// with Value/ValueChanged/ValueExpression parameters following Blazor conventions.
/// </summary>
/// <typeparam name="TValue">The type of the input's value.</typeparam>
public abstract class HelixInputBase<TValue> : HelixComponentBase, Abstractions.IHasDisabled
{
    /// <summary>
    /// Gets or sets the current value of the input.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets a callback that is invoked when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value,
    /// used for validation and field identification.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets whether the input is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the input is read-only.
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets the current value, converting null to default for value types.
    /// </summary>
    protected TValue? CurrentValue
    {
        get => Value;
        set
        {
            if (EqualityComparer<TValue>.Default.Equals(value, Value))
                return;

            Value = value;
            _ = ValueChanged.InvokeAsync(value);
        }
    }
}

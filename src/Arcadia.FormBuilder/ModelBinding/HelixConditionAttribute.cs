using Arcadia.FormBuilder.Schema;

namespace Arcadia.FormBuilder.ModelBinding;

/// <summary>
/// Declares a conditional visibility rule for a property based on another field's value.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class HelixConditionAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the field this condition depends on.
    /// </summary>
    public string Field { get; }

    /// <summary>
    /// Gets or sets the value to compare against (for Equals/NotEquals).
    /// </summary>
    public new string? Equals { get; set; }

    /// <summary>
    /// Gets or sets the value for NotEquals comparison.
    /// </summary>
    public string? NotEquals { get; set; }

    /// <summary>
    /// Gets or sets the action when condition is met. Default is Show.
    /// </summary>
    public ConditionalAction Action { get; set; } = ConditionalAction.Show;

    /// <summary>
    /// Creates a new condition attribute.
    /// </summary>
    /// <param name="field">The field name this depends on.</param>
    public HelixConditionAttribute(string field)
    {
        Field = field;
    }

    internal ConditionalRule ToRule()
    {
        if (Equals is not null)
        {
            return new ConditionalRule
            {
                Field = Field,
                Operator = ConditionalOperator.Equals,
                Value = Equals,
                Action = Action
            };
        }

        if (NotEquals is not null)
        {
            return new ConditionalRule
            {
                Field = Field,
                Operator = ConditionalOperator.NotEquals,
                Value = NotEquals,
                Action = Action
            };
        }

        return new ConditionalRule
        {
            Field = Field,
            Operator = ConditionalOperator.IsNotEmpty,
            Action = Action
        };
    }
}

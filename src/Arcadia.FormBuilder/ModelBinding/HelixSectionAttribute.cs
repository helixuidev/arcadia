namespace Arcadia.FormBuilder.ModelBinding;

/// <summary>
/// Groups a property into a named form section.
/// Properties with the same section name are grouped together.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class HelixSectionAttribute : Attribute
{
    /// <summary>
    /// Gets the section title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets or sets the section description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the section display order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets whether the section is collapsible.
    /// </summary>
    public bool Collapsible { get; set; }

    /// <summary>
    /// Creates a new section attribute.
    /// </summary>
    /// <param name="title">The section title.</param>
    public HelixSectionAttribute(string title)
    {
        Title = title;
    }
}

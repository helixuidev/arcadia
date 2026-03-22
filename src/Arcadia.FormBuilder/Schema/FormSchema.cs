namespace Arcadia.FormBuilder.Schema;

/// <summary>
/// Defines the complete schema for a dynamic form, including fields,
/// sections, layout, and validation configuration.
/// </summary>
public class FormSchema
{
    /// <summary>
    /// Gets or sets the form title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the form description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the number of layout columns (1-12 grid).
    /// </summary>
    public int Columns { get; set; } = 1;

    /// <summary>
    /// Gets or sets the form sections. Each section groups related fields.
    /// </summary>
    public List<FormSectionSchema> Sections { get; set; } = new();

    /// <summary>
    /// Gets or sets fields that are not inside any section (flat layout).
    /// </summary>
    public List<FieldSchema> Fields { get; set; } = new();

    /// <summary>
    /// Gets or sets the submit button text.
    /// </summary>
    public string SubmitText { get; set; } = "Submit";

    /// <summary>
    /// Gets or sets the cancel button text. Null hides the cancel button.
    /// </summary>
    public string? CancelText { get; set; }

    /// <summary>
    /// Gets all fields across all sections and the root fields list.
    /// </summary>
    public IEnumerable<FieldSchema> AllFields
    {
        get
        {
            foreach (var f in Fields)
            {
                yield return f;
                if (f.Children is not null)
                {
                    foreach (var child in f.Children)
                        yield return child;
                }
            }
            foreach (var s in Sections)
            {
                foreach (var f in s.Fields)
                {
                    yield return f;
                    if (f.Children is not null)
                    {
                        foreach (var child in f.Children)
                            yield return child;
                    }
                }
            }
        }
    }
}

/// <summary>
/// Defines a section within a form schema that groups related fields.
/// </summary>
public class FormSectionSchema
{
    /// <summary>
    /// Gets or sets the section title (rendered as fieldset legend).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the section description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the fields in this section.
    /// </summary>
    public List<FieldSchema> Fields { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of columns for this section's layout.
    /// Inherits from the form schema if not set.
    /// </summary>
    public int? Columns { get; set; }

    /// <summary>
    /// Gets or sets whether this section is collapsible.
    /// </summary>
    public bool Collapsible { get; set; }

    /// <summary>
    /// Gets or sets whether the section starts collapsed.
    /// Only applies when <see cref="Collapsible"/> is true.
    /// </summary>
    public bool Collapsed { get; set; }
}

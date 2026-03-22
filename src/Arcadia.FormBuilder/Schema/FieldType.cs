namespace Arcadia.FormBuilder.Schema;

/// <summary>
/// The type of field to render in the form.
/// </summary>
public enum FieldType
{
    /// <summary>Single-line text input.</summary>
    Text,

    /// <summary>Numeric input.</summary>
    Number,

    /// <summary>Multi-line text input.</summary>
    TextArea,

    /// <summary>Dropdown/select input.</summary>
    Select,

    /// <summary>Checkbox (boolean) input.</summary>
    Checkbox,

    /// <summary>Radio button group.</summary>
    RadioGroup,

    /// <summary>Date picker.</summary>
    Date,

    /// <summary>Toggle switch (boolean).</summary>
    Switch,

    /// <summary>File upload.</summary>
    File,

    /// <summary>Autocomplete/combobox.</summary>
    Autocomplete,

    /// <summary>Repeating section with add/remove rows.</summary>
    Repeater,

    /// <summary>Multi-select dropdown with tag chips.</summary>
    MultiSelect,

    /// <summary>Multiple checkboxes bound to string[].</summary>
    CheckboxGroup,

    /// <summary>Time picker.</summary>
    Time,

    /// <summary>Date range (start/end pair).</summary>
    DateRange,

    /// <summary>Range slider with min/max/step.</summary>
    Slider,

    /// <summary>Color picker.</summary>
    Color,

    /// <summary>Password input with visibility toggle.</summary>
    Password,

    /// <summary>Masked input (phone, SSN, custom patterns).</summary>
    Masked,

    /// <summary>Star/heart rating.</summary>
    Rating,

    /// <summary>Hidden value carrier.</summary>
    Hidden
}

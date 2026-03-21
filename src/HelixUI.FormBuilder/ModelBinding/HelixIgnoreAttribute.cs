namespace HelixUI.FormBuilder.ModelBinding;

/// <summary>
/// Excludes a property from form generation.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class HelixIgnoreAttribute : Attribute
{
}

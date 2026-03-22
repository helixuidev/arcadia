namespace Arcadia.Core.Accessibility;

/// <summary>
/// Provides helper methods for constructing ARIA attributes used in accessible components.
/// </summary>
public static class AriaHelper
{
    /// <summary>
    /// Builds an <c>aria-describedby</c> or <c>aria-labelledby</c> value
    /// from one or more element IDs, filtering out null/empty entries.
    /// </summary>
    /// <param name="ids">The element IDs to combine.</param>
    /// <returns>A space-separated string of IDs, or null if no valid IDs are provided.</returns>
    public static string? Join(params string?[] ids)
    {
        if (ids is null || ids.Length == 0)
            return null;

        var filtered = new List<string>();
        foreach (var id in ids)
        {
            if (!string.IsNullOrWhiteSpace(id))
                filtered.Add(id);
        }

        return filtered.Count == 0 ? null : string.Join(" ", filtered);
    }

    /// <summary>
    /// Returns the string value for <c>aria-expanded</c>.
    /// </summary>
    /// <param name="expanded">Whether the element is expanded.</param>
    public static string Expanded(bool expanded) => expanded ? "true" : "false";

    /// <summary>
    /// Returns the string value for <c>aria-selected</c>.
    /// </summary>
    /// <param name="selected">Whether the element is selected.</param>
    public static string Selected(bool selected) => selected ? "true" : "false";

    /// <summary>
    /// Returns the string value for <c>aria-checked</c>, including the "mixed" state.
    /// </summary>
    /// <param name="checked">True, false, or null for mixed/indeterminate.</param>
    public static string Checked(bool? @checked) => @checked switch
    {
        true => "true",
        false => "false",
        null => "mixed"
    };

    /// <summary>
    /// Returns the string value for <c>aria-hidden</c>.
    /// </summary>
    /// <param name="hidden">Whether the element is hidden from assistive technology.</param>
    public static string Hidden(bool hidden) => hidden ? "true" : "false";

    /// <summary>
    /// Returns the string value for <c>aria-disabled</c>.
    /// </summary>
    /// <param name="disabled">Whether the element is disabled.</param>
    public static string Disabled(bool disabled) => disabled ? "true" : "false";

    /// <summary>
    /// Returns the string value for <c>aria-current</c>.
    /// </summary>
    /// <param name="current">The type of current item (e.g., "page", "step", "date", "true").</param>
    public static string? Current(string? current) =>
        string.IsNullOrWhiteSpace(current) ? null : current;

    /// <summary>
    /// Returns "true" or null for boolean ARIA attributes where absence means false.
    /// Useful for attributes like <c>aria-pressed</c>, <c>aria-required</c>.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public static string? TrueOrNull(bool value) => value ? "true" : null;
}

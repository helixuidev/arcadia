using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Arcadia.FormBuilder.Components.Fields;

/// <summary>
/// File upload field supporting single and multiple file selection.
/// </summary>
public partial class FileField : FieldBase
{
    /// <summary>
    /// Gets or sets the list of files currently selected by the user. Defaults to an empty collection. Supports two-way binding.
    /// </summary>
    [Parameter] public IReadOnlyList<IBrowserFile> Value { get; set; } = Array.Empty<IBrowserFile>();

    /// <summary>
    /// Callback invoked when the user selects one or more files, enabling two-way binding with the parent component.
    /// </summary>
    [Parameter] public EventCallback<IReadOnlyList<IBrowserFile>> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the file type filter applied to the file chooser dialog, using standard MIME types or extensions (e.g., ".pdf,.docx", "image/*").
    /// Leave null to allow all file types.
    /// </summary>
    [Parameter] public string? Accept { get; set; }

    /// <summary>
    /// Gets or sets whether the user can select more than one file at a time. Defaults to false.
    /// Enable this for bulk upload scenarios such as image galleries or document batches.
    /// </summary>
    [Parameter] public bool Multiple { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed file size in bytes. Defaults to 10 MB (10,485,760 bytes).
    /// Increase this for large media uploads or decrease it to enforce stricter size limits on user submissions.
    /// </summary>
    [Parameter] public long MaxFileSize { get; set; } = 10 * 1024 * 1024;

    private List<IBrowserFile> SelectedFiles { get; set; } = new();

    private async Task HandleFileChange(InputFileChangeEventArgs e)
    {
        SelectedFiles = Multiple
            ? e.GetMultipleFiles().ToList()
            : new List<IBrowserFile> { e.File };

        await ValueChanged.InvokeAsync(SelectedFiles);
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }
}

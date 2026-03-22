using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Arcadia.FormBuilder.Components.Fields;

/// <summary>
/// File upload field supporting single and multiple file selection.
/// </summary>
public partial class FileField : FieldBase
{
    [Parameter] public IReadOnlyList<IBrowserFile> Value { get; set; } = Array.Empty<IBrowserFile>();
    [Parameter] public EventCallback<IReadOnlyList<IBrowserFile>> ValueChanged { get; set; }
    [Parameter] public string? Accept { get; set; }
    [Parameter] public bool Multiple { get; set; }
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

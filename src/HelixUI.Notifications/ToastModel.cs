namespace HelixUI.Notifications;

/// <summary>
/// Represents a single toast notification instance.
/// </summary>
public class ToastModel
{
    /// <summary>
    /// Gets the unique identifier for this toast.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the toast severity level.
    /// </summary>
    public ToastLevel Level { get; set; } = ToastLevel.Info;

    /// <summary>
    /// Gets or sets the toast title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the toast message body.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the auto-dismiss duration in milliseconds.
    /// Set to 0 to disable auto-dismiss.
    /// </summary>
    public int Duration { get; set; } = 5000;

    /// <summary>
    /// Gets or sets whether the toast can be manually dismissed.
    /// </summary>
    public bool Dismissible { get; set; } = true;

    /// <summary>
    /// Gets the timestamp when the toast was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

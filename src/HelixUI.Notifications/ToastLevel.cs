namespace HelixUI.Notifications;

/// <summary>
/// The severity level of a toast notification.
/// </summary>
public enum ToastLevel
{
    /// <summary>Informational message.</summary>
    Info,

    /// <summary>Success confirmation.</summary>
    Success,

    /// <summary>Warning that requires attention.</summary>
    Warning,

    /// <summary>Error that requires action.</summary>
    Error
}

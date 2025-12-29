namespace DidIDoThatApp.Models;

/// <summary>
/// Application settings stored in preferences.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Whether notifications are globally enabled.
    /// </summary>
    public bool NotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Default reminder lead time in days.
    /// </summary>
    public int DefaultReminderLeadTimeDays { get; set; } = 3;

    /// <summary>
    /// Whether the app has been initialized with seed data.
    /// </summary>
    public bool IsFirstLaunchComplete { get; set; }

    /// <summary>
    /// Whether notification permission has been requested.
    /// </summary>
    public bool NotificationPermissionRequested { get; set; }
}

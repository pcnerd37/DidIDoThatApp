using DidIDoThatApp.Models;

namespace DidIDoThatApp.Services.Interfaces;

/// <summary>
/// Service for managing application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    AppSettings GetSettings();

    /// <summary>
    /// Saves the application settings.
    /// </summary>
    void SaveSettings(AppSettings settings);

    /// <summary>
    /// Gets whether notifications are enabled.
    /// </summary>
    bool NotificationsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the default reminder lead time in days.
    /// </summary>
    int DefaultReminderLeadTimeDays { get; set; }

    /// <summary>
    /// Gets or sets whether the first launch initialization is complete.
    /// </summary>
    bool IsFirstLaunchComplete { get; set; }

    /// <summary>
    /// Gets or sets whether notification permission has been requested.
    /// </summary>
    bool NotificationPermissionRequested { get; set; }
}

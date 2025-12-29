using DidIDoThatApp.Helpers;
using DidIDoThatApp.Models;
using DidIDoThatApp.Services.Interfaces;

namespace DidIDoThatApp.Services;

/// <summary>
/// Service for managing application settings using Preferences.
/// </summary>
public class SettingsService : ISettingsService
{
    public bool NotificationsEnabled
    {
        get => Preferences.Get(Constants.SettingsKeys.NotificationsEnabled, true);
        set => Preferences.Set(Constants.SettingsKeys.NotificationsEnabled, value);
    }

    public int DefaultReminderLeadTimeDays
    {
        get => Preferences.Get(Constants.SettingsKeys.DefaultReminderLeadTimeDays, 3);
        set => Preferences.Set(Constants.SettingsKeys.DefaultReminderLeadTimeDays, value);
    }

    public bool IsFirstLaunchComplete
    {
        get => Preferences.Get(Constants.SettingsKeys.IsFirstLaunchComplete, false);
        set => Preferences.Set(Constants.SettingsKeys.IsFirstLaunchComplete, value);
    }

    public bool NotificationPermissionRequested
    {
        get => Preferences.Get(Constants.SettingsKeys.NotificationPermissionRequested, false);
        set => Preferences.Set(Constants.SettingsKeys.NotificationPermissionRequested, value);
    }

    public AppSettings GetSettings()
    {
        return new AppSettings
        {
            NotificationsEnabled = NotificationsEnabled,
            DefaultReminderLeadTimeDays = DefaultReminderLeadTimeDays,
            IsFirstLaunchComplete = IsFirstLaunchComplete,
            NotificationPermissionRequested = NotificationPermissionRequested
        };
    }

    public void SaveSettings(AppSettings settings)
    {
        NotificationsEnabled = settings.NotificationsEnabled;
        DefaultReminderLeadTimeDays = settings.DefaultReminderLeadTimeDays;
        IsFirstLaunchComplete = settings.IsFirstLaunchComplete;
        NotificationPermissionRequested = settings.NotificationPermissionRequested;
    }
}

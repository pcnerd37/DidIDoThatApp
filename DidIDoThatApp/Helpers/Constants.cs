namespace DidIDoThatApp.Helpers;

/// <summary>
/// Application-wide constants.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Database filename.
    /// </summary>
    public const string DatabaseFilename = "dididothat.db";

    /// <summary>
    /// Gets the full path to the database file.
    /// </summary>
    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    /// <summary>
    /// Settings keys for preferences storage.
    /// </summary>
    public static class SettingsKeys
    {
        public const string NotificationsEnabled = "notifications_enabled";
        public const string DefaultReminderLeadTimeDays = "default_reminder_lead_time_days";
        public const string IsFirstLaunchComplete = "is_first_launch_complete";
        public const string NotificationPermissionRequested = "notification_permission_requested";
    }

    /// <summary>
    /// Default category names and icons.
    /// </summary>
    public static class DefaultCategories
    {
        public static readonly (string Name, string Icon)[] Categories =
        [
            ("Home", "🏠"),
            ("Car", "🚗"),
            ("Personal", "👤"),
            ("Pet", "🐾"),
            ("Business", "💼")
        ];
    }

    /// <summary>
    /// Notification constants.
    /// </summary>
    public static class Notifications
    {
        public const string ChannelId = "dididothat_reminders";
        public const string ChannelName = "Task Reminders";
        public const string ChannelDescription = "Notifications for upcoming maintenance tasks";
        
        /// <summary>
        /// Background task identifier for iOS.
        /// </summary>
        public const string BackgroundTaskIdentifier = "com.dididothat.notificationrefresh";
    }

    /// <summary>
    /// Navigation routes.
    /// </summary>
    public static class Routes
    {
        public const string Dashboard = "dashboard";
        public const string Tasks = "tasks";
        public const string TaskDetail = "taskdetail";
        public const string AddEditTask = "addedittask";
        public const string Categories = "categories";
        public const string Settings = "settings";
    }
}

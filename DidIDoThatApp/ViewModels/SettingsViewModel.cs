using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DidIDoThatApp.Services.Interfaces;

namespace DidIDoThatApp.ViewModels;

/// <summary>
/// ViewModel for the Settings page.
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly INotificationService _notificationService;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IExportService _exportService;

    public SettingsViewModel(
        ISettingsService settingsService,
        INotificationService notificationService,
        IBackgroundTaskService backgroundTaskService,
        IExportService exportService)
    {
        _settingsService = settingsService;
        _notificationService = notificationService;
        _backgroundTaskService = backgroundTaskService;
        _exportService = exportService;
        Title = "Settings";
    }

    [ObservableProperty]
    private bool _notificationsEnabled;

    [ObservableProperty]
    private int _defaultReminderLeadTimeDays;

    [ObservableProperty]
    private bool _hasNotificationPermission;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    public int[] LeadTimeOptions => [1, 2, 3, 5, 7, 14];

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var settings = _settingsService.GetSettings();
            NotificationsEnabled = settings.NotificationsEnabled;
            DefaultReminderLeadTimeDays = settings.DefaultReminderLeadTimeDays;
            HasNotificationPermission = await _notificationService.HasPermissionAsync();
            AppVersion = $"{AppInfo.VersionString} ({AppInfo.BuildString})";
        });
    }

    partial void OnNotificationsEnabledChanged(bool value)
    {
        _settingsService.NotificationsEnabled = value;

        if (value)
        {
            // Reschedule all notifications
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _notificationService.RecalculateAllNotificationsAsync();
                await _backgroundTaskService.RegisterDailyNotificationTaskAsync();
            });
        }
        else
        {
            // Cancel all notifications
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _notificationService.CancelAllNotificationsAsync();
                await _backgroundTaskService.UnregisterDailyNotificationTaskAsync();
            });
        }
    }

    partial void OnDefaultReminderLeadTimeDaysChanged(int value)
    {
        _settingsService.DefaultReminderLeadTimeDays = value;
    }

    [RelayCommand]
    private async Task RequestNotificationPermissionAsync()
    {
        var granted = await _notificationService.RequestPermissionAsync();
        HasNotificationPermission = granted;

        if (!granted)
        {
            await Shell.Current.DisplayAlert(
                "Permission Required",
                "To receive task reminders, please enable notifications in your device settings.",
                "OK");
        }
    }

    [RelayCommand]
    private async Task OpenSystemNotificationSettingsAsync()
    {
        try
        {
            AppInfo.ShowSettingsUI();
        }
        catch
        {
            await Shell.Current.DisplayAlert(
                "Unable to Open Settings",
                "Please open your device settings and enable notifications for this app manually.",
                "OK");
        }
    }

    [RelayCommand]
    private async Task ExportDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var success = await _exportService.ExportDataAsJsonAsync();
            if (!success)
            {
                await Shell.Current.DisplayAlert(
                    "Export Failed",
                    "Unable to export data. Please try again.",
                    "OK");
            }
        });
    }
}

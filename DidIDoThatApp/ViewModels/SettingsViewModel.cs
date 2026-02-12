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

    public int[] LeadTimeOptions => new int[] { 1, 2, 3, 5, 7, 14 };

    private bool _isLoadingSettings;

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        await ExecuteAsync(async () =>
        {
            _isLoadingSettings = true;
            try
            {
                var settings = _settingsService.GetSettings();
                NotificationsEnabled = settings.NotificationsEnabled;
                DefaultReminderLeadTimeDays = settings.DefaultReminderLeadTimeDays;
                HasNotificationPermission = await _notificationService.HasPermissionAsync();
                AppVersion = $"{AppInfo.VersionString} ({AppInfo.BuildString})";
            }
            finally
            {
                _isLoadingSettings = false;
            }
        });
    }

    partial void OnNotificationsEnabledChanged(bool value)
    {
        if (_isLoadingSettings)
            return;

        _settingsService.NotificationsEnabled = value;

        if (value)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await _notificationService.RecalculateAllNotificationsAsync();
                    await _backgroundTaskService.RegisterDailyNotificationTaskAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Notification setup failed: {ex}");
                }
            });
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await _notificationService.CancelAllNotificationsAsync();
                    await _backgroundTaskService.UnregisterDailyNotificationTaskAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Notification teardown failed: {ex}");
                }
            });
        }
    }

    partial void OnDefaultReminderLeadTimeDaysChanged(int value)
    {
        if (_isLoadingSettings)
            return;

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

    [RelayCommand]
    private async Task ImportDataAsync()
    {
        // Confirm with the user
        var confirm = await Shell.Current.DisplayAlert(
            "Import Data",
            "This will import data from a previously exported file. Existing data will be preserved - only new items will be added.\n\nContinue?",
            "Import",
            "Cancel");

        if (!confirm)
            return;

        await ExecuteAsync(async () =>
        {
            var result = await _exportService.ImportDataFromJsonAsync();
            
            await Shell.Current.DisplayAlert(
                result.Success ? "Import Complete" : "Import Failed",
                result.Message,
                "OK");

            // Invalidate cache if anything was imported
            if (result.Success && (result.CategoriesImported > 0 || result.TasksImported > 0 || result.LogsImported > 0))
            {
                App.DataPrefetchService?.InvalidateCache();
            }
        });
    }

    [RelayCommand]
    private async Task OpenPrivacyPolicyAsync()
    {
        try
        {
            // TODO: Update this URL to your actual GitHub Pages URL after setup
            var privacyPolicyUrl = "https://pcnerd37.github.io/DidIDoThatApp/privacy-policy";
            await Browser.OpenAsync(privacyPolicyUrl, BrowserLaunchMode.SystemPreferred);
        }
        catch
        {
            await Shell.Current.DisplayAlert(
                "Unable to Open",
                "Could not open the privacy policy. Please check your internet connection.",
                "OK");
        }
    }
}

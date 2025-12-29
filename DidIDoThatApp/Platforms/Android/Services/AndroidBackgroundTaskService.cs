#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using DidIDoThatApp.Services.Interfaces;

namespace DidIDoThatApp.Platforms.Android.Services;

/// <summary>
/// Android-specific background task service using AlarmManager.
/// </summary>
public class AndroidBackgroundTaskService : IBackgroundTaskService
{
    private const string ActionRecalculateNotifications = "com.dididothat.RECALCULATE_NOTIFICATIONS";
    private const int AlarmRequestCode = 1001;
    private bool _isRegistered;

    public Task RegisterDailyNotificationTaskAsync()
    {
        var context = global::Android.App.Application.Context;
        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        
        if (alarmManager == null)
        {
            System.Diagnostics.Debug.WriteLine("AlarmManager not available");
            return Task.CompletedTask;
        }

        var intent = new Intent(context, typeof(NotificationRecalculationReceiver));
        intent.SetAction(ActionRecalculateNotifications);

        var pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            pendingIntentFlags |= PendingIntentFlags.Immutable;
        }

        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            AlarmRequestCode,
            intent,
            pendingIntentFlags);

        if (pendingIntent == null)
        {
            System.Diagnostics.Debug.WriteLine("Failed to create PendingIntent");
            return Task.CompletedTask;
        }

        // Calculate trigger time for 4 AM tomorrow
        var calendar = Java.Util.Calendar.Instance!;
        calendar.Set(Java.Util.CalendarField.HourOfDay, 4);
        calendar.Set(Java.Util.CalendarField.Minute, 0);
        calendar.Set(Java.Util.CalendarField.Second, 0);
        
        // If it's already past 4 AM, schedule for tomorrow
        if (calendar.TimeInMillis <= Java.Lang.JavaSystem.CurrentTimeMillis())
        {
            calendar.Add(Java.Util.CalendarField.Date, 1);
        }

        // Set repeating alarm
        alarmManager.SetRepeating(
            AlarmType.RtcWakeup,
            calendar.TimeInMillis,
            AlarmManager.IntervalDay,
            pendingIntent);

        _isRegistered = true;
        System.Diagnostics.Debug.WriteLine("Daily notification recalculation scheduled");
        return Task.CompletedTask;
    }

    public Task UnregisterDailyNotificationTaskAsync()
    {
        var context = global::Android.App.Application.Context;
        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        
        if (alarmManager == null) return Task.CompletedTask;

        var intent = new Intent(context, typeof(NotificationRecalculationReceiver));
        intent.SetAction(ActionRecalculateNotifications);

        var pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            pendingIntentFlags |= PendingIntentFlags.Immutable;
        }

        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            AlarmRequestCode,
            intent,
            pendingIntentFlags);

        if (pendingIntent != null)
        {
            alarmManager.Cancel(pendingIntent);
        }

        _isRegistered = false;
        System.Diagnostics.Debug.WriteLine("Daily notification recalculation cancelled");
        return Task.CompletedTask;
    }

    public Task<bool> IsTaskRegisteredAsync()
    {
        return Task.FromResult(_isRegistered);
    }
}

/// <summary>
/// Broadcast receiver that handles the alarm and recalculates notifications.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter([ActionRecalculateNotifications])]
public class NotificationRecalculationReceiver : BroadcastReceiver
{
    private const string ActionRecalculateNotifications = "com.dididothat.RECALCULATE_NOTIFICATIONS";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent?.Action != ActionRecalculateNotifications) return;

        System.Diagnostics.Debug.WriteLine("Notification recalculation triggered");
        
        // Note: In a full implementation, you would:
        // 1. Start a foreground service or use JobIntentService for longer work
        // 2. Access the database to recalculate notifications
        // 3. Cancel existing notifications and schedule new ones
        // For now, this serves as the trigger point
    }
}
#endif

using DidIDoThatApp.Data;
using DidIDoThatApp.Helpers;
using DidIDoThatApp.Models;
using DidIDoThatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plugin.LocalNotification;
using IAppNotificationService = DidIDoThatApp.Services.Interfaces.INotificationService;

namespace DidIDoThatApp.Services;

/// <summary>
/// Service for managing local notifications.
/// </summary>
public class NotificationService : IAppNotificationService
{
    private readonly AppDbContext _context;
    private readonly ISettingsService _settingsService;

    public NotificationService(AppDbContext context, ISettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
    }

    public async Task<bool> RequestPermissionAsync()
    {
        var result = await LocalNotificationCenter.Current.RequestNotificationPermission();
        _settingsService.NotificationPermissionRequested = true;
        return result;
    }

    public async Task<bool> HasPermissionAsync()
    {
        return await LocalNotificationCenter.Current.AreNotificationsEnabled();
    }

    public async Task ScheduleTaskNotificationAsync(TaskItem task, DateTime dueDate)
    {
        if (!_settingsService.NotificationsEnabled || !task.IsReminderEnabled)
        {
            return;
        }

        // Cancel any existing notification for this task
        await CancelTaskNotificationAsync(task.Id);

        var notificationTime = StatusCalculator.CalculateNotificationTime(task, dueDate);
        if (notificationTime == null)
        {
            return;
        }

        var notification = new NotificationRequest
        {
            NotificationId = GetNotificationId(task.Id),
            Title = "Task Reminder",
            Description = $"{task.Name} is due soon!",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notificationTime.Value
            },
            CategoryType = NotificationCategoryType.Reminder,
            Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions
            {
                ChannelId = Constants.Notifications.ChannelId,
                Priority = Plugin.LocalNotification.AndroidOption.AndroidPriority.High
            }
        };

        await LocalNotificationCenter.Current.Show(notification);
    }

    public async Task CancelTaskNotificationAsync(Guid taskId)
    {
        LocalNotificationCenter.Current.Cancel(GetNotificationId(taskId));
        await Task.CompletedTask;
    }

    public async Task CancelAllNotificationsAsync()
    {
        LocalNotificationCenter.Current.CancelAll();
        await Task.CompletedTask;
    }

    public async Task RecalculateAllNotificationsAsync()
    {
        if (!_settingsService.NotificationsEnabled)
        {
            await CancelAllNotificationsAsync();
            return;
        }

        var tasks = await _context.Tasks
            .Where(t => t.IsReminderEnabled)
            .Include(t => t.TaskLogs)
            .ToListAsync();

        foreach (var task in tasks)
        {
            var lastCompleted = task.TaskLogs
                .OrderByDescending(l => l.CompletedDate)
                .FirstOrDefault()?.CompletedDate;

            var dueDate = StatusCalculator.CalculateDueDate(task, lastCompleted);
            if (dueDate.HasValue)
            {
                await ScheduleTaskNotificationAsync(task, dueDate.Value);
            }
        }
    }

    public TimeSpan GetNotificationLeadTime(TaskItem task)
    {
        return StatusCalculator.GetNotificationLeadTime(task);
    }

    /// <summary>
    /// Generates a consistent notification ID from a task GUID.
    /// </summary>
    private static int GetNotificationId(Guid taskId)
    {
        // Use the first 4 bytes of the GUID as an int
        return BitConverter.ToInt32(taskId.ToByteArray(), 0);
    }
}

using DidIDoThatApp.Models;

namespace DidIDoThatApp.Services.Interfaces;

/// <summary>
/// Service for managing local notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Requests notification permission from the user.
    /// </summary>
    Task<bool> RequestPermissionAsync();

    /// <summary>
    /// Checks if notification permission has been granted.
    /// </summary>
    Task<bool> HasPermissionAsync();

    /// <summary>
    /// Schedules a notification for a task.
    /// </summary>
    Task ScheduleTaskNotificationAsync(TaskItem task, DateTime dueDate);

    /// <summary>
    /// Cancels a scheduled notification for a task.
    /// </summary>
    Task CancelTaskNotificationAsync(Guid taskId);

    /// <summary>
    /// Cancels all scheduled notifications.
    /// </summary>
    Task CancelAllNotificationsAsync();

    /// <summary>
    /// Recalculates and reschedules all notifications.
    /// </summary>
    Task RecalculateAllNotificationsAsync();

    /// <summary>
    /// Gets the notification lead time based on the task's frequency.
    /// Tasks due within 14 days get 3 days lead time.
    /// Tasks due beyond 14 days get 7 days lead time.
    /// </summary>
    TimeSpan GetNotificationLeadTime(TaskItem task);
}

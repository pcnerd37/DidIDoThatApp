using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using TaskStatus = DidIDoThatApp.Models.Enums.TaskStatus;

namespace DidIDoThatApp.Helpers;

/// <summary>
/// Helper class for calculating task status and due dates.
/// </summary>
public static class StatusCalculator
{
    /// <summary>
    /// Calculates the due date for a task based on its last completion.
    /// </summary>
    /// <param name="task">The task to calculate the due date for.</param>
    /// <param name="lastCompletedDate">The date the task was last completed, or null if never completed.</param>
    /// <returns>The due date, or null if the task has never been completed (considered immediately overdue).</returns>
    public static DateTime? CalculateDueDate(TaskItem task, DateTime? lastCompletedDate)
    {
        if (lastCompletedDate == null)
        {
            return null; // Never completed = overdue
        }

        return lastCompletedDate.Value.Add(GetFrequencyTimeSpan(task));
    }

    /// <summary>
    /// Determines the current status of a task.
    /// </summary>
    /// <param name="task">The task to evaluate.</param>
    /// <param name="lastCompletedDate">The date the task was last completed.</param>
    /// <param name="now">The current date/time for comparison.</param>
    /// <returns>The task's current status.</returns>
    public static TaskStatus CalculateStatus(TaskItem task, DateTime? lastCompletedDate, DateTime now)
    {
        var dueDate = CalculateDueDate(task, lastCompletedDate);

        // If never completed, the task is overdue
        if (dueDate == null)
        {
            return TaskStatus.Overdue;
        }

        // If due date is in the past, the task is overdue
        if (dueDate.Value < now)
        {
            return TaskStatus.Overdue;
        }

        // Calculate the "due soon" threshold (20% of frequency interval before due date)
        var frequencySpan = GetFrequencyTimeSpan(task);
        var dueSoonThreshold = TimeSpan.FromTicks((long)(frequencySpan.Ticks * 0.20));
        var dueSoonStartDate = dueDate.Value.Subtract(dueSoonThreshold);

        // If we're within the "due soon" window
        if (now >= dueSoonStartDate)
        {
            return TaskStatus.DueSoon;
        }

        return TaskStatus.UpToDate;
    }

    /// <summary>
    /// Gets the frequency as a TimeSpan with more accurate month calculation.
    /// </summary>
    public static TimeSpan GetFrequencyTimeSpan(TaskItem task)
    {
        return task.FrequencyUnit switch
        {
            FrequencyUnit.Days => TimeSpan.FromDays(task.FrequencyValue),
            FrequencyUnit.Weeks => TimeSpan.FromDays(task.FrequencyValue * 7),
            FrequencyUnit.Months => TimeSpan.FromDays(task.FrequencyValue * 30), // Approximate
            _ => TimeSpan.FromDays(task.FrequencyValue)
        };
    }

    /// <summary>
    /// Calculates the notification lead time based on the task's frequency.
    /// Tasks due within 14 days get 3 days lead time.
    /// Tasks due beyond 14 days get 7 days lead time.
    /// </summary>
    /// <param name="task">The task to calculate lead time for.</param>
    /// <returns>The lead time as a TimeSpan.</returns>
    public static TimeSpan GetNotificationLeadTime(TaskItem task)
    {
        var frequencySpan = GetFrequencyTimeSpan(task);
        
        // If task frequency is 14 days or less, use 3-day lead time
        // Otherwise use 7-day lead time
        return frequencySpan.TotalDays <= 14 
            ? TimeSpan.FromDays(3) 
            : TimeSpan.FromDays(7);
    }

    /// <summary>
    /// Calculates when a notification should be sent for a task.
    /// </summary>
    /// <param name="task">The task to calculate notification time for.</param>
    /// <param name="dueDate">The task's due date.</param>
    /// <returns>When the notification should be sent, or null if notification shouldn't be scheduled.</returns>
    public static DateTime? CalculateNotificationTime(TaskItem task, DateTime? dueDate)
    {
        if (dueDate == null || !task.IsReminderEnabled)
        {
            return null;
        }

        var leadTime = GetNotificationLeadTime(task);
        var notificationTime = dueDate.Value.Subtract(leadTime);

        // Don't schedule notifications in the past
        if (notificationTime < DateTime.Now)
        {
            return null;
        }

        return notificationTime;
    }

    /// <summary>
    /// Gets a human-readable description of time until due or overdue.
    /// </summary>
    /// <param name="dueDate">The due date.</param>
    /// <param name="now">The current date/time.</param>
    /// <returns>A human-readable string describing the time relationship.</returns>
    public static string GetDueDescription(DateTime? dueDate, DateTime now)
    {
        if (dueDate == null)
        {
            return "Never completed";
        }

        var diff = dueDate.Value - now;

        if (diff.TotalDays < 0)
        {
            var overdueDays = Math.Abs((int)diff.TotalDays);
            return overdueDays == 1 ? "1 day overdue" : $"{overdueDays} days overdue";
        }

        if (diff.TotalDays < 1)
        {
            return "Due today";
        }

        if (diff.TotalDays < 2)
        {
            return "Due tomorrow";
        }

        var daysUntil = (int)diff.TotalDays;
        return $"Due in {daysUntil} days";
    }
}

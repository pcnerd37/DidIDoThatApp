using DidIDoThatApp.Data;
using DidIDoThatApp.Helpers;
using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using DidIDoThatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using TaskStatus = DidIDoThatApp.Models.Enums.TaskStatus;

namespace DidIDoThatApp.Services;

/// <summary>
/// Service for managing maintenance tasks.
/// </summary>
public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly ITaskLogService _taskLogService;
    private readonly INotificationService _notificationService;

    public TaskService(
        AppDbContext context,
        ITaskLogService taskLogService,
        INotificationService notificationService)
    {
        _context = context;
        _taskLogService = taskLogService;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllTasksAsync()
    {
        return await _context.Tasks
            .Include(t => t.Category)
            .Include(t => t.TaskLogs)
            .OrderBy(t => t.Category!.Name)
            .ThenBy(t => t.Name)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksByCategoryAsync(Guid categoryId)
    {
        return await _context.Tasks
            .Where(t => t.CategoryId == categoryId)
            .Include(t => t.TaskLogs)
            .OrderBy(t => t.Name)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<TaskItem?> GetTaskByIdAsync(Guid id)
    {
        return await _context.Tasks
            .Include(t => t.Category)
            .Include(t => t.TaskLogs)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TaskItem>> GetOverdueTasksAsync()
    {
        var allTasks = await GetAllTasksAsync();
        var now = DateTime.Now;

        return allTasks
            .Where(t => GetTaskStatusSync(t, now) == TaskStatus.Overdue)
            .OrderBy(t => GetDueDateSync(t) ?? DateTime.MinValue)
            .ToList();
    }

    public async Task<IReadOnlyList<TaskItem>> GetDueSoonTasksAsync()
    {
        var allTasks = await GetAllTasksAsync();
        var now = DateTime.Now;

        return allTasks
            .Where(t => GetTaskStatusSync(t, now) == TaskStatus.DueSoon)
            .OrderBy(t => GetDueDateSync(t))
            .ToList();
    }

    public async Task<IReadOnlyList<TaskItem>> GetRecentlyCompletedTasksAsync(int count = 5)
    {
        var recentLogs = await _context.TaskLogs
            .OrderByDescending(l => l.CompletedDate)
            .Take(count * 2) // Take more to handle distinct tasks
            .Select(l => l.TaskItemId)
            .Distinct()
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);

        var tasks = await _context.Tasks
            .Where(t => recentLogs.Contains(t.Id))
            .Include(t => t.Category)
            .Include(t => t.TaskLogs)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        // Order by most recent completion
        return tasks
            .OrderByDescending(t => t.TaskLogs.Max(l => l.CompletedDate))
            .Take(count)
            .ToList();
    }

    public async Task<TaskItem> CreateTaskAsync(
        Guid categoryId,
        string name,
        string? description,
        int frequencyValue,
        FrequencyUnit frequencyUnit,
        bool isReminderEnabled = true)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Name = name.Trim(),
            Description = description?.Trim(),
            FrequencyValue = frequencyValue,
            FrequencyUnit = frequencyUnit,
            IsReminderEnabled = isReminderEnabled,
            CreatedDate = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return task;
    }

    public async Task<TaskItem?> UpdateTaskAsync(
        Guid id,
        string name,
        string? description,
        int frequencyValue,
        FrequencyUnit frequencyUnit,
        bool isReminderEnabled)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskLogs)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (task == null)
        {
            return null;
        }

        task.Name = name.Trim();
        task.Description = description?.Trim();
        task.FrequencyValue = frequencyValue;
        task.FrequencyUnit = frequencyUnit;
        task.IsReminderEnabled = isReminderEnabled;

        await _context.SaveChangesAsync();

        // Reschedule notification if reminder settings changed
        if (isReminderEnabled)
        {
            var dueDate = await GetDueDateAsync(id);
            if (dueDate.HasValue)
            {
                await _notificationService.ScheduleTaskNotificationAsync(task, dueDate.Value);
            }
        }
        else
        {
            await _notificationService.CancelTaskNotificationAsync(id);
        }

        return task;
    }

    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return false;
        }

        await _notificationService.CancelTaskNotificationAsync(id);

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<TaskLog> CompleteTaskAsync(Guid taskId, DateTime? completedDate = null, string? notes = null)
    {
        var task = await GetTaskByIdAsync(taskId);
        if (task == null)
        {
            throw new ArgumentException($"Task with ID {taskId} not found.", nameof(taskId));
        }

        var log = await _taskLogService.CreateLogAsync(
            taskId,
            completedDate ?? DateTime.Now,
            notes);

        // Recalculate and schedule next notification
        if (task.IsReminderEnabled)
        {
            var newDueDate = StatusCalculator.CalculateDueDate(task, log.CompletedDate);
            if (newDueDate.HasValue)
            {
                await _notificationService.ScheduleTaskNotificationAsync(task, newDueDate.Value);
            }
        }

        return log;
    }

    public async Task<DateTime?> GetLastCompletedDateAsync(Guid taskId)
    {
        var log = await _taskLogService.GetMostRecentLogAsync(taskId);
        return log?.CompletedDate;
    }

    public async Task<DateTime?> GetDueDateAsync(Guid taskId)
    {
        var task = await GetTaskByIdAsync(taskId);
        if (task == null)
        {
            return null;
        }

        var lastCompleted = await GetLastCompletedDateAsync(taskId);
        return StatusCalculator.CalculateDueDate(task, lastCompleted);
    }

    public async Task<TaskStatus> GetTaskStatusAsync(Guid taskId)
    {
        var task = await GetTaskByIdAsync(taskId);
        if (task == null)
        {
            return TaskStatus.Overdue;
        }

        var lastCompleted = await GetLastCompletedDateAsync(taskId);
        return StatusCalculator.CalculateStatus(task, lastCompleted, DateTime.Now);
    }

    // Helper methods for synchronous status calculation when we already have task data
    private static TaskStatus GetTaskStatusSync(TaskItem task, DateTime now)
    {
        var lastCompleted = task.TaskLogs
            .OrderByDescending(l => l.CompletedDate)
            .FirstOrDefault()?.CompletedDate;

        return StatusCalculator.CalculateStatus(task, lastCompleted, now);
    }

    private static DateTime? GetDueDateSync(TaskItem task)
    {
        var lastCompleted = task.TaskLogs
            .OrderByDescending(l => l.CompletedDate)
            .FirstOrDefault()?.CompletedDate;

        return StatusCalculator.CalculateDueDate(task, lastCompleted);
    }
}

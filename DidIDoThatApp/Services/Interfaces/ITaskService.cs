using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using TaskStatus = DidIDoThatApp.Models.Enums.TaskStatus;

namespace DidIDoThatApp.Services.Interfaces;

/// <summary>
/// Service for managing maintenance tasks.
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Gets all tasks.
    /// </summary>
    Task<IReadOnlyList<TaskItem>> GetAllTasksAsync();

    /// <summary>
    /// Gets all tasks for a specific category.
    /// </summary>
    Task<IReadOnlyList<TaskItem>> GetTasksByCategoryAsync(Guid categoryId);

    /// <summary>
    /// Gets a task by its ID.
    /// </summary>
    Task<TaskItem?> GetTaskByIdAsync(Guid id);

    /// <summary>
    /// Gets all overdue tasks.
    /// </summary>
    Task<IReadOnlyList<TaskItem>> GetOverdueTasksAsync();

    /// <summary>
    /// Gets all tasks that are due soon.
    /// </summary>
    Task<IReadOnlyList<TaskItem>> GetDueSoonTasksAsync();

    /// <summary>
    /// Gets recently completed tasks.
    /// </summary>
    Task<IReadOnlyList<TaskItem>> GetRecentlyCompletedTasksAsync(int count = 5);

    /// <summary>
    /// Creates a new task.
    /// </summary>
    Task<TaskItem> CreateTaskAsync(
        Guid categoryId,
        string name,
        string? description,
        int frequencyValue,
        FrequencyUnit frequencyUnit,
        bool isReminderEnabled = true);

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    Task<TaskItem?> UpdateTaskAsync(
        Guid id,
        string name,
        string? description,
        int frequencyValue,
        FrequencyUnit frequencyUnit,
        bool isReminderEnabled);

    /// <summary>
    /// Deletes a task.
    /// </summary>
    Task<bool> DeleteTaskAsync(Guid id);

    /// <summary>
    /// Marks a task as completed.
    /// </summary>
    Task<TaskLog> CompleteTaskAsync(Guid taskId, DateTime? completedDate = null, string? notes = null);

    /// <summary>
    /// Gets the last completion date for a task.
    /// </summary>
    Task<DateTime?> GetLastCompletedDateAsync(Guid taskId);

    /// <summary>
    /// Gets the due date for a task.
    /// </summary>
    Task<DateTime?> GetDueDateAsync(Guid taskId);

    /// <summary>
    /// Gets the current status of a task.
    /// </summary>
    Task<TaskStatus> GetTaskStatusAsync(Guid taskId);
}

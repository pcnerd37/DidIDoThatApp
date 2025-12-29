using DidIDoThatApp.Models;

namespace DidIDoThatApp.Services.Interfaces;

/// <summary>
/// Service for managing task completion logs.
/// </summary>
public interface ITaskLogService
{
    /// <summary>
    /// Gets all logs for a specific task.
    /// </summary>
    Task<IReadOnlyList<TaskLog>> GetLogsForTaskAsync(Guid taskId);

    /// <summary>
    /// Gets the most recent log for a task.
    /// </summary>
    Task<TaskLog?> GetMostRecentLogAsync(Guid taskId);

    /// <summary>
    /// Creates a new completion log.
    /// </summary>
    Task<TaskLog> CreateLogAsync(Guid taskId, DateTime completedDate, string? notes = null);

    /// <summary>
    /// Deletes a log entry.
    /// </summary>
    Task<bool> DeleteLogAsync(Guid logId);

    /// <summary>
    /// Gets all logs within a date range.
    /// </summary>
    Task<IReadOnlyList<TaskLog>> GetLogsInRangeAsync(DateTime start, DateTime end);
}

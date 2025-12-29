using DidIDoThatApp.Data;
using DidIDoThatApp.Models;
using DidIDoThatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DidIDoThatApp.Services;

/// <summary>
/// Service for managing task completion logs.
/// </summary>
public class TaskLogService : ITaskLogService
{
    private readonly AppDbContext _context;

    public TaskLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TaskLog>> GetLogsForTaskAsync(Guid taskId)
    {
        return await _context.TaskLogs
            .Where(l => l.TaskItemId == taskId)
            .OrderByDescending(l => l.CompletedDate)
            .ToListAsync();
    }

    public async Task<TaskLog?> GetMostRecentLogAsync(Guid taskId)
    {
        return await _context.TaskLogs
            .Where(l => l.TaskItemId == taskId)
            .OrderByDescending(l => l.CompletedDate)
            .FirstOrDefaultAsync();
    }

    public async Task<TaskLog> CreateLogAsync(Guid taskId, DateTime completedDate, string? notes = null)
    {
        var log = new TaskLog
        {
            Id = Guid.NewGuid(),
            TaskItemId = taskId,
            CompletedDate = completedDate,
            Notes = notes
        };

        _context.TaskLogs.Add(log);
        await _context.SaveChangesAsync();

        return log;
    }

    public async Task<bool> DeleteLogAsync(Guid logId)
    {
        var log = await _context.TaskLogs.FindAsync(logId);
        if (log == null)
        {
            return false;
        }

        _context.TaskLogs.Remove(log);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IReadOnlyList<TaskLog>> GetLogsInRangeAsync(DateTime start, DateTime end)
    {
        return await _context.TaskLogs
            .Where(l => l.CompletedDate >= start && l.CompletedDate <= end)
            .OrderByDescending(l => l.CompletedDate)
            .Include(l => l.TaskItem)
            .ToListAsync();
    }
}

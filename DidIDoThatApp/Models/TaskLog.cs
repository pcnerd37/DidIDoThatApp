using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DidIDoThatApp.Models;

/// <summary>
/// Represents a completion log entry for a task.
/// Each time a task is marked complete, a new TaskLog is created.
/// </summary>
public class TaskLog
{
    /// <summary>
    /// Unique identifier for the log entry.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the task this log belongs to.
    /// </summary>
    public Guid TaskItemId { get; set; }

    /// <summary>
    /// Date and time the task was completed.
    /// </summary>
    public DateTime CompletedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional notes about this completion.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property for the parent task.
    /// </summary>
    [ForeignKey(nameof(TaskItemId))]
    public TaskItem? TaskItem { get; set; }
}

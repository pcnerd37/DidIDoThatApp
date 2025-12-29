using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DidIDoThatApp.Models.Enums;

namespace DidIDoThatApp.Models;

/// <summary>
/// Represents a recurring maintenance task.
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Unique identifier for the task.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the category this task belongs to.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Name of the task.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the task.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// The numeric value of the frequency (e.g., 3 for "every 3 months").
    /// </summary>
    [Range(1, 365)]
    public int FrequencyValue { get; set; } = 1;

    /// <summary>
    /// The unit of the frequency (Days, Weeks, Months).
    /// </summary>
    public FrequencyUnit FrequencyUnit { get; set; } = FrequencyUnit.Months;

    /// <summary>
    /// Whether reminders are enabled for this task.
    /// </summary>
    public bool IsReminderEnabled { get; set; } = true;

    /// <summary>
    /// Date and time the task was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for the parent category.
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    /// <summary>
    /// Navigation property for completion logs.
    /// </summary>
    public ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();

    /// <summary>
    /// Gets the frequency as a TimeSpan.
    /// </summary>
    [NotMapped]
    public TimeSpan FrequencyTimeSpan => FrequencyUnit switch
    {
        FrequencyUnit.Days => TimeSpan.FromDays(FrequencyValue),
        FrequencyUnit.Weeks => TimeSpan.FromDays(FrequencyValue * 7),
        FrequencyUnit.Months => TimeSpan.FromDays(FrequencyValue * 30), // Approximate
        _ => TimeSpan.FromDays(FrequencyValue)
    };

    /// <summary>
    /// Gets a human-readable frequency description.
    /// </summary>
    [NotMapped]
    public string FrequencyDescription => FrequencyValue == 1
        ? $"Every {FrequencyUnit.ToString().TrimEnd('s')}"
        : $"Every {FrequencyValue} {FrequencyUnit}";
}

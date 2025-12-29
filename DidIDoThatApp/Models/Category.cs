using System.ComponentModel.DataAnnotations;

namespace DidIDoThatApp.Models;

/// <summary>
/// Represents a category for grouping maintenance tasks.
/// </summary>
public class Category
{
    /// <summary>
    /// Unique identifier for the category.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the category.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional icon identifier for the category.
    /// </summary>
    [MaxLength(50)]
    public string? Icon { get; set; }

    /// <summary>
    /// Date and time the category was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if this is a default/system category that cannot be deleted.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Navigation property for tasks in this category.
    /// </summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}

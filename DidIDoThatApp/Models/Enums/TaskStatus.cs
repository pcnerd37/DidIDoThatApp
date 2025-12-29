namespace DidIDoThatApp.Models.Enums;

/// <summary>
/// Represents the current status of a maintenance task.
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is not yet due. DueDate > Now
    /// </summary>
    UpToDate = 0,

    /// <summary>
    /// Task is due soon. DueDate within 20% of frequency interval from Now.
    /// </summary>
    DueSoon = 1,

    /// <summary>
    /// Task is overdue. DueDate < Now or task has never been completed.
    /// </summary>
    Overdue = 2
}

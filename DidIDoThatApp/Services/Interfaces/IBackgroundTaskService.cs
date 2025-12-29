namespace DidIDoThatApp.Services.Interfaces;

/// <summary>
/// Service for managing background tasks for notification recalculation.
/// </summary>
public interface IBackgroundTaskService
{
    /// <summary>
    /// Registers the background task for daily notification recalculation.
    /// </summary>
    Task RegisterDailyNotificationTaskAsync();

    /// <summary>
    /// Unregisters the background task.
    /// </summary>
    Task UnregisterDailyNotificationTaskAsync();

    /// <summary>
    /// Checks if the background task is registered.
    /// </summary>
    Task<bool> IsTaskRegisteredAsync();
}

using DidIDoThatApp.Services.Interfaces;

namespace DidIDoThatApp.Services;

/// <summary>
/// Default implementation of background task service.
/// Platform-specific implementations will override this.
/// </summary>
public class BackgroundTaskService : IBackgroundTaskService
{
    public virtual Task RegisterDailyNotificationTaskAsync()
    {
        // Base implementation does nothing
        // Platform-specific implementations will override
        return Task.CompletedTask;
    }

    public virtual Task UnregisterDailyNotificationTaskAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task<bool> IsTaskRegisteredAsync()
    {
        return Task.FromResult(false);
    }
}

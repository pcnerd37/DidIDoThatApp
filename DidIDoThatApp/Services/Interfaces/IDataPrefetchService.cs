namespace DidIDoThatApp.Services.Interfaces;

/// <summary>
/// Service for prefetching and caching data to improve navigation performance.
/// </summary>
public interface IDataPrefetchService
{
    /// <summary>
    /// Gets whether data has been prefetched and is ready.
    /// </summary>
    bool IsDataReady { get; }

    /// <summary>
    /// Prefetches all commonly needed data in the background.
    /// </summary>
    Task PrefetchAllAsync();

    /// <summary>
    /// Invalidates the cache and forces a refresh on next access.
    /// </summary>
    void InvalidateCache();

    /// <summary>
    /// Gets cached categories or fetches them if not cached.
    /// </summary>
    Task<IReadOnlyList<Models.Category>> GetCategoriesAsync();

    /// <summary>
    /// Gets cached tasks or fetches them if not cached.
    /// </summary>
    Task<IReadOnlyList<Models.TaskItem>> GetTasksAsync();

    /// <summary>
    /// Gets cached overdue tasks or fetches them if not cached.
    /// </summary>
    Task<IReadOnlyList<Models.TaskItem>> GetOverdueTasksAsync();

    /// <summary>
    /// Gets cached due soon tasks or fetches them if not cached.
    /// </summary>
    Task<IReadOnlyList<Models.TaskItem>> GetDueSoonTasksAsync();

    /// <summary>
    /// Gets cached recently completed tasks or fetches them if not cached.
    /// </summary>
    Task<IReadOnlyList<Models.TaskItem>> GetRecentlyCompletedTasksAsync(int count = 5);
}

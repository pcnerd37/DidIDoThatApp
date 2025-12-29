using DidIDoThatApp.Models;
using DidIDoThatApp.Services.Interfaces;

namespace DidIDoThatApp.Services;

/// <summary>
/// Service for prefetching and caching data to improve navigation performance.
/// </summary>
public class DataPrefetchService : IDataPrefetchService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _lock = new(1, 1);
    
    private IReadOnlyList<Category>? _cachedCategories;
    private IReadOnlyList<TaskItem>? _cachedTasks;
    private IReadOnlyList<TaskItem>? _cachedOverdueTasks;
    private IReadOnlyList<TaskItem>? _cachedDueSoonTasks;
    private IReadOnlyList<TaskItem>? _cachedRecentlyCompleted;
    private DateTime _lastPrefetch = DateTime.MinValue;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public bool IsDataReady => _cachedTasks != null && _cachedCategories != null;

    public DataPrefetchService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PrefetchAllAsync()
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Skip if recently prefetched
            if (DateTime.UtcNow - _lastPrefetch < TimeSpan.FromSeconds(30) && IsDataReady)
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

            // Prefetch all data in parallel
            var categoriesTask = categoryService.GetAllCategoriesAsync();
            var tasksTask = taskService.GetAllTasksAsync();
            var overdueTask = taskService.GetOverdueTasksAsync();
            var dueSoonTask = taskService.GetDueSoonTasksAsync();
            var recentTask = taskService.GetRecentlyCompletedTasksAsync(5);

            await Task.WhenAll(categoriesTask, tasksTask, overdueTask, dueSoonTask, recentTask).ConfigureAwait(false);

            _cachedCategories = await categoriesTask;
            _cachedTasks = await tasksTask;
            _cachedOverdueTasks = await overdueTask;
            _cachedDueSoonTasks = await dueSoonTask;
            _cachedRecentlyCompleted = await recentTask;
            _lastPrefetch = DateTime.UtcNow;

            System.Diagnostics.Debug.WriteLine($"DataPrefetchService: Prefetched {_cachedCategories.Count} categories, {_cachedTasks.Count} tasks");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DataPrefetchService error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public void InvalidateCache()
    {
        _cachedCategories = null;
        _cachedTasks = null;
        _cachedOverdueTasks = null;
        _cachedDueSoonTasks = null;
        _cachedRecentlyCompleted = null;
        _lastPrefetch = DateTime.MinValue;
    }

    private bool IsCacheExpired => DateTime.UtcNow - _lastPrefetch > CacheExpiration;

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        if (_cachedCategories == null || IsCacheExpired)
        {
            await PrefetchAllAsync().ConfigureAwait(false);
        }
        return _cachedCategories ?? [];
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksAsync()
    {
        if (_cachedTasks == null || IsCacheExpired)
        {
            await PrefetchAllAsync().ConfigureAwait(false);
        }
        return _cachedTasks ?? [];
    }

    public async Task<IReadOnlyList<TaskItem>> GetOverdueTasksAsync()
    {
        if (_cachedOverdueTasks == null || IsCacheExpired)
        {
            await PrefetchAllAsync().ConfigureAwait(false);
        }
        return _cachedOverdueTasks ?? [];
    }

    public async Task<IReadOnlyList<TaskItem>> GetDueSoonTasksAsync()
    {
        if (_cachedDueSoonTasks == null || IsCacheExpired)
        {
            await PrefetchAllAsync().ConfigureAwait(false);
        }
        return _cachedDueSoonTasks ?? [];
    }

    public async Task<IReadOnlyList<TaskItem>> GetRecentlyCompletedTasksAsync(int count = 5)
    {
        if (_cachedRecentlyCompleted == null || IsCacheExpired)
        {
            await PrefetchAllAsync().ConfigureAwait(false);
        }
        return _cachedRecentlyCompleted?.Take(count).ToList() ?? [];
    }
}

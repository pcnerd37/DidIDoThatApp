using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DidIDoThatApp.Helpers;
using DidIDoThatApp.Models;
using DidIDoThatApp.Services.Interfaces;
using TaskStatus = DidIDoThatApp.Models.Enums.TaskStatus;

namespace DidIDoThatApp.ViewModels;

/// <summary>
/// ViewModel for a task item with calculated status properties.
/// </summary>
public partial class TaskItemViewModel : ObservableObject
{
    private readonly TaskItem _task;

    public TaskItemViewModel(TaskItem task)
    {
        _task = task;
        UpdateCalculatedProperties();
    }

    public Guid Id => _task.Id;
    public string Name => _task.Name;
    public string? Description => _task.Description;
    public string FrequencyDescription => _task.FrequencyDescription;
    public string? CategoryName => _task.Category?.Name;
    public string? CategoryIcon => _task.Category?.Icon;
    public bool IsReminderEnabled => _task.IsReminderEnabled;
    public TaskItem Task => _task;

    [ObservableProperty]
    private DateTime? _lastCompletedDate;

    [ObservableProperty]
    private DateTime? _dueDate;

    [ObservableProperty]
    private TaskStatus _status;

    [ObservableProperty]
    private string _statusDescription = string.Empty;

    [ObservableProperty]
    private Color _statusColor = Colors.Gray;

    public void UpdateCalculatedProperties()
    {
        var lastCompleted = _task.TaskLogs
            .OrderByDescending(l => l.CompletedDate)
            .FirstOrDefault()?.CompletedDate;

        LastCompletedDate = lastCompleted;
        DueDate = StatusCalculator.CalculateDueDate(_task, lastCompleted);
        Status = StatusCalculator.CalculateStatus(_task, lastCompleted, DateTime.Now);
        StatusDescription = StatusCalculator.GetDueDescription(DueDate, DateTime.Now);
        StatusColor = Status switch
        {
            TaskStatus.Overdue => Colors.Red,
            TaskStatus.DueSoon => Colors.Orange,
            TaskStatus.UpToDate => Colors.Green,
            _ => Colors.Gray
        };
    }
}

/// <summary>
/// ViewModel for the Dashboard page.
/// </summary>
public partial class DashboardViewModel : BaseViewModel
{
    private readonly ITaskService _taskService;
    private readonly IDataPrefetchService? _prefetchService;

    public DashboardViewModel(ITaskService taskService)
    {
        _taskService = taskService;
        _prefetchService = App.DataPrefetchService;
        Title = "Dashboard";
    }

    [ObservableProperty]
    private ObservableCollection<TaskItemViewModel> _overdueTasks = [];

    [ObservableProperty]
    private ObservableCollection<TaskItemViewModel> _dueSoonTasks = [];

    [ObservableProperty]
    private ObservableCollection<TaskItemViewModel> _recentlyCompletedTasks = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoTasks))]
    private bool _hasOverdueTasks;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoTasks))]
    private bool _hasDueSoonTasks;

    [ObservableProperty]
    private bool _hasRecentlyCompletedTasks;

    /// <summary>
    /// True when there are no overdue or due soon tasks to display.
    /// </summary>
    public bool HasNoTasks => !HasOverdueTasks && !HasDueSoonTasks;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            IReadOnlyList<TaskItem> overdue;
            IReadOnlyList<TaskItem> dueSoon;
            IReadOnlyList<TaskItem> recent;

            // Use prefetch service if available for faster loading
            if (_prefetchService != null && _prefetchService.IsDataReady)
            {
                overdue = await _prefetchService.GetOverdueTasksAsync();
                dueSoon = await _prefetchService.GetDueSoonTasksAsync();
                recent = await _prefetchService.GetRecentlyCompletedTasksAsync(5);
            }
            else
            {
                overdue = await _taskService.GetOverdueTasksAsync();
                dueSoon = await _taskService.GetDueSoonTasksAsync();
                recent = await _taskService.GetRecentlyCompletedTasksAsync(5);
            }

            OverdueTasks = new ObservableCollection<TaskItemViewModel>(
                overdue.Select(t => new TaskItemViewModel(t)));
            HasOverdueTasks = OverdueTasks.Count > 0;

            DueSoonTasks = new ObservableCollection<TaskItemViewModel>(
                dueSoon.Select(t => new TaskItemViewModel(t)));
            HasDueSoonTasks = DueSoonTasks.Count > 0;

            RecentlyCompletedTasks = new ObservableCollection<TaskItemViewModel>(
                recent.Select(t => new TaskItemViewModel(t)));
            HasRecentlyCompletedTasks = RecentlyCompletedTasks.Count > 0;
        }, allowReentry: true);
    }

    [RelayCommand]
    private async Task CompleteTaskAsync(TaskItemViewModel? taskVm)
    {
        if (taskVm == null) return;

        await ExecuteAsync(async () =>
        {
            await _taskService.CompleteTaskAsync(taskVm.Id);
            _prefetchService?.InvalidateCache();
        });

        // Reload data after ExecuteAsync completes to avoid nested IsBusy blocking
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task NavigateToTaskDetailAsync(TaskItemViewModel? taskVm)
    {
        if (taskVm == null) return;

        try
        {
            await Shell.Current.GoToAsync($"{Constants.Routes.TaskDetail}?taskId={taskVm.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex}");
        }
    }
}

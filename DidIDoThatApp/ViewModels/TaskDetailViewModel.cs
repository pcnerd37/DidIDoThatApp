using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DidIDoThatApp.Helpers;
using DidIDoThatApp.Models;
using DidIDoThatApp.Services.Interfaces;
using TaskStatus = DidIDoThatApp.Models.Enums.TaskStatus;

namespace DidIDoThatApp.ViewModels;

/// <summary>
/// ViewModel for the Task Detail page.
/// </summary>
[QueryProperty(nameof(TaskIdString), "taskId")]
public partial class TaskDetailViewModel : BaseViewModel
{
    private readonly ITaskService _taskService;
    private readonly ITaskLogService _taskLogService;

    public TaskDetailViewModel(ITaskService taskService, ITaskLogService taskLogService)
    {
        _taskService = taskService;
        _taskLogService = taskLogService;
        Title = "Task Details";
    }

    /// <summary>
    /// String version of TaskId for Shell navigation query property.
    /// </summary>
    public string? TaskIdString
    {
        get => TaskId.ToString();
        set
        {
            if (Guid.TryParse(value, out var guid))
            {
                TaskId = guid;
            }
        }
    }

    [ObservableProperty]
    private Guid _taskId;

    [ObservableProperty]
    private TaskItem? _task;

    [ObservableProperty]
    private string _taskName = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string _frequencyDescription = string.Empty;

    [ObservableProperty]
    private string? _categoryName;

    [ObservableProperty]
    private string? _categoryIcon;

    [ObservableProperty]
    private bool _isReminderEnabled;

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

    [ObservableProperty]
    private ObservableCollection<TaskLog> _completionHistory = [];

    partial void OnTaskIdChanged(Guid value)
    {
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (TaskId == Guid.Empty) return;

        await ExecuteAsync(async () =>
        {
            Task = await _taskService.GetTaskByIdAsync(TaskId);
            if (Task == null) return;

            TaskName = Task.Name;
            Title = Task.Name;
            Description = Task.Description;
            FrequencyDescription = Task.FrequencyDescription;
            CategoryName = Task.Category?.Name;
            CategoryIcon = Task.Category?.Icon;
            IsReminderEnabled = Task.IsReminderEnabled;

            var logs = await _taskLogService.GetLogsForTaskAsync(TaskId);
            CompletionHistory = new ObservableCollection<TaskLog>(logs);

            UpdateStatusProperties();
        });
    }

    private void UpdateStatusProperties()
    {
        if (Task == null) return;

        var lastCompleted = Task.TaskLogs
            .OrderByDescending(l => l.CompletedDate)
            .FirstOrDefault()?.CompletedDate;

        LastCompletedDate = lastCompleted;
        DueDate = StatusCalculator.CalculateDueDate(Task, lastCompleted);
        Status = StatusCalculator.CalculateStatus(Task, lastCompleted, DateTime.Now);
        StatusDescription = StatusCalculator.GetDueDescription(DueDate, DateTime.Now);
        StatusColor = Status switch
        {
            TaskStatus.Overdue => Colors.Red,
            TaskStatus.DueSoon => Colors.Orange,
            TaskStatus.UpToDate => Colors.Green,
            _ => Colors.Gray
        };
    }

    [RelayCommand]
    private async Task CompleteTaskAsync()
    {
        if (Task == null) return;

        await ExecuteAsync(async () =>
        {
            await _taskService.CompleteTaskAsync(TaskId);
        });

        // Reload data after ExecuteAsync completes to avoid nested IsBusy blocking
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task CompleteWithDateAsync()
    {
        if (Task == null) return;

        // TODO: Show date picker dialog
        var selectedDate = DateTime.Now;

        await ExecuteAsync(async () =>
        {
            await _taskService.CompleteTaskAsync(TaskId, selectedDate);
        });

        // Reload data after ExecuteAsync completes to avoid nested IsBusy blocking
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task NavigateToEditAsync()
    {
        await Shell.Current.GoToAsync($"{Constants.Routes.AddEditTask}?taskId={TaskId}");
    }

    [RelayCommand]
    private async Task DeleteTaskAsync()
    {
        if (Task == null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Task",
            $"Are you sure you want to delete '{TaskName}'?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            await ExecuteAsync(async () =>
            {
                await _taskService.DeleteTaskAsync(TaskId);
                await Shell.Current.GoToAsync("..");
            });
        }
    }

    [RelayCommand]
    private async Task DeleteLogAsync(TaskLog? log)
    {
        if (log == null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Log",
            "Are you sure you want to delete this completion record?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            await ExecuteAsync(async () =>
            {
                await _taskLogService.DeleteLogAsync(log.Id);
            });

            // Reload data after ExecuteAsync completes to avoid nested IsBusy blocking
            await LoadDataAsync();
        }
    }
}

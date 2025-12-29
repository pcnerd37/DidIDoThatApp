using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DidIDoThatApp.Helpers;
using DidIDoThatApp.Models;
using DidIDoThatApp.Services.Interfaces;

namespace DidIDoThatApp.ViewModels;

/// <summary>
/// ViewModel for a category with its tasks.
/// </summary>
public partial class CategoryGroupViewModel : ObservableObject
{
    public CategoryGroupViewModel(Category category, IEnumerable<TaskItemViewModel> tasks)
    {
        Category = category;
        Tasks = new ObservableCollection<TaskItemViewModel>(tasks);
    }

    public Category Category { get; }
    public string Name => $"{Category.Icon} {Category.Name}";
    public Guid Id => Category.Id;

    [ObservableProperty]
    private ObservableCollection<TaskItemViewModel> _tasks;

    [ObservableProperty]
    private bool _isExpanded = true;
}

/// <summary>
/// ViewModel for the Task List page.
/// </summary>
public partial class TaskListViewModel : BaseViewModel
{
    private readonly ITaskService _taskService;
    private readonly ICategoryService _categoryService;

    public TaskListViewModel(ITaskService taskService, ICategoryService categoryService)
    {
        _taskService = taskService;
        _categoryService = categoryService;
        Title = "Tasks";
    }

    [ObservableProperty]
    private ObservableCollection<CategoryGroupViewModel> _categoryGroups = [];

    [ObservableProperty]
    private bool _hasNoTasks;

    [ObservableProperty]
    private Guid? _selectedCategoryId;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var allTasks = await _taskService.GetAllTasksAsync();

            var groups = new List<CategoryGroupViewModel>();

            foreach (var category in categories)
            {
                var tasksForCategory = allTasks
                    .Where(t => t.CategoryId == category.Id)
                    .Select(t => new TaskItemViewModel(t))
                    .ToList();

                if (tasksForCategory.Count > 0 || SelectedCategoryId == null)
                {
                    groups.Add(new CategoryGroupViewModel(category, tasksForCategory));
                }
            }

            CategoryGroups = new ObservableCollection<CategoryGroupViewModel>(groups);
            HasNoTasks = !allTasks.Any();
        }, allowReentry: true);
    }

    [RelayCommand]
    private async Task CompleteTaskAsync(TaskItemViewModel? taskVm)
    {
        if (taskVm == null) return;

        await ExecuteAsync(async () =>
        {
            await _taskService.CompleteTaskAsync(taskVm.Id);
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

    [RelayCommand]
    private async Task NavigateToAddTaskAsync()
    {
        await Shell.Current.GoToAsync(Constants.Routes.AddEditTask);
    }

    [RelayCommand]
    private async Task DeleteTaskAsync(TaskItemViewModel? taskVm)
    {
        if (taskVm == null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Task",
            $"Are you sure you want to delete '{taskVm.Name}'?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            await ExecuteAsync(async () =>
            {
                await _taskService.DeleteTaskAsync(taskVm.Id);
            });

            // Reload data after ExecuteAsync completes to avoid nested IsBusy blocking
            await LoadDataAsync();
        }
    }
}
